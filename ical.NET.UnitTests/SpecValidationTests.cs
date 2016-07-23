using System;
using Ical.Net;
using Ical.Net.DataTypes;
using Ical.Net.Serialization;
using Ical.Net.Serialization.iCalendar.Serializers;
using NUnit.Framework;
using SpecValidation;
using System.Threading.Tasks;
using System.Linq;

namespace ical.NET.UnitTests
{
    [TestFixture]
    public class SpecValidationTests
    {
        #region HelperMethods
        static async Task onlineValidateCal(Calendar cal)
        {
            var serializer = new CalendarSerializer(new SerializationContext());
            var serializedCalendar = serializer.SerializeToString(cal);

            var valErrs = await Validate.ValidationErrors(serializedCalendar);
            Console.WriteLine("[scroll to bottom to see anything which didn't validate]\r\nSerialized output:\r\n");
            Console.WriteLine(serializedCalendar);

            Console.WriteLine(string.Join(Environment.NewLine, valErrs.Select(ve => $"{ve.Message} near line #{ve.LineNumber}\r\nReference:\t{ve.RelevantSpec}\r\nLine Value:\t{ve.LineValue}\r\n\r\n")));

            CollectionAssert.IsEmpty(valErrs);
        }

        private const string _requiredParticipant = "REQ-PARTICIPANT"; //this string may be added to the api in the future
        #endregion //HelperMethods

        [Test, Category("SpecValidation")]
        public async Task KitchenSinkValidates()
        {
            var cal = new Calendar
            {
                Method = "REQUEST",
                Version = "2.0"
            };

            var timezone = VTimeZone.FromSystemTimeZone(TimeZoneTest._customTimeZone);
            cal.AddTimeZone(timezone);

            var evt = new Event
            {
                Class = "PRIVATE",
                Created = new CalDateTime(2010, 3, 25, 12, 53, 35, timezone.Id),
                DtStamp = new CalDateTime(2010, 3, 25, 12, 53, 35),
                LastModified = new CalDateTime(DateTime.SpecifyKind(new DateTime(2010, 3, 27, 13, 53, 35), DateTimeKind.Utc)),
                Sequence = 0,
                Uid = "42f58d4f-847e-46f8-9f4a-ce52697682cf",
                Priority = 5,
                Location = "here",
                Summary = "test",
                DtStart = new CalDateTime(2012, 3, 25, 12, 50, 00),
                DtEnd = new CalDateTime(2012, 3, 25, 13, 10, 00),
                Transparency = TransparencyType.Opaque,
                Status = EventStatus.Confirmed,
                Organizer = new Organizer("james@example.com") // no MAILTO: - should be added
            };
            evt.Attendees.Add(new  Attendee("MAILTO:james@example.com")
            {
                CommonName = "James James",
                Role = _requiredParticipant,
                Rsvp = true,
                ParticipationStatus = ParticipationStatus.Tentative
            });

            var attendee = new Attendee("MAILTO:mary@example.com")
            {
                CommonName = "Mary Mary",
                Role = _requiredParticipant,
                Rsvp = true,
                //until problems with iGrouping are sorted out best not to asign list directly BM
                //DelegatedFrom = new List<string>() { "mailto:immud@example.com" },
                //Members = new List<string>() { "mailto:DEV-GROUP@example.com" }
            };
            attendee.DelegatedFrom.Add("mailto:immud@example.com");
            attendee.Members.Add("mailto:DEV-GROUP@example.com");

            evt.Attendees.Add(attendee);

            evt.Alarms.Add(new Alarm
            {
                Action = AlarmAction.Display,
                Summary = "test",
                Trigger = new Trigger(TimeSpan.FromHours(-1)),
                Description = "descr."
            });
            cal.Events.Add(evt);

            await onlineValidateCal(cal);
        }

    }
}
