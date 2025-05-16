<?php

namespace App\Http\Requests\Api\Save;

use Illuminate\Foundation\Http\FormRequest;

class ConfirmUploadSave extends FormRequest
{
    public function rules(): array
    {
        return [
            'file_id' => 'required|string',
            'hash' => 'required|string',
        ];
    }
}
