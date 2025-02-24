<?php

namespace App\Console\Commands;

use Illuminate\Console\Command;
use Illuminate\Support\Facades\Http;
use Illuminate\Support\Facades\Log;
use Illuminate\Support\Facades\Storage;

class FetchSteamApps extends Command
{
    /**
     * The name and signature of the console command.
     *
     * @var string
     */
    protected $signature = 'steam:fetch-apps';

    /**
     * The console command description.
     *
     * @var string
     */
    protected $description = 'Запрашивает список игр Steam и сохраняет его в JSON-файл';

    /**
     * Execute the console command.
     */
    public function handle()
    {
        $url = 'https://api.steampowered.com/ISteamApps/GetAppList/v2/';

        $response = Http::get($url);

        if($response->successful()) {
            $data = $response->json();

            $fileName = 'steam_apps/AllGames_at_'.now()->format('Y-m-d_H-i-s').'.json';

            Storage::disk('local')->put($fileName, json_encode($data, JSON_PRETTY_PRINT | JSON_UNESCAPED_UNICODE));
            // Логируем успешное выполнение
            Log::channel('steam')->info("✅ Данные успешно сохранены: {$fileName}");
        }
        else {
            // Логируем ошибку запроса
            Log::channel('steam')->error("❌ Ошибка запроса: " . $response->status());
        }
    }
}
