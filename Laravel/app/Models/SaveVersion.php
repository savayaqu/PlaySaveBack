<?php

namespace App\Models;

use Illuminate\Database\Eloquent\Model;

class SaveVersion extends Model
{
    protected $fillable = [
        'save_id',
        'version',
        'file_path',
    ];
    public function saveRelation()
    {
        return $this->belongsTo(Save::class);
    }
}
