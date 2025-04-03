<?php

namespace App\Services;

use App\Exceptions\ApiException;
use App\Exceptions\GoogleApiException;
use Google\Client;
use Google\Service\Drive;
use Google\Service\Drive\DriveFile;
use Google\Service\Drive\Permission;
use Illuminate\Support\Facades\Crypt;

class GoogleDriveService
{
    private $client;
    private $driveService;
    private $userCloudService;

    public function __construct($userCloudService)
    {
        $this->userCloudService = $userCloudService;
        $this->initializeClient();
    }

    private function initializeClient(): void
    {
        $this->client = new Client();
        $this->client->setClientId(env('GOOGLE_DRIVE_CLIENT_ID'));
        $this->client->setClientSecret(env('GOOGLE_DRIVE_CLIENT_SECRET'));
        $this->client->setAccessType('offline');
        $this->setAccessToken();
        $this->driveService = new Drive($this->client);
    }

    private function setAccessToken(): void
    {
        $accessToken = Crypt::decryptString($this->userCloudService->access_token);
        $refreshToken = Crypt::decryptString($this->userCloudService->refresh_token);

        $this->client->setAccessToken([
            'access_token' => $accessToken,
            'refresh_token' => $refreshToken,
            'expires_in' => $this->userCloudService->expires_at->diffInSeconds(now()),
        ]);

        if ($this->client->isAccessTokenExpired()) {
            $this->refreshToken();
        }
    }

    private function refreshToken(): void
    {
        $refreshToken = Crypt::decryptString($this->userCloudService->refresh_token);
        $newToken = $this->client->fetchAccessTokenWithRefreshToken($refreshToken);

        $this->userCloudService->update([
            'access_token' => Crypt::encryptString($newToken['access_token']),
            'expires_at' => now()->addSeconds($newToken['expires_in']),
            'refresh_token' => isset($newToken['refresh_token'])
                ? Crypt::encryptString($newToken['refresh_token'])
                : $this->userCloudService->refresh_token,
        ]);

        $this->client->setAccessToken($newToken);
    }

    public function generateResumableUploadUrl(string $fileName, string $folderPath): string
    {
        try {
            $folderId = $this->createFolderStructure($folderPath);

            // 1. Создаем метаданные файла
            $fileMetadata = new DriveFile([
                'name' => $fileName,
                'parents' => [$folderId]
            ]);

            // 2. Получаем авторизованный HTTP-клиент
            $httpClient = $this->driveService->getClient()->authorize();

            // 3. Создаем PSR-7 запрос вручную
            $uri = 'https://www.googleapis.com/upload/drive/v3/files?' . http_build_query([
                    'uploadType' => 'resumable',
                    'fields' => 'id',
                    'supportsAllDrives' => 'true'
                ]);

            $request = new \GuzzleHttp\Psr7\Request(
                'POST',
                $uri,
                [
                    'Content-Type' => 'application/json',
                    'X-Upload-Content-Type' => 'application/octet-stream'
                ],
                json_encode($fileMetadata)
            );

            // 4. Отправляем запрос
            $response = $httpClient->send($request);

            // 5. Получаем URL для загрузки
            $location = $response->getHeaderLine('Location');
            if (empty($location)) {
                throw new ApiException('Google Drive did not return upload URL');
            }

            return $location;
        } catch (\Exception $e) {
            throw new ApiException('Failed to generate upload URL: ' . $e->getMessage());
        }
    }

    private function createFolderStructure(string $path): string
    {
        $parts = explode('/', $path);
        $parentId = 'root';

        foreach ($parts as $folderName) {
            $parentId = $this->getOrCreateFolder($folderName, $parentId);
        }

        return $parentId;
    }

    public function getOrCreateFolder($folderName, $parentId = null)
    {
        $query = "mimeType='application/vnd.google-apps.folder' and name='{$folderName}' and trashed=false";
        if ($parentId) {
            $query .= " and '{$parentId}' in parents";
        }

        $response = $this->driveService->files->listFiles([
            'q' => $query,
            'fields' => 'files(id)',
            'pageSize' => 1
        ]);

        if (count($response->getFiles()) > 0) {
            return $response->getFiles()[0]->getId();
        }

        $folderMetadata = new DriveFile([
            'name' => $folderName,
            'mimeType' => 'application/vnd.google-apps.folder',
            'parents' => $parentId ? [$parentId] : [],
        ]);

        $folder = $this->driveService->files->create($folderMetadata, ['fields' => 'id']);
        return $folder->getId();
    }


    public function uploadFile($filePath, $fileName, $folderId)
    {
        try {
            $fileMetadata = new DriveFile([
                'name' => $fileName,
                'parents' => [$folderId],
            ]);

            $content = file_get_contents($filePath);

            $file = $this->driveService->files->create($fileMetadata, [
                'data' => $content,
                'mimeType' => mime_content_type($filePath),
                'uploadType' => 'multipart',
                'fields' => 'id',
            ]);

            return $file->getId();
        } catch (\Google\Service\Exception $e) {
            throw GoogleApiException::fromGoogleException($e);
        } catch (\Exception $e) {
            throw new ApiException('Failed to uploadFile: ' . $e->getMessage(), $e->getCode());
        }
    }

    public function deleteFile($fileId)
    {
        try {
            $this->driveService->files->delete($fileId);
        } catch (\Google\Service\Exception $e) {
            throw GoogleApiException::fromGoogleException($e);
        } catch (\Exception $e) {
            throw new ApiException('Failed to deleteFile: ' . $e->getMessage(), $e->getCode());
        }
    }

    public function shareFile($fileId)
    {
        try {
            $permission = new Permission([
                'type' => 'anyone',
                'role' => 'reader',
            ]);

            $this->driveService->permissions->create($fileId, $permission);

            $file = $this->driveService->files->get($fileId, ['fields' => 'webViewLink']);
            return $file->getWebViewLink();
        } catch (\Google\Service\Exception $e) {
            throw GoogleApiException::fromGoogleException($e);
        } catch (\Exception $e) {
            throw new ApiException('Failed to shareFile: ' . $e->getMessage(), $e->getCode());
        }
    }

    public function downloadFile($fileId)
    {
        try {
            $fileMetadata = $this->driveService->files->get($fileId, ['fields' => 'mimeType, name']);
            $fileContent = $this->driveService->files->get($fileId, ['alt' => 'media']);

            return [
                'content' => $fileContent->getBody(),
                'mimeType' => $fileMetadata->getMimeType(),
                'fileName' => $fileMetadata->getName(),
            ];
        } catch (\Google\Service\Exception $e) {
            throw GoogleApiException::fromGoogleException($e);
        } catch (\Exception $e) {
            throw new ApiException('Failed to downloadFile: ' . $e->getMessage(), $e->getCode());
        }
    }
    public function overwriteFile($fileId, $filePath, $fileName)
    {
        try {
            // Обновляем существующий файл
            $fileMetadata = new DriveFile([
                'name' => $fileName,
            ]);

            $content = file_get_contents($filePath);

            $file = $this->driveService->files->update($fileId, $fileMetadata, [
                'data' => $content,
                'mimeType' => mime_content_type($filePath),
                'uploadType' => 'multipart',
                'fields' => 'id',
            ]);

            return $file->getId();
        } catch (\Google\Service\Exception $e) {
            throw GoogleApiException::fromGoogleException($e);
        } catch (\Exception $e) {
            throw new ApiException('Failed to overwriteFile: ' . $e->getMessage(), $e->getCode());
        }
    }
}
