<?php

namespace App\Services;

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

        $this->client = new Client();
        $this->client->setClientId(env('GOOGLE_DRIVE_CLIENT_ID'));
        $this->client->setClientSecret(env('GOOGLE_DRIVE_CLIENT_SECRET'));
        $this->client->setRedirectUri(env('GOOGLE_DRIVE_REDIRECT_URI'));
        $this->client->setAccessType('offline');
        $this->client->setPrompt('consent');

        $this->setAccessToken();
    }
    private function setAccessToken()
    {
        $accessToken = Crypt::decryptString($this->userCloudService->access_token);
        $refreshToken = Crypt::decryptString($this->userCloudService->refresh_token);

        $expiresIn = max(0, $this->userCloudService->expires_at->diffInSeconds(now()));

        $this->client->setAccessToken([
            'access_token' => $accessToken,
            'refresh_token' => $refreshToken,
            'expires_in' => $expiresIn,
            'created' => now()->timestamp - ($this->userCloudService->expires_at->timestamp - $expiresIn),
        ]);

        if ($this->client->isAccessTokenExpired()) {
            if (!empty($refreshToken)) {
                $this->refreshToken();
            } else {
                throw new \Exception('Google OAuth token expired and no refresh token is available.');
            }
        }

        $this->driveService = new Drive($this->client);
    }

    private function refreshToken()
    {
        $refreshToken = Crypt::decryptString($this->userCloudService->refresh_token);

        if (empty($refreshToken)) {
            throw new \Exception('No refresh token available. User needs to reauthorize.');
        }

        $newToken = $this->client->fetchAccessTokenWithRefreshToken($refreshToken);

        // Обновляем `refresh_token`, только если Google его выдал
        if (!empty($newToken['refresh_token'])) {
            $this->userCloudService->refresh_token = Crypt::encryptString($newToken['refresh_token']);
        }

        $this->userCloudService->update([
            'access_token' => Crypt::encryptString($newToken['access_token']),
            'expires_at' => now()->addSeconds($newToken['expires_in']),
        ]);

        $this->client->setAccessToken($newToken);
    }


    public function getOrCreateFolder($folderName, $parentFolderId = null)
    {
        $query = "mimeType='application/vnd.google-apps.folder' and name='{$folderName}' and trashed=false";
        if ($parentFolderId) {
            $query .= " and '{$parentFolderId}' in parents";
        }

        $response = $this->driveService->files->listFiles([
            'q' => $query,
            'fields' => 'files(id, name)',
        ]);

        if (count($response->getFiles())) {
            return $response->getFiles()[0]->getId();
        }

        $folderMetadata = new DriveFile([
            'name' => $folderName,
            'mimeType' => 'application/vnd.google-apps.folder',
            'parents' => $parentFolderId ? [$parentFolderId] : [],
        ]);

        $folder = $this->driveService->files->create($folderMetadata, ['fields' => 'id']);
        return $folder->getId();
    }

    public function uploadFile($filePath, $fileName, $folderId)
    {
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
    }

    public function deleteFile($fileId)
    {
        $this->driveService->files->delete($fileId);
    }

    public function shareFile($fileId)
    {
        $permission = new Permission([
            'type' => 'anyone',
            'role' => 'reader',
        ]);

        $this->driveService->permissions->create($fileId, $permission);

        $file = $this->driveService->files->get($fileId, ['fields' => 'webViewLink']);
        return $file->getWebViewLink();
    }

    public function downloadFile($fileId)
    {
        $fileMetadata = $this->driveService->files->get($fileId, ['fields' => 'mimeType, name']);
        $fileContent = $this->driveService->files->get($fileId, ['alt' => 'media']);

        return [
            'content' => $fileContent->getBody(),
            'mimeType' => $fileMetadata->getMimeType(),
            'fileName' => $fileMetadata->getName(),
        ];
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
        } catch (\Exception $e) {
            throw new \Exception('Failed to overwrite file: ' . $e->getMessage());
        }
    }
}
