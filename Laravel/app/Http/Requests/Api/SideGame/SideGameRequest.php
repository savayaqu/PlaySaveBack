<?php

namespace App\Http\Requests\Api\SideGame;

use App\Http\Requests\ApiRequest;

class SideGameRequest extends ApiRequest
{
    public function rules(): array
    {
        return [
            'name' => 'required|string|max:255'
        ];
    }
}
