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
        private iCalendarCollection _Calendars = new iCalendarCollection();
        private DateTime _StartDate;
        private DateTime _EndDate;

        public Schedule()
        {
            InitializeComponent();
        }

        /// <summary>
        /// Loads each iCalendar into our iCalendarCollection.
        /// </summary>        
        private void Schedule_Load(object sender, EventArgs e)
        {
            _Calendars.Add(iCalendar.LoadFromFile(@"Calendars\USHolidays.ics"));
            _Calendars.Add(iCalendar.LoadFromFile(@"Calendars\lotr.ics"));
            _Calendars.Add(iCalendar.LoadFromFile(@"Calendars\To-do.ics"));
            _Calendars.Add(iCalendar.LoadFromFile(@"Calendars\Barça 2006 - 2007.ics"));
        }

        /// <summary>
        /// Occurs each time a new month is selected.
        /// </summary>        
        private void cbMonth_SelectedIndexChanged(object sender, EventArgs e)
        {
            EvaluateSelectedMonth();
            FillEventList();
            FillTodoList();
        }

        /// <summary>
        /// Evaluates the selected month for recurring items
        /// </summary>
        private void EvaluateSelectedMonth()
        {
            int currentMonth = cbMonth.SelectedIndex + 1;

            // First day of the month
            _StartDate = new DateTime(2006, currentMonth, 1);
            // First day of the next month
            _EndDate = _StartDate.AddMonths(1);

            // Evaluate each recurring item in each calendar
            // for the time period we're interested in
            _Calendars.Evaluate(_StartDate, _EndDate);            
        }

        /// <summary>
        /// Fills the todo list with active items for
        /// the selected month.
        /// </summary>
        private void FillTodoList()
        {
            // Clear the list
            clbTodo.Items.Clear();

            DateTime lastDayOfMonth = _EndDate.AddSeconds(-1);
            foreach (Todo todo in _Calendars.Todos)            
            {                
                // Ensure the todo item is active as of 11:59 PM on the last day of the month
                if (todo.IsActive(lastDayOfMonth))
                {
                    clbTodo.Items.Add(todo.Summary.Value);
                }                
            }
        }

        /// <summary>
        /// Fills the event list with active items for
        /// the selected month.
        /// </summary>
        private void FillEventList()
        {
            // Clear the list
            listEvents.Clear();

            // Start at the first day of the month
            DateTime testDate = _StartDate;            
            
            // Cycle through each day of the month
            while (testDate < _EndDate)
            {
                // Cycle through each event
                foreach (Event evt in _Calendars.Events)
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