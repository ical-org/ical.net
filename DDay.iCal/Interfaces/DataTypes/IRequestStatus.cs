using System;
using System.Collections.Generic;
using System.Text;

namespace DDay.iCal
{
    public interface IRequestStatus :
        IEncodableDataType
    {
        string Description { get; set; }
        string ExtraData { get; set; }
        IStatusCode StatusCode { get; set; }
        string Language { get; set; }
    }
}
