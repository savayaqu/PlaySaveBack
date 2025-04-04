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
    public function generateResumableOverwriteUrl(string $fileId, string $fileName): string
    {
        try {
            $client = $this->driveService->getClient();
            $httpClient = $client->authorize();

            // 1. Создаем метаданные файла
            $fileMetadata = new DriveFile([
                'name' => $fileName
            ]);

            // 2. Формируем URL с параметрами
            $uri = 'https://www.googleapis.com/upload/drive/v3/files/' . urlencode($fileId) . '?' . http_build_query([
                    'uploadType' => 'resumable',
                    'supportsAllDrives' => 'true'
                ]);

            // 3. Создаем PSR-7 запрос (PATCH для обновления)
            $request = new \GuzzleHttp\Psr7\Request(
                'PATCH', // Важно использовать PATCH вместо POST
                $uri,
                [
                    'Content-Type' => 'application/json',
                    'X-Upload-Content-Type' => 'application/octet-stream',
                    'Authorization' => 'Bearer ' . $client->getAccessToken()['access_token']
                ],
                json_encode($fileMetadata)
            );

            // 4. Отправляем запрос
            $response = $httpClient->send($request);

            // 5. Проверяем ответ
            if ($response->getStatusCode() != 200) {
                throw new ApiException('Invalid status code: ' . $response->getStatusCode());
            }

            $location = $response->getHeaderLine('Location');
            if (empty($location)) {
                throw new ApiException('Location header is missing');
            }

            return $location;
        } catch (\Exception $e) {
            throw new ApiException('Failed to generate overwrite URL: ' . $e->getMessage());
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
    /**
     * Получает ID родительской папки для указанного файла
     *
     * @param string $fileId ID файла в Google Drive
     * @return string|null ID родительской папки или null, если файл в корне
     * @throws ApiException
     */
    public function getFileParentFolderId(string $fileId): ?string
    {
        try {
            $file = $this->driveService->files->get($fileId, [
                'fields' => 'parents',
                'supportsAllDrives' => true
            ]);

            if (empty($file->getParents())) {
                return null;
            }

            // Возвращаем первую родительскую папку (файл может быть в нескольких папках)
            return $file->getParents()[0];
        } catch (\Exception $e) {
            throw new ApiException('Failed to get parent folder: ' . $e->getMessage());
        }
    }

    /**
     * Переименовывает папку в Google Drive
     *
     * @param string $folderId ID папки
     * @param string $newName Новое название папки
     * @return DriveFile Обновленная папка
     * @throws ApiException
     */
    public function renameFolder(string $folderId, string $newName): DriveFile
    {
        try {
            $folderMetadata = new DriveFile([
                'name' => $newName
            ]);

            return $this->driveService->files->update($folderId, $folderMetadata, [
                'fields' => 'id,name',
                'supportsAllDrives' => true
            ]);
        } catch (\Exception $e) {
            throw new ApiException('Failed to rename folder: ' . $e->getMessage());
        }
    }

    /**
     * Получает родительскую папку файла и переименовывает её
     *
     * @param string $fileId ID файла
     * @param string $newFolderName Новое название папки
     * @return string ID переименованной папки
     * @throws ApiException Если файл не имеет родительской папки или произошла ошибка
     */
    public function getAndRenameParentFolder(string $fileId, string $newFolderName): string
    {
        $parentId = $this->getFileParentFolderId($fileId);

        if ($parentId === null) {
            throw new ApiException('The file is not in any folder (it is in the root)');
        }

        $this->renameFolder($parentId, $newFolderName);

        return $parentId;
    }
}
