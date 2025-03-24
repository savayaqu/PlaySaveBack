<?php

use Illuminate\Support\Facades\Route;

Route::view('/', 'home')->name('home');
Route::get('/auth/success', function () {
    return view('success_oauth');
});
