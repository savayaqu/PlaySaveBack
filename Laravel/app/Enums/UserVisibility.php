<?php

namespace App\Enums;

enum UserVisibility: int
{
    case Public = 1;
    case Private = 2;
    case FriendsOnly = 3;
    public function label(): string
    {
        return match ($this) {
            self::Public => "Public",
            self::Private => "Private",
            self::FriendsOnly => "Friends Only",
        };
    }
}
