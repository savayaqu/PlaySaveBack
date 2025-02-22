<?php

namespace App\Exceptions;
class ForbiddenException extends ApiException
{
    public function __construct($message = null)
    {
        $message ??= "Forbidden";
        parent::__construct($message, 403);
    }
}
