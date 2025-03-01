<?php

namespace App\Http\Controllers;

use App\Models\CloudService;
use App\Models\UserCloudService;
use Google\Client;
use Google\Service\Drive;
use Illuminate\Http\Request;
use Illuminate\Support\Facades\Auth;
use Illuminate\Support\Facades\Cache;
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
        $client->addScope("https://www.googleapis.com/auth/drive");
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

        if ($userCloudService) {
            // Если запись найдена, обновляем
            $userCloudService->update([
                'access_token' => $token['access_token'],
                'refresh_token' => $token['refresh_token'] ?? $userCloudService->refresh_token,
                'expires_at' => now()->addSeconds($token['expires_in']),
            ]);
        } else {
            // Если запись не найдена, создаем
            UserCloudService::query()->create([
                'cloud_service_id' => $cloudService->id,
                'user_id' => $userId,
                'access_token' => $token['access_token'],
                'refresh_token' => $token['refresh_token'] ?? null,
                'expires_at' => now()->addSeconds($token['expires_in']),
            ]);
        }

        return response(null, 204);
    }

    public function uploadFile(Request $request)
    {
        $request->validate([
            'file' => 'required|file',
        ]);

        $file = $request->file('file');
        $filePath = $file->getPathname();
        $fileName = $file->getClientOriginalName();

        $cloudService = CloudService::query()->where('name', 'Google Drive')->first();
        $service = UserCloudService::query()->where('user_id', 1)->where('cloud_service_id', $cloudService->id)->first();

        $client = new Client();
        $client->setClientId(env('GOOGLE_DRIVE_CLIENT_ID'));
        $client->setClientSecret(env('GOOGLE_DRIVE_CLIENT_SECRET'));
        $client->setRedirectUri(env('GOOGLE_DRIVE_REDIRECT_URI'));
        $client->setAccessToken([
            'access_token' => $service->access_token,
            'refresh_token' => $service->refresh_token,
            'expires_in' => $service->expires_at->diffInSeconds(now()),
            'created' => now()->timestamp - ($service->expires_at->timestamp - $service->expires_at->diffInSeconds(now())),
        ]);

        if ($client->isAccessTokenExpired()) {
            $newToken = $client->fetchAccessTokenWithRefreshToken($service->refresh_token);
            $service->update([
                'access_token' => $newToken['access_token'],
                'expires_at' => now()->addSeconds($newToken['expires_in']),
            ]);
            $client->setAccessToken([
                'access_token' => $newToken['access_token'],
                'refresh_token' => $service->refresh_token, // Токен обновления остается прежним
            ]);
        }

        $driveService = new Drive($client);

        $fileMetadata = new Drive\DriveFile(['name' => $fileName]);
        $content = file_get_contents($filePath);

        try {
            $file = $driveService->files->create($fileMetadata, [
                'data' => $content,
                'mimeType' => mime_content_type($filePath),
                'uploadType' => 'multipart',
                'fields' => 'id',
            ]);
        } catch (\Exception $e) {
            return response()->json(['error' => 'Failed to upload file to Google Drive: ' . $e->getMessage()], 500);
        }

        return response()->json(['fileId' => $file->id], 200);
        //TODO: файл успешно загружается, но надо связывать его, крч доработать
    }

}
