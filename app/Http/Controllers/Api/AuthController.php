<?php

namespace App\Http\Controllers\Api;

use App\Http\Controllers\Controller;
use App\Http\Requests\Api\SignUpRequest;
use App\Models\User;
use Illuminate\Support\Str;

class AuthController extends Controller
{
    public function signUp(SignUpRequest $request)
    {
        $validated = $request->validated();
        $user = User::query()->create([...$validated]);
        $token = $user->createToken(Str::random(100))->plainTextToken;
        return response()->json(['user' => $user, 'token' => $token]);
    }

}
