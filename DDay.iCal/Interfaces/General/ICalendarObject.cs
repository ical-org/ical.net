using System;
using System.Collections.Generic;
using System.Text;

namespace DDay.iCal
{
    public interface ICalendarObject :
        IKeyedObject<string>,
        ILoadable,
        ICopyable,        
        IServiceProvider
    {
        event EventHandler<ObjectEventArgs<ICalendarObject>> ChildAdded;
        event EventHandler<ObjectEventArgs<ICalendarObject>> ChildRemoved;

        /// <summary>
        /// The name of the calendar object.
        /// Every calendar object can be assigned
        /// a name.
        /// </summary>
        string Name { get; set; }

        /// <summary>
        /// Returns the parent of this object.
        /// </summary>
        ICalendarObject Parent { get; set; }

        /// <summary>
        /// Returns a list of children of this object.
        /// </summary>
        IList<ICalendarObject> Children { get; }

        /// <summary>
        /// Returns the iCalendar that this object
        /// is associated with.
        /// </summary>
        IICalendar Calendar { get; }
        IICalendar iCalendar { get; }

        /// <summary>
        /// Returns the line number where this calendar
        /// object was found during parsing.
        /// </summary>
        int Line { get; set; }

        /// <summary>
        /// Returns the column number where this calendar
        /// object was found during parsing.
        /// </summary>
        int Column { get; set; }

        /// <summary>
        /// Adds a child object to the current object.
        /// </summary>
        void AddChild(ICalendarObject child);

        /// <summary>
        /// Removes a child object from the current object.
        /// </summary>
        void RemoveChild(ICalendarObject child);
    }
}
