//
// Copyright ical.net project maintainers and contributors.
// Licensed under the MIT license.
//

using System;
using System.Linq;
using NUnit.Framework;

namespace Ical.Net.Tests;

[TestFixture]
public class JournalTest
{
    [Test, Category("Journal")]
    public void Journal1()
    {
        var iCal = Calendar.Load(IcsFiles.Journal1);
        ProgramTest.TestCal(iCal);
        Assert.That(iCal.Journals, Has.Count.EqualTo(1));
        var j = iCal.Journals[0];

        Assert.Multiple(() =>
        {
            Assert.That(j, Is.Not.Null, "Journal entry was null");
            Assert.That(j.Status, Is.EqualTo(JournalStatus.Draft), "Journal entry should have been in DRAFT status, but it was in " + j.Status + " status.");
            Assert.That(j.Class, Is.EqualTo("PUBLIC"), "Journal class should have been PUBLIC, but was " + j.Class + ".");
        });
        Assert.That(j.Start, Is.Null);
    }

    [Test, Category("Journal")]
    public void Journal2()
    {
        var iCal = Calendar.Load(IcsFiles.Journal2);
        ProgramTest.TestCal(iCal);
        Assert.That(iCal.Journals, Has.Count.EqualTo(1));
        var j = iCal.Journals.First();

        Assert.Multiple(() =>
        {
            Assert.That(j, Is.Not.Null, "Journal entry was null");
            Assert.That(j.Status, Is.EqualTo(JournalStatus.Final), "Journal entry should have been in FINAL status, but it was in " + j.Status + " status.");
            Assert.That(j.Class, Is.EqualTo("PRIVATE"), "Journal class should have been PRIVATE, but was " + j.Class + ".");
            Assert.That(j.Organizer.CommonName, Is.EqualTo("JohnSmith"), "Organizer common name should have been JohnSmith, but was " + j.Organizer.CommonName);
            Assert.That(
                string.Equals(
                    j.Organizer.SentBy.OriginalString,
                    "mailto:jane_doe@host.com",
                    StringComparison.OrdinalIgnoreCase),
                Is.True,
                "Organizer should have had been SENT-BY 'mailto:jane_doe@host.com'; it was sent by '" + j.Organizer.SentBy + "'");
            Assert.That(
                string.Equals(
                    j.Organizer.DirectoryEntry.OriginalString,
                    "ldap://host.com:6666/o=3DDC%20Associates,c=3DUS??(cn=3DJohn%20Smith)",
                    StringComparison.OrdinalIgnoreCase),
                Is.True,
                "Organizer's directory entry should have been 'ldap://host.com:6666/o=3DDC%20Associates,c=3DUS??(cn=3DJohn%20Smith)', but it was '" + j.Organizer.DirectoryEntry + "'");
            Assert.That(
                j.Organizer.Value.OriginalString,
                Is.EqualTo("MAILTO:jsmith@host.com"));
            Assert.That(
                j.Organizer.Value.UserInfo,
                Is.EqualTo("jsmith"));
            Assert.That(
                j.Organizer.Value.Host,
                Is.EqualTo("host.com"));
            Assert.That(j.Start, Is.Null);
        });
    }
}