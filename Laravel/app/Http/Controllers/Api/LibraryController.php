<?php

namespace App\Http\Controllers\Api;

use App\Http\Controllers\Controller;
use App\Http\Resources\LibraryResource;
use App\Http\Resources\UserResource;
use App\Models\Game;
use App\Models\Library;
use Illuminate\Http\JsonResponse;
use Illuminate\Http\Request;

class LibraryController extends Controller
{
    public function getLibrary(): JsonResponse
    {
        $user = auth()->user();
        $libraries = $user->libraries()->with('game')->simplePaginate(30);
        return LibraryResource::collection($libraries)->response();
    }
    public function addToLibrary(Game $game): JsonResponse
    {
        $user = auth()->user();
        $library = Library::query()->firstOrCreate([
           'user_id' => $user->id,
           'game_id' => $game->id,
        ]);
        return response()->json(LibraryResource::make($library), 201);
    }
    public function toggleFavorite(Game $game): JsonResponse
    {
        $user = auth()->user();
        $library = $user->libraries()->where('game_id', $game->id)->first();

        if ($library)
            $library->update(['is_favorite' => !$library->is_favorite]);
        return response()->json(null, 204);
    }
}
