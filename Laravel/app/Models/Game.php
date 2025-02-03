<?php

namespace App\Models;

use Illuminate\Database\Eloquent\Model;
use Illuminate\Support\Facades\Http;

class Game extends Model
{
    protected $fillable = [
        'name',
        'path_to_exe',
        'path_to_icon',
        'hidden',
        'last_played_at',
        'total_time',
        'user_id',
    ];

    public function saves()
    {
        return $this->hasMany(Save::class);
    }
    public function user()
    {
        return $this->belongsToMany(User::class);
    }

}
