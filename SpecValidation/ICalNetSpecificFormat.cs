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
    internal class ICalNetSpecificFormat
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
