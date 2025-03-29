<?php

namespace App\Http\Controllers\Api;

use App\Http\Controllers\Controller;
use App\Http\Requests\Api\Library\EditLibraryGameRequest;
use App\Http\Resources\LibraryResource;
use App\Http\Resources\UserResource;
use App\Models\Game;
use App\Models\Library;
use App\Models\SideGame;
use Illuminate\Http\JsonResponse;
use Illuminate\Http\Request;

class LibraryController extends Controller
{
    public function getLibrary(Request $request): JsonResponse
    {
        $user = auth()->user();

        // Проверяем, есть ли параметр limit в запросе и равен ли он 0
        if ($request->has('limit') && $request->input('limit') == 0) {
            // Если limit равен 0, загружаем все записи без пагинации
            $libraries = $user->libraries()
                ->with(['game', 'sideGame'])
                ->orderByDesc('time_played')
                ->get();
        } else {
            // Иначе используем пагинацию с указанным лимитом или по умолчанию 30
            $perPage = $request->input('limit', 30);
            $libraries = $user->libraries()->with(['game', 'sideGame'])->simplePaginate($perPage);
        }

        return LibraryResource::collection($libraries)->response();
    }

    public function addToLibrary(Game $game): JsonResponse
    {
        $user = auth()->user();
        $library = Library::query()->firstOrCreate([
            'user_id' => $user->id,
            'game_id' => $game->id,
        ]);
        $library->load('game');
        return response()->json(LibraryResource::make($library), 201);
    }

    public function toggleFavorite(Game $game): JsonResponse
    {
        $user = auth()->user();
        $library = $user->libraries()->where('game_id', $game->id)->firstOrFail();
        $library->update(['is_favorite' => !$library->is_favorite]);
        return response()->json(null, 204);
    }

    public function toggleSideGameFavorite(SideGame $sideGame): JsonResponse
    {
        $user = auth()->user();
        $library = $user->libraries()->where('side_game_id', $sideGame->id)->firstOrFail();
        $library->update(['is_favorite' => !$library->is_favorite]);
        return response()->json(null, 204);
    }

    public function updateLibraryGame(Game $game, EditLibraryGameRequest $request): JsonResponse
    {
        $user = auth()->user();
        $library = $user->libraries()->where('game_id', $game->id)->firstOrFail();
        $library->update($request->validated());
        return response()->json(null, 204);
    }

    public function updateSideGameLibrary(SideGame $sideGame, EditLibraryGameRequest $request): JsonResponse
    {
        $user = auth()->user();
        $library = $user->libraries()->where('side_game_id', $sideGame->id)->firstOrFail();
        $library->update($request->validated());
        return response()->json(null, 204);
    }

    public function removeFromLibrary(Game $game): JsonResponse
    {
        $user = auth()->user();
        $library = $user->libraries()->where('game_id', $game->id)->firstOrFail();
        $library->delete();
        return response()->json(null, 204);
    }

    public function removeSideGameFromLibrary(SideGame $sideGame): JsonResponse
    {
        $user = auth()->user();
        $library = $user->libraries()->where('side_game_id', $sideGame->id)->firstOrFail();
        $library->delete();
        return response()->json(null, 204);
    }
}
