using System;
using System.Collections.Generic;
using System.Text;

namespace DDay.iCal
{
    public interface IOrganizer :
        IEncodableDataType
    {
        Uri SentBy { get; set; }
        string CommonName { get; set; }
        Uri DirectoryEntry { get; set; }
        Uri Value { get; set; }
    }
}
