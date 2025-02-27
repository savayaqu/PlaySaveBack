<?php

namespace App\Http\Requests\Api\User;

use App\Http\Requests\ApiRequest;

class UpdateProfileRequest extends ApiRequest
{
    public function rules(): array
    {
        return [
            'email' => 'nullable|email|max:255|unique:users,email,' . auth()->id(),
            'current_password' => 'string',
            'new_password' => 'string|min:6|confirmed',
            'new_password_confirmation' => 'string|min:6',
            'nickname' => 'string|unique:users,nickname|max:64',
            'header' => 'sometimes|nullable|url|required_without:header_file',
            'header_file' => 'sometimes|nullable|image|mimes:jpeg,png,jpg,gif,svg|max:2048|required_without:header',
            'avatar' => 'sometimes|nullable|url|required_without:avatar_file',
            'avatar_file' => 'sometimes|nullable|image|mimes:jpeg,png,jpg,gif,svg|max:2048|required_without:avatar',
        ];
    }
}
