<?php

namespace App\Models;

use App\Enums\UserVisibility;
use Illuminate\Database\Eloquent\Factories\HasFactory;
use Illuminate\Foundation\Auth\User as Authenticatable;
use Illuminate\Notifications\Notifiable;
use Illuminate\Support\Facades\Storage;
use Illuminate\Database\Eloquent\Relations\HasMany;
use Illuminate\Support\Str;
use Laravel\Sanctum\HasApiTokens;

class User extends Authenticatable
{
    use HasFactory, Notifiable, HasApiTokens;
    protected $fillable = [
        'email',
        'password',
        'nickname',
        'avatar',
        'login',
        'header',
        'key',
        'visibility'
    ];

    /**
     * The attributes that should be hidden for serialization.
     *
     * @var list<string>
     */
    protected $hidden = [
        'password',
        'remember_token',
        'key'
    ];
    // Установка по умолчанию
    protected $attributes = [
        'visibility' => UserVisibility::Private,
    ];
    /**
     * Get the attributes that should be cast.
     *
     * @return array<string, string>
     */
    protected function casts(): array
    {
        return [
            'email_verified_at' => 'datetime',
            'password' => 'hashed',
            'key' => 'hashed'
        ];
    }
    public function getImage($value): ?string
    {
        if ($value === null) {
            return null;
        }
        if(!Str::startsWith($value, 'http')){
            return asset(Storage::url($value));
        }
        return $value;
    }
    public function saves(): HasMany
    {
        return $this->hasMany(Save::class);
    }
    public function userCloudService(): HasMany
    {
        return $this->hasMany(UserCloudService::class);
    }
    public function saveAccesses(): HasMany
    {
        return $this->hasMany(SaveAccess::class);
    }
    public function libraries(): HasMany
    {
        return $this->hasMany(Library::class);
    }
}
