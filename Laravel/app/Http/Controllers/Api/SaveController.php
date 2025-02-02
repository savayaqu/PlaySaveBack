<?php

namespace App\Http\Controllers\Api;

use App\Http\Controllers\Controller;
use App\Models\CustomGame;
use App\Models\Game;
use App\Models\User;
use Illuminate\Http\Request;
use Illuminate\Support\Facades\Auth;

class SaveController extends Controller
{
    public function uploadSave(Request $request)
    {
        $user = $request->user();
        $game = Game::find($request->game_id);
        $customGame = CustomGame::find($request->custom_game_id);

        if ($request->hasFile('save_file'))
            $path = $request
                ->file('save_file')
                ->store("saves/$user->nickname/" . $game->name ?? $customGame->name, 'public');


    }
}
