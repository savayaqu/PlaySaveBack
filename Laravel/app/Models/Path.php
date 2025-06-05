<?php

namespace App\Models;

use Illuminate\Database\Eloquent\Model;
use Illuminate\Database\Eloquent\Relations\BelongsTo;

class Path extends Model
{
    protected $table = 'paths';
    protected $fillable = [
        'game_id',
        'path'
    ];
    public function game(): BelongsTo
    {
        return $this->belongsTo(Game::class);
    }
}
