<?php

namespace App\Http\Requests\Api\Save;

use App\Http\Requests\ApiRequest;

class ConfirmUploadSave extends ApiRequest
{
    public function rules(): array
    {
        return [
            'file_id' => 'required|string',
            'file_hash' => 'required|string',
        ];
    }
}
