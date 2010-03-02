using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace DDay.iCal
{
    public delegate void UIDChangedEventHandler(object sender, IText OldUID, IText NewUID);

    public interface IUniqueComponent :
        ICalendarComponent
    {
        /// <summary>
        /// Fires when the UID of the component has changed.
        /// </summary>
        event UIDChangedEventHandler UIDChanged;

        IText UID { get; set; }
    }
}
