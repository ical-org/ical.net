using System;
using System.Collections.Generic;
using System.Text;
using System.ComponentModel;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace DDay.iCal.Test
{
    [TestClass]
    [DeploymentItem("Calendars", "Calendars")]
    public class ComponentTest
    {
        [TestMethod, Category("Component")]
        public void UniqueComponent1()
        {
            IICalendar iCal = new iCalendar();
            Event evt = iCal.Create<Event>();

            Assert.IsNotNull(evt.UID);
            Assert.IsNotNull(evt.Created);
            Assert.IsNotNull(evt.DTStamp);
        }
    }
}
