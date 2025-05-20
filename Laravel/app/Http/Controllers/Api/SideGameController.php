<?php

namespace App\Http\Controllers\Api;

use App\Http\Controllers\Controller;
use App\Http\Requests\Api\SideGame\SideGameRequest;
use App\Http\Resources\GameResource;
use App\Http\Resources\LibraryResource;
use App\Http\Resources\SaveResource;
use App\Http\Resources\SideGameResource;
use App\Models\Game;
use App\Models\Library;
use App\Models\Save;
use App\Models\SideGame;
use Illuminate\Http\Request;
use Illuminate\Http\JsonResponse;
use Illuminate\Support\Facades\Http;

class SideGameController extends Controller
{
    // Добавление сторонней игры
    public function addSideGame(SideGameRequest $request): JsonResponse
    {
        $user = auth()->user();
        $sideGame = $user->sideGames()->firstOrCreate($request->validated());
        $library = Library::query()->firstOrCreate([
            'user_id' => $user->id,
            'side_game_id' => $sideGame->id,
        ])->load('sideGame');
        return response()->json(LibraryResource::make($library), 201);
    }
    /**
     * Получить конкретную стороннюю игру.
     */
    public function getSideGame(SideGame $sideGame): JsonResponse
    {
        $user = auth()->user();
        $library = $user->libraries()->where('side_game_id', $sideGame->id)->firstOrFail();
        $saves = $user->saves()->where('side_game_id',$sideGame->id)->get();
        return response()->json([
            'side_game' => SideGameResource::make($sideGame),
            'library' => $library ? LibraryResource::make($library) : null,
            'saves' => $saves->isEmpty() ? null : SaveResource::collection($saves),
        ]);
    }


    public function removeSideGame(SideGame $sideGame): JsonResponse
    {
        $user = auth()->user();
        $user->sideGames()->findOrFail($sideGame->id)->delete();
        //$sideGame->delete();
        return response()->json(null, 204);
    }
}
