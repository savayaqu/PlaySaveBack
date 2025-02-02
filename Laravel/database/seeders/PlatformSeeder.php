<?php

namespace Database\Seeders;

use App\Models\Platform;
use Illuminate\Database\Console\Seeds\WithoutModelEvents;
use Illuminate\Database\Seeder;

class PlatformSeeder extends Seeder
{
    /**
     * Run the database seeds.
     */
    public function run(): void
    {
        Platform::query()->create(['name' => 'Steam']);
        Platform::query()->create(['name' => 'EGS']);
        Platform::query()->create(['name' => 'GOG']);
        Platform::query()->create(['name' => 'Itch.io']);
    }
}
