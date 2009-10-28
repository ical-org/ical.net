using System;
using System.Collections.Generic;
using System.Text;
using NUnit.Framework;
using DDay.iCal.DataTypes;
using System.IO;
using System.Runtime.Serialization;
using System.Collections;
using DDay.iCal.Components;

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
        public void Binary1()
        {
            Binary b = new Binary();
            b.FormatType = "application/binary";
            b.Data = new byte[] { 1, 2, 3, 4 };
            b.Name = "ATTACH";
            SerializeTest(b, "Binary1.xml");
        }

        [Test, Category("DataContractSerialization")]
        public void Cal_Address1()
        {
            Cal_Address ca = new Cal_Address();
            ca.CommonName = "The Postmaster";
            ca.DirectoryEntry = "ldap://host.com:6666/o=eDABC%20Industries,c=3DUS??(cn=3DBJim%20Dolittle)";
            ca.Value = "mailto:test123@test.com";
            SerializeTest(ca, "Cal_Address1.xml");
        }

        [Test, Category("DataContractionSerialization")]
        public void DaySpecifier1()
        {
            DaySpecifier ds = new DaySpecifier(DayOfWeek.Monday, FrequencyOccurrence.First);
            SerializeTest(ds, "DaySpecifier1.xml");
        }

        [Test, Category("DataContractionSerialization")]
        public void Duration1()
        {
            Duration d = TimeSpan.FromHours(1.234);
            SerializeTest(d, "Duration1.xml");
        }

        [Test, Category("DataContractionSerialization")]
        public void Event1()
        {
            iCalendar iCal = new iCalendar();
            
            Event evt = iCal.Create<Event>();
            evt.Summary = "Summary";
            evt.Description = "Description";
            evt.Start = DateTime.Today.AddDays(1).AddHours(8).AddMinutes(30);
            evt.Duration = TimeSpan.FromHours(1);
            evt.AddComment("Comment 1");
            evt.AddComment("Comment 2");
            evt.AddContact("Doug Day (555) 555-5555");
            
            Alarm a = new Alarm();
            a.Trigger = new Trigger(TimeSpan.FromHours(-2));
            a.Action = AlarmAction.Display;
            evt.AddAlarm(a);

            evt.AddAttachment(new byte[] { 1, 2, 3, 4 });
            evt.AddCategory("Work");
            evt.AddRecurrencePattern(new RecurrencePattern("FREQ=DAILY,INTERVAL=3"));
            evt.AddResource("Projector");
            evt.Geo = new Geo(40.766852,-111.963375);
            evt.Location = "SLC Airport";
            evt.Organizer = "mailto:doug@ddaysoftware.com";

            SerializeTest(iCal, "Event1.xml");
        }
        
        [Test, Category("DataContractionSerialization")]
        public void Float1()
        {
            Float f = new Float("1.234");
            SerializeTest(f, "Float1.xml");
        }

        [Test, Category("DataContractionSerialization")]
        public void Geo1()
        {
            Geo g = new Geo(123.143, 52.1234);
            SerializeTest(g, "Geo1.xml");
        }

        [Test, Category("DataContractionSerialization")]
        public void iCalDateTime1()
        {
            iCalDateTime icdt = DateTime.Now;
            SerializeTest(icdt, "iCalDateTime1.xml");
        }

        [Test, Category("DataContractionSerialization")]
        public void Integer1()
        {
            Integer i = 342;
            SerializeTest(i, "Integer1.xml");
        }

        [Test, Category("DataContractionSerialization")]
        public void Period1()
        {
            Period p = new Period();
            p.StartTime = DateTime.Now.AddHours(2);
            p.EndTime = DateTime.Now.AddHours(3);
            SerializeTest(p, "Period1.xml");
        }

        [Test, Category("DataContractionSerialization")]
        public void RecurrenceDates1()
        {
            RecurrenceDates d = new RecurrenceDates();
            d.Periods.Add(new Period(DateTime.Now.AddHours(2), DateTime.Now.AddHours(3)));
            d.Periods.Add(new Period(DateTime.Now.AddHours(4), DateTime.Now.AddHours(5)));
            SerializeTest(d, "RecurrenceDates1.xml");
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

        [Test, Category("DataContractionSerialization")]
        public void Text1()
        {
            Text t = new Text();
            t.Value = "Testing";
            t.SetEncoding(EncodingType.Base64);
            SerializeTest(t, "Text1.xml");
        }
        
        [Test, Category("DataContractionSerialization")]
        public void TextCollection1()
        {
            TextCollection tc = new TextCollection();
            tc.Values.Add("Test1");
            tc.Values.Add("Test2");
            tc.Values.Add("Test3");
            SerializeTest(tc, "TextCollection1.xml");
        }

        [Test, Category("DataContractionSerialization")]
        public void Trigger1()
        {
            Trigger t = new Trigger(TimeSpan.FromHours(-1));
            SerializeTest(t, "Trigger1.xml");
        }

        [Test, Category("DataContractionSerialization")]
        public void TZID1()
        {
            TZID tzid = new TZID("America/Boise");
            SerializeTest(tzid, "TZID1.xml");
        }

        [Test, Category("DataContractionSerialization")]
        public void URI()
        {
            URI uri = "http://www.google.com";
            SerializeTest(uri, "URI1.xml");
        }

        [Test, Category("DataContractionSerialization")]
        public void UTC_Offset1()
        {
            UTC_Offset o = new UTC_Offset(TimeSpan.FromHours(-6));
            SerializeTest(o, "UTC_Offset1.xml");
        }
    }
}
