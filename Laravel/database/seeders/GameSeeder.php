<?php

namespace Database\Seeders;

use App\Models\Game;
use Illuminate\Database\Console\Seeds\WithoutModelEvents;
use Illuminate\Database\Seeder;

class GameSeeder extends Seeder
{
    /**
     * Run the database seeds.
     */
    public function run(): void
    {
        $file = $this->command->ask("Укажи полный путь до файла json");

        if (empty($file)) {
            $this->command->info("Путь к файлу не указан.");
            return;
        }

        if (!file_exists($file)) {
            $this->command->info("Файл {$file} не существует.");
            return;
        }

        $fileExtension = pathinfo($file, PATHINFO_EXTENSION);
        if ($fileExtension !== 'json') {
            $this->command->info("Файл должен быть в формате JSON.");
            return;
        }

        $this->command->info("Начало обработки файла...");

        try {
            // Чтение содержимого файла
            $jsonContent = file_get_contents($file);
            $data = json_decode($jsonContent, true);

            if (json_last_error() !== JSON_ERROR_NONE) {
                $this->command->error("Ошибка при декодировании JSON: " . json_last_error_msg());
                return;
            }

            $this->command->getOutput()->progressStart(count($data));

            foreach ($data as $appData) {
                $gameSteamId = $appData['id'];
                $gameName = $appData['name'];
                $gameIcon = $appData['clientIcon'];

                // Проверяем, существует ли запись с таким именем
                if (!Game::where('name', $gameName)->exists()) {
                    Game::query()->updateOrCreate(
                        ['steam_id' => $gameSteamId],
                        [
                            'name' => $gameName,
                            'icon_hash' => $gameIcon,
                        ]
                    );
                }

                $this->command->getOutput()->progressAdvance();
            }

            $this->command->getOutput()->progressFinish();
            $this->command->info("Обработка завершена!");
        } catch (\Exception $e) {
            $this->command->error("Произошла ошибка: " . $e->getMessage());
        }
    }
}
