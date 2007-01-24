using System;
using System.Collections.Generic;
using System.Text;
using System.Collections;
using System.IO;
using System.Resources;
using System.Web;
using System.Web.UI;
using DDay.iCal.Components;
using DDay.iCal.DataTypes;
using System.Reflection;
using System.Text.RegularExpressions;
using NUnit.Framework;

namespace DDay.iCal.Test
{
    [TestFixture]
    public class Todo
    {
        private TZID tzid;

        [TestFixtureSetUp]
        public void InitAll()
        {
            tzid = new TZID("US-Eastern");
        }

        static public void DoTests()
        {
            Todo t = new Todo();
            t.InitAll();
            t.TODO1();
            t.TODO2();
            t.TODO3();
            t.TODO4();
            t.TODO5();
            t.TODO6();
            t.TODO7();
        }
                
        public void TestTodoActive(string calendar, ArrayList items)
        {
            iCalendar iCal = iCalendar.LoadFromFile(@"Calendars\Todo\" + calendar);
            Program.TestCal(iCal);
            DDay.iCal.Components.Todo todo = (DDay.iCal.Components.Todo)iCal.Todos[0];
            todo.Evaluate(new Date_Time(2006, 7, 28, tzid, iCal), new Date_Time(2010, 1, 1, tzid, iCal));

            for (int i = 0; i < items.Count; i += 2)
            {
                Date_Time dt = (Date_Time)items[i];
                dt.iCalendar = iCal;
                dt.TZID = tzid;

                bool tf = (bool)items[i + 1];
                if (tf)
                    Assert.IsTrue(todo.IsActive(dt), "Todo should be active at " + dt);
                else Assert.IsFalse(todo.IsActive(dt), "Todo should not be active at " + dt);
            }
        }

        public void TestTodoCompleted(string calendar, ArrayList items)
        {
            iCalendar iCal = iCalendar.LoadFromFile(@"Calendars\Todo\" + calendar);
            Program.TestCal(iCal);
            DDay.iCal.Components.Todo todo = (DDay.iCal.Components.Todo)iCal.Todos[0];
            todo.Evaluate(new Date_Time(2006, 7, 28, tzid, iCal), new Date_Time(2010, 1, 1, tzid, iCal));

            for (int i = 0; i < items.Count; i += 2)
            {
                Date_Time dt = (Date_Time)items[i];
                dt.iCalendar = iCal;
                dt.TZID = tzid;

                bool tf = (bool)items[i + 1];
                if (tf)
                    Assert.IsTrue(todo.IsCompleted(dt), "Todo should be completed at " + dt);
                else Assert.IsFalse(todo.IsCompleted(dt), "Todo should not be completed at " + dt);
            }
        }

        [Test, Category("Todo")]
        public void TODO1()
        {
            ArrayList items = new ArrayList();
            items.Add(new Date_Time(2200, 12, 31, 0, 0, 0)); items.Add(true);

            TestTodoActive("TODO1.ics", items);
        }

        [Test, Category("Todo")]
        public void TODO2()
        {
            ArrayList items = new ArrayList();
            items.Add(new Date_Time(2006, 7, 28, 8, 0, 0)); items.Add(false);
            items.Add(new Date_Time(2006, 7, 28, 8, 59, 59)); items.Add(false);
            items.Add(new Date_Time(2006, 7, 28, 9, 0, 0)); items.Add(true);
            items.Add(new Date_Time(2200, 12, 31, 0, 0, 0)); items.Add(true);

            TestTodoActive("TODO2.ics", items);
        }

        [Test, Category("Todo")]
        public void TODO3()
        {
            ArrayList items = new ArrayList();
            items.Add(new Date_Time(2006, 7, 28, 8, 0, 0)); items.Add(false);
            items.Add(new Date_Time(2200, 12, 31, 0, 0, 0)); items.Add(false);

            TestTodoActive("TODO3.ics", items);
        }

        [Test, Category("Todo")]
        public void TODO4()
        {
            ArrayList items = new ArrayList();
            items.Add(new Date_Time(2006, 07, 28, 8, 0, 0)); items.Add(true);
            items.Add(new Date_Time(2006, 07, 28, 9, 0, 0)); items.Add(true);
            items.Add(new Date_Time(2006, 8, 1, 0, 0, 0)); items.Add(true);

            TestTodoCompleted("TODO4.ics", items);
        }

        [Test, Category("Todo")]
        public void TODO5()
        {
            ArrayList items = new ArrayList();
            items.Add(new Date_Time(2006, 7, 28, 9, 0, 0)); items.Add(false);
            items.Add(new Date_Time(2006, 7, 29, 9, 0, 0)); items.Add(false);
            items.Add(new Date_Time(2006, 7, 30, 9, 0, 0)); items.Add(false);
            items.Add(new Date_Time(2006, 7, 31, 9, 0, 0)); items.Add(false);
            items.Add(new Date_Time(2006, 8, 1, 9, 0, 0)); items.Add(false);
            items.Add(new Date_Time(2006, 8, 2, 9, 0, 0)); items.Add(false);
            items.Add(new Date_Time(2006, 8, 3, 9, 0, 0)); items.Add(false);
            items.Add(new Date_Time(2006, 8, 4, 9, 0, 0)); items.Add(true);
            items.Add(new Date_Time(2006, 8, 5, 9, 0, 0)); items.Add(true);
            items.Add(new Date_Time(2006, 8, 6, 9, 0, 0)); items.Add(true);
            items.Add(new Date_Time(2006, 8, 7, 9, 0, 0)); items.Add(true);
            items.Add(new Date_Time(2006, 8, 8, 9, 0, 0)); items.Add(true);

            TestTodoActive("TODO5.ics", items);
        }

        [Test, Category("Todo")]
        public void TODO6()
        {
            ArrayList items = new ArrayList();
            items.Add(new Date_Time(2006, 7, 28, 9, 0, 0)); items.Add(false);
            items.Add(new Date_Time(2006, 7, 29, 9, 0, 0)); items.Add(false);
            items.Add(new Date_Time(2006, 7, 30, 9, 0, 0)); items.Add(false);
            items.Add(new Date_Time(2006, 7, 31, 9, 0, 0)); items.Add(false);
            items.Add(new Date_Time(2006, 8, 1, 9, 0, 0)); items.Add(false);
            items.Add(new Date_Time(2006, 8, 2, 9, 0, 0)); items.Add(false);
            items.Add(new Date_Time(2006, 8, 3, 9, 0, 0)); items.Add(false);
            items.Add(new Date_Time(2006, 8, 4, 9, 0, 0)); items.Add(true);
            items.Add(new Date_Time(2006, 8, 5, 9, 0, 0)); items.Add(true);
            items.Add(new Date_Time(2006, 8, 6, 9, 0, 0)); items.Add(true);
            items.Add(new Date_Time(2006, 8, 7, 9, 0, 0)); items.Add(true);
            items.Add(new Date_Time(2006, 8, 8, 9, 0, 0)); items.Add(true);

            TestTodoActive("TODO6.ics", items);
        }

        [Test, Category("Todo")]
        public void TODO7()
        {
            ArrayList items = new ArrayList();
            items.Add(new Date_Time(2006, 7, 28, 9, 0, 0)); items.Add(false);
            items.Add(new Date_Time(2006, 7, 29, 9, 0, 0)); items.Add(false);
            items.Add(new Date_Time(2006, 7, 30, 9, 0, 0)); items.Add(false);
            items.Add(new Date_Time(2006, 7, 31, 9, 0, 0)); items.Add(false);
            items.Add(new Date_Time(2006, 8, 1, 9, 0, 0)); items.Add(false);
            items.Add(new Date_Time(2006, 8, 2, 9, 0, 0)); items.Add(false);
            items.Add(new Date_Time(2006, 8, 3, 9, 0, 0)); items.Add(false);
            items.Add(new Date_Time(2006, 8, 4, 9, 0, 0)); items.Add(false);
            items.Add(new Date_Time(2006, 8, 5, 9, 0, 0)); items.Add(false);
            items.Add(new Date_Time(2006, 8, 6, 9, 0, 0)); items.Add(false);
            items.Add(new Date_Time(2006, 8, 30, 9, 0, 0)); items.Add(false);
            items.Add(new Date_Time(2006, 8, 31, 9, 0, 0)); items.Add(false);
            items.Add(new Date_Time(2006, 8, 31, 9, 0, 0)); items.Add(false);
            items.Add(new Date_Time(2006, 9, 1, 9, 0, 0)); items.Add(true);
            items.Add(new Date_Time(2006, 9, 2, 9, 0, 0)); items.Add(true);
            items.Add(new Date_Time(2006, 9, 3, 9, 0, 0)); items.Add(true);

            TestTodoActive("TODO7.ics", items);
        }
    }
}
