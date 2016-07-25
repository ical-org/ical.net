using System.Collections.Generic;
using Ical.Net.Interfaces.DataTypes;
using Ical.Net.Interfaces.Evaluation;

namespace Ical.Net.Interfaces.Components
{
    public interface IRecurringComponent : IUniqueComponent, IRecurrable, IAlarmContainer
    {
        IList<IAttachment> Attachments { get; set; }
        IList<string> Categories { get; set; }
        string Class { get; set; }
        IList<string> Contacts { get; set; }
        /// <summary>
        ///The UTC date/time that the calendar information was created by the calendar user agent in the calendar store.This is analogous to the creation date and time for a file in the file system. 
        /// </summary>
        IDateTime Created { get; set; }
        string Description { get; set; }
        /// <summary>
        /// the UTC date/time that the information associated with the calendar component was last revised in the calendar store. Note: This is analogous to the modification date and time for a file in the file system
        /// </summary>
        IDateTime LastModified { get; set; }
        int Priority { get; set; }
        IList<string> RelatedComponents { get; set; }
        int Sequence { get; set; }
        string Summary { get; set; }
    }
}