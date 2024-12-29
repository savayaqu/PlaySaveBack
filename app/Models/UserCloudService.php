<?php

namespace App\Models;

use Illuminate\Database\Eloquent\Model;

class UserCloudService extends Model
{
    protected $table = 'user_cloud_service';
    protected $fillable = [
        'access_token',
        'refresh_token',
        'expires_at',
        'cloud_service_id',
        'user_id',
    ];
    public function user()
    {
        return $this->belongsTo(User::class);
    }
    public function cloudService()
    {
        return $this->belongsTo(CloudService::class);
    }
    public function saves()
    {
        return $this->hasMany(Save::class);
    }
}
