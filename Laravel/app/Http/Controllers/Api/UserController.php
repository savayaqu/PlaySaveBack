<?php

namespace App\Http\Controllers\Api;

use App\Http\Controllers\Controller;
use App\Http\Requests\Api\User\UpdateMeRequest;

class UserController extends Controller
{
    public function getMe()
    {
        $user = auth()->user();
        return response()->json($user);
    }
    public function updateMe(UpdateMeRequest $request)
    {
        $user = auth()->user();
        if ($request->hasFile('avatar'))
        {
            $path = $request->file('avatar')->store("avatars/$user->nickname", 'public');
            $user->update(['avatar' => $path]);
        }
        $user->update($request->except('avatar'));
        return response()->json($user);
    }
}
