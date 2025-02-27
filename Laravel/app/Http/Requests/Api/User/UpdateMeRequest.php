<?php

namespace App\Http\Requests\Api\User;

use App\Http\Requests\ApiRequest;
use Illuminate\Validation\Rule;

class UpdateMeRequest extends ApiRequest
{
    public function rules(): array
    {
        return [
            'email' => [
                'string',
                'email',
                'max:255',
                Rule::unique('users')->ignore($this->id),
            ],
            'password' => 'string|min:6|max:255|confirmed',
            'password_confirmation' => 'string|min:6|max:255',
            'nickname' => [
                'string',
                'min:6',
                'max:255',
                Rule::unique('users')->ignore($this->id),
            ],
            'avatar' => 'nullable|image|mimes:jpeg,png,jpg,gif,svg',
        ];
    }
}
