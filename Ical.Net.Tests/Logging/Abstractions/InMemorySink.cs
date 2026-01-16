//
// Copyright ical.net project maintainers and contributors.
// Licensed under the MIT license.
//

using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Globalization;
using Serilog.Core;
using Serilog.Formatting.Display;

namespace Ical.Net.Tests.Logging.Abstractions;

internal class InMemorySink : ILogEventSink
{
    public static InMemorySink Instance = null!;

    private ConcurrentQueue<Serilog.Events.LogEvent> Events { get; } = new();

    public InMemorySink()
    {
        Instance = this;
        OutputTemplate = Options.DefaultOutputTemplate;
    }

    public int MaxLogsCount { get; set; } = Options.DefaultMaxLogsCount;

    public string OutputTemplate { get; set; }

    public IEnumerable<string> Logs
    {
        get
        {
            var formatter = new MessageTemplateTextFormatter(OutputTemplate, CultureInfo.InvariantCulture);
            using var writer = new System.IO.StringWriter();

            foreach (var logEvent in Events)
            {
                formatter.Format(logEvent, writer);
                yield return writer.ToString().TrimEnd(System.Environment.NewLine.ToCharArray());
                writer.GetStringBuilder().Clear();
            }
        }
    }

    public void Emit(Serilog.Events.LogEvent logEvent)
    {
        if (Events.Count >= MaxLogsCount)
        {
            Events.TryDequeue(out _);
        }
        Events.Enqueue(logEvent);
    }
}
