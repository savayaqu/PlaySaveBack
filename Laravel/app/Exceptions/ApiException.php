<?php

namespace App\Exceptions;

use Illuminate\Http\Exceptions\HttpResponseException;

class ApiException extends HttpResponseException
{
    public function __construct(string $message = "",int $code = 500, $errors = [], $data = [])
    {
        $body = [
            'code' => $code,
            'message' => $message,
        ];
        if(count($errors))
            $body['errors'] = $errors;
        if($data && is_array($data)) {
            $body = array_merge($body, $data);
        }
        parent::__construct(response()->json($body, $code));
    }
    /**
     * Фабричный метод для создания исключения из другого исключения
     */
    public static function fromException(\Exception $e, string $prefix = ''): self
    {
        $processed = self::parseErrorMessage($e->getMessage());

        return new self(
            $prefix . ($processed['message'] ?? $e->getMessage()),
            $processed['code'] ?? $e->getCode(),
        );
    }

    /**
     * Парсит сообщение об ошибке, извлекая данные из JSON если они есть
     */
    private static function parseErrorMessage(string $message): array
    {
        $normalizedMessage = stripcslashes($message);

        if (preg_match('/^\s*{/', $normalizedMessage)) {
            $jsonError = json_decode($normalizedMessage, true);

            if (isset($jsonError['error'])) {
                return [
                    'message' => $jsonError['error']['message'] ?? $message,
                    'code' => $jsonError['error']['code'] ?? 500,
                ];
            }
        }
        return [];
    }
}
