using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using System.IO;
using System.Text.RegularExpressions;
using System.Runtime.Serialization;

namespace DDay.iCal.Serialization.iCalendar
{
    public class FreeBusyEntrySerializer : 
        PeriodSerializer
    {
        #region Overrides

        public override Type TargetType
        {
            get { return typeof(FreeBusyEntry); }
        }

        public override string SerializeToString(object obj)
        {
            IFreeBusyEntry entry = obj as IFreeBusyEntry;
            if (entry != null)
            {
                switch (entry.Type)
                {
                    case FreeBusyType.Busy:
                        entry.Parameters.Remove("FMTYPE");
                        break;
                    case FreeBusyType.BusyTentative:
                        entry.Parameters.Set("FMTYPE", "BUSY-TENTATIVE");
                        break;
                    case FreeBusyType.BusyUnavailable:
                        entry.Parameters.Set("FMTYPE", "BUSY-UNAVAILABLE");
                        break;
                    case FreeBusyType.Free:
                        entry.Parameters.Set("FMTYPE", "FREE");
                        break;
                }
            }

            return base.SerializeToString(obj);
        }

        public override object Deserialize(TextReader tr)
        {
            IFreeBusyEntry entry = base.Deserialize(tr) as IFreeBusyEntry;
            if (entry != null)
            {
                if (entry.Parameters.ContainsKey("FMTYPE"))
                {
                    string value = entry.Parameters.Get("FMTYPE");
                    if (value != null)
                    {
                        switch (value.ToUpperInvariant())
                        {
                            case "FREE": entry.Type = FreeBusyType.Free; break;
                            case "BUSY": entry.Type = FreeBusyType.Busy; break;
                            case "BUSY-UNAVAILABLE": entry.Type = FreeBusyType.BusyUnavailable; break;
                            case "BUSY-TENTATIVE": entry.Type = FreeBusyType.BusyTentative; break;
                            default: break;
                        }
                    }
                }
            }

            return entry;
        }

        #endregion
    }
}
