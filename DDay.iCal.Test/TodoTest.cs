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
    public class TodoTest
    {
        private string tzid;

        [TestFixtureSetUp]
        public void InitAll()
        {
            tzid = "US-Eastern";
        }
                
        public void TestTodoActive(string calendar, ArrayList items, params int[] numPeriods)
        {
            IICalendar iCal = iCalendar.LoadFromFile(@"Calendars\Todo\" + calendar)[0];
            ProgramTest.TestCal(iCal);
            ITodo todo = iCal.Todos[0];
            
            for (int i = 0; i < items.Count; i += 2)
            {
                iCalDateTime dt = (iCalDateTime)items[i];                
                dt.TZID = tzid;

                bool tf = (bool)items[i + 1];
                if (tf)
                    Assert.IsTrue(todo.IsActive(dt), "Todo should be active at " + dt);
                else Assert.IsFalse(todo.IsActive(dt), "Todo should not be active at " + dt);
            }

            if (numPeriods != null &&
                numPeriods.Length > 0)
            {
                IEvaluator evaluator = todo.GetService(typeof(IEvaluator)) as IEvaluator;
                Assert.IsNotNull(evaluator);
                Assert.AreEqual(
                    numPeriods[0],
                    evaluator.Periods.Count,
                    "Todo should have " + numPeriods[0] + " occurrences after evaluation; it had " + evaluator.Periods.Count);
            }
        }

        public void TestTodoCompleted(string calendar, ArrayList items)
        {
            IICalendar iCal = iCalendar.LoadFromFile(@"Calendars\Todo\" + calendar)[0];
            ProgramTest.TestCal(iCal);
            ITodo todo = iCal.Todos[0];
            
            for (int i = 0; i < items.Count; i += 2)
            {
                IDateTime dt = (IDateTime)items[i];
                dt.TZID = tzid;

                bool tf = (bool)items[i + 1];
                if (tf)
                    Assert.IsTrue(todo.IsCompleted(dt), "Todo should be completed at " + dt);
                else Assert.IsFalse(todo.IsCompleted(dt), "Todo should not be completed at " + dt);
            }
        }

        [Test, Category("Todo")]
        public void Todo1()
        {
            ArrayList items = new ArrayList();
            items.Add(new iCalDateTime(2200, 12, 31, 0, 0, 0)); items.Add(true);

            TestTodoActive("Todo1.ics", items);
        }

        [Test, Category("Todo")]
        public void Todo2()
        {
            ArrayList items = new ArrayList();
            items.Add(new iCalDateTime(2006, 7, 28, 8, 0, 0)); items.Add(false);
            items.Add(new iCalDateTime(2006, 7, 28, 8, 59, 59)); items.Add(false);
            items.Add(new iCalDateTime(2006, 7, 28, 9, 0, 0)); items.Add(true);
            items.Add(new iCalDateTime(2200, 12, 31, 0, 0, 0)); items.Add(true);

            TestTodoActive("Todo2.ics", items);
        }

        [Test, Category("Todo")]
        public void Todo3()
        {
            ArrayList items = new ArrayList();
            items.Add(new iCalDateTime(2006, 7, 28, 8, 0, 0)); items.Add(false);
            items.Add(new iCalDateTime(2200, 12, 31, 0, 0, 0)); items.Add(false);

            TestTodoActive("Todo3.ics", items);
        }

        [Test, Category("Todo")]
        public void Todo4()
        {
            ArrayList items = new ArrayList();
            items.Add(new iCalDateTime(2006, 07, 28, 8, 0, 0)); items.Add(true);
            items.Add(new iCalDateTime(2006, 07, 28, 9, 0, 0)); items.Add(true);
            items.Add(new iCalDateTime(2006, 8, 1, 0, 0, 0)); items.Add(true);

            TestTodoCompleted("Todo4.ics", items);
        }

        [Test, Category("Todo")]
        public void Todo5()
        {
            ArrayList items = new ArrayList();
            items.Add(new iCalDateTime(2006, 7, 28, 9, 0, 0)); items.Add(false);
            items.Add(new iCalDateTime(2006, 7, 29, 9, 0, 0)); items.Add(false);
            items.Add(new iCalDateTime(2006, 7, 30, 9, 0, 0)); items.Add(false);
            items.Add(new iCalDateTime(2006, 7, 31, 9, 0, 0)); items.Add(false);
            items.Add(new iCalDateTime(2006, 8, 1, 9, 0, 0)); items.Add(false);
            items.Add(new iCalDateTime(2006, 8, 2, 9, 0, 0)); items.Add(false);
            items.Add(new iCalDateTime(2006, 8, 3, 9, 0, 0)); items.Add(false);
            items.Add(new iCalDateTime(2006, 8, 4, 9, 0, 0)); items.Add(true);
            items.Add(new iCalDateTime(2006, 8, 5, 9, 0, 0)); items.Add(true);
            items.Add(new iCalDateTime(2006, 8, 6, 9, 0, 0)); items.Add(true);
            items.Add(new iCalDateTime(2006, 8, 7, 9, 0, 0)); items.Add(true);
            items.Add(new iCalDateTime(2006, 8, 8, 9, 0, 0)); items.Add(true);

            TestTodoActive("Todo5.ics", items);
        }

        [Test, Category("Todo")]
        public void Todo6()
        {
            ArrayList items = new ArrayList();
            items.Add(new iCalDateTime(2006, 7, 28, 9, 0, 0)); items.Add(false);
            items.Add(new iCalDateTime(2006, 7, 29, 9, 0, 0)); items.Add(false);
            items.Add(new iCalDateTime(2006, 7, 30, 9, 0, 0)); items.Add(false);
            items.Add(new iCalDateTime(2006, 7, 31, 9, 0, 0)); items.Add(false);
            items.Add(new iCalDateTime(2006, 8, 1, 9, 0, 0)); items.Add(false);
            items.Add(new iCalDateTime(2006, 8, 2, 9, 0, 0)); items.Add(false);
            items.Add(new iCalDateTime(2006, 8, 3, 9, 0, 0)); items.Add(false);
            items.Add(new iCalDateTime(2006, 8, 4, 9, 0, 0)); items.Add(true);
            items.Add(new iCalDateTime(2006, 8, 5, 9, 0, 0)); items.Add(true);
            items.Add(new iCalDateTime(2006, 8, 6, 9, 0, 0)); items.Add(true);
            items.Add(new iCalDateTime(2006, 8, 7, 9, 0, 0)); items.Add(true);
            items.Add(new iCalDateTime(2006, 8, 8, 9, 0, 0)); items.Add(true);

            TestTodoActive("Todo6.ics", items);
        }

        [Test, Category("Todo")]
        public void Todo7()
        {
            ArrayList items = new ArrayList();
            items.Add(new iCalDateTime(2006, 7, 28, 9, 0, 0)); items.Add(false);
            items.Add(new iCalDateTime(2006, 7, 29, 9, 0, 0)); items.Add(false);
            items.Add(new iCalDateTime(2006, 7, 30, 9, 0, 0)); items.Add(false);
            items.Add(new iCalDateTime(2006, 7, 31, 9, 0, 0)); items.Add(false);
            items.Add(new iCalDateTime(2006, 8, 1, 9, 0, 0)); items.Add(false);
            items.Add(new iCalDateTime(2006, 8, 2, 9, 0, 0)); items.Add(false);
            items.Add(new iCalDateTime(2006, 8, 3, 9, 0, 0)); items.Add(false);
            items.Add(new iCalDateTime(2006, 8, 4, 9, 0, 0)); items.Add(false);
            items.Add(new iCalDateTime(2006, 8, 5, 9, 0, 0)); items.Add(false);
            items.Add(new iCalDateTime(2006, 8, 6, 9, 0, 0)); items.Add(false);
            items.Add(new iCalDateTime(2006, 8, 30, 9, 0, 0)); items.Add(false);
            items.Add(new iCalDateTime(2006, 8, 31, 9, 0, 0)); items.Add(false);
            items.Add(new iCalDateTime(2006, 8, 31, 9, 0, 0)); items.Add(false);
            items.Add(new iCalDateTime(2006, 9, 1, 9, 0, 0)); items.Add(true);
            items.Add(new iCalDateTime(2006, 9, 2, 9, 0, 0)); items.Add(true);
            items.Add(new iCalDateTime(2006, 9, 3, 9, 0, 0)); items.Add(true);

            TestTodoActive("Todo7.ics", items);
        }

        [Test, Category("Todo")]
        public void Todo7_1()
        {
            IICalendar iCal = iCalendar.LoadFromFile(@"Calendars\Todo\Todo7.ics")[0];
            ITodo todo = iCal.Todos[0];

            ArrayList items = new ArrayList();
            items.Add(new iCalDateTime(2006, 7, 28, 9, 0, 0, tzid)); 
            items.Add(new iCalDateTime(2006, 8, 4, 9, 0, 0, tzid)); 
            items.Add(new iCalDateTime(2006, 9, 1, 9, 0, 0, tzid));
            items.Add(new iCalDateTime(2006, 10, 6, 9, 0, 0, tzid));
            items.Add(new iCalDateTime(2006, 11, 3, 9, 0, 0, tzid));
            items.Add(new iCalDateTime(2006, 12, 1, 9, 0, 0, tzid));
            items.Add(new iCalDateTime(2007, 1, 5, 9, 0, 0, tzid));
            items.Add(new iCalDateTime(2007, 2, 2, 9, 0, 0, tzid));
            items.Add(new iCalDateTime(2007, 3, 2, 9, 0, 0, tzid));
            items.Add(new iCalDateTime(2007, 4, 6, 9, 0, 0, tzid));

            IList<Occurrence> occurrences = todo.GetOccurrences(
                new iCalDateTime(2006, 7, 1, 9, 0, 0),
                new iCalDateTime(2007, 7, 1, 9, 0, 0));

            // FIXME: Count is not properly restricting recurrences to 10.
            // What's going wrong here?
            Assert.AreEqual(
                items.Count,
                occurrences.Count,
                "TODO should have " + items.Count + " occurrences; it has " + occurrences.Count);

            for (int i = 0; i < items.Count; i++)
                Assert.AreEqual(items[i], occurrences[i].Period.StartTime, "TODO should occur at " + items[i] + ", but does not.");            
        }

        [Test, Category("Todo")]
        public void Todo8()
        {
            ArrayList items = new ArrayList();
            items.Add(new iCalDateTime(2006, 7, 28, 9, 0, 0)); items.Add(false);
            items.Add(new iCalDateTime(2006, 7, 29, 9, 0, 0)); items.Add(false);
            items.Add(new iCalDateTime(2006, 7, 30, 9, 0, 0)); items.Add(false);
            items.Add(new iCalDateTime(2006, 7, 31, 9, 0, 0)); items.Add(false);
            items.Add(new iCalDateTime(2006, 8, 1, 9, 0, 0)); items.Add(false);
            items.Add(new iCalDateTime(2006, 8, 2, 9, 0, 0)); items.Add(false);
            items.Add(new iCalDateTime(2006, 8, 3, 9, 0, 0)); items.Add(false);
            items.Add(new iCalDateTime(2006, 8, 4, 9, 0, 0)); items.Add(false);
            items.Add(new iCalDateTime(2006, 8, 5, 9, 0, 0)); items.Add(false);
            items.Add(new iCalDateTime(2006, 8, 6, 9, 0, 0)); items.Add(false);
            items.Add(new iCalDateTime(2006, 8, 30, 9, 0, 0)); items.Add(false);
            items.Add(new iCalDateTime(2006, 8, 31, 9, 0, 0)); items.Add(false);
            items.Add(new iCalDateTime(2006, 8, 31, 9, 0, 0)); items.Add(false);
            items.Add(new iCalDateTime(2006, 9, 1, 9, 0, 0)); items.Add(false);
            items.Add(new iCalDateTime(2006, 9, 2, 9, 0, 0)); items.Add(false);
            items.Add(new iCalDateTime(2006, 9, 3, 9, 0, 0)); items.Add(false);
            items.Add(new iCalDateTime(2006, 10, 10, 9, 0, 0)); items.Add(false);
            items.Add(new iCalDateTime(2006, 11, 15, 9, 0, 0)); items.Add(false);
            items.Add(new iCalDateTime(2006, 12, 5, 9, 0, 0)); items.Add(false);
            items.Add(new iCalDateTime(2007, 1, 3, 9, 0, 0)); items.Add(false);
            items.Add(new iCalDateTime(2007, 1, 4, 9, 0, 0)); items.Add(false);
            items.Add(new iCalDateTime(2007, 1, 5, 9, 0, 0)); items.Add(false);
            items.Add(new iCalDateTime(2007, 1, 6, 9, 0, 0)); items.Add(false);
            items.Add(new iCalDateTime(2007, 1, 7, 9, 0, 0)); items.Add(false);
            items.Add(new iCalDateTime(2007, 2, 1, 9, 0, 0)); items.Add(false);
            items.Add(new iCalDateTime(2007, 2, 2, 8, 59, 59)); items.Add(false);
            items.Add(new iCalDateTime(2007, 2, 2, 9, 0, 0)); items.Add(true);
            items.Add(new iCalDateTime(2007, 2, 3, 9, 0, 0)); items.Add(true);
            items.Add(new iCalDateTime(2007, 2, 4, 9, 0, 0)); items.Add(true);

            TestTodoActive("Todo8.ics", items);
        }

        [Test, Category("Todo")]
        public void Todo9()
        {
            ArrayList items = new ArrayList();
            items.Add(new iCalDateTime(2006, 7, 28, 9, 0, 0)); items.Add(false);
            items.Add(new iCalDateTime(2006, 7, 29, 9, 0, 0)); items.Add(false);
            items.Add(new iCalDateTime(2006, 7, 30, 9, 0, 0)); items.Add(false);
            items.Add(new iCalDateTime(2006, 8, 17, 9, 0, 0)); items.Add(false);
            items.Add(new iCalDateTime(2006, 8, 18, 9, 0, 0)); items.Add(false);
            items.Add(new iCalDateTime(2006, 8, 19, 9, 0, 0)); items.Add(false);
            items.Add(new iCalDateTime(2006, 9, 7, 9, 0, 0)); items.Add(false);
            items.Add(new iCalDateTime(2006, 9, 8, 8, 59, 59)); items.Add(false);
            items.Add(new iCalDateTime(2006, 9, 8, 9, 0, 0)); items.Add(true);
            items.Add(new iCalDateTime(2006, 9, 9, 9, 0, 0)); items.Add(true);

            TestTodoActive("Todo9.ics", items, 3);            
        }

        // FIXME: re-implement
        //[Test, Category("Todo")]
        //public void TODO10()
        //{
        //    iCalendar iCal = new iCalendar();
        //    Todo todo = iCal.Create<Todo>();

        //    todo.Summary = "xxxx";
        //    todo.Description = "fdsdsfds";

        //    // Set Start & Due date
        //    todo.DTStart = new iCalDateTime(2007, 1, 1, 8, 0, 0);
        //    todo.Due = new iCalDateTime(2007, 1, 7);
        //    todo.Created = new iCalDateTime(DateTime.SpecifyKind(new DateTime(2007, 1, 1), DateTimeKind.Utc));
        //    todo.DTStamp = new iCalDateTime(DateTime.SpecifyKind(new DateTime(2007, 1, 1), DateTimeKind.Utc));
        //    todo.UID = "b6709c95-5523-46aa-a7e5-1b5ea034b86a";

        //    // Create an alarm
        //    Alarm al = new Alarm();
        //    al.Trigger = new Trigger(TimeSpan.FromMinutes(-30));
        //    al.Action = AlarmAction.Display;
        //    al.Description = "Reminder";

        //    // Add the alarm to the todo item
        //    todo.Alarms.Add(al);

        //    // Save into calendar file.
        //    iCalendarSerializer serializer = new iCalendarSerializer();
        //    string serializedTodo = serializer.SerializeToString(iCal);

        //    Assert.AreEqual(
        //        "BEGIN:VCALENDAR\r\nVERSION:2.0\r\nPRODID:-//ddaysoftware.com//NONSGML DDay.iCal 1.0//EN\r\nBEGIN:VTODO\r\nCREATED:20070101T000000Z\r\nDESCRIPTION:fdsdsfds\r\nDTSTAMP:20070101T000000Z\r\nDTSTART:20070101T080000\r\nDUE;VALUE=DATE:20070107\r\nSEQUENCE:0\r\nSTATUS:NEEDS-ACTION\r\nSUMMARY:xxxx\r\nUID:b6709c95-5523-46aa-a7e5-1b5ea034b86a\r\nBEGIN:VALARM\r\nACTION:DISPLAY\r\nDESCRIPTION:Reminder\r\nTRIGGER:-PT30M\r\nEND:VALARM\r\nEND:VTODO\r\nEND:VCALENDAR\r\n",
        //        serializedTodo);

        //    List<AlarmOccurrence> alarms = todo.PollAlarms(
        //        new iCalDateTime(2007, 1, 1),
        //        new iCalDateTime(2007, 2, 1));
                        
        //    iCalDateTime expectedAlarm = new iCalDateTime(2007, 1, 1, 7, 30, 0);
        //    Assert.AreEqual(1, alarms.Count, "There should be exactly 1 alarm");
        //    Assert.AreEqual(expectedAlarm, alarms[0].DateTime, "The alarm should occur at " + expectedAlarm);
        //}

        // FIXME: re-implement
        //[Test, Category("Todo")]
        //public void TODO11()
        //{
        //    iCalendar iCal = new iCalendar();
        //    Todo todo = iCal.Create<Todo>();

        //    todo.Summary = "xxxx";
        //    todo.Description = "fdsdsfds";

        //    // Set Start & Due date
        //    todo.DTStart = new iCalDateTime(2007, 1, 1, 8, 0, 0);
        //    todo.Due = new iCalDateTime(2007, 1, 7);
        //    todo.Created = new iCalDateTime(DateTime.SpecifyKind(new DateTime(2007, 1, 1), DateTimeKind.Utc));
        //    todo.DTStamp = new iCalDateTime(DateTime.SpecifyKind(new DateTime(2007, 1, 1), DateTimeKind.Utc));
        //    todo.UID = "b6709c95-5523-46aa-a7e5-1b5ea034b86a";

        //    // Add an alarm in my task 
        //    Alarm al = new Alarm(todo);
        //    al.Action = AlarmAction.Display;
        //    al.Description = "Reminder";
        //    al.Trigger = new Trigger();

        //    // Set reminder time 
        //    al.Trigger.DateTime = new DateTime(2007, 1, 6, 8, 0, 0);

        //    // Save into calendar file.
        //    iCalendarSerializer serializer = new iCalendarSerializer();
        //    string serializedTodo = serializer.SerializeToString(iCal);

        //    Assert.AreEqual(
        //        "BEGIN:VCALENDAR\r\nVERSION:2.0\r\nPRODID:-//ddaysoftware.com//NONSGML DDay.iCal 1.0//EN\r\nBEGIN:VTODO\r\nCREATED:20070101T000000Z\r\nDESCRIPTION:fdsdsfds\r\nDTSTAMP:20070101T000000Z\r\nDTSTART:20070101T080000\r\nDUE;VALUE=DATE:20070107\r\nSEQUENCE:0\r\nSTATUS:NEEDS-ACTION\r\nSUMMARY:xxxx\r\nUID:b6709c95-5523-46aa-a7e5-1b5ea034b86a\r\nBEGIN:VALARM\r\nACTION:DISPLAY\r\nDESCRIPTION:Reminder\r\nTRIGGER;VALUE=DATE-TIME:20070106T080000\r\nEND:VALARM\r\nEND:VTODO\r\nEND:VCALENDAR\r\n",
        //        serializedTodo);
        //}
    }
}
