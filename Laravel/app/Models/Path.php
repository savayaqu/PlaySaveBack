<?php

namespace App\Models;

use Illuminate\Database\Eloquent\Model;
use App\Enums\CloudStatus;

class Path extends Model
{
    protected $table = 'paths';
    protected $fillable = [ 'game_id', 'path'];
    public function game()
    {
        return $this->belongsTo(Game::class);
    }
}
