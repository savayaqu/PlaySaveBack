<?php

namespace App\Http\Resources;

use App\Enums\UserVisibility;
use App\Models\User;
use Illuminate\Http\Request;
use Illuminate\Http\Resources\Json\JsonResource;

class UserResource extends JsonResource
{
    /**
     * Transform the resource into an array.
     *
     * @return array<string, mixed>
     */
    public function toArray(Request $request): array
    {
        $user = $request->user();
        $isThis = $user?->id === $this->id;
        return [
            'id' => $this->id,
            'nickname' => $this->nickname,
            'header' => $this->getImage($this->header),
            'avatar' => $this->getImage($this->avatar),
            $this->mergeWhen($isThis, [
                'login' => $this->login,
                'email' => $this->email,
                'visibility' => $this->visibility,
                'library' => $this->whenLoaded('libraries', fn() =>
                $this->when($this->libraries->isNotEmpty(), fn() => LibraryResource::collection($this->libraries))
                ),
            ]),
            $this->mergeWhen(!$isThis, [
                'library' => $this->whenLoaded('libraries', fn() =>
                $this->when($this->libraries->isNotEmpty(), fn() => LibraryResource::collection($this->libraries))
                ),
            ])
        ];
    }
}
