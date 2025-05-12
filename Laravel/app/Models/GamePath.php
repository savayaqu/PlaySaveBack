<?php

namespace App\Models;

use Illuminate\Database\Eloquent\Model;
use App\Enums\CloudStatus;

class GamePath extends Model
{
    protected $table = 'game_paths';
    protected $fillable = [ 'game_id', 'path'];
    public function game()
    {
        return $this->belongsTo(Game::class);
    }
}
