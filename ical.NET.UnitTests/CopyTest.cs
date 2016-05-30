using Ical.Net;
using Ical.Net.Interfaces;
using NUnit.Framework;

namespace ical.NET.UnitTests
{
    [TestFixture]
    public class CopyTest
    {
        private void CopyCalendarTest(string filename)
        {
            var iCal1 = Calendar.LoadFromFile(@"Calendars\Serialization\" + filename)[0];
            var iCal2 = iCal1.Copy<ICalendar>();
            SerializationTest.CompareCalendars(iCal1, iCal2);
        }

        [Test, Category("Copy")]
        public void CopyAttachment3()
        {
            CopyCalendarTest("Attachment3.ics");
        }

        [Test, Category("Copy")]
        public void CopyBug2148092()
        {
            CopyCalendarTest("Bug2148092.ics");
        }

        [Test, Category("Copy")]
        public void CopyCaseInsensitive1()
        {
            CopyCalendarTest("CaseInsensitive1.ics");
        }

        [Test, Category("Copy")]
        public void CopyCaseInsensitive2()
        {
            CopyCalendarTest("CaseInsensitive2.ics");
        }

        [Test, Category("Copy")]
        public void CopyCaseInsensitive3()
        {
            CopyCalendarTest("CaseInsensitive3.ics");
        }

        [Test, Category("Copy")]
        public void CopyCategories1()
        {
            CopyCalendarTest("Categories1.ics");
        }

        [Test, Category("Copy")]
        public void CopyDuration1()
        {
            CopyCalendarTest("Duration1.ics");
        }        

        [Test, Category("Copy")]
        public void CopyEncoding1()
        {
            CopyCalendarTest("Encoding1.ics");
        }

        [Test, Category("Copy")]
        public void CopyEvent1()
        {
            CopyCalendarTest("Event1.ics");
        }

        [Test, Category("Copy")]
        public void CopyEvent2()
        {
            CopyCalendarTest("Event2.ics");
        }

        [Test, Category("Copy")]
        public void CopyEvent3()
        {
            CopyCalendarTest("Event3.ics");
        }

        [Test, Category("Copy")]
        public void CopyEvent4()
        {
            CopyCalendarTest("Event4.ics");
        }

        [Test, Category("Copy")]
        public void CopyGeographicLocation1()
        {
            CopyCalendarTest("GeographicLocation1.ics");
        }

        [Test, Category("Copy")]
        public void CopyLanguage1()
        {
            CopyCalendarTest("Language1.ics");
        }

        [Test, Category("Copy")]
        public void CopyLanguage2()
        {
            CopyCalendarTest("Language2.ics");
        }

        [Test, Category("Copy")]
        public void CopyLanguage3()
        {
            CopyCalendarTest("Language3.ics");
        }

        [Test, Category("Copy")]
        public void CopyTimeZone1()
        {
            CopyCalendarTest("TimeZone1.ics");
        }

        [Test, Category("Copy")]
        public void CopyTimeZone2()
        {
            CopyCalendarTest("TimeZone2.ics");
        }

        [Test, Category("Copy")]
        public void CopyTimeZone3()
        {
            CopyCalendarTest("TimeZone3.ics");
        }

        [Test, Category("Copy")]
        public void CopyXProperty1()
        {
            CopyCalendarTest("XProperty1.ics");
        }

        [Test, Category("Copy")]
        public void CopyXProperty2()
        {
            CopyCalendarTest("XProperty2.ics");
        }
    }
}
