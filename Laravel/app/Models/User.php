<?php

namespace App\Models;

use Illuminate\Database\Eloquent\Factories\HasFactory;
use Illuminate\Foundation\Auth\User as Authenticatable;
use Illuminate\Notifications\Notifiable;
use Illuminate\Support\Facades\Storage;
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
    ];

    /**
     * The attributes that should be hidden for serialization.
     *
     * @var list<string>
     */
    protected $hidden = [
        'password',
        'remember_token',
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
        ];
    }
    public function saves()
    {
        return $this->hasMany(Save::class);
    }
    public function userCloudService()
    {
        return $this->hasMany(UserCloudService::class);
    }
    public function saveAccesses()
    {
        return $this->hasMany(SaveAccess::class);
    }
    public function libraries()
    {
        return $this->hasMany(Library::class);
    }
}
