<?php

namespace Database\Seeders;

use App\Models\Game;
use App\Models\Publisher;
use Illuminate\Database\Seeder;
use Illuminate\Support\Facades\File;

class GamePublisherSeeder extends Seeder
{
    public function run(): void
    {
        $directory = "C:\\OSPanel\\domains\\collection_diplom\\docs\\STEAM\\alllgamesparced\\steam_data";
        if (!is_dir($directory)) {
            $this->command->error("Директория {$directory} не найдена.");
            return;
        }
        $this->command->info("Ищем файлы...");
        // Получаем список файлов
        $files = array_diff(scandir($directory), ['.', '..']);
        $files = array_values($files); // Чтобы индексы начали с 0
        if (empty($files)) {
            $this->command->info("Директория {$directory} не содержит файлов.");
            return;
        }
        $this->command->info("Начало обработки файлов...");
        // Берем только первые 1000 файлов
        $files = array_slice($files, 0, 260000);
        // Создаем прогресс-бар
        $this->command->getOutput()->progressStart(count($files));
        foreach ($files as $file) {
            // Используем pathinfo для получения расширения
            $fileExtension = pathinfo($file, PATHINFO_EXTENSION);
            // Проверяем, что это json файл
            if ($fileExtension === 'json') {
                $content = File::get($directory . '\\' . $file); // Указываем полный путь к файлу
                try {
                    $data = json_decode($content, true);
                    foreach ($data as $appId => $appData) {
                        if (isset($appData['success']) && $appData['success'] &&
                            isset($appData['data']['type']) && $appData['data']['type'] === 'game') {
                            $name = $appData['data']['name'] ?? 'Unknown';
                            $gameSteamId = $appData['data']['steam_appid'] ?? '0';
                            $publisherName = $appData['data']['publishers'][0] ?? 'Unknown';
                            // Сохраняем или обновляем издателя
                            $publisher = Publisher::updateOrCreate(
                                ['name' => $publisherName]
                            );
                            // Сохраняем или обновляем игру
                            Game::updateOrCreate(
                                [
                                    'steam_id' => $gameSteamId,
                                    'name' => $name,
                                    'publisher_id' => $publisher->id,
                                    'image' => "https://cdn.cloudflare.steamstatic.com/steam/apps/$gameSteamId/library_600x900.jpg"
                                ]
                            );
                        }
                    }
                } catch (\Exception $e) {
                    //$this->command->error("Ошибка обработки файла {$file}: {$e->getMessage()}");
                }
            }
            // Обновляем прогресс-бар
            $this->command->getOutput()->progressAdvance();
        }
        // Завершаем прогресс-бар
        $this->command->getOutput()->progressFinish();
        $this->command->info("Обработка файлов завершена!");
    }
}
