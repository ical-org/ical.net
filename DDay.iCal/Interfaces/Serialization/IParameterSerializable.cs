using System;
using System.Collections.Generic;
using System.Text;
using DDay.iCal;

namespace DDay.iCal.Serialization
{
    /// <summary>
    /// Indicates that the property can serialize parameters.
    /// </summary>
    public interface IParameterSerializable
    {
        List<ICalendarParameter> Parameters { get; }
        List<ICalendarParameter> DisallowedParameters { get; }
    }
}
