<?php

namespace App\Http\Controllers\Api;

use App\Http\Controllers\Controller;
use App\Models\Game;
use Illuminate\Http\Request;

class GameController extends Controller
{
    public function index(Request $request)
    {
        $perPage = $request->input('per_page', 10); // Количество игр на странице
        $search = $request->input('search'); // Параметр поиска

        $query = Game::query();

        // Если есть параметр поиска, фильтруем по названию
        if (!empty($search)) {
            $query->where('name', 'like', '%' . $search . '%');
        }

        // Пагинация
        $games = $query->paginate($perPage);

        // Возвращаем JSON-ответ
        return response()->json($games);
    }
    public function showGame($gameId)
    {
        $game = Game::query()->findOrFail($gameId);
        $steamGame = $game->steamGame();
        return response()->json([
            'game' => $game,
            'steamGame' => $steamGame,
        ]);
    }

}
