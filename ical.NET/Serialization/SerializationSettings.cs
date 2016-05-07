using System;
using Ical.Net.Interfaces.Serialization;

namespace Ical.Net.Serialization
{
    public class SerializationSettings :
        ISerializationSettings
    {
        #region Private Fields

        private Type _mICalendarType = typeof(Net.Calendar);        
        private bool _mEnsureAccurateLineNumbers;
        private ParsingModeType _mParsingMode = ParsingModeType.Strict;
        private bool _mStoreExtraSerializationData;        

        #endregion

        #region ISerializationSettings Members

        virtual public Type ICalendarType
        {
            get { return _mICalendarType; }
            set { _mICalendarType = value; }
        }

        virtual public bool EnsureAccurateLineNumbers
        {
            get { return _mEnsureAccurateLineNumbers; }
            set { _mEnsureAccurateLineNumbers = value; }
        }

        virtual public ParsingModeType ParsingMode
        {
            get { return _mParsingMode; }
            set { _mParsingMode = value; }
        }

        virtual public bool StoreExtraSerializationData
        {
            get { return _mStoreExtraSerializationData; }
            set { _mStoreExtraSerializationData = value; }
        }

        #endregion
    }
}
