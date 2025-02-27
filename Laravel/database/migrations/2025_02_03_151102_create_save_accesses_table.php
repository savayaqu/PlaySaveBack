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
        Schema::create('save_accesses', function (Blueprint $table) {
            $table->id();
            $table->foreignId('user_id')->constrained('users');
            $table->foreignId('save_id')->constrained('saves');
            $table->enum('access_type', ['one_time', 'permanent'])->default('one_time');
            $table->timestamp('expires_at')->nullable();
            $table->string('token')->nullable()->unique();
            $table->boolean('is_post')->default(false);
            $table->timestamps();
        });
    }

    /**
     * Reverse the migrations.
     */
    public function down(): void
    {
        Schema::dropIfExists('save_accesses');
    }
};
