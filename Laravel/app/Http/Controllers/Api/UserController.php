<?php

namespace App\Http\Controllers\Api;

use App\Exceptions\ApiException;
use App\Http\Controllers\Controller;
use App\Http\Requests\Api\User\UpdateProfileRequest;
use App\Http\Resources\CloudServiceResource;
use App\Http\Resources\GameResource;
use App\Http\Resources\SideGameResource;
use App\Http\Resources\UserResource;
use App\Models\CloudService;
use App\Models\User;
use Illuminate\Http\JsonResponse;
use Illuminate\Support\Facades\Hash;
use Illuminate\Support\Facades\Log;
use Illuminate\Support\Facades\Storage;

class UserController extends Controller
{

    // Получение своего профиля
    public function getProfile(): JsonResponse
    {
        $user = auth()->user();
        return response()->json(UserResource::make($user));
    }

    // Обновление профиля
    public function updateProfile(UpdateProfileRequest $request): JsonResponse
    {
        $user = auth()->user();
        if (!$user instanceof User) {
            throw new \RuntimeException('Authenticated user is not an instance of User model.');
        }

        if ($request->current_password != null) {
            if (!Hash::check($request->current_password, $user->password)) {
                throw new ApiException('Invalid current password', 401);
            }
        }

        $data = $request->validated();
        $storage = Storage::disk('public');

        // Генерация уникального идентификатора для файлов
        $uniqueId = uniqid();

        // Обработка аватара
        if ($request->hasFile('avatar_file')) {
            // Удаляем старый аватар, если он существует
            if ($user->avatar) {
                try {
                    $storage->delete($user->avatar);
                } catch (\Exception $e) {
                    Log::error("Failed to delete old avatar: " . $e->getMessage());
                }
            }

            // Сохраняем новый аватар с уникальным именем
            $extension = $request->file('avatar_file')->getClientOriginalExtension();
            $filename = "avatar_{$uniqueId}.{$extension}";

            $path = $request->file('avatar_file')->storeAs(
                $user->login,
                $filename,
                'public'
            );
            $data['avatar'] = $path;
        }

        // Обработка хедера
        if ($request->hasFile('header_file')) {
            // Удаляем старый хедер, если он существует
            if ($user->header) {
                try {
                    $storage->delete($user->header);
                } catch (\Exception $e) {
                    Log::error("Failed to delete old header: " . $e->getMessage());
                }
            }

            // Сохраняем новый хедер с уникальным именем
            $extension = $request->file('header_file')->getClientOriginalExtension();
            $filename = "header_{$uniqueId}.{$extension}";

            $path = $request->file('header_file')->storeAs(
                $user->login,
                $filename,
                'public'
            );
            $data['header'] = $path;
        }

        $user->update($data);
        return response()->json(UserResource::make($user));
    }

    // Получение облачных сервисов
    public function getCloudServices(): JsonResponse
    {
        $services = CloudService::all();
        return response()->json(CloudServiceResource::collection($services));
    }

    // Получение статистики
    public function getStatistic(): JsonResponse
    {
        $user = auth()->user();
        $totalPlayed = $user->libraries()->get()->sum('time_played');

        $recentlyPlayed = $user->libraries()
            ->get()
            ->sortByDesc('last_played_at')
            ->take(6)
            ->map(function ($item) {
                $gameData = $item->game_id
                    ? ['game' => GameResource::make($item->game)]
                    : ['sideGame' => SideGameResource::make($item->sideGame)];

                return array_merge($gameData, [
                    'time_played' => $item->time_played
                ]);
            })
            ->values();

        return response()->json([
            'totalPlayed' => $totalPlayed,
            'recentlyPlayed' => $recentlyPlayed,
        ]);
    }
}
