using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using System.Reflection;
using DDay.iCal.Components;
using DDay.iCal.DataTypes;
using DDay.iCal.Serialization;

namespace DDay.iCal.Components
{
    /// <summary>
    /// This class is used by the parsing framework as a factory class
    /// for <see cref="iCalendar"/> components.  Generally, you should
    /// not need to use this class directly.
    /// </summary>
    public class ComponentBase : iCalObject
    {
        public const string ALARM = "VALARM";
        public const string EVENT = "VEVENT";
        public const string FREEBUSY = "VFREEBUSY";
        public const string TODO = "VTODO";
        public const string JOURNAL = "VJOURNAL";
        public const string TIMEZONE = "VTIMEZONE";
        public const string DAYLIGHT = "DAYLIGHT";
        public const string STANDARD = "STANDARD";        

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
                case ALARM: return new Alarm(parent); break;
                case EVENT: return new Event(parent); break;
                case FREEBUSY: return new FreeBusy(parent); break;
                case JOURNAL: return new Journal(parent); break;
                case TIMEZONE: return new DDay.iCal.Components.TimeZone(parent); break;
                case TODO: return new Todo(parent); break;
                case DAYLIGHT:
                case STANDARD:
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
