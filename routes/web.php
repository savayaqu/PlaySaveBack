<?php

use App\Http\Controllers\GoogleDriveController;
use App\Http\Controllers\Web\AuthController;
use App\Http\Controllers\Web\GameController;
use Illuminate\Support\Facades\Route;

Route::view('/', 'home')->name('home');
Route::controller(AuthController::class)
->group(function ($auth) {
    $auth->view('signup', 'auth.signup')->name('signupForm');
    $auth->post('signup', 'signup')     ->name('signup');
    $auth->view('login' , 'auth.login') ->name('loginForm');
    $auth->post('login' , 'login')      ->name('login');
    $auth->get ('logout', 'logout')     ->middleware('auth:web')->name('logout');
    $auth->get('profile', 'profile')   ->name('profile');
});
Route::controller(GoogleDriveController::class)
    ->middleware('auth:web')
    ->group(function ($googleDrive) {
    $googleDrive->get('google-drive/auth-url', 'getAuthUrl')->name('google-drive-auth-url');
    $googleDrive->get('google-drive/callback', 'callback')->name('google-drive-callback');
//upload под вопросом
    $googleDrive->get('google-drive/upload', 'uploadFile');
//
});
Route::get('/games', [GameController::class, 'index'])->name('games.index');
Route::get('/games/{steam_id}', [GameController::class, 'show'])->name('games.show');
