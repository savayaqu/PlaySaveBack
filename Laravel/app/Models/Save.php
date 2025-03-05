<?php

namespace App\Models;

use Illuminate\Database\Eloquent\Model;

class Save extends Model
{
    protected $fillable = [
      'file_id',
      'file_name',
      'version',
      'size',
      'description',
      'user_id',
      'game_id',
      'user_cloud_service_id',
    ];
    public function user()
    {
        return $this->belongsTo(User::class);
    }
    public function game()
    {
        return $this->belongsTo(Game::class);
    }
    public function userCloudService()
    {
        return $this->belongsTo(UserCloudService::class);
    }
    public function saveAccesses()
    {
        return $this->hasMany(SaveAccess::class);
    }
}
