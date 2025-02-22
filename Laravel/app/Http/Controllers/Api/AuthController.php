<?php

namespace App\Http\Controllers\Api;

use App\Exceptions\ApiException;
use App\Exceptions\UnauthorizedException;
use App\Http\Controllers\Controller;
use App\Http\Requests\Api\Auth\SignInRequest;
use App\Http\Requests\Api\Auth\SignUpRequest;
use App\Http\Resources\UserResource;
use App\Models\User;
use Illuminate\Http\JsonResponse;
use Illuminate\Http\Request;
use Illuminate\Support\Facades\Auth;
use Illuminate\Support\Str;
use Random\RandomException;

class AuthController extends Controller
{
    /**
     * @throws RandomException
     */
    public function signUp(SignUpRequest $request): JsonResponse
    {
        $key = random_int(100000, 999999);
        $user = User::query()->create([
            ...$request->validated(),
            'key' => $key,
            'nickname' => $request->input('login'),
        ]);
        $token = $user->createToken(Str::random(100))->plainTextToken;

        return response()->json(['user' => UserResource::make($user), 'token' => $token, 'key' => $key], 201);
    }
    public function signIn(SignInRequest $request): JsonResponse
    {
        $identifier = $request->input('identifier'); // Может быть email или login
        $password = $request->input('password');

        // Определяем, является ли идентификатор email'ом
        $field = filter_var($identifier, FILTER_VALIDATE_EMAIL) ? 'email' : 'login';

        // Попытка аутентификации
        if (!Auth::attempt([$field => $identifier, 'password' => $password])) {
            throw new ApiException('Invalid credentials', 401);
        }

        $user = Auth::user();

        if (!$user instanceof User) {
            throw new UnauthorizedException();
        }

        // Создаем токен
        $token = $user->createToken(Str::random(100))->plainTextToken;

        // Загружаем связанные данные
        $user->load('libraries');

        return response()->json([
            'user' => UserResource::make($user),
            'token' => $token,
        ]);
    }

    public function logout(Request $request): JsonResponse
    {
        $request->user()->currentAccessToken()->delete();
        return response()->json(null, 204);
    }
    public function logoutAll(Request $request): JsonResponse
    {
        $request->user()->tokens()->delete();
        return response()->json(null, 204);
    }
}
