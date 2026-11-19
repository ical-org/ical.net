//
// Copyright ical.net project maintainers and contributors.
// Licensed under the MIT license.
//

using System.IO;
using System.Text;
using System.Threading;
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

    [Test]
    public async Task SerializeAsync_SmallCalendarFlushesAtCompletion()
    {
        using var icsStream = new MemoryStream(Encoding.UTF8.GetBytes(IcsFiles.Event4));
        var cal = await CalendarSerializer.DeserializeAsync<Calendar>(icsStream);
        using var outputStream = new TrackingMemoryStream();

        await CalendarSerializer.SerializeAsync(outputStream, cal);

        Assert.That(outputStream.FlushCount, Is.EqualTo(1));
    }

    private sealed class TrackingMemoryStream : MemoryStream
    {
        public int FlushCount { get; private set; }

        public override Task FlushAsync(CancellationToken cancellationToken)
        {
            FlushCount++;
            return base.FlushAsync(cancellationToken);
        }
    }
}
