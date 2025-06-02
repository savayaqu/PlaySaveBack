<?php

namespace App\Http\Requests\Api\Mail;

use App\Http\Requests\ApiRequest;

class SendResetCodeRequest extends ApiRequest
{
    public function rules(): array
    {
        return [
            'email' => 'required|email|exists:users,email',
        ];
    }
}
