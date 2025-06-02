<?php

namespace App\Http\Requests\Api\Auth;

use App\Http\Requests\ApiRequest;

class RestoreFromMailRequest extends ApiRequest
{
    public function rules(): array
    {
        return [
            'reset_token' => 'required|string',
            'email' => 'required|string|email|exists:users,email',
            'new_password' => 'required|string|min:6|confirmed',
            'new_password_confirmation' => 'required|string|min:6',
            'logout' => 'boolean',
        ];
    }
}
