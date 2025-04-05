using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace PSB.Api.Response.GoogleDrive
{
    public class FileIdResponse
    {
        [JsonPropertyName("id")] public required string Id { get; set; }
    }
}
