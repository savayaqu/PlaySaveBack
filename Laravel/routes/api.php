<?php

use App\Http\Controllers\Api\AuthController;
use App\Http\Controllers\Api\GameController;
use App\Http\Controllers\Api\SaveController;
use App\Http\Controllers\Api\UserController;
use App\Http\Controllers\Api\LibraryController;
use Illuminate\Support\Facades\Route;

Route::controller(AuthController::class)->group(function () {
    Route::post('register', 'signUp'); // Регистрация
    Route::post('login',   'signIn');  // Авторизация
    Route::middleware('auth:sanctum')->group(function () {
        Route::get('logout',    'logout');    // Выход с одного устройства
        Route::get('logoutAll', 'logoutAll'); // Выход со всех устройств
    });
});
Route::middleware('auth:sanctum')->group(function () {
    Route::controller(UserController::class)->group(function () {
        Route::prefix('profile')->group(function () {
            Route::get('', 'getProfile');            //Просмотр своего профиля
            Route::post('', 'updateProfile');        //Обновление профиля
            Route::get('{user}', 'getOtherProfile'); // Просмотр чужого профиля
        });
    });
    Route::controller(LibraryController::class)->group(function () {
       Route::prefix('library')->group(function () {
          Route::get('', 'getLibrary');                     //Получить свою библиотеку
           Route::prefix('game/{game}')->group(function () {
               Route::post('', 'addToLibrary');             // Добавить игру в библиотеку
               Route::patch('', 'toggleFavorite');          // Добавить/убрать игру в Избранное
               Route::delete('', 'removeFromLibrary');      // Удалить игру из библиотеки
               Route::patch('update', 'updateLibraryGame'); // Изменить данные игры в библиотеки
           });
       });
    });
    //TODO: сделать этот контроллер
    Route::controller(SaveController::class)->group(function () {
        //Просмотр всех сохранений
        //Просмотр всех сохранений к игре
        //Просмотр сохранений в виде поста
        //Добавление сохранения
        //Удаление сохранения
    });
    Route::controller(GameController::class)->group(function () {
        Route::prefix('games')->group(function () {
            Route::get('', 'getGames');      // Просмотр всех игр
            Route::get('{game}', 'getGame'); // Просмотр игры
        });
    });
    Route::controller(\App\Http\Controllers\GoogleDriveController::class)->group(function () {
        Route::prefix('google-drive')->group(function () {
            Route::get('auth-url', 'getAuthUrl');
            Route::get('callback', 'callback')->withoutMiddleware('auth:sanctum');
            Route::post('upload', 'uploadFile');
        });
    });
});



