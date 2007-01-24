using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using System.IO;
using System.Reflection;
using DDay.iCal.Objects;
using DDay.iCal.DataTypes;

namespace DDay.iCal.Serialization.iCalendar.Objects
{
    public class UniqueComponentSerializer : ComponentBaseSerializer
    {
        #region Constructors

        public UniqueComponentSerializer(UniqueComponent uc) : base(uc) {}
        
        #endregion        
    }
}
