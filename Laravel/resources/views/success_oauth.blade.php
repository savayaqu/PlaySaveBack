<!DOCTYPE html>
<html lang="ru">
<head>
    <meta charset="UTF-8">
    <meta name="viewport" content="width=device-width, initial-scale=1.0">
    <title>Вход выполнен</title>
    <style>
        body {
            font-family: -apple-system, BlinkMacSystemFont, 'Segoe UI', Roboto, Helvetica, Arial, sans-serif;
            line-height: 1.6;
            color: #333;
            max-width: 600px;
            margin: 0 auto;
            padding: 2rem;
            text-align: center;
        }
        .container {
            background: #f9f9f9;
            border-radius: 8px;
            padding: 2rem;
            box-shadow: 0 2px 4px rgba(0,0,0,0.05);
        }
        h2 {
            color: #2c3e50;
            margin-top: 0;
        }
        p {
            margin-bottom: 0;
            color: #7f8c8d;
        }
        .loader {
            margin: 1.5rem auto;
            width: 50px;
            height: 4px;
            background: #ecf0f1;
            position: relative;
            overflow: hidden;
            border-radius: 2px;
        }
        .loader:after {
            content: '';
            position: absolute;
            left: 0;
            width: 50%;
            height: 100%;
            background: #3498db;
            animation: loading 2s ease-in-out infinite;
            border-radius: 2px;
        }
        @keyframes loading {
            0% { left: -50%; }
            100% { left: 150%; }
        }
    </style>
    <script>
        setTimeout(() => {
            window.location.href = "{{ env('DEEPLINK') }}";
        }, 2000);
    </script>
</head>
<body>
<div class="container">
    <h2>Вход выполнен успешно</h2>
    <div class="loader"></div>
    <p>Автоматическое перенаправление...</p>
</div>
</body>
</html>
