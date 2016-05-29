using System.Collections;
using System.Linq;
using Ical.Net;
using Ical.Net.DataTypes;
using Ical.Net.Interfaces.DataTypes;
using Ical.Net.Interfaces.Evaluation;
using NUnit.Framework;

namespace ical.NET.UnitTests
{
    [TestFixture]
    public class TodoTest
    {
        private const string _tzid = "US-Eastern";

        public void TestTodoActive(string calendar, ArrayList items, params int[] numPeriods)
        {
            var iCal = Calendar.LoadFromFile(@"Calendars\Todo\" + calendar)[0];
            ProgramTest.TestCal(iCal);
            var todo = iCal.Todos[0];
            
            for (var i = 0; i < items.Count; i += 2)
            {
                var dt = (CalDateTime)items[i];                
                dt.TzId = _tzid;

                var tf = (bool)items[i + 1];
                if (tf)
                    Assert.IsTrue(todo.IsActive(dt), "Todo should be active at " + dt);
                else Assert.IsFalse(todo.IsActive(dt), "Todo should not be active at " + dt);
            }

            if (numPeriods != null &&
                numPeriods.Length > 0)
            {
                var evaluator = todo.GetService(typeof(IEvaluator)) as IEvaluator;
                Assert.IsNotNull(evaluator);
                Assert.AreEqual(
                    numPeriods[0],
                    evaluator.Periods.Count,
                    "Todo should have " + numPeriods[0] + " occurrences after evaluation; it had " + evaluator.Periods.Count);
            }
        }

        public void TestTodoCompleted(string calendar, ArrayList items)
        {
            var iCal = Calendar.LoadFromFile(@"Calendars\Todo\" + calendar)[0];
            ProgramTest.TestCal(iCal);
            var todo = iCal.Todos[0];
            
            for (var i = 0; i < items.Count; i += 2)
            {
                var dt = (IDateTime)items[i];
                dt.TzId = _tzid;

                var tf = (bool)items[i + 1];
                if (tf)
                    Assert.IsTrue(todo.IsCompleted(dt), "Todo should be completed at " + dt);
                else Assert.IsFalse(todo.IsCompleted(dt), "Todo should not be completed at " + dt);
            }
        }

        [Test, Category("Todo")]
        public void Todo1()
        {
            var items = new ArrayList();
            items.Add(new CalDateTime(2200, 12, 31, 0, 0, 0)); items.Add(true);

            TestTodoActive("Todo1.ics", items);
        }

        [Test, Category("Todo")]
        public void Todo2()
        {
            var items = new ArrayList();
            items.Add(new CalDateTime(2006, 7, 28, 8, 0, 0)); items.Add(false);
            items.Add(new CalDateTime(2006, 7, 28, 8, 59, 59)); items.Add(false);
            items.Add(new CalDateTime(2006, 7, 28, 9, 0, 0)); items.Add(true);
            items.Add(new CalDateTime(2200, 12, 31, 0, 0, 0)); items.Add(true);

            TestTodoActive("Todo2.ics", items);
        }

        [Test, Category("Todo")]
        public void Todo3()
        {
            var items = new ArrayList();
            items.Add(new CalDateTime(2006, 7, 28, 8, 0, 0)); items.Add(false);
            items.Add(new CalDateTime(2200, 12, 31, 0, 0, 0)); items.Add(false);

            TestTodoActive("Todo3.ics", items);
        }

        [Test, Category("Todo")]
        public void Todo4()
        {
            var items = new ArrayList();
            items.Add(new CalDateTime(2006, 07, 28, 8, 0, 0)); items.Add(true);
            items.Add(new CalDateTime(2006, 07, 28, 9, 0, 0)); items.Add(true);
            items.Add(new CalDateTime(2006, 8, 1, 0, 0, 0)); items.Add(true);

            TestTodoCompleted("Todo4.ics", items);
        }

        [Test, Category("Todo")]
        public void Todo5()
        {
            var items = new ArrayList();
            items.Add(new CalDateTime(2006, 7, 28, 9, 0, 0)); items.Add(false);
            items.Add(new CalDateTime(2006, 7, 29, 9, 0, 0)); items.Add(false);
            items.Add(new CalDateTime(2006, 7, 30, 9, 0, 0)); items.Add(false);
            items.Add(new CalDateTime(2006, 7, 31, 9, 0, 0)); items.Add(false);
            items.Add(new CalDateTime(2006, 8, 1, 9, 0, 0)); items.Add(false);
            items.Add(new CalDateTime(2006, 8, 2, 9, 0, 0)); items.Add(false);
            items.Add(new CalDateTime(2006, 8, 3, 9, 0, 0)); items.Add(false);
            items.Add(new CalDateTime(2006, 8, 4, 9, 0, 0)); items.Add(true);
            items.Add(new CalDateTime(2006, 8, 5, 9, 0, 0)); items.Add(true);
            items.Add(new CalDateTime(2006, 8, 6, 9, 0, 0)); items.Add(true);
            items.Add(new CalDateTime(2006, 8, 7, 9, 0, 0)); items.Add(true);
            items.Add(new CalDateTime(2006, 8, 8, 9, 0, 0)); items.Add(true);

            TestTodoActive("Todo5.ics", items);
        }

        [Test, Category("Todo")]
        public void Todo6()
        {
            var items = new ArrayList();
            items.Add(new CalDateTime(2006, 7, 28, 9, 0, 0)); items.Add(false);
            items.Add(new CalDateTime(2006, 7, 29, 9, 0, 0)); items.Add(false);
            items.Add(new CalDateTime(2006, 7, 30, 9, 0, 0)); items.Add(false);
            items.Add(new CalDateTime(2006, 7, 31, 9, 0, 0)); items.Add(false);
            items.Add(new CalDateTime(2006, 8, 1, 9, 0, 0)); items.Add(false);
            items.Add(new CalDateTime(2006, 8, 2, 9, 0, 0)); items.Add(false);
            items.Add(new CalDateTime(2006, 8, 3, 9, 0, 0)); items.Add(false);
            items.Add(new CalDateTime(2006, 8, 4, 9, 0, 0)); items.Add(true);
            items.Add(new CalDateTime(2006, 8, 5, 9, 0, 0)); items.Add(true);
            items.Add(new CalDateTime(2006, 8, 6, 9, 0, 0)); items.Add(true);
            items.Add(new CalDateTime(2006, 8, 7, 9, 0, 0)); items.Add(true);
            items.Add(new CalDateTime(2006, 8, 8, 9, 0, 0)); items.Add(true);

            TestTodoActive("Todo6.ics", items);
        }

        [Test, Category("Todo")]
        public void Todo7()
        {
            var items = new ArrayList();
            items.Add(new CalDateTime(2006, 7, 28, 9, 0, 0)); items.Add(false);
            items.Add(new CalDateTime(2006, 7, 29, 9, 0, 0)); items.Add(false);
            items.Add(new CalDateTime(2006, 7, 30, 9, 0, 0)); items.Add(false);
            items.Add(new CalDateTime(2006, 7, 31, 9, 0, 0)); items.Add(false);
            items.Add(new CalDateTime(2006, 8, 1, 9, 0, 0)); items.Add(false);
            items.Add(new CalDateTime(2006, 8, 2, 9, 0, 0)); items.Add(false);
            items.Add(new CalDateTime(2006, 8, 3, 9, 0, 0)); items.Add(false);
            items.Add(new CalDateTime(2006, 8, 4, 9, 0, 0)); items.Add(false);
            items.Add(new CalDateTime(2006, 8, 5, 9, 0, 0)); items.Add(false);
            items.Add(new CalDateTime(2006, 8, 6, 9, 0, 0)); items.Add(false);
            items.Add(new CalDateTime(2006, 8, 30, 9, 0, 0)); items.Add(false);
            items.Add(new CalDateTime(2006, 8, 31, 9, 0, 0)); items.Add(false);
            items.Add(new CalDateTime(2006, 8, 31, 9, 0, 0)); items.Add(false);
            items.Add(new CalDateTime(2006, 9, 1, 9, 0, 0)); items.Add(true);
            items.Add(new CalDateTime(2006, 9, 2, 9, 0, 0)); items.Add(true);
            items.Add(new CalDateTime(2006, 9, 3, 9, 0, 0)); items.Add(true);

            TestTodoActive("Todo7.ics", items);
        }

        [Test, Category("Todo")]
        public void Todo7_1()
        {
            var iCal = Calendar.LoadFromFile(@"Calendars\Todo\Todo7.ics")[0];
            var todo = iCal.Todos[0];

            var items = new ArrayList();
            items.Add(new CalDateTime(2006, 7, 28, 9, 0, 0, _tzid)); 
            items.Add(new CalDateTime(2006, 8, 4, 9, 0, 0, _tzid)); 
            items.Add(new CalDateTime(2006, 9, 1, 9, 0, 0, _tzid));
            items.Add(new CalDateTime(2006, 10, 6, 9, 0, 0, _tzid));
            items.Add(new CalDateTime(2006, 11, 3, 9, 0, 0, _tzid));
            items.Add(new CalDateTime(2006, 12, 1, 9, 0, 0, _tzid));
            items.Add(new CalDateTime(2007, 1, 5, 9, 0, 0, _tzid));
            items.Add(new CalDateTime(2007, 2, 2, 9, 0, 0, _tzid));
            items.Add(new CalDateTime(2007, 3, 2, 9, 0, 0, _tzid));
            items.Add(new CalDateTime(2007, 4, 6, 9, 0, 0, _tzid));

            var occurrences = todo.GetOccurrences(
                new CalDateTime(2006, 7, 1, 9, 0, 0),
                new CalDateTime(2007, 7, 1, 9, 0, 0)).OrderBy(o => o.Period.StartTime).ToList();

            // FIXME: Count is not properly restricting recurrences to 10.
            // What's going wrong here?
            Assert.AreEqual(
                items.Count,
                occurrences.Count,
                "TODO should have " + items.Count + " occurrences; it has " + occurrences.Count);

            for (var i = 0; i < items.Count; i++)
                Assert.AreEqual(items[i], occurrences[i].Period.StartTime, "TODO should occur at " + items[i] + ", but does not.");            
        }

        [Test, Category("Todo")]
        public void Todo8()
        {
            var items = new ArrayList();
            items.Add(new CalDateTime(2006, 7, 28, 9, 0, 0)); items.Add(false);
            items.Add(new CalDateTime(2006, 7, 29, 9, 0, 0)); items.Add(false);
            items.Add(new CalDateTime(2006, 7, 30, 9, 0, 0)); items.Add(false);
            items.Add(new CalDateTime(2006, 7, 31, 9, 0, 0)); items.Add(false);
            items.Add(new CalDateTime(2006, 8, 1, 9, 0, 0)); items.Add(false);
            items.Add(new CalDateTime(2006, 8, 2, 9, 0, 0)); items.Add(false);
            items.Add(new CalDateTime(2006, 8, 3, 9, 0, 0)); items.Add(false);
            items.Add(new CalDateTime(2006, 8, 4, 9, 0, 0)); items.Add(false);
            items.Add(new CalDateTime(2006, 8, 5, 9, 0, 0)); items.Add(false);
            items.Add(new CalDateTime(2006, 8, 6, 9, 0, 0)); items.Add(false);
            items.Add(new CalDateTime(2006, 8, 30, 9, 0, 0)); items.Add(false);
            items.Add(new CalDateTime(2006, 8, 31, 9, 0, 0)); items.Add(false);
            items.Add(new CalDateTime(2006, 8, 31, 9, 0, 0)); items.Add(false);
            items.Add(new CalDateTime(2006, 9, 1, 9, 0, 0)); items.Add(false);
            items.Add(new CalDateTime(2006, 9, 2, 9, 0, 0)); items.Add(false);
            items.Add(new CalDateTime(2006, 9, 3, 9, 0, 0)); items.Add(false);
            items.Add(new CalDateTime(2006, 10, 10, 9, 0, 0)); items.Add(false);
            items.Add(new CalDateTime(2006, 11, 15, 9, 0, 0)); items.Add(false);
            items.Add(new CalDateTime(2006, 12, 5, 9, 0, 0)); items.Add(false);
            items.Add(new CalDateTime(2007, 1, 3, 9, 0, 0)); items.Add(false);
            items.Add(new CalDateTime(2007, 1, 4, 9, 0, 0)); items.Add(false);
            items.Add(new CalDateTime(2007, 1, 5, 9, 0, 0)); items.Add(false);
            items.Add(new CalDateTime(2007, 1, 6, 9, 0, 0)); items.Add(false);
            items.Add(new CalDateTime(2007, 1, 7, 9, 0, 0)); items.Add(false);
            items.Add(new CalDateTime(2007, 2, 1, 9, 0, 0)); items.Add(false);
            items.Add(new CalDateTime(2007, 2, 2, 8, 59, 59)); items.Add(false);
            items.Add(new CalDateTime(2007, 2, 2, 9, 0, 0)); items.Add(true);
            items.Add(new CalDateTime(2007, 2, 3, 9, 0, 0)); items.Add(true);
            items.Add(new CalDateTime(2007, 2, 4, 9, 0, 0)); items.Add(true);

            TestTodoActive("Todo8.ics", items);
        }

        [Test, Category("Todo")]
        public void Todo9()
        {
            var items = new ArrayList();
            items.Add(new CalDateTime(2006, 7, 28, 9, 0, 0)); items.Add(false);
            items.Add(new CalDateTime(2006, 7, 29, 9, 0, 0)); items.Add(false);
            items.Add(new CalDateTime(2006, 7, 30, 9, 0, 0)); items.Add(false);
            items.Add(new CalDateTime(2006, 8, 17, 9, 0, 0)); items.Add(false);
            items.Add(new CalDateTime(2006, 8, 18, 9, 0, 0)); items.Add(false);
            items.Add(new CalDateTime(2006, 8, 19, 9, 0, 0)); items.Add(false);
            items.Add(new CalDateTime(2006, 9, 7, 9, 0, 0)); items.Add(false);
            items.Add(new CalDateTime(2006, 9, 8, 8, 59, 59)); items.Add(false);
            items.Add(new CalDateTime(2006, 9, 8, 9, 0, 0)); items.Add(true);
            items.Add(new CalDateTime(2006, 9, 9, 9, 0, 0)); items.Add(true);

            TestTodoActive("Todo9.ics", items, 3);            
        }
    }
}
