<?php

namespace App\Http\Controllers\Api;

use App\Http\Controllers\Controller;
use App\Http\Resources\LibraryResource;
use App\Http\Resources\UserResource;
use App\Models\Library;
use Illuminate\Http\JsonResponse;
use Illuminate\Http\Request;

class LibraryController extends Controller
{
    public function getLibrary(): JsonResponse
    {
        $user = auth()->user();
        $libraries = Library::query()->where('user_id', $user->id)->with('game')->simplePaginate(30);
        return LibraryResource::collection($libraries)->response();
    }
}
