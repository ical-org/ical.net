using System;
using System.Collections.Generic;
using System.Text;
using System.Collections;
using System.IO;
using System.Resources;
using System.Web;
using System.Web.UI;
using System.Reflection;
using System.Text.RegularExpressions;
using NUnit.Framework;
using DDay.iCal.Components;
using DDay.iCal.DataTypes;
using DDay.iCal.Serialization;

namespace DDay.iCal.Test
{
    [TestFixture]
    public class Copy
    {
        private TZID tzid;

        [TestFixtureSetUp]
        public void InitAll()
        {            
            tzid = new TZID("US-Eastern");
        }

        static public void DoTests()
        {
            Copy c = new Copy();
            c.InitAll();
            c.COPY1();
            c.COPY2();
            c.COPY3();
            c.COPY4();
            c.COPY5();
            c.COPY6();
            c.COPY7();
            c.COPY8();
            c.COPY9();
            c.COPY10();
            c.COPY11();
            c.COPY12();
            c.COPY13();
            c.COPY14();
            c.COPY15();
        }

        private void CopyTest(string filename)
        {
            iCalendar iCal1 = iCalendar.LoadFromFile(@"Calendars\Serialization\" + filename);
            iCalendar iCal2 = iCal1.Copy();
            Serialization.CompareCalendars(iCal1, iCal2);
        }

        [Test, Category("Copy")]
        public void COPY1()
        {
            CopyTest("SERIALIZE1.ics");            
        }

        [Test, Category("Copy")]
        public void COPY2()
        {
            CopyTest("SERIALIZE2.ics");
        }

        [Test, Category("Copy")]
        public void COPY3()
        {
            CopyTest("SERIALIZE3.ics");
        }

        [Test, Category("Copy")]
        public void COPY4()
        {
            CopyTest("SERIALIZE4.ics");
        }

        [Test, Category("Copy")]
        public void COPY5()
        {
            CopyTest("SERIALIZE5.ics");
        }

        [Test, Category("Copy")]
        public void COPY6()
        {
            CopyTest("SERIALIZE6.ics");
        }

        [Test, Category("Copy")]
        public void COPY7()
        {
            CopyTest("SERIALIZE7.ics");
        }

        [Test, Category("Copy")]
        public void COPY8()
        {
            CopyTest("SERIALIZE8.ics");
        }

        [Test, Category("Copy")]
        public void COPY9()
        {
            CopyTest("SERIALIZE9.ics");
        }

        [Test, Category("Copy")]
        public void COPY10()
        {
            CopyTest("SERIALIZE10.ics");
        }

        [Test, Category("Copy")]
        public void COPY11()
        {
            CopyTest("SERIALIZE11.ics");
        }

        [Test, Category("Copy")]
        public void COPY12()
        {
            CopyTest("SERIALIZE12.ics");
        }

        [Test, Category("Copy")]
        public void COPY13()
        {
            CopyTest("SERIALIZE13.ics");
        }

        [Test, Category("Copy")]
        public void COPY14()
        {
            CopyTest("SERIALIZE14.ics");
        }

        [Test, Category("Copy")]
        public void COPY15()
        {
            CopyTest("SERIALIZE15.ics");
        }
    }
}
