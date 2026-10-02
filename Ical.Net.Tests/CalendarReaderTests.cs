//
// Copyright ical.net project maintainers and contributors.
// Licensed under the MIT license.
//

using System.IO;
using Ical.Net.Serialization;
using NUnit.Framework;

namespace Ical.Net.Tests;

internal class CalendarReaderTests
{
    private class OneByteStream(byte[] buffer) : MemoryStream(buffer)
    {
        public override int Read(byte[] buffer, int offset, int count)
            => base.Read(buffer, offset, 1);
    }

    [Test]
    public void ReadsUntilEntireContentLineIsBuffered()
    {
        var data = """
            DESCRIPTION:This line is folded
              and ends here

            """u8;

        // Stream reads one byte at a time. This is to verify
        // that the reader will read from the stream repeatedly
        // until an entire content line is found.
        var stream = new OneByteStream(data.ToArray());
        var reader = new CalendarReader(stream);

        using (Assert.EnterMultipleScope())
        {
            Assert.That(reader.ReadContentLine(), Is.True);
            Assert.That(reader.ReadName(), Is.EqualTo("DESCRIPTION"));
            Assert.That(reader.GetTextValue(), Is.EqualTo("This line is folded and ends here"));
        }
    }
}
