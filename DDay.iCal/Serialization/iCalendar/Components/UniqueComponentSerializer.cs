using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using System.IO;
using System.Reflection;
using DDay.iCal.Components;
using DDay.iCal.DataTypes;

namespace DDay.iCal.Serialization.iCalendar.Components
{
    public class UniqueComponentSerializer : ComponentBaseSerializer
    {
        #region Constructors

        public UniqueComponentSerializer(UniqueComponent uc) : base(uc) {}
        
        #endregion        
    }
}
