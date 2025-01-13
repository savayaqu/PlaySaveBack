<?php

use Illuminate\Support\Facades\Route;
use App\Http\Controllers\Web\AuthController;

Route::view('/', 'home')->name('home');
Route::controller(AuthController::class)
->group(function ($auth) {
    $auth->view('signup', 'auth.signup')->name('signupForm');
    $auth->post('signup', 'signup')     ->name('signup');
    $auth->view('login' , 'auth.login') ->name('loginForm');
    $auth->post('login' , 'login')      ->name('login');
    $auth->get ('logout', 'logout')     ->middleware('auth:web')->name('logout');
});
