<!DOCTYPE html>
<html lang="ru">
<head>
    <meta charset="UTF-8">
    <meta name="viewport" content="width=device-width, initial-scale=1.0">
    <title>Вы успешно вошли</title>
    <script>
        setTimeout(() => {
            window.location.href = "{{ env('DEEPLINK') }}";
        }, 2000); // Редирект через 2 секунды
    </script>
</head>
<body>
<h2>Вы успешно вошли!</h2>
<p>Сейчас вы будете перенаправлены...</p>
</body>
</html>
