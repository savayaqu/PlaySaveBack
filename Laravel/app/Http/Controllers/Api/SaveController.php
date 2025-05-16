<?php

namespace App\Http\Controllers\Api;

use App\Enums\CloudStatus;
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
    // Получение сохранений к игре
    public function getMySavesGame(Game $game)
    {
        $user = auth()->user();
        $saves = $user->saves()
            ->where('game_id', $game->id)
            ->whereHas('userCloudService', function ($query) {
                $query->where('status', CloudStatus::Active);
            })
            ->with('userCloudService')
            ->get();
        return response()->json(['saves' => SaveResource::collection($saves)]);
    }
    // Получение сохранений к сторонней игре
    public function getMySavesSideGame(SideGame $sideGame)
    {
        $user = auth()->user();
        $saves = $user->saves()
            ->where('side_game_id', $sideGame->id)
            ->whereHas('userCloudService', function ($query) {
                $query->where('status', CloudStatus::Active);
            })
            ->with('userCloudService')
            ->get();
        return response()->json(['saves' => SaveResource::collection($saves)]);
    }
    // Перезапись сохранения
    public function updateSave(Save $save, OverwriteSaveRequest $request)
    {
        $user = auth()->user();
        $cloudService = CloudService::query()
            ->where('name', 'Google Drive')
            ->first();

        if($save->userCloudService()
            ->where('cloud_service_id', $cloudService->id)
            ->where('status', CloudStatus::Active)
            ->exists())
        {
            $fileId = $save->file_id;
            $service = $user->userCloudService()
                ->where('cloud_service_id', $cloudService->id)
                ->where('status', CloudStatus::Active)
                ->first();
            $googleDriveService = new GoogleDriveService($service);
            $googleDriveService->getAndRenameParentFolder($fileId, $request->version);
        }
        $save->update($request->validated());

        return response()->json(SaveResource::make($save));
    }
}
