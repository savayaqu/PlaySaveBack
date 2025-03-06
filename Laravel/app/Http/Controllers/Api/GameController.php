<?php

namespace App\Http\Controllers\Api;

use App\Http\Controllers\Controller;
use App\Http\Requests\Api\Game\AddGameRequest;
use App\Http\Requests\Api\Game\EditGameRequest;
use App\Http\Resources\GameResource;
use App\Http\Resources\LibraryResource;
use App\Http\Resources\SaveResource;
use App\Models\Game;
use App\Models\Library;
use Illuminate\Http\JsonResponse;
use Illuminate\Http\Request;

class GameController extends Controller
{
    public function getGames(Request $request): JsonResponse
    {
        $games = Game::query()->paginate(30);
        return GameResource::collection($games)->response();
    }
    public function getGame(Game $game): JsonResponse
    {
        $user = auth()->user();
        $library = Library::query()->where('game_id', $game->id)->where('user_id', $user->id)->first();
        $saves = $user->saves()->where('game_id',$game->id)->get();
        return response()->json([
            'game' => GameResource::make($game),
            'library' => $library ? LibraryResource::make($library) : null,
            'saves' => $saves->isEmpty() ? null : SaveResource::collection($saves),
        ]);
    }
}
