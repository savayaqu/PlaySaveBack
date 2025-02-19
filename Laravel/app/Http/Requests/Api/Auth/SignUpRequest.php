<?php

namespace App\Http\Requests\Api\Auth;

use App\Http\Requests\ApiRequest;

class SignUpRequest extends ApiRequest
{
    public function rules(): array
    {
        return [
            'login' => 'required|string|unique:users',
            'email' => 'nullable|string|email|max:255|unique:users',
            'password' => 'required|string|min:6|max:255|confirmed',
            'password_confirmation' => 'required|string|min:6|max:255',
        ];
    }
}
