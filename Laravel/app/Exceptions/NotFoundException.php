<?php

namespace App\Exceptions;
class NotFoundException extends ApiException
{
    public function __construct($message = null)
    {
        $message ??= "Not Found";
        parent::__construct($message, 404);
    }
}
