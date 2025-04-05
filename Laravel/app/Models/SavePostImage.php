<?php

namespace App\Models;

use Illuminate\Database\Eloquent\Model;
use Illuminate\Database\Eloquent\Relations\BelongsTo;

class SavePostImage extends Model
{
   protected $table = 'save_post_images';
   protected $fillable = ['save_post_id', 'save_image_id'];
   public function savePost() : BelongsTo
   {
       return $this->belongsTo(SavePost::class);
   }
   public function saveImage() : BelongsTo
   {
       return $this->belongsTo(SaveImage::class);
   }
}
