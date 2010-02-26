using System;
using System.Collections.Generic;
using System.Text;
using System.Collections;
using System.IO;
using System.Resources;
using System.Web;
using System.Web.UI;
using DDay.iCal;
using DDay.iCal;
using System.Reflection;
using System.Text.RegularExpressions;
using NUnit.Framework;

namespace DDay.iCal.Test
{
    [TestFixture]
    public class JournalTest
    {
        private TZID tzid;

        [TestFixtureSetUp]
        public void InitAll()
        {
            tzid = new TZID("US-Eastern");
        }

        static public void DoTests()
        {
            JournalTest j = new JournalTest();
            j.InitAll();
            j.JOURNAL1();
            j.JOURNAL2();
        }

        [Test, Category("Journal")]
        public void JOURNAL1()
        {
            IICalendar iCal = iCalendar.LoadFromFile(@"Calendars\Journal\JOURNAL1.ics");
            ProgramTest.TestCal(iCal);
            DDay.iCal.Journal j = (DDay.iCal.Journal)iCal.Journals[0];

            Assert.IsNotNull(j, "Journal entry was null");
            Assert.IsTrue(j.Status == JournalStatus.Draft, "Journal entry should have been in DRAFT status, but it was in " + j.Status.ToString() + " status.");
            Assert.IsTrue(j.Class.Value == "PUBLIC", "Journal class should have been PUBLIC, but was " + j.Class + ".");
            Assert.IsNull(j.DTStart);
        }

        [Test, Category("Journal")]
        public void JOURNAL2()
        {
            IICalendar iCal = iCalendar.LoadFromFile(@"Calendars\Journal\JOURNAL2.ics");
            ProgramTest.TestCal(iCal);
            DDay.iCal.Journal j = (DDay.iCal.Journal)iCal.Journals[0];

            Assert.IsNotNull(j, "Journal entry was null");
            Assert.IsTrue(j.Status == JournalStatus.Final, "Journal entry should have been in FINAL status, but it was in " + j.Status.ToString() + " status.");
            Assert.IsTrue(j.Class.Value == "PRIVATE", "Journal class should have been PRIVATE, but was " + j.Class + ".");
            Assert.IsTrue(j.Organizer.CommonName.Value == "JohnSmith", "Organizer common name should have been JohnSmith, but was " + j.Organizer.CommonName.ToString());
            Assert.IsTrue(
                string.Equals(
                    j.Organizer.SentBy.Value,
                    "mailto:jane_doe@host.com",
                    StringComparison.InvariantCultureIgnoreCase),
                "Organizer should have had been SENT-BY 'mailto:jane_doe@host.com'; it was sent by '" + j.Organizer.SentBy.Value + "'");
            Assert.IsTrue(
                string.Equals(
                    j.Organizer.DirectoryEntry.Value,
                    "ldap://host.com:6666/o=3DDC%20Associates,c=3DUS??(cn=3DJohn%20Smith)",
                    StringComparison.InvariantCultureIgnoreCase),
                "Organizer's directory entry should have been 'ldap://host.com:6666/o=3DDC%20Associates,c=3DUS??(cn=3DJohn%20Smith)', but it was '" + j.Organizer.DirectoryEntry.Value + "'");
            Assert.IsNull(j.DTStart);
        }
    }
}
