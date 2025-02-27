<?php

namespace App\Http\Requests\Api\Game;

use App\Http\Requests\ApiRequest;
use Illuminate\Validation\Rule;

class EditGameRequest extends ApiRequest
{
    public function rules(): array
    {
        return [
            'path_to_exe' => 'nullable|string',
            'path_to_icon' => 'nullable|string',
            'hidden' =>'boolean',
            'last_played_at' => 'nullable|date',
            'name' => [
                'string',
                Rule::unique('games')->where(function ($query) {
                    return $query->where('user_id', $this->user()->id);
                })->ignore($this->route('game')->id),
            ],
        ];
    }
}
