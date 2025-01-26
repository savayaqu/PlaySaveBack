<?php

namespace App\Models;

use Illuminate\Database\Eloquent\Model;

class CloudService extends Model
{
    protected $fillable = ['name', 'api_endpoint', 'description'];
    public function userCloudServices()
    {
        return $this->hasMany(UserCloudService::class);
    }
}
