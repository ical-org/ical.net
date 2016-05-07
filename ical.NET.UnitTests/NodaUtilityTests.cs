using System;
using System.Diagnostics;
using NodaTime;
using NUnit.Framework;

namespace DDay.iCal.Test
{
    public class NodaUtilityTests
    {
        [Test]
        public void ToTimeZoneTests()
        {
            var now = DateTime.Parse("2016-04-10 10:37:00");
            var actual = NodaUtility.ToTimeZone(now, "America/New_York", "America/Los_Angeles", TimeSpan.FromHours(-4));
            var la = DateTime.Parse("2016-04-10 07:37:00");
            var expected = new ZonedDateTime(LocalDateTime.FromDateTime(la), NodaUtility.GetTimeZone("America/Los_Angeles"), Offset.FromHours(-7));
            Assert.AreEqual(expected, actual);
            Assert.AreEqual(expected.ToDateTimeUtc(), actual.ToDateTimeUtc());

            var totalTime = TimeSpan.Zero;
            for (var i = 0; i < 100; i++)
            {
                var timer = Stopwatch.StartNew();
                actual = NodaUtility.ToTimeZone(now, "America/New_York", "America/Los_Angeles", TimeSpan.FromHours(-4));
                expected = new ZonedDateTime(LocalDateTime.FromDateTime(la), NodaUtility.GetTimeZone("America/Los_Angeles"), Offset.FromHours(-7));
                timer.Stop();
                Assert.AreEqual(expected, actual);
                Assert.AreEqual(expected.ToDateTimeUtc(), actual.ToDateTimeUtc());
                totalTime = totalTime.Add(timer.Elapsed);
            }
            Assert.IsTrue(totalTime < TimeSpan.FromMilliseconds(2));
        }
    }
}
