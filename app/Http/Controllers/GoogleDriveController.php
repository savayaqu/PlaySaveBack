<?php

namespace App\Http\Controllers;

use App\Models\CloudService;
use App\Models\UserCloudService;
use Google\Client;
use Illuminate\Http\Request;
use Illuminate\Support\Facades\Auth;

class GoogleDriveController extends Controller
{
    public function getAuthUrl(Request $request)
    {
        $client = new Client();
        $client->setClientId(env('GOOGLE_DRIVE_CLIENT_ID'));
        $client->setClientSecret(env('GOOGLE_DRIVE_CLIENT_SECRET'));
        $client->setRedirectUri(env('GOOGLE_DRIVE_REDIRECT_URI'));
        $client->addScope("https://www.googleapis.com/auth/drive");
        return ($client->createAuthUrl());
    }

    public function callback(Request $request)
    {
        $client = new Client();
        $client->setClientId(env('GOOGLE_DRIVE_CLIENT_ID'));
        $client->setClientSecret(env('GOOGLE_DRIVE_CLIENT_SECRET'));
        $client->setRedirectUri(env('GOOGLE_DRIVE_REDIRECT_URI'));
        $cloudService = CloudService::query()->where('name', 'Google Drive')->first();
        $token = $client->fetchAccessTokenWithAuthCode($request->get('code'));
        UserCloudService::query()->create([
            'cloud_service_id' => $cloudService->id,
            //'user_id' => Auth::id(),
            //'user_id' => session('user_id'),
            'user_id'=> 1, //TODO: так дело не пойдёт
            'access_token' => $token['access_token'],
            'refresh_token' => $token['refresh_token'] ?? null,
            //'expires_at' => $token['expires_at'] ?? null,
            'expires_at' => now()->addSeconds($token['expires_in']),
        ]);
        return response()->json(['status' => 'success']);
    }
    public function logout()
    {
        dd(session('user_id'));
    }
}
