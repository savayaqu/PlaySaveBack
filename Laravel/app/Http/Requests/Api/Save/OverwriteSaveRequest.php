<?php

namespace App\Http\Requests\Api\Save;

use App\Http\Requests\ApiRequest;
use Illuminate\Validation\Rule;

class OverwriteSaveRequest extends ApiRequest
{
    public function rules(): array
    {
        return [
            'version' => 'string',
            'description' => 'nullable|string',
            'hash' => 'string',
            'last_sync_at' => 'date',
        ];
    }
}
