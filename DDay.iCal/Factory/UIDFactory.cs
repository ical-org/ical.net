using System;
using System.Collections.Generic;
using System.Text;

namespace DDay.iCal
{
    public class UIDFactory
    {
        virtual public string Build()
        {
            return Guid.NewGuid().ToString();
        }
    }
}
