﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace PSB.Api.Response
{
    public class ConnectionServiceResponse
    {
        [JsonPropertyName("success")] public bool Success { get; set; }
        [JsonPropertyName("url")] public required string Url { get; set; }

    }
}
