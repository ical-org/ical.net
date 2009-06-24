using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using System.IO;
using System.Text.RegularExpressions;
using DDay.iCal.Components;
using DDay.iCal.DataTypes;
using DDay.iCal.Serialization.xCal.Components;

namespace DDay.iCal.Serialization.xCal.DataTypes
{
    public class EncodableDataTypeSerializer : FieldSerializer
    {
        #region Private Fields

        private EncodableDataType _DataType;

        #endregion

        #region Constructors

        public EncodableDataTypeSerializer(EncodableDataType dataType) : base(dataType)
        {
            _DataType = dataType;
        }

        #endregion

        #region Protected Methods

        protected string Encode(string value)
        {
            UTF8Encoding encoding = new UTF8Encoding();

            switch (_DataType.Encoding)
            {
                case "BASE64": return Convert.ToBase64String(encoding.GetBytes(value));
                case "7BIT":
                case "8BIT":
                    value = Regex.Replace(value, @"[^\r]\n", "\r\n");
                    value = Regex.Replace(value, @"\r[^\n]", "\r\n");

                    bool is7Bit = _DataType.Encoding.Equals("7BIT");

                    List<byte> data = new List<byte>(encoding.GetBytes(value));
                    for (int i = data.Count - 1; i >= 0; i--)
                    {
                        if (data[i] == 0)
                            data.RemoveAt(i);

                        if (is7Bit && data[i] > 127)
                            data.RemoveAt(i);
                    }

                    return encoding.GetString(data.ToArray());
                default:
                    return value;
            }
        }

        #endregion

        #region Overrides

        public override string SerializeToString()
        {
            return Encode(DataType.ToString());
        }

        public override void Serialize(System.Xml.XmlTextWriter xtw)
        {
            xtw.WriteString(SerializeToString());
        }

        #endregion
    }
}
