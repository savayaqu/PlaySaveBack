<?php

namespace App\Http\Controllers\Api;

use App\Exceptions\ApiException;
use App\Http\Controllers\Controller;
use App\Http\Requests\Api\Auth\SignInRequest;
use App\Http\Requests\Api\Auth\SignUpRequest;
use App\Http\Resources\UserResource;
use App\Models\User;
use Illuminate\Http\JsonResponse;
use Illuminate\Http\Request;
use Illuminate\Support\Facades\Auth;
use Illuminate\Support\Str;

class AuthController extends Controller
{
    public function signUp(SignUpRequest $request): JsonResponse
    {
        $user = User::query()->create([
            ...$request->validated(),
        ]);
        $token = $user->createToken(Str::random(100))->plainTextToken;
        return response()->json(['user' => UserResource::make($user), 'token' => $token], 201);
    }
    public function signIn(SignInRequest $request): JsonResponse
    {
        if(!Auth::attempt($request->only(['email', 'login', 'password'])))
            throw new ApiException('Invalid credentials', 401);
        $user = Auth::user();
        $token = $user->createToken(Str::random(100))->plainTextToken;
        return response()->json(['user' => UserResource::make($user), 'token' => $token]);
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
