<?php

namespace App\Models;

use Illuminate\Database\Eloquent\Model;
use Illuminate\Database\Eloquent\Relations\BelongsTo;

class Save extends Model
{
    protected $hidden = ['file_id'];
    protected $fillable = [
      'file_id',
      'file_name',
      'version',
      'size',
      'description',
      'user_id',
      'game_id',
      'side_game_id',
      'user_cloud_service_id',
      'last_sync_at',
      'hash',
    ];
    public function user(): BelongsTo
    {
        return $this->belongsTo(User::class);
    }
    public function game(): BelongsTo
    {
        return $this->belongsTo(Game::class);
    }
    public function sideGame(): BelongsTo
    {
        return $this->belongsTo(SideGame::class);
    }
    public function userCloudService(): BelongsTo
    {
        return $this->belongsTo(UserCloudService::class);
    }
}
