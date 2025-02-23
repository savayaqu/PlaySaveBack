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
        for ($i = 0; $i < 200; $i++) {
            Game::query()->updateOrCreate([
                'name' => fake()->unique()->name(),
                'icon' => fake()->imageUrl(),
                'header' => fake()->imageUrl(),
                'description' => fake()->text(),
            ]);
        }
        $user = User::query()->firstOrCreate([
           'login' => 'admin',
           'nickname' => 'admin',
           'password' => 'Admin123!',
           'key' => 666666
        ]);
        for($i = 0; $i < 5; $i++) {
            $log =  fake()->unique()->word();
            User::query()->firstOrCreate([
                'login' => $log,
                'nickname' => $log,
                'password' => 'Test123!',
                'key' => 666666,
            ]);
        }
        for($i = 0; $i < 40; $i++) {
            Library::query()->firstOrCreate([
                'user_id' => $user->id,
                'game_id' => Game::query()->inRandomOrder()->first()->id,
            ]);
        }

    }
}
