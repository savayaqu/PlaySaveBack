<?php

namespace App\Http\Controllers\Api;

use App\Http\Controllers\Controller;
use App\Http\Resources\SaveResource;
use App\Models\Game;
use Illuminate\Http\Request;

class SaveController extends Controller
{
    public function getMySavesGame(Game $game)
    {
        $user = auth()->user();
        $saves = $user->saves()->where('game_id', $game->id)->get();
        return response()->json(['saves' => SaveResource::collection($saves)]);
    }
}
