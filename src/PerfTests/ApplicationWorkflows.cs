using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using BenchmarkDotNet.Attributes;
using Ical.Net;
using Ical.Net.DataTypes;
using NUnit.Framework;

namespace PerfTests
{
    public class ApplicationWorkflows
    {
        const int DaysPerYear = 365;
        static readonly TimeSpan _oneYear = TimeSpan.FromDays(DaysPerYear);
        static readonly DateTime _searchStart = DateTime.Now.Subtract(_oneYear);
        static readonly DateTime _searchEnd = DateTime.Now.Add(_oneYear);
        static readonly List<string> _manyCalendars = GetIcalStrings();

        static List<string> GetIcalStrings()
        {
            var currentDirectory = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
            var topLevelIcsPath = Path.GetFullPath(Path.Combine(currentDirectory!, @".\Calendars"));
            return Directory.EnumerateFiles(topLevelIcsPath, "*.ics", SearchOption.AllDirectories)
                .Select(File.ReadAllText)
                .Distinct(StringComparer.OrdinalIgnoreCase)
                .Where(s => !s.Contains("InternetExplorer") && !s.Contains("SECONDLY"))
                .OrderByDescending(s => s.Length)
                .Take(10)
                .ToList();
        }

        [Benchmark]
        public void SingleThreaded() => SingleThreaded_();
        [Test(ExpectedResult = DaysPerYear)]
        public static int SingleThreaded_() => _manyCalendars
                .SelectMany(Calendar.Load<Calendar>)
                .SelectMany(c => c.Events)
                .SelectMany(e => e.GetOccurrences(_searchStart, _searchEnd))
                .Count();

        [Benchmark]
        public int ParallelUponDeserialize() => ParallelUponDeserialize_();
        [Test(ExpectedResult = DaysPerYear)]
        public static int ParallelUponDeserialize_() => _manyCalendars
                .AsParallel()
                .SelectMany(Calendar.Load<Calendar>)
                .SelectMany(c => c.Events)
                .SelectMany(e => e.GetOccurrences(_searchStart, _searchEnd))
                .Count();

        [Benchmark]
        public List<Occurrence> ParallelUponGetOccurrences() => ParallelUponGetOccurrences_();
        public static List<Occurrence> ParallelUponGetOccurrences_()
        {
            return _manyCalendars
                .SelectMany(Calendar.Load<Calendar>)
                .SelectMany(c => c.Events)
                .AsParallel()
                .SelectMany(e => e.GetOccurrences(_searchStart, _searchEnd))
                .ToList();
        }

        [Benchmark]
        public int ParallelDeserializeSequentialGatherEventsParallelGetOccurrences()
            => ParallelDeserializeSequentialGatherEventsParallelGetOccurrences_();
        [Test(ExpectedResult = DaysPerYear)]
        public static int ParallelDeserializeSequentialGatherEventsParallelGetOccurrences_() => _manyCalendars
                .AsParallel()
                .SelectMany(Calendar.Load<Calendar>)
                .AsSequential()
                .SelectMany(c => c.Events)
                .SelectMany(e => e.GetOccurrences(_searchStart, _searchEnd))
                .Count();
    }
}