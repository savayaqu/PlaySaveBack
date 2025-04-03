<?php

namespace App\Http\Controllers;

use App\Exceptions\ConflictException;
use App\Http\Requests\Api\Save\OverwriteSaveRequest;
use App\Http\Requests\Api\Save\UploadSaveRequest;
use App\Http\Resources\SaveResource;
use App\Models\CloudService;
use App\Models\Game;
use App\Models\Save;
use App\Models\SideGame;
use App\Models\UserCloudService;
use App\Services\GoogleDriveService;
use Carbon\Carbon;
use Google\Client;
use Google\Service\Drive;
use Illuminate\Http\JsonResponse;
use Illuminate\Http\Request;
use Illuminate\Support\Facades\Auth;
use Illuminate\Support\Facades\Cache;
use Illuminate\Support\Facades\Crypt;
use Illuminate\Support\Facades\Log;
use phpseclib3\Exception\TimeoutException;
use Symfony\Component\HttpKernel\Exception\ConflictHttpException;

class GoogleDriveController extends Controller
{
    public function getAuthUrl(Request $request): JsonResponse
    {
        $user = $request->user();
        $client = new Client();
        $client->setClientId(env('GOOGLE_DRIVE_CLIENT_ID'));
        $client->setClientSecret(env('GOOGLE_DRIVE_CLIENT_SECRET'));
        $client->setRedirectUri(env('GOOGLE_DRIVE_REDIRECT_URI'));
        $client->addScope("https://www.googleapis.com/auth/drive.file");
        $client->setAccessType('offline');
        $client->setPrompt('consent');

        $state = bin2hex(random_bytes(16)) . '_' . $user->id;
        Cache::put("oauth_state:{$state}", $user->id, now()->addMinutes(10));
        $client->setState($state);

        return response()->json([
            'success' => true,
            'url' => $client->createAuthUrl()
        ]);
    }

    public function callback(Request $request)
    {
        $state = $request->state;
        $userId = Cache::pull("oauth_state:{$state}");

        $client = new Client();
        $client->setClientId(env('GOOGLE_DRIVE_CLIENT_ID'));
        $client->setClientSecret(env('GOOGLE_DRIVE_CLIENT_SECRET'));
        $client->setRedirectUri(env('GOOGLE_DRIVE_REDIRECT_URI'));

        $cloudService = CloudService::query()->where('name', 'Google Drive')->first();
        $token = $client->fetchAccessTokenWithAuthCode($request->get('code'));

        $userCloudService = UserCloudService::query()->updateOrCreate(
            [
                'user_id' => $userId,
                'cloud_service_id' => $cloudService->id,
            ],
            [
                'access_token' => Crypt::encryptString($token['access_token']),
                'refresh_token' => Crypt::encryptString($token['refresh_token'] ?? null),
                'expires_at' => now()->addSeconds($token['expires_in']),
            ]
        );
        return redirect('auth/success');
    }
    /**
     * Генерирует URL для прямой загрузки файла в Google Drive
     */
    public function generateUploadUrl(Request $request): JsonResponse
    {
        $user = auth()->user();
        $game = $this->resolveGame($request);

        // Проверка существующей версии
        $existSave = $user->saves()
            ->where(function($q) use ($game) {
                $q->where('game_id', $game->id)
                    ->orWhere('side_game_id', $game->id);
            })
            ->where('version', $request->version)
            ->exists();

        if ($existSave) {
            throw new ConflictException();
        }

        // Создаем запись о файле до загрузки
        $saveData = [
            'version' => $request->version,
            'description' => $request->description,
            'user_id' => $user->id,
            'status' => 'uploading',
            'file_name' => $request->input('file_name'),
            'size' => $request->input('file_size'),
        ];

        $game instanceof Game
            ? $saveData['game_id'] = $game->id
            : $saveData['side_game_id'] = $game->id;

        $save = Save::create($saveData);

        // Генерируем URL для загрузки
        $cloudService = CloudService::where('name', 'Google Drive')->first();
        $service = UserCloudService::where('user_id', $user->id)
            ->where('cloud_service_id', $cloudService->id)
            ->first();

        $googleDriveService = new GoogleDriveService($service);

        $folderPath = "PlaySaveBack/{$game->name}/{$request->version}";
        $uploadUrl = $googleDriveService->generateResumableUploadUrl(
            fileName: $request->input('file_name'),
            folderPath: $folderPath
        );

        return response()->json([
            'upload_url' => $uploadUrl,
            'save_id' => $save->id,
            'expires_at' => now()->addHours(1)->toIso8601String()
        ]);
    }
    /**
     * Подтверждает успешную загрузку файла
     */
    public function confirmUpload(Request $request, Save $save): JsonResponse
    {
        $request->validate([
            'file_id' => 'required|string',
            'file_hash' => 'required|string'
        ]);

        $user = auth()->user();
        $cloudService = CloudService::where('name', 'Google Drive')->first();
        $service = UserCloudService::where('user_id', $user->id)
            ->where('cloud_service_id', $cloudService->id)
            ->first();

        $save->update([
            'file_id' => $request->file_id,
            'hash' => $request->file_hash,
            'last_sync_at' => now(),
            'user_cloud_service_id' => $service->id
        ]);

        return response()->json(SaveResource::make($save));
    }
    private function resolveGame(Request $request)
    {
        if ($request->has('side_game_id') && $request->side_game_id != null) {
            return SideGame::findOrFail($request->side_game_id);
        }
        return Game::findOrFail($request->game_id);
    }

    public function downloadFile(Save $save)
    {
        $user = auth()->user();
        $fileId = $save->file_id;
        $cloudService = CloudService::query()->where('name', 'Google Drive')->first();
        $service = UserCloudService::query()->where('user_id', $user->id)->where('cloud_service_id', $cloudService->id)->first();

        $googleDriveService = new GoogleDriveService($service);
        $fileData = $googleDriveService->downloadFile($fileId);

        return response($fileData['content'], 200, [
            'Content-Type' => $fileData['mimeType'],
            'Content-Disposition' => 'attachment; filename="' . $fileData['fileName'] . '"',
        ]);
    }

    public function shareFile(Save $save)
    {
        $user = auth()->user();
        $fileId = $save->file_id;
        $cloudService = CloudService::query()->where('name', 'Google Drive')->first();
        $service = UserCloudService::query()->where('user_id', $user->id)->where('cloud_service_id', $cloudService->id)->first();

        $googleDriveService = new GoogleDriveService($service);
        $url = $googleDriveService->shareFile($fileId);

        return response()->json(['url' => $url], 200);
    }

    public function deleteFile(Save $save)
    {
        $user = auth()->user();
        $fileId = $save->file_id;
        $cloudService = CloudService::query()->where('name', 'Google Drive')->first();
        $service = UserCloudService::query()->where('user_id', $user->id)->where('cloud_service_id', $cloudService->id)->first();

        $googleDriveService = new GoogleDriveService($service);
        $googleDriveService->deleteFile($fileId);

        Save::query()->where('file_id', $fileId)->where('user_id', $user->id)->delete();

        return response()->json(['message' => 'File deleted successfully'], 200);
    }
    public function overwriteFile(OverwriteSaveRequest $request, Save $save)
    {
        $user = auth()->user();
        $fileId = $save->file_id;
        $file = $request->file('file');
        $filePath = $file->getPathname();
        $fileName = $file->getClientOriginalName();
        $fileSize = $file->getSize();

        $cloudService = CloudService::query()->where('name', 'Google Drive')->first();
        $service = UserCloudService::query()->where('user_id', $user->id)->where('cloud_service_id', $cloudService->id)->first();

        $googleDriveService = new GoogleDriveService($service);

        try {
            // Перезаписываем файл
            $newFileId = $googleDriveService->overwriteFile($fileId, $filePath, $fileName);
            // Обновляем запись в базе данных
            $save = Save::query()->where('file_id', $fileId)->where('user_id', $user->id)->first();
            $save->update([
                'file_id' => $newFileId,
                'file_name' => $fileName,
                'size' => $fileSize,
                'hash' => hash('sha256', file_get_contents($file)),
                'description' => $request->description ?? null,
                'last_sync_at' => Carbon::now(),
            ]);

            return response()->json(SaveResource::make($save), 200);
        } catch (\Exception $e) {
            return response()->json(['error' => $e->getMessage()], 500);
        }
    }
}
