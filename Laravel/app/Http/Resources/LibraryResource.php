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
            'game_id' => $this->when(!$this->relationLoaded('game'), $this->game_id),
            'side_game_id' => $this->when(!$this->relationLoaded('sideGame'), $this->side_game_id),
            'last_played_at' => $this->last_played_at,
            'time_played' => $this->time_played,
            'is_favorite' => $this->is_favorite,
            'game' => $this->when(
                $this->relationLoaded('game') && !is_null($this->game),
                fn () => GameResource::make($this->game)
            ),
            'sideGame' => $this->when(
                $this->relationLoaded('sideGame') && !is_null($this->sideGame),
                fn () => SideGameResource::make($this->sideGame)
            ),
        ];
    }
}
