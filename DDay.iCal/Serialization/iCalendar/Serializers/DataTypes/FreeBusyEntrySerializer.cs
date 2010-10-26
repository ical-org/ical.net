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
                switch (entry.Status)
                {
                    case FreeBusyStatus.Busy:
                        entry.Parameters.Remove("FMTYPE");
                        break;
                    case FreeBusyStatus.BusyTentative:
                        entry.Parameters.Set("FMTYPE", "BUSY-TENTATIVE");
                        break;
                    case FreeBusyStatus.BusyUnavailable:
                        entry.Parameters.Set("FMTYPE", "BUSY-UNAVAILABLE");
                        break;
                    case FreeBusyStatus.Free:
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
                            case "FREE": entry.Status = FreeBusyStatus.Free; break;
                            case "BUSY": entry.Status = FreeBusyStatus.Busy; break;
                            case "BUSY-UNAVAILABLE": entry.Status = FreeBusyStatus.BusyUnavailable; break;
                            case "BUSY-TENTATIVE": entry.Status = FreeBusyStatus.BusyTentative; break;
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
