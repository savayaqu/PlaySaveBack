<?php

namespace App\Http\Controllers\Api;

use App\Http\Controllers\Controller;
use App\Models\SideGame;
use Illuminate\Http\Request;
use Illuminate\Http\JsonResponse;

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
        return response()->json($sideGame);
    }

    /**
     * Добавить новую стороннюю игру.
     */
    public function addSideGame(Request $request): JsonResponse
    {
        $validatedData = $request->validate([
            'name' => 'required|string|max:255',
            'user_id' => 'required|exists:users,id',
        ]);

        $sideGame = SideGame::create($validatedData);

        return response()->json($sideGame, 201);
    }

    /**
     * Обновить существующую стороннюю игру.
     */
    public function updateSideGame(SideGame $sideGame, Request $request): JsonResponse
    {
        $validatedData = $request->validate([
            'name' => 'sometimes|string|max:255',
            'user_id' => 'sometimes|exists:users,id',
        ]);

        $sideGame->update($validatedData);

        return response()->json($sideGame);
    }

    /**
     * Удалить стороннюю игру.
     */
    public function removeSideGame(SideGame $sideGame): JsonResponse
    {
        $sideGame->delete();

        return response()->json(null, 204);
    }
}
