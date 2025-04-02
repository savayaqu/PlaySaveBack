<?php

namespace App\Http\Requests\Api\Post;

use App\Http\Requests\ApiRequest;

class CreatePostRequest extends ApiRequest
{
    public function rules(): array
    {
        return [
            'title' => 'required|string',
            'content' => 'required|string',
            'images' => 'nullable|array|max:3', // Максимум 3 файла
            'images.*' => 'image|mimes:jpeg,jpg,png|max:1024', // Каждый файл: JPEG/PNG, ≤1MB
        ];
    }
}
