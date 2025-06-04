//
// Copyright ical.net project maintainers and contributors.
// Licensed under the MIT license.
//

using BenchmarkDotNet.Attributes;
using Ical.Net.DataTypes;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Ical.Net.Evaluation;

namespace Ical.Net.Benchmarks;

public class ApplicationWorkflows
{
    private static readonly CalDateTime _searchStart = CalDateTime.Now.AddDays(-365);
    private static readonly CalDateTime _searchEnd = CalDateTime.Now;
    private static readonly List<string> _manyCalendars = GetIcalStrings();

    private static List<string> GetIcalStrings()
    {
        var testProjectDirectory = Runner.FindParentFolder("Ical.Net.Tests", Directory.GetCurrentDirectory());
        var topLevelIcsPath = Path.GetFullPath(Path.Combine(testProjectDirectory, "Calendars"));
        return Directory.EnumerateFiles(topLevelIcsPath, "*.ics", SearchOption.AllDirectories)
            .Where(p => !p.EndsWith("DateTime1.ics")) // contains a deliberate error
            .Select(File.ReadAllText)
            .Distinct(StringComparer.OrdinalIgnoreCase)
            .Where(s => !s.Contains("InternetExplorer") && !s.Contains("SECONDLY"))
            .OrderByDescending(s => s.Length)
            .Take(10)
            .ToList();
    }

    [Benchmark]
    public List<Occurrence> SingleThreaded()
    {
        return _manyCalendars
            .SelectMany(Calendar.Load<Calendar>)
            .SelectMany(c => c.Events)
            .SelectMany(e => e.GetOccurrences(_searchStart).TakeWhile(p => p.Period.StartTime.LessThan(_searchEnd)))
            .ToList();
    }

    [Benchmark]
    public List<Occurrence> ParallelUponDeserialize()
    {
        return _manyCalendars
            .AsParallel()
            .SelectMany(Calendar.Load<Calendar>)
            .SelectMany(c => c.Events)
            .SelectMany(e => e.GetOccurrences(_searchStart).TakeWhile(p => p.Period.StartTime.LessThan(_searchEnd)))
            .ToList();
    }

    [Benchmark]
    public List<Occurrence> ParallelUponGetOccurrences()
    {
        return _manyCalendars
            .SelectMany(Calendar.Load<Calendar>)
            .SelectMany(c => c.Events)
            .AsParallel()
            .SelectMany(e => e.GetOccurrences(_searchStart).TakeWhile(p => p.Period.StartTime.LessThan(_searchEnd)))
            .ToList();
    }

    [Benchmark]
    public List<Occurrence> ParallelDeserializeSequentialGatherEventsParallelGetOccurrences()
    {
        return _manyCalendars
            .AsParallel()
            .SelectMany(Calendar.Load<Calendar>)
            .AsSequential()
            .SelectMany(c => c.Events)
            .SelectMany(e => e.GetOccurrences(_searchStart).TakeWhile(p => p.Period.StartTime.LessThan(_searchEnd)))
            .ToList();
    }
}
