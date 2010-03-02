using System;
using System.Collections.Generic;
using System.Text;

namespace DDay.iCal
{
    public class UIDFactory
    {
        virtual public IText New()
        {
            return (Text)Guid.NewGuid().ToString();
        }
    }
}
