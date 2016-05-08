using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using NodaTime;

namespace ical.net
{
    internal class RecurrenceCalculator
    {
        private readonly ZonedDateTime _startTime;
        private readonly RecurrenceRuleSet _repetitionRules;
        private readonly RecurrenceRuleSet _exceptions;
        public RecurrenceCalculator(ZonedDateTime startTime, RecurrenceRuleSet repetitionRepetitionRules, RecurrenceRuleSet exceptions)
        {
            _startTime = startTime;
            _repetitionRules = repetitionRepetitionRules;
            _exceptions = exceptions;
        }

        public RecurrenceCalculator(ZonedDateTime startTime, RecurrenceRuleSet repetitionRepetitionRules) : this(startTime, repetitionRepetitionRules, null) {}

        public ISet<ZonedDateTime> GetRecurrences()
        {
            var temp = _startTime;
            var recurrences = new HashSet<ZonedDateTime> {temp};
            for (var i = recurrences.Count; i < _repetitionRules.Count; i++)
            {
                temp = temp.Plus(_repetitionRules.Interval);
                recurrences.Add(temp);
            }
            return recurrences.OrderBy(r => r, ZonedDateTime.Comparer.Instant).ToImmutableSortedSet();
        }
    }
}
