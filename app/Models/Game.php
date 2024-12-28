<?php

namespace App\Models;

use Illuminate\Database\Eloquent\Model;

class Game extends Model
{
    protected $fillable = ['name', 'image', 'publisher_id'];
    public function publisher()
    {
        return $this->belongsTo(Publisher::class);
    }
    public function saves()
    {
        return $this->hasMany(Save::class);
    }
}
