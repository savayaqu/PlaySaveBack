<?php

namespace App\Models;

use Illuminate\Database\Eloquent\Model;

class CustomGame extends Model
{
    protected $fillable = [
        'name',
        'description',
        'user_id',
    ];
    public function user()
    {
        return $this->belongsTo(User::class);
    }
    public function saves()
    {
        return $this->hasMany(Save::class);
    }
    public function collections()
    {
        return $this->hasMany(Collection::class);
    }
}
