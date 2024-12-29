<?php

namespace App\Http\Controllers\Api;

use App\Exceptions\ApiException;
use App\Http\Controllers\Controller;
use App\Http\Requests\Api\SignInRequest;
use App\Http\Requests\Api\SignUpRequest;
use App\Models\User;
use Illuminate\Http\Request;
use Illuminate\Support\Facades\Auth;
use Illuminate\Support\Str;
use Illuminate\Http\JsonResponse;

class AuthController extends Controller
{
    public function signUp(SignUpRequest $request): JsonResponse
    {
        $validated = $request->validated();
        $user = User::query()->create([...$validated]);
        $token = $user->createToken(Str::random(100))->plainTextToken;
        return response()->json(['user' => $user, 'token' => $token]);
    }
    public function signIn(SignInRequest $request): JsonResponse
    {
        if(!Auth::attempt($request->only(['email', 'password'])))
            throw new ApiException('Invalid credentials', 401);
        $user = Auth::user();
        $token = $user->createToken(Str::random(100))->plainTextToken;
        return response()->json(['user' => $user, 'token' => $token]);
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
