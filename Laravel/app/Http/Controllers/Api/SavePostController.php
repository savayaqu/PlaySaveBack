<?php

namespace App\Http\Controllers\Api;

use App\Http\Controllers\Controller;
use App\Http\Requests\Api\Post\CreatePostRequest;
use App\Models\Save;
use App\Models\SaveImage;
use App\Models\SavePost;
use App\Models\SavePostImage;

class SavePostController extends Controller
{
    public function createPost(Save $save, CreatePostRequest $request)
    {
        $user = auth()->user();
        if(SavePost::query()->where('save_id', $save->id)->exists()){
            //TODO: fix this
            return response()->json(["already exists"]);
        }
        // Получаем валидированные данные (без images)
        $validatedData = $request->validated();
        unset($validatedData['images']); // Удаляем images из массива

        // Создаем пост
        $post = SavePost::query()->create([
            'save_id' => $save->id,
            ...$validatedData // Разворачиваем оставшиеся данные
        ]);

        // Если есть картинки, сохраняем их
        if ($request->hasFile('images')) {
            foreach ($request->file('images') as $image) {
                // Путь: storage/app/public/userId/posts/saveId/images
                $path = $image->store("$user->id/posts/$save->id/images", 'public');
                $saveImage = SaveImage::query()->create(['path' => $path]);
                $post->savePostImages()->create(['save_image_id' => $saveImage->id]);
            }
        }
        $post->load('savePostImages');
        return response()->json([
            'post' => $post,
        ]);
    }
}
