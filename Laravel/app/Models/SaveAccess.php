<?php

namespace App\Models;

use Illuminate\Database\Eloquent\Model;

class SaveAccess extends Model
{
    protected $fillable = [
        'user_id',
        'save_id',
        'access_type',
        'expires_at',
        'token',
    ];
    public function saveRelation()
    {
        return $this->belongsTo(Save::class);
    }
}
