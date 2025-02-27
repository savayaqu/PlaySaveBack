<?php

namespace App\Enums;

enum GamePlatform: int
{
    case Steam = 1;
    case EGS = 2;
    case GOG = 3;
    case ItchIO = 4;
}
