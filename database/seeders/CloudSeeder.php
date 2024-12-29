<?php

namespace Database\Seeders;

use App\Models\CloudService;
use Illuminate\Database\Seeder;

class CloudSeeder extends Seeder
{
    /**
     * Run the database seeds.
     */
    public function run(): void
    {
        CloudService::query()->firstOrCreate([
           'name' => 'local',
           'api_endpoint' => env('APP_URL'),
        ]);
        CloudService::query()->firstOrCreate([
            'name' => 'Google Drive',
            'api_endpoint' => 'https://www.googleapis.com/drive/v3',
            'description' => 'Google Drive API',
        ]);
    }
}
