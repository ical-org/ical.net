using AngleSharp.Parser.Html;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Net.Mime;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace SpecValidation
{
    //Hi Brent,
    //
    //Thanks for your comments and the report on the date-time / tzid issue (the fix has been made and will be included in the next release) (now available).  I don't see a problem of you referring to the validator from another site, 
    //so long as it does not overload my server.  For unit tests I would assume the files will be fairly small and would not put a strain on the server.  Additionally, I limit feeds to 512KB since the ical feed is loaded into memory 
    //to do its validation tests.  So, you have my permission.  A link to either  http://icalendar.org and/or http://icalendar.org/validator.html would be appreciated.
    //
    //There is currently a bug with the permalink feature when a link contains a question mark "?", that will be fixed soon but it may impact your icalendar posting (now available).
    //
    //I think the JSON web service is a great idea. I will take a look at it.  Like you, this is a project I work on my spare time so it may take some time to investigate and implement it.  I don't think there is an icalendar validator that currently does this.
    //
    //
    //I'm glad to hear of your success using the validator in your project and thanks for your work and support of open source software. 
    //Also, BTW, I just added your .NET project to the resources page at http://icalendar.org/resources.html
    //
    //-- 
    //Dan Cogliano, Z Content http://www.zcontent.net/
    //Author of Zap Calendar and Zap Weather
    //
    //Like us on Facebook https://www.facebook.com/zcontent
    //
    internal class ICalendarOrgSpecificFormat
    {
        internal static HttpContent ToUrlSpecificFormContent(string serializedIcal)
        {
            const string formData = "form-data";
            //http://stackoverflow.com/questions/18059588/httpclient-multipart-form-post-in-c-sharp#18067973
            var multipart = new MultipartFormDataContent();
            var empty = new byte[0];
            HttpContent content = new ByteArrayContent(empty);
            var contentDisposition = new ContentDispositionHeaderValue(formData) { Name = "jform[ical_url]" };
            content.Headers.ContentDisposition = contentDisposition;
            multipart.Add(content);

            content = new ByteArrayContent(empty);
            contentDisposition = new ContentDispositionHeaderValue(formData) { Name = "jform[ical_file]", FileName = string.Empty };
            content.Headers.ContentDisposition = contentDisposition;
            content.Headers.ContentType = new MediaTypeHeaderValue(MediaTypeNames.Application.Octet);
            multipart.Add(content);

            content = new ByteArrayContent(Encoding.ASCII.GetBytes(serializedIcal));
            contentDisposition = new ContentDispositionHeaderValue(formData) { Name = "jform[ical_text]" };
            content.Headers.ContentDisposition = contentDisposition;
            multipart.Add(content);

            content = new ByteArrayContent(Encoding.ASCII.GetBytes("validate"));
            contentDisposition = new ContentDispositionHeaderValue(formData){ Name = "jform[task]" };
            content.Headers.ContentDisposition = contentDisposition;
            multipart.Add(content);

            return multipart;
        }

        internal static async Task<ICollection<ValidationFailIssue>> FromResponse(Stream source)
        {
            var returnVar = new List<ValidationFailIssue>();
            var parser = new HtmlParser();
            var doc = await parser.ParseAsync(source);
            var results = doc.GetElementById("results");

            if (string.IsNullOrWhiteSpace(results.InnerHtml)) {
                //to do - some kind of invalid format exception
                throw new Exception("div#results Is Empty - usually this means POST request has returned as per GET request & data has not been parsed");
            }

            var errorHeader = results.GetElementsByClassName("heading").FirstOrDefault(e=>e.TextContent.IndexOf("Errors", StringComparison.OrdinalIgnoreCase)>-1);
            var errorList = errorHeader?.ParentElement.GetElementsByTagName("ol");

            if (errorList==null || !errorList.Any()) { return returnVar; }

            foreach (var li in errorList.First().Children)
            {
                var valFail = new ValidationFailIssue();
                valFail.RelevantSpec = li.GetElementsByClassName("reference")[0].GetElementsByTagName("a")[0].TextContent;
                string analysisText = li.TextContent;
                analysisText = analysisText.Replace(valFail.RelevantSpec, string.Empty);
                analysisText = analysisText.Replace("Reference:", string.Empty);
                Match lineRef = Regex.Match(analysisText, @"near[\r\n\s]+line[\r\n\s]+#[\r\n\s]*(\d+)");
                if (lineRef.Success)
                {
                    valFail.LineNumber = int.Parse(lineRef.Groups[1].Value);
                    analysisText = analysisText.Replace(lineRef.Value, string.Empty);
                }
                valFail.Message = Regex.Replace(analysisText, @"[\r\n\s]+", " ").Trim();
                returnVar.Add(valFail);
            }
            return returnVar;
        }
    }
}
