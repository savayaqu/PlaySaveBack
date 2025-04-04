<?php

use App\Http\Controllers\Api\AuthController;
use App\Http\Controllers\Api\GameController;
use App\Http\Controllers\Api\SaveController;
use App\Http\Controllers\Api\UserController;
use App\Http\Controllers\Api\LibraryController;
use App\Http\Controllers\Api\SideGameController;
use App\Http\Controllers\GoogleDriveController;
use App\Http\Controllers\Api\SavePostController;
use Illuminate\Support\Facades\Route;

Route::controller(AuthController::class)->group(function () {
    Route::post('register', 'signUp'); // Регистрация
    Route::post('login',   'signIn');  // Авторизация
    Route::post('restore-from-key', 'restoreFromKey'); // Восстановление с помощью ключа
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
    Route::controller(SavePostController::class)->group(function () {
        Route::prefix('posts')->group(function () {

            Route::prefix('create/{save}')->group(function () {
                Route::post('', 'createPost'); //Создать пост к сохранению
            });
            //TODO: do this
            Route::get('', 'getPosts'); // Просмотр всех постов
            Route::get('{savePost}', 'showPost'); // Просмотр конкретного поста
            Route::get('game/{game}', 'showPostToGame'); // Просмотр постов к игре
        });
    });
    Route::controller(SaveController::class)->group(function () {
        Route::prefix('saves')->group(function () {
            // Просмотр своих сохранений
           Route::get('game/{game}/my', 'getMySavesGame');
           Route::get('sidegame/{sideGame}/my', 'getMySavesSideGame');

           Route::prefix('{save}')->group(function () {
               Route::patch('', 'updateSave');
               Route::controller(GoogleDriveController::class)->group(function () {
                   Route::prefix('google-drive')->group(function () { // Действия с Google Drive
                       // Управление файлами
                       Route::post('generate-overwrite-url', 'generateOverwriteUrl');
                       Route::get('download', 'downloadFile');
                       Route::get('share', 'shareFile');
                       Route::delete('delete', 'deleteFile');
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
                //TODO: это сделай
                Route::get('paths', 'getPaths'); // Просмотр путей до сохранения
                Route::post('', 'addPath'); // Добавить путь до сохранения
            });
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
            // Аутентификация
            Route::get('auth-url', 'getAuthUrl');
            Route::get('callback', 'callback')->withoutMiddleware('auth:sanctum');

            // Новая система загрузки
            Route::post('generate-upload-url', 'generateUploadUrl'); // Генерация URL для загрузки
            Route::post('confirm-upload/{save}', 'confirmUpload'); // Подтверждение загрузки

        });
    });
});



