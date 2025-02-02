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
        Schema::create('user_collections', function (Blueprint $table) {
            $table->id();
            $table->foreignId('user_id')->constrained('users');
            $table->foreignId('game_id')->nullable()->constrained('games');
            $table->foreignId('custom_game_id')->nullable()->constrained('custom_games');
            $table->foreignId('collection_id')->constrained('collections');
            $table->text('path_to_game')->nullable();
            $table->unique(['user_id', 'collection_id', 'game_id']);
            $table->unique(['user_id', 'collection_id', 'custom_game_id']);
            $table->timestamps();
        });
    }

    /**
     * Reverse the migrations.
     */
    public function down(): void
    {
        Schema::dropIfExists('user_collections');
    }
};
