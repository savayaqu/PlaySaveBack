@extends('layouts.layout')
@section('title', 'Профиль • PlaySaveBack')
@section('content')
    <div class="row justify-content-center">
        <div class="col-md-6">
            <div class="card mt-5">
                <div class="profile-container">
                    <h1>Профиль пользователя</h1>
                    <p><strong>Имя:</strong> {{ $user->nickname }}</p>
                    <p><strong>Email:</strong> {{ $user->email }}</p>
                    <p><strong>ID пользователя:</strong> {{ $user->id }}</p>
                    <img class="img-fluid" src="{{ $user->avatar }}" alt="avatar"/>
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

                    <!-- Проверка, подключен ли Google Drive -->
                    @if (!$cloudServices->contains('cloudService.name', 'Google Drive'))
                        <a href="{{ route('google-drive-auth-url') }}">Подключить Google Drive</a>
                    @endif
                </div>
            </div>
        </div>
    </div>
@endsection
