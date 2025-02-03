<?php

namespace App\Http\Controllers\Api;

use App\Http\Controllers\Controller;
use App\Http\Requests\Api\Game\AddGameRequest;
use App\Http\Requests\Api\Game\EditGameRequest;
use App\Models\Game;

class GameController extends Controller
{
    public function getGames()
    {
        $user = auth()->user();
        $games = Game::query()->where('user_id', $user->id)->get();
        return response()->json($games);
    }
    public function getGame(Game $game)
    {
        $user = auth()->user();
        return response()->json($game);
    }
    public function addGame(AddGameRequest $request)
    {
        $user = auth()->user();
        $validated = $request->validated();
        $game = Game::query()->create([
            ...$validated,
            'user_id' => $user->id,
        ]);
        return response()->json($game, 201);
    }
    public function editGame(Game $game, EditGameRequest $request)
    {
        $game->update($request->all());
        return response()->json($game);
    }
}
