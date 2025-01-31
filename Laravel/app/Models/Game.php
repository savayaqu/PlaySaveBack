<?php

namespace App\Models;

use Illuminate\Database\Eloquent\Model;
use Illuminate\Support\Facades\Http;

class Game extends Model
{
    protected $fillable = ['name', 'image', 'publisher_id'];
    public function publisher()
    {
        return $this->belongsTo(Publisher::class);
    }
    public function saves()
    {
        return $this->hasMany(Save::class);
    }
    public function collections()
    {
        return $this->hasMany(Collection::class);
    }
    public function steamGame()
    {
        $steamId = $this->steam_id ?? null;
        if (!$steamId) {
            return response()->json(['error' => 'Steam ID not provided'], 400);
        }

        $steamApiUrl = "https://store.steampowered.com/api/appdetails?appids={$steamId}&l=ru";
        $response = Http::get($steamApiUrl);
        $steamData = $response->json();

        if (!isset($steamData[$steamId]) || !$steamData[$steamId]['success']) {
            return response()->json(['error' => 'Game not found'], 404);
        }

        return $steamData[$steamId]['data'];
    }

}
