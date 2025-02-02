<?php

namespace Database\Seeders;

use App\Models\Developer;
use App\Models\DeveloperGame;
use App\Models\Game;
use App\Models\Genre;
use App\Models\GenreGame;
use App\Models\Platform;
use App\Models\Publisher;
use App\Models\PublisherGame;
use App\Models\Tag;
use App\Models\TagGame;
use DateTime;
use Illuminate\Database\Seeder;
use Illuminate\Support\Facades\File;
use Illuminate\Support\Facades\Log;

class SteamGameSeeder extends Seeder
{
    public function run(): void
    {
        $this->command->info("Сидер игр стима");
        $platformSteam = Platform::query()->where('name', 'Steam')->firstOrFail();
        $directory = $this->command->ask("Укажи полный путь до папки с файлами json");

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
                            $publishers = $appData['data']['publishers'] ?? null;
                            $developers = $appData['data']['developers']?? null;
                            $tags = $appData['data']['categories'] ?? null;
                            $genres = $appData['data']['genres']?? null;
                            $movies = $appData['data']['movies'] ?? null;

                            $releaseDateRaw = $appData['data']['release_date']['date'] ?? null;
                            if ($releaseDateRaw) {
                                $releaseDateObj = DateTime::createFromFormat('d M, Y', $releaseDateRaw);

                                if ($releaseDateObj) {
                                    $releaseDate = $releaseDateObj->format('Y-m-d');
                                } else {
                                   // \Log::error("Ошибка парсинга даты: '{$releaseDateRaw}'");
                                    $releaseDate = null;
                                }
                            } else {
                                $releaseDate = null;
                            }

                            $screenshots = $appData['data']['screenshots'] ?? null;
                            $aboutTheGame = $appData['data']['about_the_game'] ?? 'Unknown';
                            $minimumPcRequire = $appData['data']['pc_requirements']['minimum'] ?? null;
                            $game = Game::query()->updateOrCreate(['name' => $name],
                                [
                                    'name' => $name,
                                    'about_the_game' => $aboutTheGame,
                                    'release_date' => $releaseDate,
                                    'pc_requirements' => $minimumPcRequire,
                                    'screenshots' => $screenshots ? json_encode($screenshots) : null,
                                    'movies' => $movies ? json_encode($movies) : null,
                                    'header_image' => "https://cdn.cloudflare.steamstatic.com/steam/apps/$gameSteamId/library_hero.jpg",
                                    'collection_image' => "https://cdn.cloudflare.steamstatic.com/steam/apps/$gameSteamId/library_600x900.jpg",
                                    'platform_id' => $platformSteam->id,
                                    'platform_game_id' => $gameSteamId,
                                ]
                            );
                            foreach ($genres as $genre) {
                                $genre = Genre::query()->updateOrCreate(['name' => $genre['description']]);
                                GenreGame::query()->updateOrCreate(['genre_id' => $genre->id, 'game_id' => $game->id],);
                            }
                            foreach ($publishers as $publisher) {
                                $publisher = Publisher::query()->updateOrCreate(['name' => $publisher]);
                                PublisherGame::query()->updateOrCreate(['publisher_id' => $publisher->id, 'game_id' => $game->id],);
                            }
                            foreach ($developers as $developer) {
                                $developer = Developer::query()->updateOrCreate(['name' => $developer]);
                                DeveloperGame::query()->updateOrCreate(['developer_id' => $developer->id, 'game_id' => $game->id],);
                            }
                            foreach ($tags as $tag) {
                                $tag = Tag::query()->updateOrCreate(['name' => $tag['description']]);
                                TagGame::query()->updateOrCreate(['tag_id' => $tag->id, 'game_id' => $game->id],);
                            }
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
