<?php

namespace App\Models;

use Illuminate\Database\Eloquent\Model;
use Illuminate\Database\Eloquent\Relations\BelongsTo;

class Library extends Model
{
    protected $fillable = [
      'user_id',
      'game_id',
      'last_played_at',
      'time_played',
      'is_favorite',
    ];
    public function user(): BelongsTo
    {
        return $this->belongsTo(User::class);
    }
    public function game(): BelongsTo
    {
        return $this->belongsTo(Game::class);
    }
}

