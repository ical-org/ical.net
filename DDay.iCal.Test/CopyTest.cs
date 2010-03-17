using System;
using System.Collections.Generic;
using System.Text;
using System.Collections;
using System.IO;
using System.Resources;
using System.Web;
using System.Reflection;
using System.Text.RegularExpressions;
using NUnit.Framework;

namespace DDay.iCal.Test
{
    [TestFixture]
    public class CopyTest
    {
        private string tzid;

        [TestFixtureSetUp]
        public void InitAll()
        {            
            tzid = "US-Eastern";
        }

        private void CopyCalendarTest(string filename)
        {
            IICalendar iCal1 = iCalendar.LoadFromFile(@"Calendars\Serialization\" + filename);
            IICalendar iCal2 = iCal1.Copy<IICalendar>();
            SerializationTest.CompareCalendars(iCal1, iCal2);
        }

        [Test, Category("Copy")]
        public void COPY1()
        {
            CopyCalendarTest("SERIALIZE1.ics");            
        }

        [Test, Category("Copy")]
        public void COPY2()
        {
            CopyCalendarTest("SERIALIZE2.ics");
        }

        [Test, Category("Copy")]
        public void COPY3()
        {
            CopyCalendarTest("SERIALIZE3.ics");
        }

        [Test, Category("Copy")]
        public void COPY4()
        {
            CopyCalendarTest("SERIALIZE4.ics");
        }

        [Test, Category("Copy")]
        public void COPY5()
        {
            CopyCalendarTest("SERIALIZE5.ics");
        }

        [Test, Category("Copy")]
        public void COPY6()
        {
            CopyCalendarTest("SERIALIZE6.ics");
        }

        [Test, Category("Copy")]
        public void COPY7()
        {
            CopyCalendarTest("SERIALIZE7.ics");
        }

        [Test, Category("Copy")]
        public void COPY8()
        {
            CopyCalendarTest("SERIALIZE8.ics");
        }

        [Test, Category("Copy")]
        public void COPY9()
        {
            CopyCalendarTest("SERIALIZE9.ics");
        }

        [Test, Category("Copy")]
        public void COPY10()
        {
            CopyCalendarTest("SERIALIZE10.ics");
        }

        [Test, Category("Copy")]
        public void COPY11()
        {
            CopyCalendarTest("SERIALIZE11.ics");
        }

        [Test, Category("Copy")]
        public void COPY12()
        {
            CopyCalendarTest("SERIALIZE12.ics");
        }

        [Test, Category("Copy")]
        public void COPY13()
        {
            CopyCalendarTest("SERIALIZE13.ics");
        }

        [Test, Category("Copy")]
        public void COPY14()
        {
            CopyCalendarTest("SERIALIZE14.ics");
        }

        [Test, Category("Copy")]
        public void COPY15()
        {
            CopyCalendarTest("SERIALIZE15.ics");
        }
    }
}
