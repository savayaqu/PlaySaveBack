<?php

namespace App\Http\Requests\Api;

use App\Http\Requests\ApiRequest;

class SignInRequest extends ApiRequest
{
    public function rules(): array
    {
        return [
            'email' => 'required|string|email|max:255',
            'password' => 'required|string|min:6|max:255',
        ];
    }
}
