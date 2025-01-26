@extends('layouts.layout')
@section('title', 'Вход • Bububu')
@section('content')
    <div class="row justify-content-center">
        <div class="col-md-6">
            <div class="card mt-5">
                <div class="card-header bg-primary text-white">
                    <h2 class="mb-0">Вход</h2>
                </div>
                <div class="card-body">
                    @error('error')
                    <p class="alert alert-danger">{{ $message }}</p>
                    @enderror
                    <form action="{{ route('login') }}" method="POST" enctype="application/x-www-form-urlencoded">
                        @csrf
                        <div class="form-group mt-3">
                            <label for="email">Электронная почта</label>
                            <input id="email" type="email" name="email" class="form-control" placeholder="Введите почту" required>
                        </div>
                        <div class="form-group mt-3">
                            <label for="password">Пароль</label>
                            <input id="password" type="password" name="password" class="form-control" placeholder="Введите пароль" required>
                        </div>
                        <button type="submit" class="btn btn-primary btn-block mt-3">Войти</button>
                    </form>
                </div>
            </div>
        </div>
    </div>
@endsection
