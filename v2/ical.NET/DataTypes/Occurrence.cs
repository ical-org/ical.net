using System;
using ical.net.Interfaces.Evaluation;

namespace ical.net.DataTypes
{
    public class Occurrence : IComparable<Occurrence>
    {
        public Period Period { get; set; }
        public IRecurrable Source { get; set; }

        public Occurrence(Occurrence ao)
        {
            Period = ao.Period;
            Source = ao.Source;
        }

        public Occurrence(IRecurrable recurrable, Period period)
        {
            Source = recurrable;
            Period = period;
        }

        public bool Equals(Occurrence other)
        {
            return Equals(Period, other.Period) && Equals(Source, other.Source);
        }

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj))
            {
                return false;
            }
            return obj is Occurrence && Equals((Occurrence) obj);
        }

        public override int GetHashCode()
        {
            unchecked
            {
                return ((Period?.GetHashCode() ?? 0) * 397) ^ (Source?.GetHashCode() ?? 0);
            }
        }

        public override string ToString()
        {
            var s = "Occurrence";
            if (Source != null)
            {
                s = Source.GetType().Name + " ";
            }

            if (Period != null)
            {
                s += "(" + Period.StartTime + ")";
            }

            return s;
        }

        public int CompareTo(Occurrence other)
        {
            return Period.CompareTo(other.Period);
        }
    }
}