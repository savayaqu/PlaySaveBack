<?php

namespace App\Http\Requests\Api\Save;

use App\Http\Requests\ApiRequest;

class UploadSaveRequest extends ApiRequest
{
    public function rules(): array
    {
        return [
            'file_size' => 'required|integer',
            'file_name' => 'required|string',
            'version' => 'required|string',
            'side_game_id' => 'nullable|integer|exists:libraries,side_game_id|required_without:game_id|prohibits:game_id',
            'game_id' => 'nullable|integer|exists:libraries,game_id|required_without:side_game_id|prohibits:side_game_id',
            'description' => 'nullable|string',
        ];
    }
}
