<?php

namespace App\Http\Resources;

use Illuminate\Http\Request;
use Illuminate\Http\Resources\Json\JsonResource;

class CloudServiceResource extends JsonResource
{
    public function toArray($request): array
    {
        $user = $request->user();

        // Проверяем, подключен ли сервис
        $userCloudService = $user->userCloudService()
            ->where('cloud_service_id', $this->id)
            ->first();

        $isConnected = (bool) $userCloudService;
        $expiresAt = $isConnected ? $userCloudService->expires_at : null;
        $cloudServiceId = $isConnected ? $userCloudService->id : null;

        return [
            'id' => $this->id,
            'name' => $this->name,
            'icon' => $this->icon,
            'description' => $this->description,
            'is_connected' => $isConnected,
            'user_cloud_service_id' =>$this->when($isConnected, $cloudServiceId),
            'expires_at' => $this->when($isConnected, $expiresAt), // Только если подключен
        ];
    }

}
