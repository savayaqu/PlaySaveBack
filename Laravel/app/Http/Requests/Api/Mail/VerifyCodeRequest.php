<?php

namespace App\Http\Requests\Api\Mail;

use App\Http\Requests\ApiRequest;

class VerifyCodeRequest extends ApiRequest
{
    public function rules(): array
    {
        return [
            'code' => 'required|string',
            'email' => 'required|string|email|exists:users,email',
        ];
    }
}
