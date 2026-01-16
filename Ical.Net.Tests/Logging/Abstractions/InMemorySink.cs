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
    public static InMemorySink Instance;

    private ConcurrentQueue<Serilog.Events.LogEvent> _events { get; } = new();

    public InMemorySink()
    {
        Instance = this;
    }

    public int MaxLogsCount { get; set; } = Options.DefaultMaxLogsCount;

    public string OutputTemplate { get; set; }

    public IEnumerable<string> Logs
    {
        get
        {
            var formatter = new MessageTemplateTextFormatter(OutputTemplate, CultureInfo.InvariantCulture);
            using var writer = new System.IO.StringWriter();

            foreach (var logEvent in _events)
            {
                formatter.Format(logEvent, writer);
                yield return writer.ToString().TrimEnd(System.Environment.NewLine.ToCharArray());
                writer.GetStringBuilder().Clear();
            }
        }
    }

    public void Emit(Serilog.Events.LogEvent logEvent)
    {
        if (_events.Count >= MaxLogsCount)
        {
            _events.TryDequeue(out _);
        }
        _events.Enqueue(logEvent);
    }
}
