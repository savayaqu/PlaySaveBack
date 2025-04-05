<?php

namespace App\Http\Controllers\Api;

use App\Http\Controllers\Controller;
use App\Http\Requests\Api\Save\OverwriteSaveRequest;
use App\Http\Resources\SaveResource;
use App\Models\CloudService;
use App\Models\Game;
use App\Models\Save;
use App\Models\SideGame;
use App\Models\UserCloudService;
use App\Services\GoogleDriveService;
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
    public function updateSave(Save $save, OverwriteSaveRequest $request)
    {
        $user = auth()->user();
        $cloudService = CloudService::query()->where('name', 'Google Drive')->first();

        if($save->userCloudService()->where('cloud_service_id', $cloudService->id)->exists())
        {
            $fileId = $save->file_id;
            $service = UserCloudService::query()->where('user_id', $user->id)->where('cloud_service_id', $cloudService->id)->first();
            $googleDriveService = new GoogleDriveService($service);
            $googleDriveService->getAndRenameParentFolder($fileId, $request->version);
        }
        $save->update($request->validated());

        return response()->json(SaveResource::make($save));
    }
}
