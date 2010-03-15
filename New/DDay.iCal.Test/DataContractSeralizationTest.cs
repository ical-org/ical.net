using System;
using System.Collections.Generic;
using System.Text;
using System.IO;
using System.Runtime.Serialization;
using System.Collections;
using NUnit.Framework;

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

            DataContractSerializer serializer = new DataContractSerializer(obj.GetType());
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

        // FIXME: re-implement
        //[Test, Category("DataContractSerialization")]
        //public void Attendee1()
        //{
        //    Attendee a = new Attendee();
        //    a.CommonName = "The Postmaster";
        //    a.DirectoryEntry = "ldap://host.com:6666/o=eDABC%20Industries,c=3DUS??(cn=3DBJim%20Dolittle)";
        //    a.Value = "mailto:test123@test.com";
        //    SerializeTest(a, "Attendee1.xml");
        //}

        // FIXME: re-implement
        //[Test, Category("DataContractSerialization")]
        //public void Binary1()
        //{
        //    Binary b = new Binary();
        //    b.FormatType = "application/binary";
        //    b.Data = new byte[] { 1, 2, 3, 4 };
        //    b.Name = "ATTACH";
        //    SerializeTest(b, "Binary1.xml");
        //}

        [Test, Category("DataContractionSerialization")]
        public void DaySpecifier1()
        {
            DaySpecifier ds = new DaySpecifier(DayOfWeek.Monday, FrequencyOccurrence.First);
            SerializeTest(ds, "DaySpecifier1.xml");
        }

        [Test, Category("DataContractionSerialization")]
        public void Duration1()
        {
            TimeSpan ts = TimeSpan.FromHours(1.234);
            SerializeTest(ts, "TimeSpan1.xml");
        }

        // FIXME: re-implement
        //[Test, Category("DataContractionSerialization")]
        //public void Event1()
        //{
        //    iCalendar iCal = new iCalendar();

        //    IEvent evt = iCal.Create<Event>();
        //    evt.Summary = "Summary";
        //    evt.Description = "Description";
        //    evt.Start = DateTime.Today.AddDays(1).AddHours(8).AddMinutes(30);
        //    evt.Duration = TimeSpan.FromHours(1);
        //    evt.Comments.Add("Comment 1");
        //    evt.Comments.Add("Comment 2");
        //    evt.Comments.Add("Doug Day (555) 555-5555");

        //    IAlarm a = new Alarm();
        //    a.Trigger = new Trigger(TimeSpan.FromHours(-2));
        //    a.Action = AlarmAction.Display;
        //    evt.Alarms.Add(a);

        //    evt.Attachments.Add(new Binary(new byte[] { 1, 2, 3, 4 }));
        //    evt.Categories.Add("Work");
        //    evt.RecurrenceRules.Add(new RecurrencePattern("FREQ=DAILY,INTERVAL=3"));
        //    evt.Resources.Add("Projector");
        //    evt.GeographicLocation = new GeographicLocation(40.766852, -111.963375);
        //    evt.Location = "SLC Airport";
        //    evt.Organizer = new Organizer("mailto:doug@ddaysoftware.com");

        //    SerializeTest(iCal, "Event1.xml");
        //}

        [Test, Category("DataContractionSerialization")]
        public void Geo1()
        {
            GeographicLocation g = new GeographicLocation(123.143, 52.1234);
            SerializeTest(g, "Geo1.xml");
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
            p.StartTime = DateTime.Now.AddHours(2);
            p.EndTime = DateTime.Now.AddHours(3);
            SerializeTest(p, "Period1.xml");
        }

        // FIXME: re-implement
        //[Test, Category("DataContractionSerialization")]
        //public void RecurrenceDate1()
        //{
        //    IRecurrenceDate d = new RecurrenceDate();
        //    d.Periods.Add(new Period(DateTime.Now.AddHours(2), DateTime.Now.AddHours(3)));
        //    d.Periods.Add(new Period(DateTime.Now.AddHours(4), DateTime.Now.AddHours(5)));
        //    SerializeTest(d, "RecurrenceDate1.xml");
        //}

        [Test, Category("DataContractionSerialization")]
        public void RecurrencePattern1()
        {
            IRecurrencePattern rp = new RecurrencePattern();
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
        //    Trigger t = new Trigger(TimeSpan.FromHours(-1));
        //    SerializeTest(t, "Trigger1.xml");
        //}
        
        // FIXME: re-implement
        //[Test, Category("DataContractionSerialization")]
        //public void UTCOffset1()
        //{
        //    IUTCOffset o = new UTC_Offset(TimeSpan.FromHours(-6));
        //    SerializeTest(o, "UTCOffset1.xml");
        //}
    }
}
