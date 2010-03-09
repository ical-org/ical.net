using System;
using System.Collections.Generic;
using System.Text;

namespace DDay.iCal.Serialization
{
    public interface IEncodingProvider
    {
        string Encode(string encoding, byte[] data);
        string DecodeString(string encoding, string value);
        byte[] DecodeData(string encoding, string value);
    }
}
