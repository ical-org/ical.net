using System;
using System.Diagnostics;
using System.Collections.Generic;
using System.Text;
using System.Runtime.Serialization;
using DDay.iCal.Serialization;

namespace DDay.iCal
{
    /// <summary>
    /// A class that represents the geographical location of an
    /// <see cref="Event"/> or <see cref="Todo"/> item.
    /// </summary>
    [DebuggerDisplay("{Latitude};{Longitude}")]
#if DATACONTRACT
    [DataContract(Name = "Geo", Namespace = "http://www.ddaysoftware.com/dday.ical/2009/07/")]
#endif
    [Serializable]
    public class Geo : iCalDataType
    {
        #region Private Fields

        private Float m_Latitude;
        private Float m_Longitude;

        #endregion

        #region Public Properties

#if DATACONTRACT
        [DataMember(Order = 1)]
#endif
        public Float Latitude
        {
            get { return m_Latitude; }
            set { m_Latitude = value; }
        }

#if DATACONTRACT
        [DataMember(Order = 2)]
#endif
        public Float Longitude
        {
            get { return m_Longitude; }
            set { m_Longitude = value; }
        }

        #endregion

        #region Constructors

        public Geo() { }
        public Geo(string value) : this()
        {
            CopyFrom(Parse(value));
        }
        public Geo(Float latitude, Float longitude)
        {
            Latitude = latitude;
            Longitude = longitude;
        }

        #endregion

        #region Overrides

        public override bool Equals(object obj)
        {
            if (obj is Geo)
            {
                Geo g = (Geo)obj;
                return g.Latitude.Equals(Latitude) && g.Longitude.Equals(Longitude);
            }
            return base.Equals(obj);
        }

        public override int GetHashCode()
        {
            return Latitude.GetHashCode() ^ Longitude.GetHashCode();
        }

        public override bool TryParse(string value, ref ICalendarObject obj)
        {
            Geo g = (Geo)obj;
            string[] values = value.Split(';');
            if (values.Length != 2)
                return false;

            g.Latitude = new Float();
            g.Longitude = new Float();
            ICalendarObject lat = g.Latitude;
            ICalendarObject lon = g.Longitude;

            if (!g.Latitude.TryParse(values[0], ref lat))
                return false;
            if (!g.Longitude.TryParse(values[1], ref lon))
                return false;

            return true;
        }

        public override void CopyFrom(ICopyable obj)
        {
            base.CopyFrom(obj);
            if (obj is Geo)
            {
                Geo g = (Geo)obj;
                Latitude = g.Latitude;
                Longitude = g.Longitude;
            }
            base.CopyFrom(obj);
        }

        public override string ToString()
        {
            return Latitude.Value.ToString("0.000000") + ";" + Longitude.Value.ToString("0.000000");
        }

        #endregion
    }
}
