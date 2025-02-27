<?php

namespace App\Http\Requests\Api\Auth;

use App\Http\Requests\ApiRequest;

class SignInRequest extends ApiRequest
{
    public function rules(): array
    {
        return [
            //'email' => 'required_without:login|nullable|string|email|max:255',
            //'login' => 'required_without:email|nullable|string|max:255',
            'identifier' => 'required|string|max:255', //Поле для логина/почты
            'password' => 'required|string|min:6|max:255',
        ];
    }
}
