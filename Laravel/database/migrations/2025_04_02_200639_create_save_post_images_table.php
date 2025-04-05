<?php

use Illuminate\Database\Migrations\Migration;
use Illuminate\Database\Schema\Blueprint;
use Illuminate\Support\Facades\Schema;

return new class extends Migration
{
    /**
     * Run the migrations.
     */
    public function up(): void
    {
        Schema::create('save_post_images', function (Blueprint $table) {
            $table->id();
            $table->foreignId('save_post_id')->constrained('save_posts')->onDelete('cascade');
            $table->foreignId('save_image_id')->constrained('save_images')->onDelete('cascade');
            $table->timestamps();
        });
    }

    /**
     * Reverse the migrations.
     */
    public function down(): void
    {
        Schema::dropIfExists('save_post_images');
    }
};
