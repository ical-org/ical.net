using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using Ical.Net.CalendarComponents;
using Ical.Net.DataTypes;
using Ical.Net.Serialization;
using Ical.Net.Serialization.DataTypes;
using NUnit.Framework;

namespace Ical.Net.CoreUnitTests
{
    [TestFixture]
    public class QuotedPrintableDeserializationTests
    {

        [Test, Category("Deserialization")]
        public void QuotedPrintable_Decription()
        {
            var iCal = SimpleDeserializer.Default.Deserialize(new StringReader(IcsFiles.QuotedPrintableContent)).Cast<Calendar>().Single();
            Assert.AreEqual(1, iCal.Events.Count);

            var evt = iCal.Events.First();

            StringAssert.StartsWith("\r\n-- Do not delete or change any of the following text. --", evt.Description);
        }

        [Test, Category("Deserialization")]
        public void QuotedPrintable_Location()
        {
            var iCal = SimpleDeserializer.Default.Deserialize(new StringReader(IcsFiles.QuotedPrintableContent)).Cast<Calendar>().Single();
            Assert.AreEqual(1, iCal.Events.Count);

            var evt = iCal.Events.First();

            Assert.AreEqual("B2/F2/Caribbean", evt.Location);
        }

        [Test, Category("Deserialization")]
        public void QuotedPrintable_Alarms()
        {
            var iCal = SimpleDeserializer.Default.Deserialize(new StringReader(IcsFiles.QuotedPrintableContent)).Cast<Calendar>().Single();
            Assert.AreEqual(1, iCal.Events.Count);

            var evt = iCal.Events.First();

            Assert.AreEqual(1, evt.Alarms.Count());
        }

        [Test, Category("Deserialization")]
        public void QuotedPrintable_UID()
        {
            var iCal = SimpleDeserializer.Default.Deserialize(new StringReader(IcsFiles.QuotedPrintableContent)).Cast<Calendar>().Single();
            Assert.AreEqual(1, iCal.Events.Count);

            var evt = iCal.Events.First();

            Assert.AreEqual("content://com.android.calendar/events/123353", evt.Uid);
        }

        [Test, Category("Deserialization")]
        public void QuotedPrintable_Summary()
        {
            var iCal = SimpleDeserializer.Default.Deserialize(new StringReader(IcsFiles.QuotedPrintableContent)).Cast<Calendar>().Single();
            Assert.AreEqual(1, iCal.Events.Count);

            var evt = iCal.Events.First();

            Assert.AreEqual("Webex for mtg w/ Arm", evt.Summary);
        }

        [Test, Category("Deserialization")]
        public void QuotedPrintable_Start()
        {
            var iCal = SimpleDeserializer.Default.Deserialize(new StringReader(IcsFiles.QuotedPrintableContent)).Cast<Calendar>().Single();
            Assert.AreEqual(1, iCal.Events.Count);

            var evt = iCal.Events.First();

            Assert.AreEqual("2016-07-25T17:30:00 UTC", evt.Start.ToString("yyyy-MM-ddTHH:mm:ss", null));
        }

        [Test, Category("Deserialization")]
        public void QuotedPrintable_End()
        {
            var iCal = SimpleDeserializer.Default.Deserialize(new StringReader(IcsFiles.QuotedPrintableContent)).Cast<Calendar>().Single();
            Assert.AreEqual(1, iCal.Events.Count);

            var evt = iCal.Events.First();

            Assert.AreEqual("2016-07-25T18:30:00 UTC", evt.End.ToString("yyyy-MM-ddTHH:mm:ss", null));
        }

        [Test, Category("Deserialization")]
        public void QuotedPrintable_Complete()
        {
            var iCal = SimpleDeserializer.Default.Deserialize(new StringReader(IcsFiles.QuotedPrintableContent)).Cast<Calendar>().Single();
            Assert.AreEqual(1, iCal.Events.Count);

            var evt = iCal.Events.First();

            Assert.IsInstanceOf<IDateTime>(evt.Properties["COMPLETED"].Value);
            var completed = evt.Properties["COMPLETED"].Value as IDateTime;
            Assert.AreEqual("2016-07-25T18:30:00 UTC", completed.ToString("yyyy-MM-ddTHH:mm:ss", null));
        }
    }
}
