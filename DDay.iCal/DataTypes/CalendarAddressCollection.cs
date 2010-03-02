using System;
using System.ComponentModel;
using System.Diagnostics;
using System.Collections.Generic;
using System.Text;
using System.Text.RegularExpressions;
using System.Collections;
using System.Runtime.Serialization;

namespace DDay.iCal
{
    /// <summary>
    /// Contains a collection of <see cref="CalendarAddress"/> objects.
    /// This is a collection of CalendarAddress objects surrounded by
    /// double quotes, and delimited by commas.
    /// <example>
    ///     For example, <c>MEMBER="mailto:projectA@example.com","mailto:projectB@example.com"</c>
    /// </example>
    /// </summary>
#if DATACONTRACT
    [DataContract(Name = "CalendarAddressCollection", Namespace = "http://www.ddaysoftware.com/dday.ical/2009/07/")]
#endif
    [Serializable]
    public class CalendarAddressCollection : iCalDataType, ICollection
    {
        #region Private Fields

        private List<CalendarAddress> m_Values;        

        #endregion

        #region Public Properties

#if DATACONTRACT
        [DataMember(Order = 1)]
#endif
        virtual public List<CalendarAddress> Values
        {
            get { return m_Values; }
            set { m_Values = value; }
        }

        #endregion

        #region Constructors

        public CalendarAddressCollection() { Values = new List<CalendarAddress>(); }
        public CalendarAddressCollection(string value)
            : this()
        {
            CopyFrom(Parse(value));            
        }

        #endregion

        #region Overrides

        public override bool Equals(object obj)
        {
            if (obj is CalendarAddressCollection)
            {
                CalendarAddressCollection cac = (CalendarAddressCollection)obj;                
                for (int i = 0; i < Values.Count; i++)
                {
                    if (!Values[i].Equals(cac.Values[i]))
                        return false;
                }
                return true;
            }
            else if (obj is CalendarAddress)
            {
                if (Values.Count == 1 && Values[0].Equals(obj))
                    return true;
            }
            return base.Equals(obj);
        }

        public override int GetHashCode()
        {
            int hashCode = 0;
            foreach (CalendarAddress ca in Values)
                hashCode ^= ca.GetHashCode();
            return hashCode;
        }

        public override string ToString()
        {
            string[] values = new string[Values.Count];
            for (int i = 0; i < Values.Count; i++)
                values[i] = Values[i].Value;

            return "\"" + string.Join("\",\"", values) + "\"";
        }

        public override void CopyFrom(ICopyable obj)
        {
            base.CopyFrom(obj);
            if (obj is CalendarAddressCollection)
            {
                CalendarAddressCollection cac = (CalendarAddressCollection)obj;
                CalendarAddress[] array = new CalendarAddress[cac.Values.Count];
                cac.CopyTo(array, 0);
                
                Values.Clear();
                Values.AddRange(array);
            }
            base.CopyFrom(obj);
        }

        public override bool TryParse(string value, ref ICalendarDataType obj)
        {
            CalendarAddressCollection cac = obj as CalendarAddressCollection;
            if (cac != null)
            {
                MatchCollection matches = Regex.Matches(value,
                    "(^|,)(\"(?<calendarAddress>[^\\\\]*?)\")?");
                cac.Values.Clear();

                foreach (Match match in matches)
                {
                    if (match.Success)
                        cac.Values.Add(new CalendarAddress(match.Groups["calendarAddress"].Value));
                    else return false;
                }

                return true;
            }
            return false;
        }

        #endregion

        #region Public Accessors

        [Browsable(false)]
        public CalendarAddress this[object obj]
        {
            get
            {                
                if (obj is int)
                    return Values[(int)obj];
                else return null;
            }
        }

        #endregion

        #region ICollection Members

        [Browsable(false)]
        public void CopyTo(Array array, int index)
        {
            if (array.GetType().GetElementType() == typeof(CalendarAddress))
                Values.CopyTo((CalendarAddress[])array, index);
            else throw new ArgumentException("Array must be a CalendarAddress array", "array");
        }

        [Browsable(false)]
        public int Count
        {
            get { return Values.Count; }
        }

        [Browsable(false)]
        public bool IsSynchronized
        {
            get { return false; }
        }

        [Browsable(false)]
        public object SyncRoot
        {
            get { return this; }
        }

        #endregion

        #region IEnumerable Members

        public IEnumerator GetEnumerator()
        {
            return Values.GetEnumerator();
        }

        #endregion
    }
}
