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
            'side_game_id' => 'nullable|integer|exists:libraries,side_game_id',
            'game_id' => 'nullable|integer|exists:libraries,game_id',
            'description' => 'nullable|string',
        ];
    }
}
