<?php

namespace App\Http\Controllers\Api;

use App\Enums\UserVisibility;
use App\Exceptions\ForbiddenException;
use App\Http\Controllers\Controller;
use App\Http\Requests\Api\User\UpdateProfileRequest;
use App\Http\Resources\CloudServiceResource;
use App\Http\Resources\LibraryResource;
use App\Http\Resources\UserResource;
use App\Models\CloudService;
use App\Models\User;
use Illuminate\Http\JsonResponse;
use Illuminate\Support\Facades\Log;
use Illuminate\Support\Facades\Storage;

class UserController extends Controller
{
    public function getProfile(): JsonResponse
    {
        $user = auth()->user();
        return response()->json(UserResource::make($user));
    }
    public function updateProfile(UpdateProfileRequest $request): JsonResponse
    {
        $user = auth()->user();
        if (!$user instanceof User) {
            throw new \RuntimeException('Authenticated user is not an instance of User model.');
        }

        $data = $request->validated();
        $storage = Storage::disk('public');

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

            // Сохраняем новый аватар
            $path = $request->file('avatar_file')->storeAs(
                $user->login,
                'avatar.' . $request->file('avatar_file')->getClientOriginalExtension(),
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

            // Сохраняем новый хедер
            $path = $request->file('header_file')->storeAs(
                $user->login,
                'header.' . $request->file('header_file')->getClientOriginalExtension(),
                'public'
            );
            $data['header'] = $path;
        }

        $user->update($data);
        return response()->json(UserResource::make($user));
    }
    public function getOtherProfile(User $user): JsonResponse
    {
        if($user->visibility == UserVisibility::Private->value)
            throw new ForbiddenException;
        $user->loadCount('saves');

        // Загружаем библиотеки с пагинацией и связанной игрой
        $libraries = $user->libraries()->with('game')->simplePaginate(10);

        return response()->json([
            'user' => UserResource::make($user),
            'library' => LibraryResource::collection($libraries)->response()->getData(true),
        ]);
    }
    public function getCloudServices(): JsonResponse
    {
        $services = CloudService::all();
        return response()->json(CloudServiceResource::collection($services));
    }
}
