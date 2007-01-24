using System;
using System.Diagnostics;
using System.Collections.Generic;
using System.Text;

namespace DDay.iCal.DataTypes
{
    /// <summary>
    /// A class that represents the geographical location of an
    /// <see cref="Event"/> or <see cref="Todo"/> item.
    /// </summary>
    [DebuggerDisplay("{Latitude};{Longitude}")]
    public class Geo : iCalDataType
    {
        #region Private Fields

        private Float m_Latitude;
        private Float m_Longitude;

        #endregion

        #region Public Properties

        public Float Latitude
        {
            get { return m_Latitude; }
            set { m_Latitude = value; }
        }

        public Float Longitude
        {
            get { return m_Longitude; }
            set { m_Longitude = value; }
        }

        #endregion

        #region Constructors

        public Geo() { }
        public Geo(string value)
            : this()
        {
            CopyFrom(Parse(value));
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

        public override bool TryParse(string value, ref object obj)
        {
            Geo g = (Geo)obj;
            string[] values = value.Split(';');
            if (values.Length != 2)
                return false;

            g.Latitude = new Float();
            g.Longitude = new Float();
            object lat = g.Latitude;
            object lon = g.Longitude;

            if (!g.Latitude.TryParse(values[0], ref lat))
                return false;
            if (!g.Longitude.TryParse(values[1], ref lon))
                return false;

            return true;
        }

        public override void CopyFrom(object obj)
        {
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
