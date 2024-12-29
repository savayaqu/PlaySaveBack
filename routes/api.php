<?php

use App\Http\Controllers\GoogleDriveController;
use Illuminate\Support\Facades\Route;
use App\Http\Controllers\Api\AuthController;
use App\Http\Controllers\Api\SaveController;

Route::post('register', [AuthController::class, 'signUp']);
Route::post('login',    [AuthController::class, 'signIn']);
Route::get('logout',    [AuthController::class, 'logout'])->middleware('auth:sanctum');
Route::get('logoutAll', [AuthController::class, 'logoutAll'])->middleware('auth:sanctum');

// Загрузка сейва на этот сервак
Route::post('uploadToServer', [SaveController::class, 'upload'])->middleware('auth:sanctum');

//TODO: доработать, токен не передать, для сессий нужен web, тяжело крч, однако добавление в сессии работает

//TODO: так как будет в wpf то хз чё выбрать. хотя привязку на сайте можно сделать и уже готовая будет, ДА-ДА-ДА

//TODO: в общем сделать вебку с сессиями для привязки гугл диска
Route::middleware(['web'])->group(function () {
    Route::get('/google-drive/auth-url', [GoogleDriveController::class, 'getAuthUrl']);
    Route::get('/google-drive/callback', [GoogleDriveController::class, 'callback']);
});
