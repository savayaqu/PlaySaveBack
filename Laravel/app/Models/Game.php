<?php

namespace App\Models;

use Illuminate\Database\Eloquent\Model;
use Illuminate\Database\Eloquent\Relations\BelongsToMany;
use Illuminate\Database\Eloquent\Relations\HasMany;
use Illuminate\Database\Eloquent\Relations\HasOne;

class Game extends Model
{
    protected $fillable = [
        'name',
        'platform',
        'game_code'
    ];

    public function saves(): HasMany
    {
        return $this->hasMany(Save::class);
    }
    public function user(): BelongsToMany
    {
        return $this->belongsToMany(User::class);
    }
    public function path(): HasOne
    {
        return $this->hasOne(Path::class);
    }
}
