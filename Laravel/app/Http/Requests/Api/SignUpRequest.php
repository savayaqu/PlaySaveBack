<?php

namespace App\Http\Requests\Api;

use App\Http\Requests\ApiRequest;

class SignUpRequest extends ApiRequest
{
    public function rules(): array
    {
        return [
            'email' => 'required|string|email|max:255|unique:users',
            'password' => 'required|string|min:6|max:255|confirmed',
            'password_confirmation' => 'required|string|min:6|max:255'
        ];
    }
}
