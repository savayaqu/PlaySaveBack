<?php

namespace Database\Seeders;

use App\Enums\GamePlatform;
use App\Models\Game;
use Illuminate\Database\Seeder;
use Illuminate\Support\Facades\DB;
use Illuminate\Support\Facades\File;
use Illuminate\Support\Facades\Log;

class GameHydraSeeder extends Seeder
{
    public function run(): void
    {
        $file = database_path("seeders/steam-games.json");

        $this->command->info("Начало обработки файла...");

        // Ключевые слова для исключения
        $excludeKeywords = ["DLC", "Expansion", "Pack", "Add-on", "Season Pass", "Soundtrack","Soundtracks", "Player Profiles", "Film", "Demo"];
        $excludePattern = '/\b(' . implode('|', array_map('preg_quote', $excludeKeywords)) . ')\b/i';

        try {
            // Чтение содержимого файла
            $jsonContent = file_get_contents($file);
            $data = json_decode($jsonContent, true);

            if (json_last_error() !== JSON_ERROR_NONE) {
                $this->command->error("Ошибка при декодировании JSON: " . json_last_error_msg());
                return;
            }

            $apps = $data;
            $totalApps = count($apps);
            $this->command->info("Всего записей для обработки: {$totalApps}");
            $this->command->getOutput()->progressStart($totalApps);

            // Обрабатываем данные порциями (chunks)
            $chunkSize = 1000; // Размер порции
            $chunks = array_chunk($apps, $chunkSize);

            foreach ($chunks as $chunk) {
                DB::beginTransaction();
                try {
                    foreach ($chunk as $appData) {
                        $gameSteamId = $appData['id'];
                        $gameName = $appData['name'];

                        // Пропускаем записи с пустым именем
                        if (empty($gameName)) {
                            continue;
                        }

                        // Проверяем, содержит ли название ключевые слова для исключения
                        if (preg_match($excludePattern, $gameName)) {
                            continue;
                        }

                        // Проверяем, существует ли запись с таким game_code и platform
                        $existingGame = Game::where('game_code', $gameSteamId)
                            ->where('platform', 1)
                            ->first();

                        // Если запись не существует, создаем новую
                        if (!$existingGame) {
                            // Проверяем, существует ли запись с таким именем
                            $existingName = Game::where('name', $gameName)->exists();

                            if (!$existingName) {
                                Game::create([
                                    'name' => $gameName,
                                    'game_code' => $gameSteamId,
                                    'platform' => GamePlatform::Steam,
                                ]);
                            }
                        }
                    }

                    DB::commit();
                } catch (\Exception $e) {
                    DB::rollBack();
                    Log::error("Ошибка при обработке порции данных: " . $e->getMessage());
                    $this->command->error("Ошибка при обработке порции данных: " . $e->getMessage());
                }

                $this->command->getOutput()->progressAdvance(count($chunk));
            }

            $this->command->getOutput()->progressFinish();
            $this->command->info("Обработка завершена!");
        } catch (\Exception $e) {
            $this->command->error("Произошла ошибка: " . $e->getMessage());
            Log::error("Ошибка в GameSeeder: " . $e->getMessage());
        }
    }
}
