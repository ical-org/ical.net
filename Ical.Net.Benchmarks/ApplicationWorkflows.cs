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

namespace Ical.Net.Benchmarks;

public class ApplicationWorkflows
{
    private static readonly TimeSpan _oneYear = TimeSpan.FromDays(365);
    private static readonly DateTime _searchStart = DateTime.Now.Subtract(_oneYear);
    private static readonly DateTime _searchEnd = DateTime.Now.Add(_oneYear);
    private static readonly List<string> _manyCalendars = GetIcalStrings();

    private static List<string> GetIcalStrings()
    {
        var testProjectDirectory = Runner.FindParentFolder("Ical.Net.Tests", Directory.GetCurrentDirectory());
        var topLevelIcsPath = Path.GetFullPath(Path.Combine(testProjectDirectory, "Calendars"));
        return Directory.EnumerateFiles(topLevelIcsPath, "*.ics", SearchOption.AllDirectories)
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
            .SelectMany(e => e.GetOccurrences(_searchStart, _searchEnd))
            .ToList();
    }

    [Benchmark]
    public List<Occurrence> ParallelUponDeserialize()
    {
        return _manyCalendars
            .AsParallel()
            .SelectMany(Calendar.Load<Calendar>)
            .SelectMany(c => c.Events)
            .SelectMany(e => e.GetOccurrences(_searchStart, _searchEnd))
            .ToList();
    }

    [Benchmark]
    public List<Occurrence> ParallelUponGetOccurrences()
    {
        return _manyCalendars
            .SelectMany(Calendar.Load<Calendar>)
            .SelectMany(c => c.Events)
            .AsParallel()
            .SelectMany(e => e.GetOccurrences(_searchStart, _searchEnd))
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
            .SelectMany(e => e.GetOccurrences(_searchStart, _searchEnd))
            .ToList();
    }
}