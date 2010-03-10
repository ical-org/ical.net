using System;
using System.Collections.Generic;
using System.Text;
using System.Collections;
using System.IO;
using System.Resources;
using System.Web;
using System.Reflection;
using System.Text.RegularExpressions;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.ComponentModel;

namespace DDay.iCal.Test
{
    [TestClass]
    public class JournalTest
    {
        static private string tzid;

        [ClassInitialize]
        static public void InitAll(TestContext context)
        {
            tzid = "US-Eastern";
        }

        [TestMethod, Category("Journal")]
        public void JOURNAL1()
        {
            IICalendar iCal = iCalendar.LoadFromFile(@"Calendars\Journal\JOURNAL1.ics");
            ProgramTest.TestCal(iCal);
            IJournal j = iCal.Journals[0];

            Assert.IsNotNull(j, "Journal entry was null");
            Assert.IsTrue(j.Status == JournalStatus.Draft, "Journal entry should have been in DRAFT status, but it was in " + j.Status.ToString() + " status.");
            Assert.IsTrue(j.Class.Equals("PUBLIC"), "Journal class should have been PUBLIC, but was " + j.Class + ".");
            Assert.IsNull(j.DTStart);
        }

        [TestMethod, Category("Journal")]
        public void JOURNAL2()
        {
            IICalendar iCal = iCalendar.LoadFromFile(@"Calendars\Journal\JOURNAL2.ics");
            ProgramTest.TestCal(iCal);
            IJournal j = iCal.Journals[0];

            Assert.IsNotNull(j, "Journal entry was null");
            Assert.IsTrue(j.Status == JournalStatus.Final, "Journal entry should have been in FINAL status, but it was in " + j.Status.ToString() + " status.");
            Assert.AreEqual("PRIVATE", j.Class, "Journal class should have been PRIVATE, but was " + j.Class + ".");
            Assert.AreEqual("JohnSmith", j.Organizer.CommonName, "Organizer common name should have been JohnSmith, but was " + j.Organizer.CommonName.ToString());
            Assert.IsTrue(
                string.Equals(
                    j.Organizer.SentBy.OriginalString,
                    "mailto:jane_doe@host.com",
                    StringComparison.InvariantCultureIgnoreCase),
                "Organizer should have had been SENT-BY 'mailto:jane_doe@host.com'; it was sent by '" + j.Organizer.SentBy.OriginalString + "'");
            Assert.IsTrue(
                string.Equals(
                    j.Organizer.DirectoryEntry.OriginalString,
                    "ldap://host.com:6666/o=3DDC%20Associates,c=3DUS??(cn=3DJohn%20Smith)",
                    StringComparison.InvariantCultureIgnoreCase),
                "Organizer's directory entry should have been 'ldap://host.com:6666/o=3DDC%20Associates,c=3DUS??(cn=3DJohn%20Smith)', but it was '" + j.Organizer.DirectoryEntry.OriginalString + "'");
            Assert.IsNull(j.DTStart);
        }
    }
}
