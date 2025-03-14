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
        Schema::create('saves', function (Blueprint $table) {
            $table->id();
            $table->string('file_id');
            $table->string('file_name');
            $table->string('version');
            $table->unsignedBigInteger('size');
            $table->text('description')->nullable();
            $table->text('hash');
            $table->dateTime('last_sync_at');
            $table->foreignId('user_id')->constrained();
            $table->foreignId('game_id')->nullable()->constrained('games');
            $table->foreignId('side_game_id')->nullable()->constrained('side_games', 'id');
            $table->foreignId('user_cloud_service_id')->constrained('user_cloud_service', 'id');
            $table->timestamps();
        });
    }

    /**
     * Reverse the migrations.
     */
    public function down(): void
    {
        Schema::dropIfExists('saves');
    }
};
