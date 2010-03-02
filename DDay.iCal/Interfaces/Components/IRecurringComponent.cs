using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace DDay.iCal
{
    public interface IRecurringComponent :
        IUniqueComponent
    {
        /// <summary>
        /// Clears a previous evaluation, usually because one of the 
        /// key elements used for evaluation has changed 
        /// (Start, End, Duration, recurrence rules, exceptions, etc.).
        /// </summary>
        void ClearEvaluation();

        /// <summary>
        /// Returns all occurrences of this component that start on the date provided.
        /// All components starting between 12:00:00AM and 11:59:59 PM will be
        /// returned.
        /// <note>
        /// This will first Evaluate() the date range required in order to
        /// determine the occurrences for the date provided, and then return
        /// the occurrences.
        /// </note>
        /// </summary>
        /// <param name="dt">The date for which to return occurrences.</param>
        /// <returns>A list of Periods representing the occurrences of this object.</returns>
        IList<IOccurrence> GetOccurrences(IDateTime dt);

        /// <summary>
        /// Returns all occurrences of this component that start within the date range provided.
        /// All components occurring between <paramref name="startTime"/> and <paramref name="endTime"/>
        /// will be returned.
        /// </summary>
        /// <param name="startTime">The starting date range</param>
        /// <param name="endTime">The ending date range</param>
        IList<IOccurrence> GetOccurrences(IDateTime startTime, IDateTime endTime);

        /// <summary>
        /// Polls alarms for the current evaluation period.  This period is defined by the 
        /// range indicated in EvalStart and EvalEnd properties.  These properties are automatically
        /// set when calling the Evaluate() method with a given date range, and indicate the date
        /// range currently "known" by the recurring component.
        /// </summary>
        /// <returns>A list of AlarmOccurrence objects, representing each alarm that has fired.</returns>
        IList<IAlarmOccurrence> PollAlarms();

        /// <summary>
        /// Polls <see cref="Alarm"/>s for occurrences within the <see cref="Evaluate"/>d
        /// time frame of this <see cref="RecurringComponent"/>.  For each evaluated
        /// occurrence if this component, each <see cref="Alarm"/> is polled for its
        /// corresponding alarm occurrences.
        /// <para>
        /// <example>
        /// The following is an example of polling alarms for an event.
        /// <code>
        /// IICalendar iCal = iCalendar.LoadFromUri(new Uri("http://somesite.com/calendar.ics"));
        /// Event evt = iCal.Events[0];
        ///
        /// // Poll the alarms on the event
        /// List<AlarmOccurrence> alarms = evt.PollAlarms();
        /// 
        /// // Here, you would eliminate alarms that the user has already dismissed.
        /// // This information should be stored somewhere outside of the .ics file.
        /// // You can use the component's UID, and the AlarmOccurence date/time 
        /// // as the primary key for each alarm occurrence.
        /// 
        /// foreach(AlarmOccurrence alarm in alarms)
        ///     MessageBox.Show(alarm.Component.Summary + "\n" + alarm.DateTime);
        /// </code>
        /// </example>
        /// </para>
        /// </summary>
        /// <param name="Start">The earliest allowable alarm occurrence to poll, or <c>null</c>.</param>
        /// <returns>A List of <see cref="Alarm.AlarmOccurrence"/> objects, one for each occurrence of the <see cref="Alarm"/>.</returns>
        IList<IAlarmOccurrence> PollAlarms(IDateTime Start, IDateTime End);
    }
}
