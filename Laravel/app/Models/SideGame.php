<?php

namespace App\Models;

use Illuminate\Database\Eloquent\Model;
use Illuminate\Database\Eloquent\Relations\BelongsTo;
use Illuminate\Database\Eloquent\Relations\HasMany;

class SideGame extends Model
{
    protected $fillable = [
        'name',
        'user_id'
    ];
    public function user(): BelongsTo
    {
        return $this->belongsTo(User::class);
    }
    public function libraries(): HasMany
    {
        return $this->hasMany(Library::class);
    }
    public function saves(): HasMany
    {
        return $this->hasMany(Save::class);
    }
}
