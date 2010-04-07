using System;
using System.Collections.Generic;
using System.Text;

namespace DDay.iCal.Serialization
{
    public interface IEncodingStack        
    {
        Encoding Current { get; }
        void Push(Encoding encoding);
        Encoding Pop();
    }
}
