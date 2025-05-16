<?php

namespace App\Http\Requests\Api\Save;

use App\Http\Requests\ApiRequest;
use Illuminate\Validation\Rule;

class UploadSaveRequest extends ApiRequest
{
    public function rules(): array
    {
        return [
            'file_size' => 'required|string',
            'file_name' => 'required|string',
            'version' => 'required|string',
            'side_game_id' => 'nullable|integer|exists:libraries,side_game_id',
            'game_id' => 'nullable|integer|exists:libraries,game_id',
            'description' => 'nullable|string',
        ];
    }
}
