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
            IICalendar iCal1 = iCalendar.LoadFromFile(@"Calendars\Serialization\" + filename)[0];
            IICalendar iCal2 = iCal1.Copy<IICalendar>();
            SerializationTest.CompareCalendars(iCal1, iCal2);
        }

        [Test, Category("Copy")]
        public void Copy1()
        {
            CopyCalendarTest("SERIALIZE1.ics");            
        }

        [Test, Category("Copy")]
        public void Copy2()
        {
            CopyCalendarTest("SERIALIZE2.ics");
        }

        [Test, Category("Copy")]
        public void Copy3()
        {
            CopyCalendarTest("SERIALIZE3.ics");
        }

        [Test, Category("Copy")]
        public void Copy4()
        {
            CopyCalendarTest("SERIALIZE4.ics");
        }

        [Test, Category("Copy")]
        public void Copy5()
        {
            CopyCalendarTest("SERIALIZE5.ics");
        }

        [Test, Category("Copy")]
        public void Copy6()
        {
            CopyCalendarTest("SERIALIZE6.ics");
        }

        [Test, Category("Copy")]
        public void Copy7()
        {
            CopyCalendarTest("SERIALIZE7.ics");
        }

        [Test, Category("Copy")]
        public void Copy8()
        {
            CopyCalendarTest("SERIALIZE8.ics");
        }

        [Test, Category("Copy")]
        public void Copy9()
        {
            CopyCalendarTest("SERIALIZE9.ics");
        }

        [Test, Category("Copy")]
        public void Copy10()
        {
            CopyCalendarTest("SERIALIZE10.ics");
        }

        [Test, Category("Copy")]
        public void Copy11()
        {
            CopyCalendarTest("SERIALIZE11.ics");
        }

        [Test, Category("Copy")]
        public void Copy12()
        {
            CopyCalendarTest("SERIALIZE12.ics");
        }

        [Test, Category("Copy")]
        public void Copy13()
        {
            CopyCalendarTest("SERIALIZE13.ics");
        }

        [Test, Category("Copy")]
        public void Copy14()
        {
            CopyCalendarTest("SERIALIZE14.ics");
        }

        [Test, Category("Copy")]
        public void Copy15()
        {
            CopyCalendarTest("SERIALIZE15.ics");
        }
    }
}
