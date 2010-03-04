using System;
using System.Collections.Generic;
using System.Text;

namespace DDay.iCal
{
    public class UIDFactory
    {
        virtual public string New()
        {
            return Guid.NewGuid().ToString();
        }
    }
}
