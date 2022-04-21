using System;
using System.Collections.Generic;
using System.Text;
using System.Text.RegularExpressions;

namespace Ical.Net.Serialization
{
	internal class EncodingProvider : IEncodingProvider
    {
        public delegate string EncoderDelegate(byte[] data);

        public delegate byte[] DecoderDelegate(string value);

        private readonly SerializationContext _mSerializationContext;

        public EncodingProvider(SerializationContext ctx)
        {
            _mSerializationContext = ctx;
        }

        protected byte[] Decode7Bit(string value)
        {
            try
            {
                var utf7 = new UTF7Encoding();
                return utf7.GetBytes(value);
            }
            catch
            {
                return null;
            }
        }

        protected byte[] Decode8Bit(string value)
        {
            try
            {
                var utf8 = new UTF8Encoding();
                return utf8.GetBytes(value);
            }
            catch
            {
                return null;
            }
        }

        protected byte[] DecodeBase64(string value)
        {
            try
            {
                return Convert.FromBase64String(value);
            }
            catch
            {
                return null;
            }
        }

		protected byte[] DecodeQuotedPrintable(string value)
		{
			var outBuffer = new List<byte>();
			for (var i = 0; i < value.Length; i++)
			{
				if (value[i] != '=')
				{
					outBuffer.Add((byte)value[i]);
					continue;
				}

				var hex = value.Substring(i + 1, 2);
				i += 2;

				if (hex == "\r\n")
					continue;

				if (!isValidHex(hex))
				{
					/* Not wrapping or valid hex, just pass it through - when getting the value, other code tends to strip out the value, meaning this code parses:
					 *    <p>Normal   0         false   false   false                             Mic=rosoftInternetExplorer4</p>=0D=0A<br class=3D"clear" />
					 *
					 * When the source has:
					 *    <p>Normal   0         false   false   false                             Mic=
					 *     rosoftInternetExplorer4</p>=0D=0A<br class=3D"clear" />
					 *
					 * So we just want to drop the = and pass the other characters through.
					 */
					foreach (var c in hex)
						outBuffer.Add((byte)c);
					continue;
				}
                
				var codePoint = Convert.ToInt32(hex, 16);
				outBuffer.Add(Convert.ToByte(codePoint));
			}

			return outBuffer.ToArray();
		}

		private static Regex hexRegex = new Regex("[0-9a-f]{2}", RegexOptions.Compiled | RegexOptions.IgnoreCase);
        private static bool isValidHex(string test)
        {
	        return hexRegex.IsMatch(test);
        }

        protected virtual DecoderDelegate GetDecoderFor(string encoding)
        {
            if (encoding == null)
            {
                return null;
            }

            switch (encoding.ToUpper())
            {
                case "7BIT":
                    return Decode7Bit;
                case "8BIT":
                    return Decode8Bit;
                case "BASE64":
                    return DecodeBase64;
                case "QUOTED-PRINTABLE":
                    // Not technically permitted under RFC, but GoToTraining uses it anyway, so let's play it safe.
	                return DecodeQuotedPrintable;
                default:
                    return null;
            }
        }

        protected string Encode7Bit(byte[] data)
        {
            try
            {
                var utf7 = new UTF7Encoding();
                return utf7.GetString(data);
            }
            catch
            {
                return null;
            }
        }

        protected string Encode8Bit(byte[] data)
        {
            try
            {
                var utf8 = new UTF8Encoding();
                return utf8.GetString(data);
            }
            catch
            {
                return null;
            }
        }

        protected string EncodeBase64(byte[] data)
        {
            try
            {
                return Convert.ToBase64String(data);
            }
            catch
            {
                return null;
            }
        }

        protected string EncodeQuotedPrintable(byte[] data)
        {
	        var sb = new StringBuilder();
	        foreach (var b in data)
	        {
		        if (b >= 33 && b <= 126 && b != 61)
		        {
			        sb.Append(Convert.ToChar(b));
			        continue;
		        }

		        sb.Append($"={b:X2}");
	        }

	        return sb.ToString();
        }

        protected virtual EncoderDelegate GetEncoderFor(string encoding)
        {
            if (encoding == null)
            {
                return null;
            }

            switch (encoding.ToUpper())
            {
                case "7BIT":
                    return Encode7Bit;
                case "8BIT":
                    return Encode8Bit;
                case "BASE64":
                    return EncodeBase64;
                case "QUOTED-PRINTABLE":
                    // Not technically permitted under RFC, but GoToTraining uses it anyway, so let's play it safe.
	                return EncodeQuotedPrintable;
                default:
                    return null;
            }
        }

        public string Encode(string encoding, byte[] data)
        {
            if (encoding == null || data == null)
            {
                return null;
            }

            var encoder = GetEncoderFor(encoding);
            //var wrapped = TextUtil.FoldLines(encoder?.Invoke(data));
            //return wrapped;
            return encoder?.Invoke(data);
        }

        public string DecodeString(string encoding, string value)
        {
            if (encoding == null || value == null)
            {
                return null;
            }

            var data = DecodeData(encoding, value);
            if (data == null)
            {
                return null;
            }

            // Decode the string into the current encoding
            var encodingStack = _mSerializationContext.GetService(typeof (EncodingStack)) as EncodingStack;
            return encodingStack.Current.GetString(data);
        }

        public byte[] DecodeData(string encoding, string value)
        {
            if (encoding == null || value == null)
            {
                return null;
            }

            var decoder = GetDecoderFor(encoding);
            return decoder?.Invoke(value);
        }
    }
}