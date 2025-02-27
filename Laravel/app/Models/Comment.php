<?php

namespace App\Models;

use Illuminate\Database\Eloquent\Model;
use Illuminate\Database\Eloquent\Relations\BelongsTo;
use Illuminate\Database\Eloquent\Relations\HasMany;

class Comment extends Model
{
    protected $fillable = [
      'parent_id',
      'user_id',
      'content',
      'rating',
      'save_id'
    ];
    // Родительский комментарий
    public function parent(): BelongsTo
    {
        return $this->belongsTo(self::class, 'parent_id');
    }

    // Дочерние комментарии (ответы)
    public function replies(): HasMany
    {
        return $this->hasMany(self::class, 'parent_id');
    }

    // Пользователь, оставивший комментарий
    public function user(): BelongsTo
    {
        return $this->belongsTo(User::class);
    }

    // Игра, к которой относится комментарий
    public function game(): BelongsTo
    {
        return $this->belongsTo(Game::class);
    }
}
