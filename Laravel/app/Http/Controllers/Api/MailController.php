<?php

namespace App\Http\Controllers\Api;

use App\Exceptions\ApiException;
use App\Http\Controllers\Controller;
use App\Http\Requests\Api\Auth\RestoreFromMailRequest;
use App\Http\Requests\Api\Mail\SendResetCodeRequest;
use App\Http\Requests\Api\Mail\VerifyCodeRequest;
use App\Http\Resources\UserResource;
use App\Mail\PasswordResetCodeMail;
use App\Models\User;
use Illuminate\Http\JsonResponse;
use Illuminate\Support\Facades\Cache;
use Illuminate\Support\Facades\Mail;
use Illuminate\Support\Facades\RateLimiter;
use Illuminate\Support\Str;

class MailController extends Controller
{
    // Отправка письма с кодом
    public function sendResetCode(SendResetCodeRequest $request): JsonResponse
    {
        $email = $request->email;

        // Проверяем лимит (1 запрос в минуту)
        if (RateLimiter::tooManyAttempts('reset-code:' . $email, 1)) {
            $seconds = RateLimiter::availableIn('reset-code:' . $email);
            throw new ApiException("Retry possible in $seconds seconds", 429);
        }

        // Увеличиваем счётчик запросов (лимит: 1 запрос в 60 сек.)
        RateLimiter::hit('reset-code:' . $email, 60);

        $code = Str::random(6);

        // Обновляем код (старый автоматически перезаписывается)
        Cache::put('password_reset_' . $email, $code, now()->addMinutes(5));

        Mail::to($email)->send(new PasswordResetCodeMail($code));

        return response()->json(null, 204);
    }
    // Подтверждение кода
    public function verifyCode(VerifyCodeRequest $request): JsonResponse
    {
        $cachedCode = Cache::get('password_reset_' . $request->email);
        if (!$cachedCode || $cachedCode !== $request->code) {
            throw new ApiException("Invalid or expired code", 403);
        }
        // Генерируем одноразовый токен для смены пароля
        $resetToken = Str::random(32);
        Cache::put('password_reset_token_' . $request->email, $resetToken, now()->addMinutes(5));

        return response()->json(['reset_token' => $resetToken]);
    }
    // Восстановление через почту
    public function restoreFromMail(RestoreFromMailRequest $request): JsonResponse
    {
        // Проверяем токен
        $savedToken = Cache::get('password_reset_token_' . $request->email);

        if (!$savedToken || $savedToken !== $request->reset_token) {
            throw new ApiException("Invalid or expired token", 403);
        }
        // Обновляем пароль
        $user = User::where('email', $request->email)->firstOrFail();
        $user->update(['password' => $request->new_password]);
        if($request->logout == true)
        {
            $user->tokens()->delete();
        }
        // Очищаем кеш
        Cache::forget('password_reset_' . $request->email);
        Cache::forget('password_reset_token_' . $request->email);
        return response()->json(null, 204);
    }
}
