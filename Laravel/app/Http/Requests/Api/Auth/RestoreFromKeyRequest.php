<?php

namespace App\Http\Requests\Api\Auth;

use App\Http\Requests\ApiRequest;

class RestoreFromKeyRequest extends ApiRequest
{
    public function rules(): array
    {
        return [
            'login' => 'required|string|exists:users,login',
            'key' => 'required|string',
            'new_password' => 'required|string|min:6|confirmed',
            'new_password_confirmation' => 'required|string|min:6',
            'logout' => 'boolean',
        ];
    }
}
