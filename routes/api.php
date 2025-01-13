<?php

use App\Http\Controllers\GoogleDriveController;
use Illuminate\Support\Facades\Route;
use App\Http\Controllers\Api\AuthController;
use App\Http\Controllers\Api\SaveController;
use App\Http\Controllers\Api\CloudServiceController;
use App\Http\Controllers\Api\CustomGameController;
use App\Http\Controllers\Api\GameController;
use App\Http\Controllers\Api\PublisherController;
use App\Http\Controllers\Api\UserController;
use App\Http\Controllers\Api\UserCloudServiceController;


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
// Пользователь
    Route::middleware('auth:sanctum')->group(function () {
        Route::controller(UserController::class)->group(function () {
            Route::post('user/update', 'updateProfile'); // Редактирование профиля
            Route::get('user', 'getProfile'); // Просмотр профиля
            Route::delete('user/cloud-storage/{id}', 'removeCloudStorage'); // Удаление привязанного облачного хранилища
        });
    });

// Сохранения
    Route::middleware('auth:sanctum')->group(function () {
        Route::controller(SaveController::class)->group(function () {
            Route::get('saves', 'getUserSaves'); // Просмотр своих сохранений (сортировка по играм, фильтры)
            Route::post('saves/update/{id}', 'updateSave'); // Обновление сохранения
            Route::post('saves/upload', 'uploadSave'); // Загрузка сохранения на сервер (с указанием сервера)
            Route::delete('saves/{id}', 'deleteSave'); // Удаление сохранения
            Route::post('saves/{id}/description', 'addDescription'); // Добавить описание сохранению
        });
    });

// Кастомные игры
    Route::middleware('auth:sanctum')->group(function () {
        Route::controller(CustomGameController::class)->group(function () {
            Route::get('custom-games', 'getCustomGames'); // Просмотр своих кастомных игр
            Route::post('custom-games', 'createCustomGame'); // Создание своей кастомной игры
            Route::delete('custom-games/{id}', 'deleteCustomGame'); // Удаление своей кастомной игры
        });
    });

// Издатели
    Route::middleware('auth:sanctum')->group(function () {
        Route::controller(PublisherController::class)->group(function () {
            Route::get('publishers', 'getPublishers'); // Просмотр издателей
            Route::get('publishers/{id}/games', 'getPublisherGames'); // Просмотр игр издателя
            Route::post('publishers', 'createPublisher'); // Создание издателя
            Route::put('publishers/{id}', 'updatePublisher'); // Редактирование издателя
            Route::delete('publishers/{id}', 'deletePublisher'); // Удаление издателя
        });
    });

// Игры
    Route::middleware('auth:sanctum')->group(function () {
        Route::controller(GameController::class)->group(function () {
            Route::post('games', 'createGame'); // Создание игры
            Route::put('games/{id}', 'updateGame'); // Редактирование игры
            Route::delete('games/{id}', 'deleteGame'); // Удаление игры
        });
    });

// Облачные хранилища
    Route::middleware('auth:sanctum')->group(function () {
        Route::controller(CloudServiceController::class)->group(function () {
            Route::get('cloud-services', 'getCloudServices'); // Просмотр всех облачных хранилищ
            Route::post('cloud-services', 'addCloudService'); // Добавление нового облачного хранилища
            Route::delete('cloud-services/{id}', 'deleteCloudService'); // Удаление облачного хранилища
        });
    });
});




// Загрузка сейва на этот сервак
Route::post('uploadToServer', [SaveController::class, 'upload'])->middleware('auth:sanctum');

//TODO: доработать, токен не передать, для сессий нужен web, тяжело крч, однако добавление в сессии работает

//TODO: так как будет в wpf то хз чё выбрать. хотя привязку на сайте можно сделать и уже готовая будет, ДА-ДА-ДА

//TODO: в общем сделать вебку с сессиями для привязки гугл диска
Route::middleware(['web'])->group(function () {
    Route::get('/google-drive/auth-url', [GoogleDriveController::class, 'getAuthUrl']);
    Route::get('/google-drive/callback', [GoogleDriveController::class, 'callback']);
});
Route::post('/google-drive/upload', [GoogleDriveController::class, 'uploadFile'])->middleware('auth:sanctum');
