<?php

namespace App\Models;

use Illuminate\Database\Eloquent\Model;
use Illuminate\Database\Eloquent\Relations\HasMany;

class CloudService extends Model
{
    protected $fillable = [
        'name',
        'icon',
        'description'
    ];
    public function userCloudServices(): HasMany
    {
        return $this->hasMany(UserCloudService::class);
    }
}
