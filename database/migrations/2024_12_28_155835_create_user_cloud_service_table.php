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
        Schema::create('user_cloud_service', function (Blueprint $table) {
            $table->id();
            $table->text('access_token')->unique();
            $table->text('refresh_token')->nullable()->unique();
            $table->timestamp('expires_at')->nullable();
            $table->foreignId('cloud_service_id')->constrained();
            $table->foreignId('user_id')->constrained();
            $table->timestamps();
        });
    }

    /**
     * Reverse the migrations.
     */
    public function down(): void
    {
        Schema::dropIfExists('user_cloud_service');
    }
};
