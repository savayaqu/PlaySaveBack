<?php

namespace App\Models;

use Illuminate\Database\Eloquent\Model;
use Illuminate\Database\Eloquent\Relations\BelongsTo;
use Illuminate\Database\Eloquent\Relations\HasMany;

class SavePost extends Model
{
    protected $table = 'save_posts';
    protected $fillable = ['save_id', 'title', 'content'];
    public function relatedSave() : BelongsTo {
        return $this->belongsTo(Save::class);
    }
    public function savePostImages() : HasMany {
        return $this->hasMany(SavePostImage::class);
    }
}
