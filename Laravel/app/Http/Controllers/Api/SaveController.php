<?php

namespace App\Http\Controllers\Api;

use App\Http\Controllers\Controller;
use App\Http\Resources\SaveResource;
use App\Models\Game;
use App\Models\SideGame;
use Illuminate\Http\Request;

class SaveController extends Controller
{
    public function getMySavesGame(Game $game)
    {
        $user = auth()->user();
        $saves = $user->saves()->where('game_id', $game->id)->get();
        return response()->json(['saves' => SaveResource::collection($saves)]);
    }
    public function getMySavesSideGame(SideGame $sideGame)
    {
        $user = auth()->user();
        $saves = $user->saves()->where('side_game_id', $sideGame->id)->get();
        return response()->json(['saves' => SaveResource::collection($saves)]);
    }
    public function getHash(Request $request)
    {
        $user = auth()->user();
        $file = $request->file('file');
        $hash = hash('sha256', file_get_contents($file)); // Вычисление хеша
        dd($hash);
    }
}
