<?php

namespace App\Http\Controllers\Web;

use App\Http\Controllers\Controller;
use App\Http\Requests\Web\SignupRequest;
use App\Models\User;
use Illuminate\Http\Request;
use Illuminate\Support\Facades\Auth;

class AuthController extends Controller
{
    public function signup(SignupRequest $request)
    {
        if ($request->hasFile('avatar'))
            $path = $request
                ->file('avatar')
                ->store('avatars', 'public');

        $user = User::create([
            ...$request->validated(),
            'avatar' => $path ?? null,
        ]);

        Auth::login($user);
        $request->session()->regenerate();
        return redirect()->route('home');
    }

    public function login(Request $request)
    {
        if (!Auth::attempt($request->only('email', 'password')))
            return back()->withErrors(['error' => 'Неверный логин и/или пароль']);

        $request->session()->regenerate();
        return redirect()->route('home');
    }

    public function logout()
    {
        Auth::logout();
        return redirect()->route('home');
    }
}
