<?php

namespace App\Models;

use Illuminate\Database\Eloquent\Model;

class Save extends Model
{
    protected $fillable = [
      'file_name',
      'file_path',
      'size',
      'is_shared',
      'description',
      'user_id',
      'game_id',
      'user_cloud_service_id',
      'custom_game_id',
    ];
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
    public function userCloudService()
    {
        return $this->belongsTo(UserCloudService::class);
    }
}
