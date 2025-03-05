<?php

namespace App\Http\Controllers;

use App\Http\Requests\Api\Save\UploadSaveRequest;
use App\Models\CloudService;
use App\Models\Game;
use App\Models\Save;
use App\Models\UserCloudService;
use Google\Client;
use Google\Service\Drive;
use Illuminate\Http\Request;
use Illuminate\Support\Facades\Auth;
use Illuminate\Support\Facades\Cache;
use Illuminate\Support\Facades\Crypt;
use Illuminate\Support\Facades\Log;

class GoogleDriveController extends Controller
{
    public function getAuthUrl(Request $request)
    {
        $user = $request->user();
        $client = new Client();
        $client->setClientId(env('GOOGLE_DRIVE_CLIENT_ID'));
        $client->setClientSecret(env('GOOGLE_DRIVE_CLIENT_SECRET'));
        $client->setRedirectUri(env('GOOGLE_DRIVE_REDIRECT_URI'));
        $client->addScope("https://www.googleapis.com/auth/drive.file");
        $client->setAccessType('offline'); // Запрашивает refresh_token
        $client->setPrompt('consent'); // Гарантирует показ экрана авторизации

        // Генерируем уникальный state (можно просто user_id)
        $state = bin2hex(random_bytes(16)) . '_' . $user->id;

        // Сохраняем state в Redis (или другую систему хранения)
        Cache::put("oauth_state:{$state}", $user->id, now()->addMinutes(10));
        $client->setState($state);
        //return redirect($client->createAuthUrl());
        return response()->json($client->createAuthUrl());
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
        // Попытка найти запись
        $userCloudService = UserCloudService::query()->where([
            'user_id' => $userId,
            'cloud_service_id' => $cloudService->id,
        ])->first();

        // Шифрование токена
        $encryptedAccessToken = Crypt::encryptString($token['access_token']);
        $encryptedRefreshToken = Crypt::encryptString($token['refresh_token']);

        if ($userCloudService) {
            // Если запись найдена, обновляем
            $userCloudService->update([
                'access_token' => $encryptedAccessToken,
                'refresh_token' => $encryptedRefreshToken ?? $userCloudService->refresh_token,
                'expires_at' => now()->addSeconds($token['expires_in']),
            ]);
        } else {
            // Если запись не найдена, создаем
            UserCloudService::query()->create([
                'cloud_service_id' => $cloudService->id,
                'user_id' => $userId,
                'access_token' => $encryptedAccessToken,
                'refresh_token' => $encryptedRefreshToken ?? null,
                'expires_at' => now()->addSeconds($token['expires_in']),
            ]);
        }

        return response(null, 204);
    }

    private function getOrCreateFolder($driveService, $folderName, $parentFolderId = null)
    {
        $query = "mimeType='application/vnd.google-apps.folder' and name='{$folderName}' and trashed=false";
        if ($parentFolderId) {
            $query .= " and '{$parentFolderId}' in parents";
        }

        $response = $driveService->files->listFiles([
            'q' => $query,
            'fields' => 'files(id, name)',
        ]);

        if (count($response->getFiles()))
            return $response->getFiles()[0]->getId();

        // Если папка не найдена, создаем её
        $folderMetadata = new Drive\DriveFile([
            'name' => $folderName,
            'mimeType' => 'application/vnd.google-apps.folder',
            'parents' => $parentFolderId ? [$parentFolderId] : [],
        ]);

        $folder = $driveService->files->create($folderMetadata, ['fields' => 'id']);
        return $folder->getId();
    }


    public function uploadFile(UploadSaveRequest $request)
    {
        $user = auth()->user();
        $game = Game::query()->find($request->game_id);
        $file = $request->file('file');
        $filePath = $file->getPathname();
        $fileName = $file->getClientOriginalName();
        $fileSize = $file->getSize();

        $cloudService = CloudService::query()->where('name', 'Google Drive')->first();
        $service = UserCloudService::query()->where('user_id', 1)->where('cloud_service_id', $cloudService->id)->first();

        // Расшифровка токенов
        $accessToken = Crypt::decryptString($service->access_token);
        $refreshToken = Crypt::decryptString($service->refresh_token);

        $client = new Client();
        $client->setClientId(env('GOOGLE_DRIVE_CLIENT_ID'));
        $client->setClientSecret(env('GOOGLE_DRIVE_CLIENT_SECRET'));
        $client->setRedirectUri(env('GOOGLE_DRIVE_REDIRECT_URI'));
        $client->setAccessToken([
            'access_token' => $accessToken,
            'refresh_token' => $refreshToken,
            'expires_in' => $service->expires_at->diffInSeconds(now()),
            'created' => now()->timestamp - ($service->expires_at->timestamp - $service->expires_at->diffInSeconds(now())),
        ]);

        if ($client->isAccessTokenExpired()) {
            $newToken = $client->fetchAccessTokenWithRefreshToken($refreshToken);

            // Шифрование новых токенов
            $encryptedAccessToken = Crypt::encryptString($newToken['access_token']);
            $encryptedRefreshToken = Crypt::encryptString($newToken['refresh_token'] ?? $refreshToken);

            // Обновление в базе данных
            $service->update([
                'access_token' => $encryptedAccessToken,
                'refresh_token' => $encryptedRefreshToken,
                'expires_at' => now()->addSeconds($newToken['expires_in']),
            ]);

            $client->setAccessToken([
                'access_token' => $newToken['access_token'],
                'refresh_token' => $refreshToken,
            ]);
        }

        $driveService = new Drive($client);

        // Создаем структуру папок
        $rootFolderName = 'PlaySaveBack';

        $gameFolderName = $game->name;
        $saveVersionFolderName = $request->version;

        // Получаем или создаем корневую папку
        $rootFolderId = $this->getOrCreateFolder($driveService, $rootFolderName);

        // Получаем или создаем папку с названием игры
        $gameFolderId = $this->getOrCreateFolder($driveService, $gameFolderName, $rootFolderId);

        // Получаем или создаем папку с версией сохранения
        $saveVersionFolderId = $this->getOrCreateFolder($driveService, $saveVersionFolderName, $gameFolderId);

        // Загружаем файл в папку с версией сохранения
        $fileMetadata = new Drive\DriveFile([
            'name' => $fileName,
            'parents' => [$saveVersionFolderId], // Указываем папку для загрузки
        ]);

        $content = file_get_contents($filePath);

        try {
            $file = $driveService->files->create($fileMetadata, [
                'data' => $content,
                'mimeType' => mime_content_type($filePath),
                'uploadType' => 'multipart',
                'fields' => 'id',
            ]);
            Save::query()->create([
               'file_id' => $file->id,
               'file_name' => $gameFolderName,
               'game_id' => $request->game_id,
               'version' => $saveVersionFolderName,
                'size' => $fileSize,
                'user_id' => $user->id,
                'user_cloud_service_id' => $user->userCloudService()->where('cloud_service_id', $cloudService->id)->first()->id,
            ]);
        } catch (\Exception $e) {
            return response()->json(['error' => 'Failed to upload file to Google Drive: ' . $e->getMessage()], 500);
        }

        return response()->json(['fileId' => $file->id], 200);
    }

}
