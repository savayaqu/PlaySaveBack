<?php

namespace App\Http\Requests\Api\Save;

use App\Http\Requests\ApiRequest;
use Illuminate\Validation\Rule;

class UploadSaveRequest extends ApiRequest
{
    public function rules(): array
    {
        return [
            'file' => 'required|file',
            'version' => 'required|string',
            'custom_game_id' => 'nullable|string',
            'game_id' => 'nullable|string|exists:libraries,game_id',
            'description' => 'nullable|string',
        ];
    }
}
