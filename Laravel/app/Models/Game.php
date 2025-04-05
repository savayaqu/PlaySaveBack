<?php

namespace App\Models;

use Illuminate\Database\Eloquent\Model;
use Illuminate\Support\Facades\Http;

class Game extends Model
{
    protected $fillable = [
        'name',
        'platform',
        'game_code'
    ];

    public function saves()
    {
        return $this->hasMany(Save::class);
    }
    public function user()
    {
        return $this->belongsToMany(User::class);
    }
    public function gamePaths()
    {
        return $this->hasMany(GamePath::class);
    }
}
