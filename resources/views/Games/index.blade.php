@extends('layouts.layout')
@section('title', 'Игры • PlaySaveBack')
@section('content')
    <div class="container">
        <h1 class="mb-4">Список игр</h1>

        <form method="GET" action="{{ route('games.index') }}" class="mb-4">
            <div class="input-group">
                <input
                    type="text"
                    name="search"
                    class="form-control"
                    placeholder="Поиск игр по названию..."
                    value="{{ request('search') }}">
                <button type="submit" class="btn btn-primary">Поиск</button>
            </div>
        </form>

        <div class="row">
            @foreach($games as $game)
                <div class="col-md-4 mb-4">
                    <div class="card h-100">
                        <img
                            src="{{ "https://shared.akamai.steamstatic.com/store_item_assets/steam/apps/$game->steam_id/capsule_231x87.jpg" }}"
                            class="card-img-top"
                            alt="{{ $game->name }}"
                        >
                        <div class="card-body">
                            <h5 class="card-title">{{ $game->name }}</h5>
                            @if($game->publisher)
                                <p class="card-text">Издатель: {{ $game->publisher->name }}</p>
                            @endif
                            <a href="{{ route('games.show', $game->steam_id) }}" class="btn btn-primary">Подробнее</a>
                        </div>
                    </div>
                </div>
            @endforeach
        </div>

        <!-- Пагинация -->
        <div>
            {{ $games->links() }}
        </div>
    </div>
@endsection
