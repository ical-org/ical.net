using System;
using System.Collections.Generic;
using System.Text;

namespace DDay.iCal
{
    public interface IGeographicLocation :
        IEncodableDataType
    {
        double Latitude { get; set; }
        double Longitude { get; set; }
    }
}
