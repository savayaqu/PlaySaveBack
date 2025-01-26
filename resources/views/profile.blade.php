@extends('layouts.layout')
@section('title', 'Профиль • PlaySaveBack')
@section('content')
    <div>
        <div>
            <div>
                <div class="container">
                    <h1>Профиль пользователя</h1>
                    <p><strong>Имя:</strong> {{ $user->nickname }}</p>
                    <p><strong>Email:</strong> {{ $user->email }}</p>
                    <p><strong>ID пользователя:</strong> {{ $user->id }}</p>
                    <img class="img-fluid" src="{{ $user->profileImage() }}" height="100" width="100" alt="avatar"/>
                </div>
                <div>
                    <h2>Список подключенных сервисов</h2>
                    @if ($cloudServices->isNotEmpty())
                        <ul>
                            @foreach($cloudServices as $service)
                                <li>{{ $service->cloudService->name }}</li>
                            @endforeach
                        </ul>
                    @else
                        <p>Нет подключенных сервисов.</p>
                    @endif

                    @if (!$cloudServices->contains('cloudService.name', 'Google Drive'))
                        <a href="{{ route('google-drive-auth-url') }}">Подключить Google Drive</a>
                    @endif
                </div>
                <div class="container">
                    <h1 class="mb-4">Моя коллекция</h1>

                    @if($collections->isEmpty())
                        <p>Ваша коллекция пуста.</p>
                    @else
                        <div class="row">
                            @foreach($collections as $collection)
                                <div class="col-md-4">
                                    <div class="card mb-4">
                                        <img src="{{ $collection->game->image }}" class="card-img-top" alt="{{ $collection->game->name }}">
                                        <div class="card-body">
                                            <h5 class="card-title">{{ $collection->game->name }}</h5>
                                            <form action="{{ route('collections.remove') }}" method="POST">
                                                @csrf
                                                <input type="hidden" name="game_id" value="{{ $collection->game->id }}">
                                                <button type="submit" class="btn btn-danger">Удалить</button>
                                            </form>
                                        </div>
                                    </div>
                                </div>
                            @endforeach
                        </div>
                    @endif
                </div>
            </div>
        </div>
    </div>
@endsection
