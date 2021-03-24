using System;
using System.Linq;
using System.Runtime.Serialization;
using Ical.Net.Serialization;
using NUnit.Framework;

namespace Ical.Net.CoreUnitTests
{
    [TestFixture]
    public class IcalCleanerTests
    {
        /// <summary>
        /// Tests an iCal file received from a customer with attende lines, that have values outside of the quotes,
        /// but they should be inside.
        /// E.g. ATTENDEE;ROLE=REQ-PARTICIPANT;PARTSTAT=NEEDS-ACTION;RSVP=TRUE;CN="Petra Einspieler-Aman"  (praes@aekktn.at):mailto:praes@aekktn.at
        /// In the above line the CN only contains Petra Einspieler-Aman. The email adress is after the ending quote.
        /// That was a problem and was fixed. This test ensures the fix.
        /// </summary>
        [Test]
        public void Attendees_SpecialCaseIcal()
        {
            // Act
            var cleanedIcal = IcalCleaner.CleanVCalFile(IcsFiles.Cleaner_Attendees1);

            // Assert
            var calendar = Calendar.Load(cleanedIcal);
            var attendees = calendar.Events[0].Attendees;

            Assert.AreEqual(17, attendees.Count);
            Assert.That(attendees.Select(x => x.Role), Is.All.EqualTo(ParticipationRole.RequiredParticipant));
            Assert.That(attendees.Select(x => x.ParticipationStatus), Is.All.EqualTo(EventParticipationStatus.NeedsAction));
            Assert.That(attendees.Select(x => x.Rsvp), Is.All.EqualTo(true));

            Assert.Contains("Johann Lintner GKK Dirketor ", attendees.Select(x => x.CommonName).ToList());
            Assert.Contains("mailto:johann.lintner@kgkk.at", attendees.Select(x => x.Value.ToString()).ToList());

            Assert.Contains("'kurt.possnig@kgkk.at'", attendees.Select(x => x.CommonName).ToList());
            Assert.Contains("mailto:kurt.possnig@kgkk.at", attendees.Select(x => x.Value.ToString()).ToList());

            Assert.Contains("'klaus.arneitz@kabeg.at'", attendees.Select(x => x.CommonName).ToList());
            Assert.Contains("mailto:klaus.arneitz@kabeg.at", attendees.Select(x => x.Value.ToString()).ToList());

            Assert.Contains("'gerhard.hofstaetter@kabeg.at'", attendees.Select(x => x.CommonName).ToList());
            Assert.Contains("mailto:gerhard.hofstaetter@kabeg.at", attendees.Select(x => x.Value.ToString()).ToList());

            Assert.Contains("'dietmar.geissler@kabeg.at'", attendees.Select(x => x.CommonName).ToList());
            Assert.Contains("mailto:dietmar.geissler@kabeg.at", attendees.Select(x => x.Value.ToString()).ToList());

            Assert.Contains("'Alfred.Markowitsch@humanomed.co.at'", attendees.Select(x => x.CommonName).ToList());
            Assert.Contains("mailto:Alfred.Markowitsch@humanomed.co.at", attendees.Select(x => x.Value.ToString()).ToList());

            Assert.Contains("'kyra.borchhadt@gmail.com'", attendees.Select(x => x.CommonName).ToList());
            Assert.Contains("mailto:kyra.borchhadt@gmail.com", attendees.Select(x => x.Value.ToString()).ToList());

            Assert.Contains("'gertwiegele@kaerngesund.at'", attendees.Select(x => x.CommonName).ToList());
            Assert.Contains("mailto:gertwiegele@kaerngesund.at", attendees.Select(x => x.Value.ToString()).ToList());

            Assert.Contains("Petra Einspieler-Aman  (praes@aekktn.at)", attendees.Select(x => x.CommonName).ToList());
            Assert.Contains("mailto:praes@aekktn.at", attendees.Select(x => x.Value.ToString()).ToList());

            Assert.Contains("'dial.ktn@utanet.at'", attendees.Select(x => x.CommonName).ToList());
            Assert.Contains("mailto:dial.ktn@utanet.at", attendees.Select(x => x.Value.ToString()).ToList());

            Assert.Contains("'gernot.waste@verbund.com'", attendees.Select(x => x.CommonName).ToList());
            Assert.Contains("mailto:gernot.waste@verbund.com", attendees.Select(x => x.Value.ToString()).ToList());

            Assert.Contains("'dialyse@khspittal.com'", attendees.Select(x => x.CommonName).ToList());
            Assert.Contains("mailto:dialyse@khspittal.com", attendees.Select(x => x.Value.ToString()).ToList());

            Assert.Contains("STADTSCHREIBER Gerhard", attendees.Select(x => x.CommonName).ToList());
            Assert.Contains("mailto:Gerhard.Stadtschreiber@ktn.gv.at", attendees.Select(x => x.Value.ToString()).ToList());

            Assert.Contains("STICKLER Gernot", attendees.Select(x => x.CommonName).ToList());
            Assert.Contains("mailto:Gernot.Stickler@ktn.gv.at", attendees.Select(x => x.Value.ToString()).ToList());

            Assert.Contains("ASTNER Robert", attendees.Select(x => x.CommonName).ToList());
            Assert.Contains("mailto:Robert.ASTNER@ktn.gv.at", attendees.Select(x => x.Value.ToString()).ToList());

            Assert.Contains("MICHELER-EISNER Ulrike", attendees.Select(x => x.CommonName).ToList());
            Assert.Contains("mailto:Ulrike.MICHELER-EISNER@ktn.gv.at", attendees.Select(x => x.Value.ToString()).ToList());

            Assert.Contains("'johannes.koinig@stmk.gv.at'", attendees.Select(x => x.CommonName).ToList());
            Assert.Contains("mailto:johannes.koinig@stmk.gv.at", attendees.Select(x => x.Value.ToString()).ToList());
        }

        /// <summary>
        /// Tests different formatted attendees.
        /// </summary>
        [Test]
        public void Attendees_DifferentFormats()
        {
            // Arrange
            const string EMAIL_ATTENDEE_1 = "mailto:conso2016@gmx.at";
            const string EMAIL_ATTENDEE_2 = "mailto:conso2017@gmx.at";
            const string EMAIL_ATTENDEE_3 = "mailto:conso2018@gmx.at";
            const string EMAIL_ATTENDEE_4 = "mailto:conso2019@gmx.at";
            const string EMAIL_ATTENDEE_5 = "mailto:emueller@consolidate.eu";
            const string EMAIL_ATTENDEE_6 = "mailto:madeline.kraus@two-morrow.at";
            const string EMAIL_ATTENDEE_7 = "mailto:conso2020@gmx.at";

            // Act
            var cleanedIcal = IcalCleaner.CleanVCalFile(IcsFiles.Cleaner_Attendees2);

            // Assert
            var calendar = Calendar.Load(cleanedIcal);
            var attendees = calendar.Events[0].Attendees;
            var attendee1 = attendees.First(x => x.Value.ToString() == EMAIL_ATTENDEE_1);
            var attendee2 = attendees.First(x => x.Value.ToString() == EMAIL_ATTENDEE_2);
            var attendee3 = attendees.First(x => x.Value.ToString() == EMAIL_ATTENDEE_3);
            var attendee4 = attendees.First(x => x.Value.ToString() == EMAIL_ATTENDEE_4);
            var attendee5 = attendees.First(x => x.Value.ToString() == EMAIL_ATTENDEE_5);
            var attendee6 = attendees.First(x => x.Value.ToString() == EMAIL_ATTENDEE_6);
            var attendee7 = attendees.First(x => x.Value.ToString() == EMAIL_ATTENDEE_7);

            Assert.AreEqual(7, attendees.Count);

            Assert.AreEqual(EMAIL_ATTENDEE_1, attendee1.Value.ToString());
            Assert.AreEqual("HPO Test (conso2016@gmx.at)", attendee1.CommonName);
            Assert.IsTrue(attendee1.Rsvp);

            Assert.AreEqual(EMAIL_ATTENDEE_2, attendee2.Value.ToString());
            Assert.AreEqual("HPO Test (conso2016@gmx.at) with many semicolons", attendee2.CommonName);
            Assert.AreEqual(ParticipationRole.RequiredParticipant, attendee2.Role);
            Assert.AreEqual(EventParticipationStatus.NeedsAction, attendee2.ParticipationStatus);
            Assert.IsTrue(attendee2.Rsvp);

            Assert.AreEqual(EMAIL_ATTENDEE_3, attendee3.Value.ToString());
            Assert.AreEqual("HPO Test (conso2016@gmx.at)  (praes@aekktn.at)", attendee3.CommonName);
            Assert.AreEqual(ParticipationRole.RequiredParticipant, attendee3.Role);
            Assert.AreEqual(EventParticipationStatus.NeedsAction, attendee3.ParticipationStatus);
            Assert.IsTrue(attendee3.Rsvp);

            Assert.AreEqual(EMAIL_ATTENDEE_4, attendee4.Value.ToString());
            Assert.AreEqual("HPO Test (conso2016@gmx.at) without RSVP", attendee4.CommonName);
            Assert.IsFalse(attendee4.Rsvp);

            Assert.AreEqual(EMAIL_ATTENDEE_5, attendee5.Value.ToString());
            Assert.AreEqual("Eike Müller (emueller@consolidate.eu). attendee with line break with tab. first line contains an email address.", attendee5.CommonName);
            Assert.IsTrue(attendee5.Rsvp);

            Assert.AreEqual(EMAIL_ATTENDEE_6, attendee6.Value.ToString());
            Assert.AreEqual("Madeline Kraus. attendee with line break with tab. first line does not contain an email address.", attendee6.CommonName);
            Assert.IsTrue(attendee6.Rsvp);

            Assert.AreEqual(EMAIL_ATTENDEE_7, attendee7.Value.ToString());
            Assert.AreEqual("HPO Test (conso2016@gmx.at) does not have a mailto", attendee7.CommonName);
            Assert.IsTrue(attendee7.Rsvp);
        }

        [Test]
        public void Attendees_RFC()
        {
            // Arrange
            const string EMAIL_ATTENDEE_1 = "mailto:joecool@example.com";
            const string EMAIL_ATTENDEE_2 = "mailto:ildoit@example.com";
            const string EMAIL_ATTENDEE_3 = "mailto:hcabot@example.com";
            const string EMAIL_ATTENDEE_4 = "mailto:jdoe@example.com";
            const string EMAIL_ATTENDEE_5 = "mailto:jimdo@example.com";
            const string EMAIL_ATTENDEE_6 = "mailto:hcabot2@example.com";
            const string EMAIL_ATTENDEE_7 = "mailto:iamboss@example.com";
            const string EMAIL_ATTENDEE_8 = "mailto:jdoe2@example.com";
            //const string EMAIL_ATTENDEE_9 = "mailto:jsmith@example.com";

            // Act
            var cleanedIcal = IcalCleaner.CleanVCalFile(IcsFiles.Cleaner_Attendees3);

            // Assert
            var calendar = Calendar.Load(cleanedIcal);
            var attendees = calendar.Events[0].Attendees;
            var attendee1 = attendees.First(x => x.Value.ToString() == EMAIL_ATTENDEE_1); // Contains: MEMBER
            var attendee2 = attendees.First(x => x.Value.ToString() == EMAIL_ATTENDEE_2); // Contains: DELEGATED-FROM
            var attendee3 = attendees.First(x => x.Value.ToString() == EMAIL_ATTENDEE_3);
            var attendee4 = attendees.First(x => x.Value.ToString() == EMAIL_ATTENDEE_4);
            var attendee5 = attendees.First(x => x.Value.ToString() == EMAIL_ATTENDEE_5); // Contains: DIR
            var attendee6 = attendees.First(x => x.Value.ToString() == EMAIL_ATTENDEE_6); // Contains: DELEGATED-FROM
            var attendee7 = attendees.First(x => x.Value.ToString() == EMAIL_ATTENDEE_7); // Contains: DELEGATED-TO
            var attendee8 = attendees.First(x => x.Value.ToString() == EMAIL_ATTENDEE_8); // Contains: DELEGATED-FROM
            //var attendee9 = attendees.First(x => x.Value.ToString() == EMAIL_ATTENDEE_9); //senty-by

            Assert.AreEqual(8, attendees.Count);

            Assert.AreEqual(EMAIL_ATTENDEE_1, attendee1.Value.ToString());
            Assert.Contains("mailto:DEV-GROUP@example.com", attendee1.Members.ToList());
            Assert.IsFalse(attendee1.Rsvp);

            Assert.AreEqual(EMAIL_ATTENDEE_2, attendee2.Value.ToString());
            Assert.Contains("mailto:immud@example.com", attendee2.DelegatedFrom.ToList());
            Assert.IsFalse(attendee2.Rsvp);

            Assert.AreEqual(EMAIL_ATTENDEE_3, attendee3.Value.ToString());
            Assert.AreEqual("Henry Cabot", attendee3.CommonName);
            Assert.AreEqual(ParticipationRole.RequiredParticipant, attendee3.Role);
            Assert.AreEqual(EventParticipationStatus.Tentative, attendee3.ParticipationStatus);
            Assert.IsFalse(attendee3.Rsvp);

            Assert.AreEqual(EMAIL_ATTENDEE_4, attendee4.Value.ToString());
            Assert.AreEqual("Jane Doe", attendee4.CommonName);
            Assert.Contains("mailto:bob@example.com", attendee4.DelegatedFrom.ToList());
            Assert.AreEqual(ParticipationRole.RequiredParticipant, attendee4.Role);
            Assert.AreEqual(EventParticipationStatus.Accepted, attendee4.ParticipationStatus);
            Assert.IsFalse(attendee4.Rsvp);

            Assert.AreEqual(EMAIL_ATTENDEE_5, attendee5.Value.ToString());
            Assert.AreEqual("John Smith", attendee5.CommonName);
            Assert.AreEqual("ldap://example.com:6666/o=ABC%20Industries,c=US???(cn=Jim%20Dolittle)", attendee5.Parameters.Get("DIR"));
            Assert.IsFalse(attendee5.Rsvp);

            Assert.AreEqual(EMAIL_ATTENDEE_6, attendee6.Value.ToString());
            Assert.AreEqual("Henry Cabot", attendee6.CommonName);
            Assert.AreEqual(ParticipationRole.RequiredParticipant, attendee6.Role);
            Assert.AreEqual(EventParticipationStatus.Tentative, attendee6.ParticipationStatus);
            Assert.Contains("mailto:iamboss@example.com", attendee6.DelegatedFrom.ToList());
            Assert.IsFalse(attendee6.Rsvp);

            Assert.AreEqual(EMAIL_ATTENDEE_7, attendee7.Value.ToString());
            Assert.AreEqual("The Big Cheese", attendee7.CommonName);
            Assert.AreEqual(ParticipationRole.NonParticipant, attendee7.Role);
            Assert.AreEqual(EventParticipationStatus.Delegated, attendee7.ParticipationStatus);
            Assert.Contains("mailto:hcabot@example.com", attendee7.DelegatedTo.ToList());
            Assert.IsFalse(attendee7.Rsvp);

            Assert.AreEqual(EMAIL_ATTENDEE_8, attendee8.Value.ToString());
            Assert.AreEqual("Jane Doe", attendee8.CommonName);
            Assert.AreEqual(ParticipationRole.RequiredParticipant, attendee8.Role);
            Assert.AreEqual(EventParticipationStatus.Accepted, attendee8.ParticipationStatus);
            Assert.IsFalse(attendee8.Rsvp);
        }

        [Test]
        public void QuotedPrintable1()
        {
            // Act
            var cleanedIcal = IcalCleaner.CleanVCalFile(IcsFiles.Cleaner_QuotedPrintable1);

            // Assert
            var calendar = Calendar.Load(cleanedIcal);
            Assert.NotNull(calendar.Events[0].Description);
            Assert.IsNotEmpty(calendar.Events[0].Description);
        }

        [Test]
        public void QuotedPrintable2()
        {
            // Act
            var cleanedIcal = IcalCleaner.CleanVCalFile(IcsFiles.Cleaner_QuotedPrintable2);

            // Assert
            var calendar = Calendar.Load(cleanedIcal);
            Assert.NotNull(calendar.Events[0].Description);
            Assert.IsNotEmpty(calendar.Events[0].Description);
        }

        [Test]
        public void QuotedPrintable3()
        {
            // Assert
            var cleanedIcal = IcalCleaner.CleanVCalFile(IcsFiles.Cleaner_QuotedPrintable3);

            // Act
            var ex = Assert.Throws<SerializationException>(() => Calendar.Load(cleanedIcal));

            // Assert
            Assert.That(ex.Message, Is.EqualTo("Could not parse line: '....................=0D=0A=0D=0A =0D=0A=0D=0A'"));
        }

        [Test]
        public void QuotedPrintable_UmlauteAndSpecialCharacters()
        {
            // Act
            var cleanedIcal = IcalCleaner.CleanVCalFile(IcsFiles.Cleaner_QuotedPrintable_UmlauteAndSpecialCharacters);

            // Assert
            var calendar = Calendar.Load(cleanedIcal);
            Assert.AreEqual(calendar.Events[0].Description, "öÖüÜäÄ–");
        }

        [Test]
        public void QuotedPrintable_LineBreakTests()
        {
            // Act
            var cleanedIcal = IcalCleaner.CleanVCalFile(IcsFiles.Cleaner_QuotedPrintable4);

            // Assert
            var calendar = Calendar.Load(cleanedIcal);
            Assert.That(calendar.Events.Select(x => x.Description), Is.All.EqualTo("This is one line!"));
        }

        [Test]
        public void NormalText_LineBreakTests()
        {
            // Act
            var cleanedIcal = IcalCleaner.CleanVCalFile(IcsFiles.Cleaner_NormalTextLineBreaks);

            // Assert
            var calendar = Calendar.Load(cleanedIcal);
            Assert.That(calendar.Events.Select(x => x.Description), Is.All.EqualTo("This is one line!"));
        }
    }
}
