using System;
using System.Collections.Generic;
using System.Text;
using NUnit.Framework;
using System.IO;
using System.Runtime.Serialization;
using System.Collections;

namespace DDay.iCal.Test
{
    [TestFixture]
    public class DataContractSeralizationTest
    {
        static public void SerializeTest(object obj, string filename)
        {
            string dir = @"Temp\Serialization";
            if (!Directory.Exists(dir))
                Directory.CreateDirectory(dir);

            FileStream fs = new FileStream(Path.Combine(dir, filename), FileMode.Create, FileAccess.Write);

#if DATACONTRACT
            DataContractSerializer serializer = new DataContractSerializer(obj.GetType());
#else
            DataContractSerializer serializer = new DataContractSerializer(
                obj.GetType(), 
                KnownTypeHelper.GetKnownTypes(), 
                0x7fff, // Maximum number of items in the object graph
                false,  // Don't ignore ExtensionDataObjects
                true,   // Preserve object references
                null);  // The DataContract surrogate
#endif
            serializer.WriteObject(fs, obj);

            object deserializedObject = null;

            fs.Close();
            fs = new FileStream(Path.Combine(dir, filename), FileMode.Open, FileAccess.Read);

            deserializedObject = serializer.ReadObject(fs);

            AssertEquality(obj, deserializedObject);
        }

        static public void AssertEquality(object obj1, object obj2)
        {
            if (obj1 is IComparable)
            {
                Assert.AreEqual(0, ((IComparable)obj1).CompareTo(obj2));
            }
            else if (
                obj1 is IEnumerable &&
                obj2 is IEnumerable)
            {
                IEnumerator currEnum = ((IEnumerable)obj1).GetEnumerator();
                IEnumerator copyEnum = ((IEnumerable)obj2).GetEnumerator();
                if (currEnum != null &&
                    copyEnum != null)
                {
                    while (currEnum.MoveNext())
                    {
                        Assert.IsTrue(copyEnum.MoveNext(), "Collection sizes do not match.");
                        AssertEquality(currEnum.Current, copyEnum.Current);
                    }
                    Assert.IsFalse(copyEnum.MoveNext(), "Collection sizes do not match.");
                }
            }
            else
            {
                Assert.AreEqual(obj1, obj2, "Original object and copy object are not equal.");
            }
        }

        [Test, Category("DataContractSerialization")]
        public void Attachment1()
        {
            IAttachment a = new Attachment();
            a.FormatType = "application/binary";
            a.Data = new byte[] { 1, 2, 3, 4 };
            SerializeTest(a, "Attachment1.xml");
        }

        // FIXME: re-implement
        //[Test, Category("DataContractSerialization")]
        //public void Attendee1()
        //{
        //    IAttendee a = new Attendee();
        //    a.CommonName = "The Postmaster";
        //    a.DirectoryEntry = "ldap://host.com:6666/o=eDABC%20Industries,c=3DUS??(cn=3DBJim%20Dolittle)";
        //    a.EmailAddress = "mailto:test123@test.com";
        //    SerializeTest(a, "Attendee1.xml");
        //}

        [Test, Category("DataContractionSerialization")]
        public void DaySpecifier1()
        {
            DaySpecifier ds = new DaySpecifier(DayOfWeek.Monday, FrequencyOccurrence.First);
            SerializeTest(ds, "DaySpecifier1.xml");
        }

        // FIXME: re-implement
        //[Test, Category("DataContractionSerialization")]
        //public void Event1()
        //{
        //    iCalendar iCal = new iCalendar();
            
        //    Event evt = iCal.Create<Event>();
        //    evt.Summary = "Summary";
        //    evt.Description = "Description";
        //    evt.Start = DateTime.Today.AddDays(1).AddHours(8).AddMinutes(30);
        //    evt.Duration = TimeSpan.FromHours(1);
        //    evt.Comments.Add("Comment 1");
        //    evt.Comments.Add("Comment 2");
        //    evt.Contacts.Add("Doug Day (555) 555-5555");
            
        //    Alarm a = new Alarm();
        //    a.Trigger = new Trigger(TimeSpan.FromHours(-2));
        //    a.Action = AlarmAction.Display;
        //    evt.Alarms.Add(a);

        //    evt.Attachments.Add(new Attachment(new byte[] { 1, 2, 3, 4 }));
        //    evt.Categories.Add("Work");
        //    evt.RecurrenceRules.Add(new RecurrencePattern("FREQ=DAILY,INTERVAL=3"));
        //    evt.Resources.Add("Projector");
        //    evt.GeographicLocation = new Geo(40.766852,-111.963375);
        //    evt.Location = "SLC Airport";
        //    evt.Organizer = "mailto:doug@ddaysoftware.com";

        //    SerializeTest(iCal, "Event1.xml");
        //}
        
        [Test, Category("DataContractionSerialization")]
        public void GeographicLocation1()
        {
            IGeographicLocation g = new GeographicLocation(123.143, 52.1234);
            SerializeTest(g, "GeographicLocation1.xml");
        }

        [Test, Category("DataContractionSerialization")]
        public void iCalDateTime1()
        {
            iCalDateTime icdt = DateTime.Now;
            SerializeTest(icdt, "iCalDateTime1.xml");
        }

        [Test, Category("DataContractionSerialization")]
        public void Period1()
        {
            Period p = new Period();
            p.StartTime = new iCalDateTime(DateTime.Now.AddHours(2));
            p.EndTime = new iCalDateTime(DateTime.Now.AddHours(3));
            SerializeTest(p, "Period1.xml");
        }

        [Test, Category("DataContractionSerialization")]
        public void PeriodList1()
        {
            IPeriodList pl = new PeriodList();
            pl.Add(new Period(new iCalDateTime(DateTime.Now.AddHours(2)), new iCalDateTime(DateTime.Now.AddHours(3))));
            pl.Add(new Period(new iCalDateTime(DateTime.Now.AddHours(4)), new iCalDateTime(DateTime.Now.AddHours(5))));
            SerializeTest(pl, "PeriodList1.xml");
        }

        [Test, Category("DataContractionSerialization")]
        public void RecurrencePattern1()
        {
            RecurrencePattern rp = new RecurrencePattern();
            rp.ByMonthDay.Add(5);
            rp.ByMonth.Add(3);
            rp.ByHour.Add(8);
            rp.ByMinute.Add(30);
            // Every March 5th at 8:30AM.
            SerializeTest(rp, "RecurrencePattern1.xml");
        }

        // FIXME: re-implement
        //[Test, Category("DataContractionSerialization")]
        //public void Trigger1()
        //{
        //    ITrigger t = new Trigger(TimeSpan.FromHours(-1));
        //    SerializeTest(t, "Trigger1.xml");
        //}

        [Test, Category("DataContractionSerialization")]
        public void UTCOffset1()
        {
            UTCOffset o = new UTCOffset(TimeSpan.FromHours(-6));
            SerializeTest(o, "UTCOffset1.xml");
        }
    }
}
