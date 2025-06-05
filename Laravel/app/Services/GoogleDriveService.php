<?php

namespace App\Services;

use App\Exceptions\ApiException;
use Google\Client;
use Google\Service\Drive;
use Google\Service\Drive\DriveFile;
use Google\Service\Drive\Permission;
use Illuminate\Support\Facades\Crypt;

class GoogleDriveService
{
    // Google API клиент
    private $client;
    // Сервис для работы с Google Drive
    private $driveService;
    // Данные пользовательского облачного сервиса
    private $userCloudService;
    // Кэш для хранения ID папок
    private array $folderCache;

    // Конструктор
    public function __construct($userCloudService)
    {
        $this->userCloudService = $userCloudService;
        $this->initializeClient();
    }

    // Инициализация клиента
    private function initializeClient(): void
    {
        // Настройка клиента Google API с использованием credentials из .env
        $this->client = new Client();
        $this->client->setClientId(env('GOOGLE_DRIVE_CLIENT_ID'));
        $this->client->setClientSecret(env('GOOGLE_DRIVE_CLIENT_SECRET'));
        $this->client->setAccessType('offline');
        $this->setAccessToken();
        $this->driveService = new Drive($this->client);
    }

    // Установка токена доступа
    private function setAccessToken(): void
    {
        // Расшифровка токенов
        $accessToken = Crypt::decryptString($this->userCloudService->access_token);
        $refreshToken = Crypt::decryptString($this->userCloudService->refresh_token);

        // Установка токенов
        $this->client->setAccessToken([
            'access_token' => $accessToken,
            'refresh_token' => $refreshToken,
            'expires_in' => $this->userCloudService->expires_at->diffInSeconds(now()),
        ]);

        // Обновление токена при необходимости
        if ($this->client->isAccessTokenExpired()) {
            $this->refreshToken();
        }
    }

    // Обновление токена
    private function refreshToken(): void
    {
        // Обновление токена доступа с помощью refresh token
        $refreshToken = Crypt::decryptString($this->userCloudService->refresh_token);
        $newToken = $this->client->fetchAccessTokenWithRefreshToken($refreshToken);

        // Сохранение новых токенов
        $this->userCloudService->update([
            'access_token' => Crypt::encryptString($newToken['access_token']),
            'expires_at' => now()->addSeconds($newToken['expires_in']),
            'refresh_token' => isset($newToken['refresh_token'])
                ? Crypt::encryptString($newToken['refresh_token'])
                : $this->userCloudService->refresh_token,
        ]);

        $this->client->setAccessToken($newToken);
    }

    // Генерация URL для загрузки файла с возможностью возобновления
    public function generateResumableUploadUrl(string $fileName, string $folderPath): string
    {
        try {
            $folderId = $this->createFolderStructure($folderPath);

            $fileMetadata = new DriveFile([
                'name' => $fileName,
                'parents' => [$folderId]
            ]);

            $httpClient = $this->driveService->getClient()->authorize();

            $uri = 'https://www.googleapis.com/upload/drive/v3/files?' . http_build_query([
                    'uploadType' => 'resumable',
                    'fields' => 'id',
                    'supportsAllDrives' => 'true'
                ]);
            $request = new Request(
                'POST',
                $uri,
                [
                    'Content-Type' => 'application/json',
                    'X-Upload-Content-Type' => 'application/octet-stream'
                ],
                json_encode($fileMetadata)
            );
            $response = $httpClient->send($request);

            $location = $response->getHeaderLine('Location');
            if (empty($location)) {
                throw new ApiException('Google Drive did not return upload URL');
            }

            return $location;
        } catch (\Exception $e) {
            throw ApiException::fromException(
                $e,
                'Failed to generate upload URL: '
            );
        }
    }

    // Генерация URL для перезаписи существующего файла
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
            $request = new Request(
                'PATCH',
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
            throw ApiException::fromException(
                $e,
                'Failed to generate overwrite URL: '
            );
        }

    }

    // Удаление файла по ID
    public function deleteFile($fileId)
    {
        try {
            $this->driveService->files->delete($fileId);
        } catch (\Exception $e) {
            throw ApiException::fromException(
                $e,
                'Failed to delete file: '
            );
        }
    }

    // Настройка доступа к файлу (публичный доступ для чтения)
    public function shareFile($fileId)
    {
        try {
            $permission = new Permission([
                'type' => 'anyone',
                'role' => 'reader',
            ]);

            $this->driveService->permissions->create($fileId, $permission);

            // Возвращает публичную ссылку на файл
            $file = $this->driveService->files->get($fileId, ['fields' => 'webViewLink']);

            return $file->getWebViewLink();
        } catch (\Exception $e) {
            throw ApiException::fromException(
                $e,
                'Failed to share file: '
            );
        }
    }

    // Скачивание файла
    public function downloadFile($fileId)
    {
        try {
            $fileMetadata = $this->driveService->files->get($fileId, ['fields' => 'mimeType, name']);
            $fileContent = $this->driveService->files->get($fileId, ['alt' => 'media']);

            // Возвращает содержимое файла, MIME-тип и имя файла
            return [
                'content' => $fileContent->getBody(),
                'mimeType' => $fileMetadata->getMimeType(),
                'fileName' => $fileMetadata->getName(),
            ];
        } catch (\Exception $e) {
            throw ApiException::fromException(
                $e,
                'Failed to generate download file: '
            );
        }
    }

    // Получение ID родительской папки для файла
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
            throw ApiException::fromException(
                $e,
                'Failed to get parent folder: '
            );
        }
    }

    // Переименование папки
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
            throw ApiException::fromException(
                $e,
                'Failed to rename folder: '
            );
        }
    }

    // Получение и переименование родительской папки файла
    public function getAndRenameParentFolder(string $fileId, string $newFolderName): string
    {
        $parentId = $this->getFileParentFolderId($fileId);

        if ($parentId === null) {
            throw new ApiException('The file is not in any folder (it is in the root)');
        }

        $this->renameFolder($parentId, $newFolderName);

        return $parentId;
    }

    // Создание структуры папок по заданному пути
    private function createFolderStructure(string $path): string
    {
        $cacheKey = 'gdrive_folder_' . md5($path);
        return cache()->rememberForever($cacheKey, function () use ($path) {
            $parts = explode('/', $path);
            $parentId = 'root';

            foreach ($parts as $folderName) {
                $parentId = $this->getOrCreateFolder($folderName, $parentId);
            }

            return $parentId;
        });
    }

    // Получение ID существующей папки или создание новой
    public function getOrCreateFolder(string $folderName, string $parentId = 'root'): string
    {
        $cacheKey = $parentId . ':' . $folderName;
        if (isset($this->folderCache[$cacheKey])) {
            return $this->folderCache[$cacheKey];
        }

        $escapedName = addcslashes($folderName, "'\\");
        $query = sprintf(
            "mimeType='application/vnd.google-apps.folder' and name='%s' and '%s' in parents and trashed=false",
            $escapedName,
            $parentId
        );

        $response = $this->driveService->files->listFiles([
            'q' => $query,
            'fields' => 'files(id)',
            'pageSize' => 1,
            'supportsAllDrives' => true,
        ]);

        if (count($response->getFiles()) > 0) {
            $folderId = $response->getFiles()[0]->getId();
            $this->folderCache[$cacheKey] = $folderId;
            return $folderId;
        }

        $folderMetadata = new DriveFile([
            'name' => $folderName,
            'mimeType' => 'application/vnd.google-apps.folder',
            'parents' => [$parentId],
        ]);

        $folder = $this->driveService->files->create($folderMetadata, [
            'fields' => 'id',
            'supportsAllDrives' => true,
        ]);

        $folderId = $folder->getId();
        $this->folderCache[$cacheKey] = $folderId;

        return $folderId;
    }
}
