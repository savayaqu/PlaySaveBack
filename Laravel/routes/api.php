<?php

use App\Http\Controllers\Api\AuthController;
use App\Http\Controllers\Api\GameController;
use App\Http\Controllers\Api\UserController;
use Illuminate\Support\Facades\Route;

Route::controller(AuthController::class)->group(function () {
    Route::post('register', 'signUp'); // Регистрация
    Route::post('login',   'signIn'); // Авторизация
    Route::middleware('auth:sanctum')->group(function () {
        Route::get('logout',    'logout'); // Выход с одного устройства
        Route::get('logoutAll', 'logoutAll'); // Выход со всех устройств
    });
});
Route::middleware('auth:sanctum')->group(function () {
    Route::controller(UserController::class)->group(function () {
        Route::prefix('profile')->group(function () {
            Route::get('', 'getProfile'); //Просмотр своего профиля
            Route::post('', 'updateProfile'); //Обновление профиля
            Route::get('{user}', 'getOtherProfile'); // Просмотр чужого профиля
        });
    });


    Route::controller(GameController::class)->group(function () {
        Route::prefix('games')->group(function () {
            Route::get('', 'getGames'); // Просмотр всех игр
            Route::get('{game}', 'getGame'); // Просмотр игры
        });
    });
});



