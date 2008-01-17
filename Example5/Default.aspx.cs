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

    /// <summary>
    /// The absolute path to the folder that contains our iCalendars
    /// </summary>
    protected string _CalendarAbsPath;

    /// <summary>
    /// A list of calendars that have been loaded from file
    /// </summary>
    protected iCalendarCollection _Calendars = null;

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

        // Get a list of todays events and upcoming events
        List<Occurrence> todaysEvents = GetTodaysEvents();
        List<Occurrence> upcomingEvents = GetUpcomingEvents();

        // Bind our list to the repeater that will display the events.
        TodaysEvents.DataSource = todaysEvents;
        TodaysEvents.DataBind();

        // Bind our list to the repeater that will display the events.
        UpcomingEvents.DataSource = upcomingEvents;
        UpcomingEvents.DataBind();
    }

    #endregion

    #region Protected Methods

    /// <summary>
    /// Gets a list of iCalendars that are in the "Calendars" directory.
    /// </summary>
    /// <returns>
    /// A list of filenames, without extensions, of the iCalendars 
    /// in the "Calendars" directory
    /// </returns>
    protected IEnumerable<string> LoadCalendarList()
    {
        foreach (string file in Directory.GetFiles(_CalendarAbsPath, "*.ics"))
            yield return Path.GetFileNameWithoutExtension(file);
    }

    /// <summary>
    /// Loads and parses the selected calendars.
    /// </summary>
    protected void LoadSelectedCalendars()
    {
        _Calendars = new iCalendarCollection();
        foreach (ListItem li in CalendarList.Items)
        {
            // Make sure the item is selected
            if (li.Selected)
            {
                // Load the calendar from the file system
                _Calendars.Add(iCalendar.LoadFromFile(Path.Combine(_CalendarAbsPath, li.Text + @".ics")));                
            }
        }
    }

    /// <summary>
    /// Gets a list of events that occur today.
    /// </summary>
    /// <returns>A list of events that occur today</returns>
    protected List<Occurrence> GetTodaysEvents()
    {
        // Load selected calendars, if we haven't already
        if (_Calendars == null)
            LoadSelectedCalendars();

        // Get all event occurrences for today
        return _Calendars.GetOccurrences<Event>(DateTime.Today);            
    }

    /// <summary>
    /// Gets a list of upcoming events (event that will occur within the
    /// next week).
    /// </summary>
    /// <returns>A list of events that will occur within the next week</returns>
    protected List<Occurrence> GetUpcomingEvents()
    {
        // Load selected calendars, if we haven't already
        if (_Calendars == null)
            LoadSelectedCalendars();

        int daysInFuture = Convert.ToInt32(ddlDaysInFuture.SelectedValue);

        // Determine the range of events we're interested in (1 week)
        DateTime startDate = DateTime.Today.AddDays(1);                           // 12:00:00 A.M. tomorrow
        DateTime endDate = DateTime.Today.AddDays(daysInFuture+1).AddSeconds(-1); // 11:59:59 P.M. on the last day

        List<Occurrence> occurrences = _Calendars.GetOccurrences<Event>(DateTime.Today, DateTime.Today.AddDays(90));

        // Get all upcoming events for the next week
        return _Calendars.GetOccurrences<Event>(startDate, endDate);
    }

    /// <summary>
    /// Returns a string representation of the start
    /// time of an event.
    /// </summary>
    /// <param name="obj">The event for which to display the start time</param>
    /// <returns>A string representation of the start time of an event</returns>
    protected string GetTimeDisplay(object obj)
    {
        Occurrence occurrence = obj as Occurrence;
        if (occurrence != null)
        {
            Event evt = occurrence.Component as Event;
            if (evt != null)
            {
                if (evt.IsAllDay)
                    return "All Day";
                else return evt.Start.Local.ToString("h:mm tt");
            }
        }
        return string.Empty;
    }

    #endregion
}
