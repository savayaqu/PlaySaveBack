<?php

namespace Database\Seeders;

use App\Models\CloudService;
use Illuminate\Database\Seeder;

class CloudSeeder extends Seeder
{
    public function run(): void
    {
        CloudService::query()->firstOrCreate([
            'name' => 'Google Drive',
            'description' => 'Google Drive API',
            'icon' => asset('assets/images/Google_Drive_icon.svg'),
        ]);
    }
}
