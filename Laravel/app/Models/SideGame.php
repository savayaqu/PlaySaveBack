<?php

namespace App\Models;

use Illuminate\Database\Eloquent\Model;

class SideGame extends Model
{
    protected $fillable = [
        'name',
        'user_id'
    ];
    public function user()
    {
        return $this->belongsTo(User::class);
    }
    public function libraries()
    {
        return $this->hasMany(Library::class);
    }
    public function saves()
    {
        return $this->hasMany(Save::class);
    }
}
