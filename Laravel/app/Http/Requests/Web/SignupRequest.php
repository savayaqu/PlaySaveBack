<?php

namespace App\Http\Requests\Web;

use Illuminate\Foundation\Http\FormRequest;

class SignupRequest extends FormRequest
{
    public function rules(): array
    {
        return [
            'email'         => 'required|string|email|max:255|unique:users',
            'nickname'      => 'required|string|max:255|unique:users',
            'password'      => 'required|string|min:8|max:255|confirmed',
            'avatar'        => 'nullable|file|mimes:jpeg,png,jpg|max:4096',
        ];
    }
}
