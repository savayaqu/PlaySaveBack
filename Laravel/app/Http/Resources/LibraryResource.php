<?php

namespace App\Http\Resources;

use Illuminate\Http\Request;
use Illuminate\Http\Resources\Json\JsonResource;

class LibraryResource extends JsonResource
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
            'game_id' => $this->game_id,
            'last_played_at' => $this->last_played_at,
            'time_played' => $this->time_played,
            'is_favorite' => $this->is_favorite,  // Здесь создаем ключ is_favourite
            'game' => $this->whenLoaded('game', fn () => GameResource::make($this->game))
        ];
    }
}
