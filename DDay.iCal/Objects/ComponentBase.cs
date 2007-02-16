using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using System.Reflection;
using DDay.iCal.Components;
using DDay.iCal.DataTypes;
using DDay.iCal.Serialization;

namespace DDay.iCal.Objects
{
    /// <summary>
    /// This class is used by the parsing framework as a factory class
    /// for <see cref="iCalendar"/> components.  Generally, you should
    /// not need to use this class directly.
    /// </summary>
    public class ComponentBase : iCalObject
    {
        #region Constructors

        public ComponentBase() : base() { }
        public ComponentBase(iCalObject parent) : base(parent) { }
        public ComponentBase(iCalObject parent, string name) : base(parent, name) { }

        #endregion

        #region Static Public Methods

        static public ComponentBase Create(iCalObject parent, string name)
        {
            switch(name.ToUpper())
            {
                case "VALARM": return new Alarm(parent); break;
                case "VEVENT": return new Event(parent); break;
                case "VFREEBUSY": return new FreeBusy(parent); break;
                case "VJOURNAL": return new Journal(parent); break;
                case "VTIMEZONE": return new DDay.iCal.Components.TimeZone(parent); break;
                case "VTODO": return new Todo(parent); break;
                case "DAYLIGHT":
                case "STANDARD":
                    return new DDay.iCal.Components.TimeZone.TimeZoneInfo(name.ToUpper(), parent); break;
                default: return new ComponentBase(parent, name); break;
            }
        }

        #endregion

        #region Overrides

        protected override System.Collections.Generic.List<object> SerializedItems
        {
            get
            {
                //
                // Get a list of all the public fields and properties
                // that are marked as Serialized.
                //
                List<object> List = new List<object>();
                foreach (FieldInfo fi in GetType().GetFields())
                    if (fi.GetCustomAttributes(typeof(SerializedAttribute), true).Length > 0)
                        List.Add(fi);
                foreach (PropertyInfo pi in GetType().GetProperties())
                    if (pi.GetCustomAttributes(typeof(SerializedAttribute), true).Length > 0)
                        List.Add(pi);
                return List;
            }
        }

        #endregion
    }
}
