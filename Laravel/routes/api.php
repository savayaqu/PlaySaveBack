<?php

use App\Http\Controllers\Api\AuthController;
use App\Http\Controllers\Api\GameController;
use App\Http\Controllers\Api\SaveController;
use App\Http\Controllers\Api\UserController;
use App\Http\Controllers\Api\LibraryController;
use App\Http\Controllers\Api\SideGameController;
use App\Http\Controllers\GoogleDriveController;
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
            Route::get('', 'getProfile');            // Просмотр своего профиля
            Route::get('services', 'getCloudServices'); //Просмотр своих подключенных облачных сервисов
            Route::post('', 'updateProfile');        // Обновление профиля
            Route::get('{user}', 'getOtherProfile'); // Просмотр чужого профиля
        });
    });
    Route::controller(LibraryController::class)->group(function () {
        Route::prefix('library')->group(function () {
            Route::get('', 'getLibrary');                     // Получить свою библиотеку
            Route::prefix('game/{game}')->group(function () {
                Route::post('', 'addToLibrary');             // Добавить игру в библиотеку
                Route::patch('', 'toggleFavorite');          // Добавить/убрать игру в Избранное
                Route::delete('', 'removeFromLibrary');      // Удалить игру из библиотеки
                Route::patch('update', 'updateLibraryGame'); // Изменить данные игры в библиотеке
            });
            Route::prefix('sidegame/{sideGame}')->group(function () {
                Route::patch('', 'toggleSideGameFavorite');          // Добавить/убрать стороннюю игру в Избранное
                Route::delete('', 'removeSideGameFromLibrary');      // Удалить стороннюю игру из библиотеки
                Route::patch('update', 'updateSideGameLibrary'); // Изменить данные сторонней игры в библиотеке
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
        Route::prefix('saves')->group(function () {
           Route::get('game/{game}/my', 'getMySavesGame'); // Просмотр моих сохранений к игре
           Route::get('sidegame/{sideGame}/my', 'getMySavesSideGame'); // Просмотр моих сохранений к сторонней игре
        });
    });
    Route::controller(GameController::class)->group(function () {
        Route::prefix('games')->group(function () {
            Route::get('', 'getGames');      // Просмотр всех игр
            Route::get('{game}', 'getGame'); // Просмотр игры
        });
    });
    Route::controller(SideGameController::class)->group(function () {
       Route::prefix('sidegames')->group(function () {
           Route::get('', 'getSideGames');      // Просмотр всех игр
           Route::get('{sideGame}', 'getSideGame'); // Просмотр игры
           Route::post('', 'addSideGame'); // Добавить стороннюю игру
           Route::patch('', 'updateSideGame'); // Редактировать стороннюю игру
           Route::delete('', 'removeSideGame'); // Удалить стороннюю игру
       });
    });
    Route::controller(GoogleDriveController::class)->group(function () {
        Route::prefix('google-drive')->group(function () {
            Route::get('auth-url', 'getAuthUrl'); // Генерация ссылка на доступ к гуглу
            Route::get('callback', 'callback')->withoutMiddleware('auth:sanctum'); // Ответ от гугла
            Route::post('upload', 'uploadFile'); // Загрузить сейв для игры
            Route::post('overwrite/{fileId}', 'overwriteFile'); // Перезаписать сейв
            Route::get('download/{fileId}', 'downloadFile'); // Скачать сейв
            Route::get('share/{fileId}', 'shareFile'); // Предоставить доступ
            Route::delete('delete/{fileId}', 'deleteFile'); // Удалить сейв
        });
    });
});



