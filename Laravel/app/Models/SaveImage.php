<?php

namespace App\Models;

use Illuminate\Database\Eloquent\Model;
use Illuminate\Database\Eloquent\Relations\HasMany;

class SaveImage extends Model
{
    protected $table = 'save_images';
    protected $fillable = ['path'];
    public function savePostImages() : HasMany {
        return $this->hasMany(SavePostImage::class);
    }
}
