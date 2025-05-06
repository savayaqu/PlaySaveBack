using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace PSB.Api.Response.GoogleDrive.Save
{
    public class ShareSaveResponse
    {
        [JsonPropertyName("url")] public required string Url { get; set; }
    }
}
