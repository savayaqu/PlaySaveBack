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
        Schema::create('games', function (Blueprint $table) {
            $table->id();
            $table->string('name')->unique();
            $table->text('about_the_game')->nullable();
            $table->date('release_date')->nullable();
            $table->text('pc_requirements')->nullable();
            $table->text('screenshots')->nullable();
            $table->text('movies')->nullable();
            $table->string('header_image')->nullable();
            $table->string('collection_image')->nullable();
            $table->foreignId('platform_id')->nullable()->constrained();
            $table->bigInteger('platform_game_id');
            $table->unique(['platform_id', 'platform_game_id']);
            $table->timestamps();
        });
    }

    /**
     * Reverse the migrations.
     */
    public function down(): void
    {
        Schema::dropIfExists('games');
    }
};
