//
// Copyright ical.net project maintainers and contributors.
// Licensed under the MIT license.
//

using System.IO;
using System.Text;
using System.Threading.Tasks;
using Ical.Net.Serialization;
using NUnit.Framework;

namespace Ical.Net.Tests;

internal class SerializeAsyncTests
{
    [Test]
    public async Task SerializeRoundTrip_Stream()
    {
        var ics = IcsFiles.Event4;

        using var icsStream = new MemoryStream(Encoding.UTF8.GetBytes(ics));

        var options = new CalendarSerializerOptions
        {
            OrderComponentProperties = false
        };

        var cal = await CalendarSerializer.DeserializeAsync<Calendar>(icsStream, options);

        using var outputStream = new MemoryStream();

        await CalendarSerializer.SerializeAsync(outputStream, cal, options);
        outputStream.Position = 0;

        using var reader = new StreamReader(outputStream);
        var output = await reader.ReadToEndAsync();

        Assert.That(output, Is.EqualTo(ics));
    }
}
