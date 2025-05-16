<?php

namespace App\Http\Controllers;

use App\Enums\CloudStatus;
use App\Exceptions\ConflictException;
use App\Exceptions\ForbiddenException;
use App\Http\Requests\Api\Save\ConfirmUploadSave;
use App\Http\Requests\Api\Save\OverwriteSaveRequest;
use App\Http\Requests\Api\Save\UploadSaveRequest;
use App\Http\Resources\SaveResource;
use App\Models\CloudService;
use App\Models\Game;
use App\Models\Save;
use App\Models\SideGame;
use App\Models\UserCloudService;
use App\Services\GoogleDriveService;
use Google\Client;
use Illuminate\Http\JsonResponse;
use Illuminate\Http\Request;
use Illuminate\Support\Facades\Cache;
use Illuminate\Support\Facades\Crypt;

class GoogleDriveController extends Controller
{
    public static function getSubFromIdToken(string $idToken): ?string
    {
        $parts = explode('.', $idToken);
        if (count($parts) < 2) {
            return null;
        }

        $payload = $parts[1];

        // Добавим padding для base64 (если не кратно 4)
        $payload .= str_repeat('=', 4 - strlen($payload) % 4);

        $json = base64_decode(strtr($payload, '-_', '+/'));
        if (!$json) {
            return null;
        }

        $data = json_decode($json, true);
        return $data['sub'] ?? null;
    }

    public function getAuthUrl(Request $request): JsonResponse
    {
        $user = $request->user();
        $client = new Client();
        $client->setClientId(env('GOOGLE_DRIVE_CLIENT_ID'));
        $client->setClientSecret(env('GOOGLE_DRIVE_CLIENT_SECRET'));
        $client->setRedirectUri(env('GOOGLE_DRIVE_REDIRECT_URI'));
        $client->addScope("openid");
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

        $token = $client->fetchAccessTokenWithAuthCode($request->get('code'));
        $idToken = $token['id_token'] ?? null;
        $externalUserId = $idToken ? GoogleDriveController::getSubFromIdToken($idToken) : null;

        $cloudService = CloudService::query()->where('name', 'Google Drive')->first();

        $userCloudService = UserCloudService::query()->updateOrCreate(
            [
                'cloud_service_id' => $cloudService->id,
                'external_user_id' => $externalUserId,
            ],
            [
                'user_id' => $userId,
                'access_token' => Crypt::encryptString($token['access_token']),
                'refresh_token' => isset($token['refresh_token']) ? Crypt::encryptString($token['refresh_token']) : null,
                'expires_at' => now()->addSeconds($token['expires_in']),
                'external_user_id' => $externalUserId,
                'status' => CloudStatus::Active
            ]
        );

        return redirect('auth/success');
    }

    /**
     * Генерирует URL для прямой загрузки файла в Google Drive
     */
    public function generateUploadUrl(UploadSaveRequest $request): JsonResponse
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
            'file_name' => $request->file_name,
            'size' => $request->file_size,
        ];

        $game instanceof Game
            ? $saveData['game_id'] = $game->id
            : $saveData['side_game_id'] = $game->id;

        $save = Save::create($saveData);

        // Генерируем URL для загрузки
        $cloudService = CloudService::where('name', 'Google Drive')->first();
        $service = $user->userCloudService()
            ->where('cloud_service_id', $cloudService->id)
            ->where('status', CloudStatus::Active)
            ->firstOrFail();

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
    public function confirmUpload(ConfirmUploadSave $request, Save $save): JsonResponse
    {
        $request->validate([
            'file_id' => 'required|string',
            'file_hash' => 'required|string'
        ]);

        $user = auth()->user();
        $cloudService = CloudService::where('name', 'Google Drive')->first();
        $service = $user->userCloudService()
            ->where('cloud_service_id', $cloudService->id)
            ->where('status', CloudStatus::Active)
            ->firstOrFail();

        $save->update([
            'file_id' => $request->file_id,
            'hash' => $request->file_hash,
            'last_sync_at' => now(),
            'user_cloud_service_id' => $service->id
        ]);

        return response()->json(SaveResource::make($save));
    }
    // Узнать тип игры
    private function resolveGame(Request $request)
    {
        if ($request->has('side_game_id') && $request->side_game_id != null) {
            return SideGame::query()->findOrFail($request->side_game_id);
        }
        return Game::query()->findOrFail($request->game_id);
    }
    // Генерация ссылки на перезапись сохранения
    public function generateOverwriteUrl(Save $save, Request $request)
    {
        $request->validate([
            'file_name' => 'required|string',
            'file_size' => 'required|integer'
        ]);

        $user = auth()->user();
        $cloudService = CloudService::where('name', 'Google Drive')->first();
        $service = $user->userCloudService()
            ->where('cloud_service_id', $cloudService->id)
            ->where('status', CloudStatus::Active)
            ->firstOrFail();
        $googleDriveService = new GoogleDriveService($service);

        return response()->json([
            'upload_url' => $googleDriveService->generateResumableOverwriteUrl(
                $save->file_id,
                $request->input('file_name')
            ),
            'expires_at' => now()->addHours(1)->toIso8601String()
        ]);
    }
    // Скаичвание сохранения
    public function downloadFile(Save $save)
    {
        $user = auth()->user();
        if($save->user_id != $user->id)
            throw new ForbiddenException();
        $fileId = $save->file_id;
        $cloudService = CloudService::query()->where('name', 'Google Drive')->first();
        $service = $user->userCloudService()
            ->where('cloud_service_id', $cloudService->id)
            ->where('status', CloudStatus::Active)
            ->firstOrFail();

        $googleDriveService = new GoogleDriveService($service);
        $fileData = $googleDriveService->downloadFile($fileId);

        return response($fileData['content'], 200, [
            'Content-Type' => $fileData['mimeType'],
            'Content-Disposition' => 'attachment; filename="' . $fileData['fileName'] . '"',
        ]);
    }
    // Поделиться сохранением
    public function shareFile(Save $save)
    {
        $user = auth()->user();
        if($save->user_id != $user->id)
            throw new ForbiddenException();
        $fileId = $save->file_id;
        $cloudService = CloudService::query()->where('name', 'Google Drive')->first();
        $service = $user->userCloudService()
            ->where('cloud_service_id', $cloudService->id)
            ->where('status', CloudStatus::Active)
            ->firstOrFail();
        $googleDriveService = new GoogleDriveService($service);
        $url = $googleDriveService->shareFile($fileId);

        return response()->json(['url' => $url]);
    }
    // Удаление сохранения
    public function deleteFile(Save $save)
    {
        $user = auth()->user();
        if($save->user_id != $user->id)
            throw new ForbiddenException();
        $fileId = $save->file_id;
        $cloudService = CloudService::query()->where('name', 'Google Drive')->first();
        $service = $user->userCloudService()
            ->where('cloud_service_id', $cloudService->id)
            ->where('status', CloudStatus::Active)
            ->firstOrFail();
        $googleDriveService = new GoogleDriveService($service);
        $googleDriveService->deleteFile($fileId);

        $save->delete();

        return response()->json(['message' => 'File deleted successfully'], 200);
    }
    // Отключение GoogleDrive
    public function disconnect(UserCloudService $userCloudService)
    {
        $user = auth()->user();

        if ($userCloudService->user_id != $user->id)
            throw new ForbiddenException();

        $userCloudService->update([
            'access_token' => null,
            'refresh_token' => null,
            'expires_at' => null,
            'status' => CloudStatus::Inactive,
        ]);
        return response()->json(null, 204);
    }
}
