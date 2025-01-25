<?php

namespace App\Http\Controllers\Web;

use App\Http\Controllers\Controller;
use App\Models\Game;
use Illuminate\Http\Request;
use Illuminate\Support\Facades\Http;

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

        // Передача данных в шаблон
        return view('games.index', compact('games'));
    }
    public function show($steam_id)
    {
        // Получаем игру из базы по steam_id
        $game = Game::where('steam_id', $steam_id)->firstOrFail();

        // Выполняем запрос к Steam API для получения подробной информации
        $steamApiUrl = "https://store.steampowered.com/api/appdetails?appids={$game->steam_id}&l=ru";
        $response = Http::get($steamApiUrl);
        $steamData = $response->json();
        // Проверяем ответ
        if ($steamData["$steam_id"]['success']) {
            $gameDetails = $steamData["$steam_id"]['data'] ?? null;
        } else {
            $gameDetails = null;
        }
        // Отправляем данные в представление
        return view('games.show', compact('game', 'gameDetails'));
    }

}
