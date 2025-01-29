<?php

namespace App\Http\Resources;

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
        return [
          'id' => $this->id,
          'email' => $this->email,
          'nickname' => $this->nickname,
          'avatar' => $this->profileImage(),
          'is_admin' => $this->is_admin,
        ];
    }
}
