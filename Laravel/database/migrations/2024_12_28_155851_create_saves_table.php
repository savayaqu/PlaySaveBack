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
            $table->string('file_name');
            $table->text('file_path');
            $table->unsignedBigInteger('size');
            $table->text('description')->nullable();
            $table->foreignId('user_id')->constrained();
            $table->foreignId('game_id')->constrained();
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
