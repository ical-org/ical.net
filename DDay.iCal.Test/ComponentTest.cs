using System;
using System.Collections.Generic;
using System.Text;
using NUnit.Framework;

namespace DDay.iCal.Test
{
    [TestFixture]
    public class ComponentTest
    {
        [Test, Category("Component")]
        public void UniqueComponent1()
        {
            iCalendar iCal = new iCalendar();
            Event evt = iCal.Create<Event>();

            Assert.IsNotNull(evt.UID);
            Assert.IsNull(evt.Created); // We don't want this to be set automatically
            Assert.IsNotNull(evt.DTStamp);
        }
    }
}
