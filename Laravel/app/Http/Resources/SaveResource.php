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
           'file' => $this->id,
           'file_name' => $this->file_name,
           'size' => $this->size,
           'description' => $this->description,
           'user_id' => $this->user_id,
           'game_id' => $this->game_id,
           'user_cloud_service_id' => $this->user_cloud_service_id,
        ];
    }
}
