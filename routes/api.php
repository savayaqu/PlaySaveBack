<?php

use App\Http\Controllers\Api\AuthController;
use Illuminate\Support\Facades\Route;

Route::post('register', [AuthController::class, 'signUp']);
Route::post('login',    [AuthController::class, 'signIn']);
Route::get('logout',    [AuthController::class, 'logout'])->middleware('auth:sanctum');
Route::get('logoutAll',    [AuthController::class, 'logoutAll'])->middleware('auth:sanctum');
