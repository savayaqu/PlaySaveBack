<?php

namespace App\Http\Controllers\Web;

use App\Http\Controllers\Controller;
use Illuminate\Http\Request;

class CollectionController extends Controller
{
    public function add(Request $request)
    {
        $request->validate([
            'game_id' => 'required|exists:games,id',
        ]);
        // Проверяем, есть ли игра в коллекции пользователя
        if (!auth()->user()->collections()->where('game_id', $request->input('game_id'))->exists()) {
            auth()->user()->collections()->create([
                'game_id' => $request->input('game_id'),
            ]);
        }

        return redirect()->back()->with('success', 'Игра добавлена в коллекцию.');
    }

    public function remove(Request $request)
    {
        $request->validate([
            'game_id' => 'required|exists:games,id',
        ]);

        // Удаляем игру из коллекции пользователя
        auth()->user()->collections()->where('game_id', $request->input('game_id'))->delete();

        return redirect()->back()->with('success', 'Игра удалена из коллекции.');
    }

    public function index()
    {
        $collections = auth()->user()->collections()->with('game')->get();

        return view('collections.index', compact('collections'));
    }
}
