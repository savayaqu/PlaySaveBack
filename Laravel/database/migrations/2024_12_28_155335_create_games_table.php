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
            $table->string('name');
            $table->text('path_to_exe')->nullable();
            $table->text('path_to_icon')->nullable();
            $table->boolean('hidden')->default(false);
            $table->date('last_played_at')->nullable();
            $table->unsignedBigInteger('total_time')->default(0);
            $table->foreignId('user_id')->constrained();
            $table->unique(['user_id', 'name']);
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
