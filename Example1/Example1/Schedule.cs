using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;
using System.Globalization;
using DDay.iCal;
using DDay.iCal.Components;
using DDay.iCal.DataTypes;

namespace Example1
{
    public partial class Schedule : Form
    {
        private iCalendar iCal;

        public Schedule()
        {
            InitializeComponent();            
        }

        private void Schedule_Load(object sender, EventArgs e)
        {
            iCal = iCalendar.LoadFromFile(@"Calendars\USHolidays.ics");
            iCal.MergeWith(iCalendar.LoadFromFile(@"Calendars\lotr.ics"));
            iCal.MergeWith(iCalendar.LoadFromFile(@"Calendars\To-do.ics"));
            iCal.MergeWith(iCalendar.LoadFromFile(@"Calendars\Barça 2006 - 2007.ics"));
            if (iCal == null)
                throw new ApplicationException("iCalendar could not be loaded.");
        }

        private void cbMonth_SelectedIndexChanged(object sender, EventArgs e)
        {
            FillEventList();
            FillTodoList();
        }

        private void FillTodoList()
        {
            int currentMonth = cbMonth.SelectedIndex + 1;

            // First day of the month
            DateTime start = new DateTime(DateTime.Now.Year, currentMonth, 1);
            // First day of the next month
            DateTime end = start.AddMonths(1);

            // For each todo, determine if there are any occurrences in the selected month
            foreach (Todo todo in iCal.Todos)
                todo.Evaluate(start, end);

            // Clear the list
            clbTodo.Items.Clear();

            // Our test date is the last day of the month, at 11:59:59 PM
            DateTime testDate = new DateTime(DateTime.Now.Year, currentMonth, 1).AddMonths(1).AddSeconds(-1);
            foreach (Todo todo in iCal.Todos)
            {
                if (todo.IsActive(testDate))
                {
                    clbTodo.Items.Add(todo.Summary.Value);
                }
            }
        }

        private void FillEventList()
        {
            int currentMonth = cbMonth.SelectedIndex + 1;

            // First day of the month
            DateTime start = new DateTime(DateTime.Now.Year, currentMonth, 1);
            // First day of the next month
            DateTime end = start.AddMonths(1);

            // For each event, determine if there are any occurrences in the selected month
            foreach (Event evt in iCal.Events)
                evt.Evaluate(start, end);

            // Clear the list
            listEvents.Clear();

            // Start at the first day of the month
            DateTime testDate = new DateTime(DateTime.Now.Year, currentMonth, 1);

            // Cycle through each day of the month
            while (testDate < end)
            {
                // Cycle through each event
                foreach (Event evt in iCal.Events)
                {
                    // Determine if the event occurs on the current test date
                    if (evt.IsActive() && evt.OccursOn(testDate))
                    {
                        // Add an item to our list view
                        string summary = testDate.ToShortDateString() + " - " + evt.Summary.Value;
                        if (evt.IsAllDay)
                            summary += " (All Day)";
                        listEvents.Items.Add(new ListViewItem(summary));
                    }
                }

                // Move to the next day
                testDate = testDate.AddDays(1);
            }
        }
    }
}