<?php

namespace App\Exceptions;
class ConflictException extends ApiException
{
    public function __construct($message = null)
    {
        $message ??= "Conflict";
        parent::__construct($message, 409);
    }
}
