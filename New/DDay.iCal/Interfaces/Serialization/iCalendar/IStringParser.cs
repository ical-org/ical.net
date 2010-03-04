using System;
using System.Collections.Generic;
using System.Text;

namespace DDay.iCal
{
    public interface IStringParser
    {
        void Parse(string value, object obj);        
    }
}
