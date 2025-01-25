<!doctype html>
<html lang="ru">
<head>
    <meta charset="UTF-8">
    <meta name="viewport"
          content="width=device-width, user-scalable=no, initial-scale=1.0, maximum-scale=2.0, minimum-scale=0.5">
    <meta http-equiv="X-UA-Compatible" content="ie=edge">
    <link rel="stylesheet" href="{{ asset('assets/style.css') }}"/>
    <link rel="stylesheet" href="{{ asset('assets/bootstrap.min.css') }}"/>
    <title>@yield('title', 'PlaySaveBack')</title>
</head>
<body>
<div class="wrapper">
    <header class="bg-dark text-white py-3">
        <div class="container d-flex justify-content-between align-items-center">
            <a class="btn btn-outline-light mr-2" href="{{route('home')}}">PlaySaveBack</a>
            @if (Route::has('login'))
                <nav class="d-flex align-items-center gap-2">
                    @auth
                        <a href="{{ route('logout') }}" class="btn btn-outline-light mr-2">Выход</a>
                    @else
                        <a href="{{ route('login') }}" class="btn btn-outline-light mr-2">Войти</a>
                        @if (Route::has('signup'))
                            <a href="{{ route('signup') }}" class="btn btn-outline-light">Регистрация</a>
                        @endif
                    @endauth
                    <a href="{{route('games.index')}}" class="btn btn-outline-light mr-2">Игры</a>
                </nav>
            @endif
        </div>
    </header>

    <main class="content container my-5">
        @yield('content')
    </main>

    <footer class="bg-dark text-white py-3">
        <div class="container text-center">
            <p class="mb-0">&copy; {{date('Y')}} PlaySaveBack. Все права защищены.</p>
        </div>
    </footer>
</div>
</body>
</html>
