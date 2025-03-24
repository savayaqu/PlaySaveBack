<?php

namespace App\Http\Controllers;

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

    public function uploadFile(UploadSaveRequest $request)
    {
        $user = auth()->user();
        $fileRequest = $request->file('file');
        $filePath = $fileRequest->getPathname();
        $fileName = $fileRequest->getClientOriginalName();
        $fileSize = $fileRequest->getSize();

        $cloudService = CloudService::query()->where('name', 'Google Drive')->first();
        $service = UserCloudService::query()->where('user_id', $user->id)->where('cloud_service_id', $cloudService->id)->first();

        $googleDriveService = new GoogleDriveService($service);

        // Определяем, какой ID использовать: side_game_id или game_id
        $gameId = $request->game_id;
        $sideGameId = $request->side_game_id;

        // Получаем название игры для создания папки
        $game = Game::query()->find($gameId); // Если game_id передан, используем его
        if ($sideGameId) {
            $game = SideGame::query()->find($sideGameId); // Если side_game_id передан, ищем игру по нему
        }

        // Создаем структуру папок
        $rootFolderId = $googleDriveService->getOrCreateFolder('PlaySaveBack');
        $gameFolderId = $googleDriveService->getOrCreateFolder($game->name, $rootFolderId);
        $saveVersionFolderId = $googleDriveService->getOrCreateFolder($request->version, $gameFolderId);

        // Загружаем файл
        $fileId = $googleDriveService->uploadFile($filePath, $fileName, $saveVersionFolderId);

        // Подготавливаем данные для создания Save
        $saveData = [
            'file_id' => $fileId,
            'file_name' => $fileName,
            'version' => $request->version,
            'size' => $fileSize,
            'description' => $request->description ?? null,
            'user_id' => $user->id,
            'hash' => hash('sha256', file_get_contents($fileRequest)),
            'last_sync_at' => Carbon::now(),
            'user_cloud_service_id' => $service->id,
        ];

        // Заполняем либо game_id, либо side_game_id
        if ($sideGameId) {
            $saveData['side_game_id'] = $sideGameId;
        } else {
            $saveData['game_id'] = $gameId;
        }

        // Сохраняем информацию о файле в базе данных
        $save = Save::query()->create($saveData);

        return response()->json(SaveResource::make($save), 201);
    }

    public function downloadFile($fileId)
    {
        $user = auth()->user();
        $cloudService = CloudService::query()->where('name', 'Google Drive')->first();
        $service = UserCloudService::query()->where('user_id', $user->id)->where('cloud_service_id', $cloudService->id)->first();

        $googleDriveService = new GoogleDriveService($service);
        $fileData = $googleDriveService->downloadFile($fileId);

        return response($fileData['content'], 200, [
            'Content-Type' => $fileData['mimeType'],
            'Content-Disposition' => 'attachment; filename="' . $fileData['fileName'] . '"',
        ]);
    }

    public function shareFile($fileId)
    {
        $user = auth()->user();
        $cloudService = CloudService::query()->where('name', 'Google Drive')->first();
        $service = UserCloudService::query()->where('user_id', $user->id)->where('cloud_service_id', $cloudService->id)->first();

        $googleDriveService = new GoogleDriveService($service);
        $url = $googleDriveService->shareFile($fileId);

        return response()->json(['url' => $url], 200);
    }

    public function deleteFile($fileId)
    {
        $user = auth()->user();
        $cloudService = CloudService::query()->where('name', 'Google Drive')->first();
        $service = UserCloudService::query()->where('user_id', $user->id)->where('cloud_service_id', $cloudService->id)->first();

        $googleDriveService = new GoogleDriveService($service);
        $googleDriveService->deleteFile($fileId);

        Save::query()->where('file_id', $fileId)->where('user_id', $user->id)->delete();

        return response()->json(['message' => 'File deleted successfully'], 200);
    }
    public function overwriteFile(OverwriteSaveRequest $request, $fileId)
    {
        $user = auth()->user();
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
