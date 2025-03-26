using System;

namespace PSB.Api
{
    public static class URLs
    {
        public static Uri BASE_URL => new Uri("https://savayaqu.duckdns.org/playsaveback");
        public static Uri API_URL => new(BASE_URL + "/api");
    }
}
