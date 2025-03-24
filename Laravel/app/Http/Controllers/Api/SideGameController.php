<?php

namespace App\Http\Controllers\Api;

use App\Http\Controllers\Controller;
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
    /**
     * Получить все сторонние игры.
     */
    public function getSideGames(): JsonResponse
    {
        $sideGames = SideGame::all();
        return response()->json($sideGames);
    }

    /**
     * Получить конкретную стороннюю игру.
     */
    public function getSideGame(SideGame $sideGame): JsonResponse
    {
        $user = auth()->user();
        $library = Library::query()->where('side_game_id', $sideGame->id)->where('user_id', $user->id)->first();
        $saves = $user->saves()->where('side_game_id',$sideGame->id)->get();
        return response()->json([
            'side_game' => SideGameResource::make($sideGame),
            'library' => $library ? LibraryResource::make($library) : null,
            'saves' => $saves->isEmpty() ? null : SaveResource::collection($saves),
        ]);
    }

    /**
     * Добавить стороннюю игру и сразу в библиотеку.
     */
    public function addSideGame(Request $request): JsonResponse
    {
        $user = auth()->user();

        $validatedData = $request->validate([
            'name' => 'required|string|max:255',
        ]);

        $sideGame = SideGame::create(['user_id' => $user->id,...$validatedData]);
        $library = Library::query()->firstOrCreate([
            'user_id' => $user->id,
            'side_game_id' => $sideGame->id,
        ]);
        $library->load('sideGame');
        return response()->json(LibraryResource::make($library), 201);

    }

    /**
     * Удалить стороннюю игру.
     */
    public function removeSideGame(SideGame $sideGame): JsonResponse
    {
        $sideGame->delete();
        // Удалить все сохранения к этой игре
        Save::query()->where('side_game_id', $sideGame->id)->delete();
        return response()->json(null, 204);
    }
}
