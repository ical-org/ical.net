using System;
using System.Data;
using System.Configuration;
using System.Collections.Generic;
using System.IO;
using System.Web;
using System.Web.Security;
using System.Web.UI;
using System.Web.UI.WebControls;
using System.Web.UI.WebControls.WebParts;
using System.Web.UI.HtmlControls;

// Required DDay.iCal namespaces
using DDay.iCal;
using DDay.iCal.Components;

public partial class _Default : System.Web.UI.Page
{
    #region Protected Fields

    protected string _CalendarAbsPath;
    protected List<iCalendar> _Calendars = null;

    #endregion

    #region Event Sorting Class

    class EventDateSorter : IComparer<Event>
    {
        #region IComparer<Event> Members

        public int Compare(Event x, Event y)
        {
            return x.Start.CompareTo(y.Start);
        }

        #endregion
    }

    #endregion

    #region Event Handlers

    protected void Page_Load(object sender, EventArgs e)
    {
        _CalendarAbsPath = MapPath("~/Calendars");

        if (!IsPostBack)
        {
            // Load our list of available calendars
            CalendarList.DataSource = LoadCalendarList();
            CalendarList.DataBind();

            // Select all calendars in the list by default
            foreach (ListItem li in CalendarList.Items)
                li.Selected = true;            
        }

        // Create an object that will sort our events by date
        EventDateSorter eventDateSorter = new EventDateSorter();

        // Build a list of today's events, sort that list by date
        // and bind it to the repeater that will display the events.
        List<Event> todaysEvents = new List<Event>(GetTodaysEvents());
        todaysEvents.Sort(eventDateSorter);
        TodaysEvents.DataSource = todaysEvents;
        TodaysEvents.DataBind();

        // Build a list of upcoming events, sort that list by date
        // and bind it to the repeater that will display the events.
        List<Event> upcomingEvents = new List<Event>(GetUpcomingEvents());
        upcomingEvents.Sort(eventDateSorter);
        UpcomingEvents.DataSource = upcomingEvents;
        UpcomingEvents.DataBind();
    }

    #endregion

    #region Protected Methods

    protected IEnumerable<string> LoadCalendarList()
    {        
        foreach (string file in Directory.GetFiles(_CalendarAbsPath, "*.ics"))
            yield return Path.GetFileNameWithoutExtension(file);
    }

    protected void LoadSelectedCalendars()
    {
        _Calendars = new List<iCalendar>();
        foreach (ListItem li in CalendarList.Items)
        {
            if (li.Selected)
            {
                iCalendar iCal = iCalendar.LoadFromFile(Path.Combine(_CalendarAbsPath, li.Text + @".ics"));
                if (iCal != null)
                    _Calendars.Add(iCal);
            }
        }
    }

    protected IEnumerable<Event> GetTodaysEvents()
    {
        // Load selected calendars, if we haven't already
        if (_Calendars == null)
            LoadSelectedCalendars();

        // Evaluate today's date to see if any events occur on it
        foreach (iCalendar iCal in _Calendars)
        {
            iCal.Evaluate(DateTime.Today, DateTime.Today.AddDays(1));

            // Iterate through each event in the calendar
            foreach(Event evt in iCal.Events)
            {
                // Check to see if the event occurs today
                if (evt.OccursOn(DateTime.Today))
                    yield return evt;
            }
        }
    }

    protected IEnumerable<Event> GetUpcomingEvents()
    {
        // Load selected calendars, if we haven't already
        if (_Calendars == null)
            LoadSelectedCalendars();

        // Evaluate events within the next week to see if any events occur
        foreach (iCalendar iCal in _Calendars)
        {
            iCal.Evaluate(DateTime.Today.AddDays(1), DateTime.Today.AddDays(8));

            for (DateTime dt = DateTime.Today.AddDays(1); dt < DateTime.Today.AddDays(8); dt = dt.AddDays(1))
            {
                // Iterate through each event in the calendar
                foreach (Event evt in iCal.Events)
                {
                    // Check to see if the event occurs on the day we're testing
                    if (evt.OccursOn(dt))
                    {
                        // Return a flattened occurence of the event,
                        // so the start time is equal to the time it
                        // recurred                        
                        yield return evt.FlattenRecurrenceOn(dt);
                    }
                }
            }
        }
    }

    #endregion
}
