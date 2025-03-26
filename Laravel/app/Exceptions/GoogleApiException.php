<?php

namespace App\Exceptions;

use AllowDynamicProperties;
use Exception;

#[AllowDynamicProperties] class GoogleApiException extends ApiException
{
    public function __construct(string $message, int $code = 0, array $details = [], array $data = [], \Throwable $previous = null)
    {
        parent::__construct($message, $code, $details, $data);
        $this->previous = $previous; // сохраняем предыдущее исключение
    }

    public static function fromGoogleException(\Google\Service\Exception $e): self
    {
        $error = json_decode($e->getMessage(), true)['error'] ?? [];

        return new self(
            $error['message'] ?? 'Google API error',
            $error['code'] ?? $e->getCode(),
            $error['errors'] ?? [],
            [], // передаем пустой массив как $data
            $e   // передаем исходное исключение как previous
        );
    }
}
