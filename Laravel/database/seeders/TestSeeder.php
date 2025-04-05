<?php

namespace Database\Seeders;

use App\Models\Game;
use App\Models\Library;
use App\Models\User;
use Illuminate\Database\Console\Seeds\WithoutModelEvents;
use Illuminate\Database\Seeder;

class TestSeeder extends Seeder
{
    /**
     * Run the database seeds.
     */
    public function run(): void
    {
        if(User::query()->count() <= 2) {
             $user = User::query()->firstOrCreate([
                'login' => 'admin',
                'nickname' => 'admin',
                'password' => 'Admin123!',
                'key' => 666666
            ]);
            User::query()->firstOrCreate([
                'login' => 'test',
                'nickname' => 'test',
                'password' => 'Test123!',
                'key' => 666666
            ]);
        }
    }
}
