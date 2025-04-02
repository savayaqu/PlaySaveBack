<?php

namespace App\Models;

use Illuminate\Database\Eloquent\Model;
use App\Enums\PathGame;

class GamePath extends Model
{
    protected $table = 'game_paths';
    protected $fillable = [ 'game_id', 'path', 'status' ];
    protected $attributes = [
      'status' => PathGame::Pending,
    ];
    public function game()
    {
        return $this->belongsTo(Game::class);
    }
}
