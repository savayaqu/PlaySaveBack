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
    Route::post('restore-from-key', 'restoreFromKey'); // Восстановление с помощью ключа
    Route::middleware('auth:sanctum')->get('logout', 'logout'); // Выход
});
Route::middleware('auth:sanctum')->group(function () {
    Route::controller(UserController::class)->group(function () {
        Route::prefix('profile')->group(function () {
            Route::get('statistic', 'getStatistic'); // Получение статистики
            Route::get('', 'getProfile');            // Получение своего профиля
            Route::get('services', 'getCloudServices'); // Получение облачных сервисов
            Route::post('', 'updateProfile');        // Обновление своего профиля
        });
    });
    Route::controller(LibraryController::class)->group(function () {
        Route::prefix('library')->group(function () {
            Route::get('', 'getLibrary');                     // Получение своей библиотеки
            Route::prefix('game/{game}')->group(function () {
                Route::post('', 'addToLibrary');             // Добавление игры в библиотеку
                Route::patch('', 'toggleFavorite');          // Добавить/убрать игру в Избранное
                Route::delete('', 'removeFromLibrary');      // Удаление игры из библиотеки
                Route::patch('update', 'updateLibraryGame'); // Изменить данные игры в библиотеке
            });
            Route::prefix('sidegame/{sideGame}')->group(function () {
                Route::patch('', 'toggleSideGameFavorite');          // Добавить/убрать стороннюю игру в Избранное
                Route::patch('update', 'updateSideGameLibrary'); // Изменить данные сторонней игры в библиотеке
            });
        });
    });
    Route::controller(SaveController::class)->group(function () {
        Route::prefix('saves')->group(function () {
            // Просмотр своих сохранений
           Route::get('game/{game}/my', 'getMySavesGame'); // Получение своих сохранений к игре
           Route::get('sidegame/{sideGame}/my', 'getMySavesSideGame'); //  Получение своих сохранений к сторонней игре
           Route::post('google-drive/generate-upload-url', [GoogleDriveController::class, 'generateUploadUrl']); // Загрузка сохранения в GoogleDrive
           Route::prefix('{save}')->group(function () {
               Route::patch('', 'updateSave'); // Обновление данных сохранения
               Route::controller(GoogleDriveController::class)->group(function () {
                   Route::prefix('google-drive')->group(function () { // Действия с Google Drive
                       Route::post('confirm-upload', 'confirmUpload'); // Подтверждение загрузки сохранения
                       // Управление файлами
                       Route::post('generate-overwrite-url', 'generateOverwriteUrl'); // Перезапись сохранения
                       Route::get('download', 'downloadFile'); // Скачивание сохранения
                       Route::get('share', 'shareFile'); // Поделиться сохранением
                       Route::delete('delete', 'deleteFile'); // Удаление сохранения
                   });
               });
           });
        });
    });
    Route::controller(GameController::class)->group(function () {
        Route::prefix('games')->group(function () {
            Route::get('', 'getGames');      // Просмотр всех игр
            Route::prefix('{game}')->group(function () {
                Route::get('', 'getGame'); // Просмотр игры
                Route::get('path', 'getPath'); // Просмотр пути до сохранений
            });
        });
    });
    Route::controller(SideGameController::class)->group(function () {
        Route::prefix('sidegames')->group(function () {
            Route::post('', 'addSideGame'); // Добавление сторонней игры
            Route::prefix('{sideGame}')->group(function () {
                Route::get('', 'getSideGame'); // Просмотр сторонней игры
                Route::delete('', 'removeSideGame'); // Удаление сторонней игры
            });
        });
    });
    Route::controller(GoogleDriveController::class)->group(function () {
        Route::prefix('google-drive')->group(function () {
            Route::get('auth-url', 'getAuthUrl'); // Авторизация в GoogleDrive
            Route::get('callback', 'callback')
                ->withoutMiddleware('auth:sanctum'); // Ответ от GoogleDrive
            Route::delete('disconnect/{userCloudService}', 'disconnect'); // Отключение
        });
    });
});



