<?php

namespace App\Models;

use App\Enums\CloudStatus;
use Illuminate\Database\Eloquent\Model;
use Illuminate\Database\Eloquent\Relations\BelongsTo;
use Illuminate\Database\Eloquent\Relations\HasMany;

class UserCloudService extends Model
{
    protected $table = 'user_cloud_services';
    protected $fillable = [
        'access_token',
        'refresh_token',
        'expires_at',
        'cloud_service_id',
        'user_id',
        'status',
        'external_user_id',
    ];
    protected $casts = [
      'expires_at' => 'datetime',
    ];
    protected $attributes = [
        'status' => CloudStatus::Inactive,
    ];
    public function user(): BelongsTo
    {
        return $this->belongsTo(User::class);
    }
    public function cloudService(): BelongsTo
    {
        return $this->belongsTo(CloudService::class);
    }
    public function saves(): HasMany
    {
        return $this->hasMany(Save::class);
    }
}
