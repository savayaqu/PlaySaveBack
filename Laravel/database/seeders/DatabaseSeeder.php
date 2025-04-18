<?php

namespace Database\Seeders;

use App\Models\Game;
use App\Models\User;
// use Illuminate\Database\Console\Seeds\WithoutModelEvents;
use Illuminate\Database\Seeder;

class DatabaseSeeder extends Seeder
{
    /**
     * Seed the application's database.
     */
    public function run(): void
    {
        //User::factory(10)->create();
        //User::factory()->create([
        //    'name' => 'Test User',
        //    'email' => 'test@example.com',
        //]);
        $this->call(CloudSeeder::class);
        //$this->call(GameSeeder::class);
        $this->call(GameHydraSeeder::class);
        $this->call(TestSeeder::class);
    }
}
