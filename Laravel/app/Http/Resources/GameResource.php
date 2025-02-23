<?php

namespace App\Http\Resources;

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
          //'icon' => $this->icon,
          'icon' => "https://i.pinimg.com/736x/9c/b1/bd/9cb1bd438f42d0b8f29142812f668a81.jpg",
          'header' => $this->header,
          'description' => $this->description,
        ];
    }
}
