using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Collections;

namespace DDay.iCal
{
    /// <summary>
    /// This class takes multiple calendar properties/property values
    /// and consolidates them into a single list.
    /// 
    /// <example>
    /// Consider the following example:
    /// 
    /// BEGIN:VEVENT
    /// CATEGORIES:APPOINTMENT,EDUCATION
    /// CATEGORIES:MEETING
    /// END:EVENT
    /// </example>
    /// 
    /// When we process this event, we don't really care that there
    /// are 2 different CATEGORIES properties, nor do we care that
    /// the first CATEGORIES property has 2 values, whereas the 
    /// second CATEGORIES property only has 1 value.  In the end,
    /// we want a list of 3 values: APPOINTMENT, EDUCATION, and MEETING.
    /// 
    /// This class consolidates properties of a given name into a list,
    /// and allows you to work with those values directly against the
    /// properties themselves.  This preserves the notion that our values
    /// are still stored directly within the calendar property, but gives
    /// us the flexibility to work with multiple properties through a
    /// single (composite) list.
    /// </summary>
    public class CalendarPropertyCompositeValueCollection<T> :
        ICompositeValueCollection<T>
    {
        #region Private Fields

        ICalendarPropertyCollection m_PropertyList;
        string m_PropertyName;

        #endregion

        #region Constructors

        public CalendarPropertyCompositeValueCollection(ICalendarPropertyCollection propertyList, string propertyName)
        {
            m_PropertyList = propertyList;
            m_PropertyName = propertyName;
        }

        #endregion

        #region Private Methods

        IEnumerable<T> FilteredAndCompositeList
        {
            get
            {
                return m_PropertyList
                    .AllOf(m_PropertyName)      // All properties with the matching key
                    .SelectMany(p => p.Values)  // Combine their values to a single list
                    .OfType<T>();               // Filter that list to only include that type we're interested in
            }
        }

        #endregion

        #region ICollection<T> Members

        public void Add(T value)
        {
            // If there's already a property containing an IList<object>
            // as its value, then let's append our value to it
            if (m_PropertyList.ContainsKey(m_PropertyName))
            {
                m_PropertyList[m_PropertyName].AddValue(value);
            }
            else
            {
                // Create a new property to store our item
                CalendarProperty p = new CalendarProperty();
                p.Name = m_PropertyName;

                // Add the value
                p.AddValue(value);

                // Add the new property to the list
                m_PropertyList.Add(p);
            }            
        }

        virtual public void Clear()
        {
            // Remove each value
            FilteredAndCompositeList
                .ToList()
                .ForEach(value => Remove(value));
        }

        virtual public bool Contains(T value)
        {
            return m_PropertyList
                .AllOf(m_PropertyName)               // Any matching property
                .Any(p => p.ContainsValue(value));   // that contains this value.
        }

        virtual public void CopyTo(T[] array, int arrayIndex)
        {
            // Get a list of values of the type we're interested in
            T[] arr = FilteredAndCompositeList.ToArray();

            // Copy these values into the other array.
            arr.CopyTo(array, arrayIndex);
        }

        /// <summary>
        /// Gets a count of the number of values
        /// </summary>
        virtual public int Count
        {
            get
            {
                return FilteredAndCompositeList.Count();
            }
        }

        /// <summary>
        /// Gets whether or not this collection is read-only.
        /// </summary>
        virtual public bool IsReadOnly
        {
            get { return false; }
        }

        /// <summary>
        /// Removes the value from the list of properties.
        /// </summary>
        /// <param name="value">The value to remove.</param>
        /// <returns>True if the value was successfully removed, false otherwise.</returns>
        virtual public bool Remove(T value)
        {
            if (m_PropertyList.ContainsKey(m_PropertyName))
            {
                // Search for a property that contains this value.
                ICalendarProperty property = m_PropertyList
                    .AllOf(m_PropertyName)
                    .Where(p => p.ContainsValue(value))
                    .FirstOrDefault();

                // If we found a property that contains this value, then let's remove it.
                if (property != null)
                {
                    // Remove the value
                    property.RemoveValue(value);

                    // If this property doesn't contain any more values, then
                    // let's also remove the property itself.
                    if (property.Values.Count() == 0)
                        m_PropertyList.Remove(property);

                    return true;
                }
            }
            return false;
        }

        #endregion

        #region IEnumerable<T> Members

        public IEnumerator<T> GetEnumerator()
        {
            return FilteredAndCompositeList.GetEnumerator();
        }

        #endregion

        #region IEnumerable Members

        IEnumerator IEnumerable.GetEnumerator()
        {
            return FilteredAndCompositeList.GetEnumerator();            
        }

        #endregion        
    }
}
