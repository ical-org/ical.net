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
            try
            {
                // We use Windows-1252 for decoding quoted printable text as
                // this works for most of the cases. There is no better solution 
                // at the moment.
                const string CHARSET = "Windows-1252";
                Encoding.RegisterProvider(CodePagesEncodingProvider.Instance);
                var quotedPrintableEncoding = Encoding.GetEncoding(CHARSET);
                var icalEncoding = _mSerializationContext.GetService<EncodingStack>().Current;

                value = DecodeQuotedPrintables(value, CHARSET);
                var bytes = quotedPrintableEncoding.GetBytes(value);

                bytes = Encoding.Convert(quotedPrintableEncoding, icalEncoding, bytes);
                return bytes;
            }
            catch
            {
                return null;
            }
        }

        private static string DecodeQuotedPrintables(string input, string charSet)
        {
            if (string.IsNullOrEmpty(charSet))
            {
                var charSetOccurences = new Regex(@"=\?.*\?Q\?", RegexOptions.IgnoreCase);
                var charSetMatches = charSetOccurences.Matches(input);
                foreach (Match match in charSetMatches)
                {
                    charSet = match.Groups[0].Value.Replace("=?", "").Replace("?Q?", "");
                    input = input.Replace(match.Groups[0].Value, "").Replace("?=", "");
                }
            }

            Encoding enc = new ASCIIEncoding();
            if (!string.IsNullOrEmpty(charSet))
            {
                try
                {
                    enc = Encoding.GetEncoding(charSet);
                }
                catch
                {
                    enc = new ASCIIEncoding();
                }
            }

            //decode iso-8859-[0-9]
            var occurences = new Regex(@"=[0-9A-Z]{2}", RegexOptions.Multiline);
            var matches = occurences.Matches(input);
            foreach (Match match in matches)
            {
                try
                {
                    byte[] b = new byte[] { byte.Parse(match.Groups[0].Value.Substring(1), System.Globalization.NumberStyles.AllowHexSpecifier) };
                    char[] hexChar = enc.GetChars(b);
                    input = input.Replace(match.Groups[0].Value, hexChar[0].ToString());
                }
                catch { }
            }

            //decode base64String (utf-8?B?)
            occurences = new Regex(@"\?utf-8\?B\?.*\?", RegexOptions.IgnoreCase);
            matches = occurences.Matches(input);
            foreach (Match match in matches)
            {
                byte[] b = Convert.FromBase64String(match.Groups[0].Value.Replace("?utf-8?B?", "").Replace("?UTF-8?B?", "").Replace("?", ""));
                string temp = Encoding.UTF8.GetString(b);
                input = input.Replace(match.Groups[0].Value, temp);
            }

            input = input.Replace("=\r\n", "");
            return input;
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