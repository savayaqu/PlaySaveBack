<?php

namespace App\Http\Controllers\Api;

use App\Http\Controllers\Controller;
use Illuminate\Http\Request;

class SaveController extends Controller
{
    public function uploadToServer(Request $request)
    {
        $file = $request->file('file');

    }
}
