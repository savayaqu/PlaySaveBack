<?php

namespace App\Http\Requests\Api\Library;

use App\Http\Requests\ApiRequest;
use Illuminate\Validation\Rule;

class EditLibraryGameRequest extends ApiRequest
{
    public function rules(): array
    {
        return [
            'last_played_at' => 'nullable|date|date_format:Y-m-d H:i:s',
            'time_played' => 'nullable|integer',
        ];
    }
}
