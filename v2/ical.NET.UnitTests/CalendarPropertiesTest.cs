using System;
using System.Linq;
using Ical.Net.Interfaces.Serialization;
using Ical.Net.Serialization;
using Ical.Net.Serialization.iCalendar.Factory;
using NUnit.Framework;

namespace Ical.Net.UnitTests
{
    [TestFixture]
    public class CalendarPropertiesTest
    {
        [Test]
        public void AddPropertyShouldNotIncludePropertyNameInValue()
        {
            var propName = "X-WR-CALNAME";
            var propValue = "Testname";

            var iCal = new Calendar();
            iCal.AddProperty(propName, propValue);

            var ctx = new SerializationContext();
            var factory = new SerializerFactory();

            // Get a serializer for our object
            var serializer = factory.Build(iCal.GetType(), ctx) as IStringSerializer;

            var result = serializer.SerializeToString(iCal);

            Console.WriteLine(result);
            var lines = result.Split(new [] { "\r\n" }, StringSplitOptions.None);
            var propLine = lines.FirstOrDefault(x => x.StartsWith("X-WR-CALNAME:"));
            Assert.AreEqual($"{propName}:{propValue}", propLine);
        }
    }
}
