<?php

namespace App\Enums;

enum PathGame: int
{
    case Pending = 1;
    case Approved = 2;
    case Rejected = 3;
}
