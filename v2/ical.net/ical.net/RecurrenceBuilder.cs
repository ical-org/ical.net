using System;
using NodaTime;

namespace ical.net
{
    internal class RecurrenceBuilder
    {
        //Event starts at 9am, recur daily 7 times
        /// <summary>
        /// 
        /// </summary>
        /// <param name="event"></param>
        /// <param name="frequency"></param>
        /// <param name="recurrenceCount"></param>
        /// <param name="interval"></param>
        /// <returns></returns>
        public static RecurrenceRule RecurEvery(Event @event, Frequency frequency, int recurrenceCount, int interval = 1)
        {
            if (interval <= 0)
            {
                throw new ArgumentException($"{interval} is not a valid interval");
            }

            if (!Enum.IsDefined(typeof(Frequency), frequency))
            {
                throw new ArgumentException($"{frequency} is not a valid Frequency");
            }

            throw new NotImplementedException();
        }

        public static RecurrenceRule RecurUntil()
        {
            throw new NotImplementedException();
        }

        //Recur every other day until DateTime

        //Recur every other month until DateTime
    }
}
