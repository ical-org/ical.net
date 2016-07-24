using System;
using System.Collections.Generic;
using System.Linq;
using Ical.Net;
using Ical.Net.DataTypes;
using Ical.Net.Interfaces.DataTypes;
using Ical.Net.Serialization;
using Ical.Net.Serialization.iCalendar.Serializers;

namespace NugetTester
{
    class Program
    {
        static void Main(string[] args)
        {
            var url = new Uri("https://raw.githubusercontent.com/rianjs/ical.net/master/ical.NET.UnitTests/Calendars/Recurrence/Daily1.ics");
            var webClient = Downloaders.LoadFromUri(url).First();
            var asyncHttpClient = Downloaders.LoadFromUriAsync(url).Result.First();
            var syncHttpClient = Downloaders.LoadFromUriSync(url).First();

            Console.WriteLine(webClient.Equals(asyncHttpClient) && webClient.Equals(syncHttpClient));
            Console.ReadLine();
        }
    }
}
