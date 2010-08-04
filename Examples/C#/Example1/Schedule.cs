using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;
using System.Globalization;
using DDay.iCal;

namespace Example1
{
    public partial class Schedule : Form
    {
        private bool _ConvertToLocalTime = false;
        private iCalendarCollection _Calendars = new iCalendarCollection();

        /// <summary>
        /// Returns 12:00 AM of the first day of the current month
        /// </summary>
        public IDateTime StartOfMonth
        {
            get { return new iCalDateTime(2006, cbMonth.SelectedIndex + 1, 1); }            
        }

        /// <summary>
        /// Returns 11:59:59 PM of the last day of the current month
        /// </summary>
        public IDateTime EndOfMonth
        {
            get { return StartOfMonth.AddMonths(1).AddTicks(-1); }
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
            _Calendars.AddRange(iCalendar.LoadFromFile(@"Calendars\USHolidays.ics"));
            _Calendars.AddRange(iCalendar.LoadFromFile(@"Calendars\lotr.ics"));
            _Calendars.AddRange(iCalendar.LoadFromFile(@"Calendars\To-do.ics"));
            _Calendars.AddRange(iCalendar.LoadFromFile(@"Calendars\Barça 2006 - 2007.ics"));
        }

        /// <summary>
        /// Refreshes the display of events/todo items.
        /// </summary>
        private void RefreshDisplay()
        {
            FillEventList();
            FillTodoList();
        }

        /// <summary>
        /// Occurs each time a new month is selected.
        /// </summary>        
        private void cbMonth_SelectedIndexChanged(object sender, EventArgs e)
        {
            RefreshDisplay();
        }

        /// <summary>
        /// Occurs when the checkbox for showing events in local time changes.
        /// </summary>
        private void cbLocalTime_CheckedChanged(object sender, EventArgs e)
        {
            _ConvertToLocalTime = cbLocalTime.Checked;
            RefreshDisplay();
        }

        private string GetEventString(Occurrence o, IEvent evt)
        {
            // Get a string that represents our event
            string summary = o.Period.StartTime.ToString("d") + " - " + evt.Summary;
            if (evt.IsAllDay)
                summary += " (All Day)";
            else
            {
                string startTime = _ConvertToLocalTime ?
                    o.Period.StartTime.Local.ToString("t") :
                    o.Period.StartTime.ToString("t") + " " + o.Period.StartTime.TimeZoneName;
                string endTime = _ConvertToLocalTime ?
                    o.Period.EndTime.Local.ToString("t") :
                    o.Period.EndTime.ToString("t") + " " + o.Period.EndTime.TimeZoneName;

                summary += " (" + startTime + " to " + endTime + ")";
            }

            return summary;
        }

        /// <summary>
        /// Fills the todo list with active items for
        /// the selected month.
        /// </summary>
        private void FillTodoList()
        {
            // Clear the list
            clbTodo.Items.Clear();

            foreach (IICalendar iCal in _Calendars)
            {
                foreach (ITodo todo in iCal.Todos)
                {
                    // Ensure the todo item is active as of 11:59 PM on the last day of the month
                    if (todo.IsActive(EndOfMonth))
                    {
                        clbTodo.Items.Add(todo.Summary);
                    }
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
            IList<Occurrence> occurrences = _Calendars.GetOccurrences<IEvent>(StartOfMonth, EndOfMonth);

            // Iterate through each occurrence
            foreach (Occurrence o in occurrences)
            {
                // Cast the component to an event
                IEvent evt = o.Source as IEvent;
                if (evt != null)
                {
                    // Make sure the event is active (hasn't been cancelled)
                    if (evt.IsActive())
                    {
                        // Add the occurrence to the list view
                        listEvents.Items.Add(new ListViewItem(GetEventString(o, evt)));
                    }
                }
            }
        }        
    }
}