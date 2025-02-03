<?php

use App\Http\Controllers\Api\AuthController;
use App\Http\Controllers\Api\CloudServiceController;
use App\Http\Controllers\Api\CustomGameController;
use App\Http\Controllers\Api\CollectionController;
use App\Http\Controllers\Api\GameController;
use App\Http\Controllers\Api\PublisherController;
use App\Http\Controllers\Api\SaveController;
use App\Http\Controllers\Api\UserController;
use Illuminate\Support\Facades\Route;


// AUTH
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
        Route::get('me', 'getMe'); //Просмотр своего профиля
        Route::post('me', 'updateMe'); //Редактирование своего профиля
    });
    Route::controller(GameController::class)->group(function () {
        Route::prefix('games')->group(function () {
            Route::post('', 'addGame'); // Добавить игру
            Route::get('', 'getGames'); // Просмотр своих игр
            Route::get('{game}', 'getGame'); // Просмотр  игры
            Route::post('{game}', 'editGame'); // Редактирование  игры
        });
    });
});



