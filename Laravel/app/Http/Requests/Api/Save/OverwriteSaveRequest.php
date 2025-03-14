<?php

namespace App\Http\Requests\Api\Save;

use App\Http\Requests\ApiRequest;
use Illuminate\Validation\Rule;

class OverwriteSaveRequest extends ApiRequest
{
    public function rules(): array
    {
        return [
            'file' => 'required|file',
            'description' => 'nullable|string',
        ];
    }
}
