<?php

namespace App\Http\Resources;

use Illuminate\Http\Request;
use Illuminate\Http\Resources\Json\JsonResource;

class SaveResource extends JsonResource
{
    /**
     * Transform the resource into an array.
     *
     * @return array<string, mixed>
     */
    public function toArray(Request $request): array
    {
        return [
           'id' => $this->id,
           'file_id' => $this->file_id,
           'file_name' => $this->file_name,
           'version' => $this->version,
           'size' => $this->size,
           'description' => $this->description,
           //'user_id' => $this->user_id,
           //'game_id' => $this->game_id,
           'user_cloud_service_id' => $this->user_cloud_service_id,
            'last_sync_at' => $this->last_sync_at,
            'hash' => $this->hash,
           'created_at' => $this->created_at->format('Y-m-d H:i:s'),
           'updated_at' => $this->created_at->format('Y-m-d H:i:s'),
        ];
    }
}
