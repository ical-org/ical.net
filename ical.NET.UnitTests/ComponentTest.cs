using Ical.Net;
using NUnit.Framework;

namespace ical.NET.UnitTests
{
    [TestFixture]
    public class ComponentTest
    {
        [Test, Category("Component")]
        public void UniqueComponent1()
        {
            var iCal = new iCalendar();
            var evt = iCal.Create<Event>();

            Assert.IsNotNull(evt.UID);
            Assert.IsNull(evt.Created); // We don't want this to be set automatically
            Assert.IsNotNull(evt.DTStamp);
        }
    }
}
