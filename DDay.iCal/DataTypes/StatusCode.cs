using System;
using System.IO;
using DDay.iCal.Serialization.iCalendar;

namespace DDay.iCal
{    
    /// <summary>
    /// An iCalendar status code.
    /// </summary>
#if !SILVERLIGHT
    [Serializable]
#endif
    public class StatusCode : 
        EncodableDataType,
        IStatusCode
    {
        #region Private Fields

        private int[] m_Parts;

        #endregion

        #region Public Properties

        public int[] Parts
        {
            get { return m_Parts; }
            set { m_Parts = value; }
        }

        public int Primary
        {
            get
            {
                if (m_Parts.Length > 0)
                    return m_Parts[0];
                return 0;
            }
        }

        public int Secondary
        {
            get
            {
                if (m_Parts.Length > 1)
                    return m_Parts[1];
                return 0;
            }
        }

        public int Tertiary
        {
            get
            {
                if (m_Parts.Length > 2)
                    return m_Parts[2];
                return 0;
            }
        }

        #endregion

        #region Constructors

        public StatusCode() { }
        public StatusCode(string value)
            : this()
        {
            StatusCodeSerializer serializer = new StatusCodeSerializer();
            CopyFrom(serializer.Deserialize(new StringReader(value)) as ICopyable);
        }

        #endregion

        #region Overrides

        public override void CopyFrom(ICopyable obj)
        {
            base.CopyFrom(obj);
            if (obj is IStatusCode)
            {
                IStatusCode sc = (IStatusCode)obj;
                Parts = new int[sc.Parts.Length];
                sc.Parts.CopyTo(Parts, 0);
            }
        }

        public override string ToString()
        {
            StatusCodeSerializer serializer = new StatusCodeSerializer();
            return serializer.SerializeToString(this);
        }

        protected bool Equals(StatusCode other)
        {
            return Equals(m_Parts, other.m_Parts);
        }

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj))
            {
                return false;
            }
            if (ReferenceEquals(this, obj))
            {
                return true;
            }
            if (obj.GetType() != this.GetType())
            {
                return false;
            }
            return Equals((StatusCode) obj);
        }

        public override int GetHashCode()
        {
            return (m_Parts != null ? m_Parts.GetHashCode() : 0);
        }

        #endregion
    }
}
