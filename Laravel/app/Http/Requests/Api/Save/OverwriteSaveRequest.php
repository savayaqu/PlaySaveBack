<?php

namespace App\Http\Requests\Api\Save;

use App\Http\Requests\ApiRequest;

class OverwriteSaveRequest extends ApiRequest
{
    public function rules(): array
    {
        return [
            'file_name' => 'required|string',
            'file_size' => 'required|integer'
        ];
    }
}
