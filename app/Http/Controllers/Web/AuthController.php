<?php

namespace App\Http\Controllers\Web;

use App\Http\Controllers\Controller;
use App\Http\Requests\Web\SignupRequest;
use App\Models\User;
use App\Models\UserCloudService;
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
            'avatar' => User::profileImage(),
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
        return redirect()->route('profile');
    }

    public function logout()
    {
        Auth::logout();
        return redirect()->route('home');
    }
    public function profile()
    {
        if(Auth::check()) {
            $user = Auth::user();
            $cloudServices = UserCloudService::query()->with(['cloudService'])->where('user_id', $user->id)->get();
            return view('profile', compact('user', 'cloudServices'));
        }
        return redirect()->route('login')->with('error', 'Вы должны войти в систему');
    }
}
