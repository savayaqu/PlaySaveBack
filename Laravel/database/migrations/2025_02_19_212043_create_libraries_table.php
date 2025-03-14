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
        Schema::create('libraries', function (Blueprint $table) {
            $table->id();
            $table->foreignId('user_id')->constrained('users');
            $table->foreignId('game_id')->nullable()->constrained('games');
            $table->foreignId('side_game_id')->nullable()->constrained('side_games');
            $table->unique(['user_id', 'game_id']);
            $table->unique(['user_id', 'side_game_id']);
            $table->dateTime('last_played_at')->nullable();
            $table->unsignedInteger('time_played')->nullable();
            $table->boolean('is_favorite');
            $table->timestamps();
        });
    }

    /**
     * Reverse the migrations.
     */
    public function down(): void
    {
        Schema::dropIfExists('libraries');
    }
};
