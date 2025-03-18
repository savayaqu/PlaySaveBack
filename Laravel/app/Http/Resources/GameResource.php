<?php

namespace App\Http\Resources;

use App\Enums\GamePlatform;
use Illuminate\Http\Request;
use Illuminate\Http\Resources\Json\JsonResource;

class GameResource extends JsonResource
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
          'name' => $this->name,
          'platform' =>  $this->platform,
          'game_code' => $this->game_code,
          $this->mergeWhen($this->platform == GamePlatform::Steam->value, [
              'header' => "https://cdn.cloudflare.steamstatic.com/steam/apps/$this->game_code/library_hero.jpg",
              'library_img' => "https://cdn.cloudflare.steamstatic.com/steam/apps/$this->game_code/library_600x900.jpg",
              'catalog_img' => "https://shared.akamai.steamstatic.com/store_item_assets/steam/apps/$this->game_code/header.jpg"
          ]),
        ];
    }
}
