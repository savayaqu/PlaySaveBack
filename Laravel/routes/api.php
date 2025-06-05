<?php

use App\Http\Controllers\Api\AuthController;
use App\Http\Controllers\Api\GameController;
use App\Http\Controllers\Api\GoogleDriveController;
use App\Http\Controllers\Api\LibraryController;
use App\Http\Controllers\Api\MailController;
use App\Http\Controllers\Api\SaveController;
use App\Http\Controllers\Api\SideGameController;
use App\Http\Controllers\Api\UserController;
use Illuminate\Support\Facades\Route;

Route::controller(AuthController::class)->group(function () {
    // Регистрация
    Route::post('register', 'signUp');
    // Авторизация
    Route::post('login',   'signIn');
    // Восстановление с помощью ключа
    Route::post('restore-from-key', 'restoreFromKey');
    // Выход
    Route::middleware('auth:sanctum')->get('logout', 'logout');
});
Route::controller(MailController::class)->group(function () {
    Route::prefix('mail')->group(function () {
        // Отправка кода на почту
        Route::post('send', 'sendResetCode');
        // Подтверждение кода
        Route::post('verify', 'verifyCode');
        // Восстановление через почту
        Route::post('restore', 'restoreFromMail');
    });
});
Route::middleware('auth:sanctum')->group(function () {
    Route::controller(UserController::class)->group(function () {
        Route::prefix('profile')->group(function () {
            // Получение статистики
            Route::get('statistic', 'getStatistic');
            // Получение своего профиля
            Route::get('', 'getProfile');
            // Получение облачных сервисов
            Route::get('services', 'getCloudServices');
            // Обновление своего профиля
            Route::post('', 'updateProfile');
        });
    });
    Route::controller(LibraryController::class)->group(function () {
        Route::prefix('library')->group(function () {
            // Получение своей библиотеки
            Route::get('', 'getLibrary');
            Route::prefix('game/{game}')->group(function () {
                // Добавление игры в библиотеку
                Route::post('', 'addToLibrary');
                // Переключение статуса избранного
                Route::patch('', 'toggleFavorite');
                // Удаление игры из библиотеки
                Route::delete('', 'removeFromLibrary');
                // Изменить данные игры в библиотеке
                Route::patch('update', 'updateLibraryGame');
            });
            Route::prefix('sidegame/{sideGame}')->group(function () {
                // Добавить/убрать стороннюю игру в Избранное
                Route::patch('', 'toggleSideGameFavorite');
                // Изменить данные сторонней игры в библиотеке
                Route::patch('update', 'updateSideGameLibrary');
            });
        });
    });
    Route::controller(SaveController::class)->group(function () {
        Route::prefix('saves')->group(function () {
            // Получение своих сохранений к игре
           Route::get('game/{game}/my', 'getMySavesGame');
            //  Получение своих сохранений к сторонней игре
           Route::get('sidegame/{sideGame}/my', 'getMySavesSideGame');
            // Загрузка сохранения в GoogleDrive
           Route::post('google-drive/generate-upload-url',
               [GoogleDriveController::class, 'generateUploadUrl']);
           Route::prefix('{save}')->group(function () {
               // Обновление данных сохранения
               Route::patch('', 'updateSave');
               Route::controller(GoogleDriveController::class)
                   ->group(function () {
                       // Действия с Google Drive
                   Route::prefix('google-drive')->group(function () {
                       // Подтверждение загрузки сохранения
                       Route::post('confirm-upload', 'confirmUpload');
                       // Перезапись сохранения
                       Route::post('generate-overwrite-url', 'generateOverwriteUrl');
                       // Скачивание сохранения
                       Route::get('download', 'downloadFile');
                       // Поделиться сохранением
                       Route::get('share', 'shareFile');
                       // Удаление сохранения
                       Route::delete('delete', 'deleteFile');
                   });
               });
           });
        });
    });
    Route::controller(GameController::class)->group(function () {
        Route::prefix('games')->group(function () {
            // Просмотр всех игр
            Route::get('', 'getGames');
            Route::prefix('{game}')->group(function () {
                // Просмотр игры
                Route::get('', 'getGame');
                // Просмотр пути до сохранений
                Route::get('path', 'getPath');
            });
        });
    });
    Route::controller(SideGameController::class)->group(function () {
        Route::prefix('sidegames')->group(function () {
            // Добавление сторонней игры
            Route::post('', 'addSideGame');
            Route::prefix('{sideGame}')->group(function () {
                // Просмотр сторонней игры
                Route::get('', 'getSideGame');
                // Удаление сторонней игры
                Route::delete('', 'removeSideGame');
            });
        });
    });
    Route::controller(GoogleDriveController::class)->group(function () {
        Route::prefix('google-drive')->group(function () {
            // Авторизация в GoogleDrive
            Route::get('auth-url', 'getAuthUrl');
            // Ответ от GoogleDrive
            Route::get('callback', 'callback')
                ->withoutMiddleware('auth:sanctum');
            // Отключение сервиса
            Route::delete('disconnect/{userCloudService}', 'disconnect');
        });
    });
});



