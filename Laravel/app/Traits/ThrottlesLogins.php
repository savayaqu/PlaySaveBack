<?php

namespace App\Traits;

use App\Exceptions\ApiException;
use Illuminate\Http\Request;
use Illuminate\Support\Facades\RateLimiter;
use Illuminate\Support\Str;
use Illuminate\Validation\ValidationException;

trait ThrottlesLogins
{
    /**
     * Проверяет, не превышено ли количество попыток входа.
     */
    protected function ensureIsNotRateLimited(Request $request, int $maxAttempts = 5): void
    {
        $throttleKey = $this->throttleKey($request);

        if (!RateLimiter::tooManyAttempts($throttleKey, $maxAttempts)) {
            return;
        }

        $seconds = RateLimiter::availableIn($throttleKey);

        throw new ApiException(trans('auth.throttle', [
        'seconds' => $seconds,
        'minutes' => ceil($seconds / 60)]),429);
    }

    /**
     * Увеличивает счетчик неудачных попыток.
     */
    protected function incrementLoginAttempts(Request $request): void
    {
        RateLimiter::hit($this->throttleKey($request));
    }

    /**
     * Сбрасывает счетчик попыток после успешного входа.
     */
    protected function clearLoginAttempts(Request $request): void
    {
        RateLimiter::clear($this->throttleKey($request));
    }

    /**
     * Генерирует ключ для ограничения попыток (IP + email/login).
     */
    protected function throttleKey(Request $request): string
    {
        return Str::transliterate(Str::lower($request->input('identifier')) . '|' . $request->ip());
    }

    /**
     * Добавляет задержку для замедления брутфорса (опционально).
     */
    protected function applyBruteForceDelay(int $seconds = 2): void
    {
        sleep($seconds);
    }
}
