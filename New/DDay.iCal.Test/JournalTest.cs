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
    public class JournalTest
    {
        private string tzid;

        [TestFixtureSetUp]
        public void InitAll()
        {
            tzid = "US-Eastern";
        }

        [Test, Category("Journal")]
        public void JOURNAL1()
        {
            IICalendar iCal = iCalendar.LoadFromFile(@"Calendars\Journal\JOURNAL1.ics")[0];
            ProgramTest.TestCal(iCal);
            IJournal j = iCal.Journals[0];

            Assert.IsNotNull(j, "Journal entry was null");
            Assert.AreEqual(JournalStatus.Draft, j.Status, "Journal entry should have been in DRAFT status, but it was in " + j.Status.ToString() + " status.");
            Assert.AreEqual("PUBLIC", j.Class, "Journal class should have been PUBLIC, but was " + j.Class + ".");
            Assert.IsNull(j.Start);
        }

        [Test, Category("Journal")]
        public void JOURNAL2()
        {
            IICalendar iCal = iCalendar.LoadFromFile(@"Calendars\Journal\JOURNAL2.ics")[0];
            ProgramTest.TestCal(iCal);
            IJournal j = iCal.Journals[0];

            Assert.IsNotNull(j, "Journal entry was null");
            Assert.AreEqual(JournalStatus.Final, j.Status, "Journal entry should have been in FINAL status, but it was in " + j.Status + " status.");
            Assert.AreEqual("PRIVATE", j.Class, "Journal class should have been PRIVATE, but was " + j.Class + ".");
            Assert.AreEqual("JohnSmith", j.Organizer.CommonName, "Organizer common name should have been JohnSmith, but was " + j.Organizer.CommonName);
            Assert.IsTrue(
                string.Equals(
                    j.Organizer.SentBy.OriginalString,
                    "mailto:jane_doe@host.com",
                    StringComparison.InvariantCultureIgnoreCase),
                "Organizer should have had been SENT-BY 'mailto:jane_doe@host.com'; it was sent by '" + j.Organizer.SentBy + "'");
            Assert.IsTrue(
                string.Equals(
                    j.Organizer.DirectoryEntry.OriginalString,
                    "ldap://host.com:6666/o=3DDC%20Associates,c=3DUS??(cn=3DJohn%20Smith)",
                    StringComparison.InvariantCultureIgnoreCase),
                "Organizer's directory entry should have been 'ldap://host.com:6666/o=3DDC%20Associates,c=3DUS??(cn=3DJohn%20Smith)', but it was '" + j.Organizer.DirectoryEntry + "'");
            Assert.IsNull(j.Start);
        }
    }
}
