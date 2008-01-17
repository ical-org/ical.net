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

        /// <summary>
        /// Returns 12:00 AM of the first day of the current month
        /// </summary>
        public DateTime StartOfMonth
        {
            get { return new DateTime(2006, cbMonth.SelectedIndex + 1, 1); }            
        }

        /// <summary>
        /// Returns 11:59 PM of the last day of the current month
        /// </summary>
        public DateTime EndOfMonth
        {
            get { return StartOfMonth.AddMonths(1).AddSeconds(-1); }
        }


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
            FillEventList();
            FillTodoList();
        }

        /// <summary>
        /// Fills the todo list with active items for
        /// the selected month.
        /// </summary>
        private void FillTodoList()
        {
            // Clear the list
            clbTodo.Items.Clear();
                        
            foreach (Todo todo in _Calendars.Todos)            
            {
                // Ensure the todo item is active as of 11:59 PM on the last day of the month
                if (todo.IsActive(EndOfMonth))
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
            // Clear our list of items
            listEvents.Items.Clear();

            // Get a list of event occurrences from each of our calendars.            
            List<Occurrence> occurrences = _Calendars.GetOccurrences<Event>(StartOfMonth, EndOfMonth);

            // Iterate through each occurrence
            foreach (Occurrence o in occurrences)
            {
                // Cast the component to an event
                Event evt = o.Component as Event;
                if (evt != null)
                {
                    // Make sure the event is active (hasn't been cancelled)
                    if (evt.IsActive())
                    {
                        // Get a string that represents our event
                        string summary = o.Period.StartTime.ToString("d") + " - " + evt.Summary.Value;
                        if (evt.IsAllDay)
                            summary += " (All Day)";
                        
                        // Add the occurrence to the list view
                        listEvents.Items.Add(new ListViewItem(summary));
                    }
                }
            }
        }
    }
}