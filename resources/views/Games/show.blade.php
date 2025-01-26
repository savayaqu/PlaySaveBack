@extends('layouts.layout')
@section('title', 'Детальный просмотр • PlaySaveBack')
@section('content')
    <div class="container">
        <h1 class="mb-4">{{ $game->name }}</h1>

        @if($gameDetails)
            <div class="row">
                <div class="col-md-6">
                    <img src="{{ $gameDetails['header_image'] }}" class="img-fluid" alt="{{ $game->name }}">
                </div>
                @auth
                    @if($isInCollection)

                        <div class="col-md-5">
                            <form action="{{ route('collections.remove') }}" method="POST">
                                @csrf
                                <input type="hidden" name="game_id" value="{{ $game->id }}">
                                <button type="submit" class="btn btn-danger">Удалить из коллекции</button>
                            </form>
                            <a href="{{ route('collections.index') }}" class="btn btn-primary">Перейти в коллекцию</a>
                        </div>
                    @else
                        <div class="col-md-5">
                            <form action="{{ route('collections.add') }}" method="POST">
                                @csrf
                                <input type="hidden" name="game_id" value="{{ $game->id }}">
                                <button type="submit" class="btn btn-success">Добавить в коллекцию</button>
                            </form>
                        </div>
                    @endif
                @endauth
                <div class="col-md-6">
                    <h3>Разработчик: {{ implode(', ', $gameDetails['developers']) }}</h3>
                    <h4>Издатель: {{ implode(', ', $gameDetails['publishers']) }}</h4>
                    <p><strong>Краткое описание:</strong></p>
                    <p>{!! $gameDetails['short_description'] !!}</p>
                    <p><strong>Описание:</strong></p>
                    <p>{!! $gameDetails['detailed_description'] !!}</p>
                    <h4>Жанры:</h4>
                    <ul>
                        @foreach($gameDetails['genres'] as $genre)
                            <li>{{ $genre['description'] }}</li>
                        @endforeach
                    </ul>
                    <h4>Категории:</h4>
                    <ul>
                        @foreach($gameDetails['categories'] as $category)
                            <li>{{ $category['description'] }}</li>
                        @endforeach
                    </ul>
                    <h4>Платформы:</h4>
                    <ul>
                        @foreach($gameDetails['platforms'] as $platform => $supported)
                            @if($supported)
                                <li>{{ ucfirst($platform) }}</li>
                            @endif
                        @endforeach
                    </ul>
                </div>
            </div>

            <h4>Скриншоты:</h4>
            <div class="row">
                @foreach($gameDetails['screenshots'] as $screenshot)
                    <div class="col-md-4">
                        <img src="{{ $screenshot['path_full'] }}" class="img-fluid" alt="Screenshot">
                    </div>
                @endforeach
            </div>

            <p><a href="{{ $gameDetails['website'] }}" target="_blank" class="btn btn-primary">Перейти на страницу игры в Steam</a></p>
        @else
            <p>Детальная информация о игре не доступна.</p>
        @endif
    </div>
@endsection
