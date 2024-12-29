<?php

namespace App\Exceptions;

use Illuminate\Http\Exceptions\HttpResponseException;

class ApiException extends HttpResponseException
{
    public function __construct(string $message = "",int $code = 0, $errors = [])
    {
        $response = [
            'message' => $message,
            'code' => $code,
        ];
        if(!empty($errors)){
            $response['errors'] = $errors;
        }
        parent::__construct(response()->json($response)->setStatusCode($code));
    }
}
