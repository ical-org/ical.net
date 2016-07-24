using System;
using System.IO;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using Ical.Net;

namespace NugetTester
{
    internal class Downloaders
    {
        //WebClient
        public static CalendarCollection LoadFromUri(Uri uri)
        {
            using (var client = new WebClient())
            {
                var rawContent = client.DownloadString(uri);
                return Calendar.LoadFromStream(new StringReader(rawContent)) as CalendarCollection;
            }
        }

        //HttpClient async
        public static async Task<CalendarCollection> LoadFromUriAsync(Uri uri)
        {
            using (var client = new HttpClient())
            {
                using (var response = await client.GetAsync(uri))
                {
                    response.EnsureSuccessStatusCode();
                    var result = await response.Content.ReadAsStringAsync();
                    return Calendar.LoadFromStream(new StringReader(result)) as CalendarCollection;
                }
            }
        }

        //HttpClient blocking
        public static CalendarCollection LoadFromUriSync(Uri uri)
        {
            using (var client = new HttpClient())
            {
                using (var response = client.GetAsync(uri).Result)
                {
                    response.EnsureSuccessStatusCode();
                    var result = response.Content.ReadAsStringAsync().Result;
                    return Calendar.LoadFromStream(new StringReader(result)) as CalendarCollection;
                }
            }
        }
    }
}
