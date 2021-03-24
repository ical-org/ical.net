using System;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;

namespace Ical.Net.Serialization
{
    /// <summary>
    /// This class contains methods taken from the csICalTranslator project (by the Consolidate dev team).
    /// The functions are restructured and refactored to get rid of the spaghetti code.
    /// </summary>
    public static class IcalCleaner
    {
        public static string CleanVCalFile(string calText)
        {
            try
            {
                calText = EnsureQuotedPrintableLineEndings(calText);
                calText = MakeMultilineTextToSingleLiner(calText);

                var lines = calText.Split(new[] { Environment.NewLine }, StringSplitOptions.RemoveEmptyEntries);
                var stringBuilder = new StringBuilder();
                foreach (var line in lines)
                {
                    var loopLine = RemoveQuotationMarks(line);
                    loopLine = FixInvalidAttendeeLines(loopLine);
                    loopLine = FixEncodingQuotedPrintable2(loopLine);

                    if (CheckFormat(loopLine))
                    {
                        stringBuilder.AppendLine(loopLine);
                    }
                }
                return stringBuilder.ToString();
            }
            catch
            {
                return string.Empty;
            }
        }

        /// <summary>
        /// Fixes an issue with customer Scherzinger where the quoted printable formatted description is not correct (AktId 2795837).
        /// 
        /// Problem:
        /// Lines of Quoted-Printable encoded data must not be longer than 76 characters.
        /// To satisfy this requirement without altering the encoded text, soft line breaks may be added as desired.
        /// A soft line break consists of an = at the end of an encoded line.
        /// At Scherzinger one line in the middle of the text was missing the = at the end.
        /// This was a problem later on in ical.net when parsing the lines and so the whole thing crashed.
        /// 
        /// Solution:
        /// If the previous and next line of a line end with =, the current line also has to.
        /// This solution does NOT work when the affected line is the next-to-last line of the text as the last line does not end with =.
        /// </summary>
        /// <param name="calText"></param>
        /// <returns></returns>
        private static string EnsureQuotedPrintableLineEndings(string calText)
        {
            if (!calText.Contains("\nDESCRIPTION;ENCODING=QUOTED-PRINTABLE:") && !calText.Contains("\nSUMMARY;ENCODING=QUOTED-PRINTABLE:"))
            {
                return calText;
            }

            var lines = calText.Split(new[] { Environment.NewLine }, StringSplitOptions.RemoveEmptyEntries);
            var stringBuilder = new StringBuilder();
            for (int i = 0; i < lines.Length; i++)
            {
                var loopLine = lines[i];

                if (i - 1 >= 0 && i + 1 < lines.Length)
                {
                    var previousLine = lines.ElementAt(i - 1);
                    var nextLine = lines.ElementAt(i + 1);
                    if (!loopLine.EndsWith("=") && previousLine.EndsWith("=") && nextLine.EndsWith("="))
                    {
                        loopLine = loopLine + "=";
                    }
                }
                stringBuilder.AppendLine(loopLine);
            }
            return stringBuilder.ToString();
        }

        /// <summary>
        /// Further investigation required to find purpose and use cases of this code.
        /// </summary>
        /// <param name="line"></param>
        /// <returns></returns>
        private static string FixEncodingQuotedPrintable2(string line)
        {
            if (line.StartsWith("DESCRIPTION;ENCODING=QUOTED-PRINTABLE:") || line.StartsWith("SUMMARY;ENCODING=QUOTED-PRINTABLE:"))
            {
                line = line.Replace("=0D=0A", @"\n");
                line = line.Replace("=3D", @"=");
            }

            return line;
        }

        /// <summary>
        /// This function brings content that is split into multiple lines to one single line.
        /// </summary>
        /// <remarks>
        /// According to RFC-5545 section 3.1 (https://tools.ietf.org/html/rfc5545#section-3.1):
        /// Long content lines SHOULD be split into a multiple line
        /// representations using a line "folding" technique.
        /// A long line can be split between any two characters by inserting a CRLF
        /// immediately followed by a single linear white-space character(i.e. SPACE or HTAB).
        /// </remarks>
        /// <param name="calText"></param>
        /// <returns></returns>
        private static string MakeMultilineTextToSingleLiner(string calText)
        {
            if (calText.Contains("\nDESCRIPTION;ENCODING=QUOTED-PRINTABLE:") || calText.Contains("\nSUMMARY;ENCODING=QUOTED-PRINTABLE:"))
            {
                calText = calText.Replace("=\r\n", "");
            }
            else
            {
                calText = calText.Replace("\r\n ", ""); // carriage return, new line, space
                calText = calText.Replace("\r\n\t", ""); // carriage return, new line, tab
            }
            return calText;
        }

        /// <summary>
        /// Fixes a Problem where there is extra text between the CN string and the mailto section, for example: CN="Mail Name"  (e@mail.at):mailto:e@mail.at
        /// Returned will be the email correctly filled in the CN string, for example: CN="Mail Name  (e@mail.at)":mailto:e@mail.at
        /// </summary>
        /// <param name="inLine"></param>
        /// <returns>The corrected line if it needs correcting</returns>
        private static string FixInvalidAttendeeLines(string inLine)
        {
            string line = inLine;
            if (line.ToLower().Contains("cn=")
                && line.ToLower().Contains(":mailto")
                && line.ToLower().Contains("\""))
            {
                var lastQuotation = line.LastIndexOf("\"");

                Regex regex = new Regex("\"(.*):mailto");
                var v = regex.Match(line.Substring(lastQuotation)); // find if there is text between the " and :mailto
                if (v.Success)
                {
                    string cutOut = v.Groups[1].ToString();
                    if (cutOut.Length > 0)
                    {
                        var indexOfFirstSemicolon = cutOut.IndexOf(";");
                        if (indexOfFirstSemicolon > -1)
                        {
                            cutOut = cutOut.Substring(0, indexOfFirstSemicolon);
                        }

                        if (cutOut.Length > 0)
                        {
                            line = inLine.Replace(cutOut, "");
                            line = line.Insert(line.LastIndexOf("\""), cutOut);  // insert the cutout text before the last "
                        }
                    }
                }
            }
            return line;
        }

        private static string RemoveQuotationMarks(string inLine)
        {
            string line = inLine;
            int startPos;
            int endPos;
            int quotePos;
            if (line != null && line.ToLower().StartsWith(@"attendee;"))
            {
                startPos = line.ToLower().IndexOf(@";cn=");
                if (startPos > 0)
                {
                    startPos = line.IndexOf("\"\"", startPos);
                    if (startPos > 0)
                    {
                        endPos = line.ToLower().IndexOf(@":mailto", startPos);
                        if (endPos > 0)
                        {
                            quotePos = line.LastIndexOf("\"", endPos, endPos - startPos);
                            if (quotePos > startPos + 2)
                            {
                                quotePos = line.LastIndexOf("\"", quotePos - 1, quotePos - 1 - startPos);
                                if (quotePos > startPos + 2)
                                {
                                    line = line.Remove(quotePos, 1);
                                }
                            }
                            line = line.Remove(startPos, 1);
                        }
                    }
                }
            }
            return line;
        }

        /// <summary>
        /// Is this really required?
        /// Is there a use case for this code?
        /// </summary>
        /// <param name="myLine"></param>
        /// <returns></returns>
        private static bool CheckFormat(string myLine)
        {
            bool returnValue = true;
            if (!string.IsNullOrEmpty(myLine))
            {
                if (myLine.ToLower().StartsWith("attendee"))
                {
                    if (!myLine.ToLower().Contains("@") && !myLine.ToLower().Contains(":urn"))
                    {
                        returnValue = false;
                    }
                }
            }
            return returnValue;
        }
    }
}
