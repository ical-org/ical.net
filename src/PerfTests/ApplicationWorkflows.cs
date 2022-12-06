using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using BenchmarkDotNet.Attributes;
using Ical.Net;
using Ical.Net.DataTypes;

namespace PerfTests
{
    public class ApplicationWorkflows
    {
        private static readonly TimeSpan _oneYear = TimeSpan.FromDays(365);
        private static readonly DateTime _searchStart = DateTime.Now.Subtract(_oneYear);
        private static readonly DateTime _searchEnd = DateTime.Now.Add(_oneYear);
        private static readonly List<string> _manyCalendars = GetIcalStrings();

        private static List<string> GetIcalStrings()
        {
            var currentDirectory = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
            var topLevelIcsPath = Path.GetFullPath(Path.Combine(currentDirectory, @"..\..\..\..\", @"Ical.Net.CoreUnitTests\Calendars"));
            return Directory.EnumerateFiles(topLevelIcsPath, "*.ics", SearchOption.AllDirectories)
                .Select(File.ReadAllText)
                .Distinct(StringComparer.OrdinalIgnoreCase)
                .Where(s => !s.Contains("InternetExplorer") && !s.Contains("SECONDLY"))
                .OrderByDescending(s => s.Length)
                .Take(10)
                .ToList();
        }

        [Benchmark]
        public static List<Occurrence> SingleThreaded()
        {
            return _manyCalendars
                .SelectMany(Calendar.Load<Calendar>)
                .SelectMany(c => c.Events)
                .SelectMany(e => e.GetOccurrences(_searchStart, _searchEnd))
                .ToList();
        }

        [Benchmark]
        public static List<Occurrence> ParallelUponDeserialize()
        {
            return _manyCalendars
                .AsParallel()
                .SelectMany(Calendar.Load<Calendar>)
                .SelectMany(c => c.Events)
                .SelectMany(e => e.GetOccurrences(_searchStart, _searchEnd))
                .ToList();
        }

        [Benchmark]
        public static List<Occurrence> ParallelUponGetOccurrences()
        {
            return _manyCalendars
                .SelectMany(Calendar.Load<Calendar>)
                .SelectMany(c => c.Events)
                .AsParallel()
                .SelectMany(e => e.GetOccurrences(_searchStart, _searchEnd))
                .ToList();
        }

        [Benchmark]
        public static List<Occurrence> ParallelDeserializeSequentialGatherEventsParallelGetOccurrences()
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
}