<?php

namespace App\Models;

use Illuminate\Database\Eloquent\Model;

class Collection extends Model
{
    protected $fillable = ['user_id', 'custom_game_id', 'game_id'];
    public function user()
    {
        return $this->belongsTo(User::class);
    }
    public function game()
    {
        return $this->belongsTo(Game::class);
    }
    public function customGame()
    {
        return $this->belongsTo(CustomGame::class);
    }
}
