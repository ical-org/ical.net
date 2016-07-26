using System.Collections.Generic;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading.Tasks;

namespace SpecValidation
{
    public class ValidationFailIssue
    {
        public string RelevantSpec { get; set; }
        public int? LineNumber { get; set; }
        public string LineValue { get; set; }
        public string Message { get; set; }
    }

    public static class Validate
    {
        public static async Task<ICollection<ValidationFailIssue>> ValidationErrors(string serializedIcal)
        {
            using (var handler = new HttpClientHandler { AutomaticDecompression = DecompressionMethods.Deflate | DecompressionMethods.GZip })
            {
                using (var client = new HttpClient(handler))
                {
                    client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue(System.Net.Mime.MediaTypeNames.Text.Html));
                    //client.DefaultRequestHeaders.AcceptLanguage.ParseAdd("en-US,en;q=0.5");
                    //client.DefaultRequestHeaders.UserAgent.ParseAdd("Mozilla/5.0 (Windows NT 6.1; WOW64; Trident/7.0; MASP; rv:11.0) like Gecko");
                    //client.DefaultRequestHeaders.Add("Connection", "Keep-Alive");
                    //client.DefaultRequestHeaders.Referrer = new System.Uri("http://icalendar.org/validator.html");

                    using (HttpContent content = ICalendarOrgSpecificFormat.ToUrlSpecificFormContent(serializedIcal))
                    {
                        using (var response = await client.PostAsync("http://icalendar.org/validator.html", content))
                        {
                            //this should move to JSON when appropriate service available
                            var returnVar = await ICalendarOrgSpecificFormat.FromResponse(await response.Content.ReadAsStreamAsync());

                            string[] lines = null;
                            foreach (var v in returnVar)
                            {
                                if (v.LineNumber.HasValue)
                                {
                                    v.LineValue = (lines ?? (lines = serializedIcal.Replace("\r", string.Empty).Split('\n')))[v.LineNumber.Value - 1];
                                }
                            }

                            return returnVar;
                        }
                    }
                }
            }

        }
    }
}
