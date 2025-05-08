//
// Copyright ical.net project maintainers and contributors.
// Licensed under the MIT license.
//

using System.Runtime.Serialization;
using Ical.Net.Evaluation;

namespace Ical.Net.CalendarComponents;

/// <summary>
/// A class that represents an RFC 5545 VJOURNAL component.
/// </summary>
public class Journal : RecurringComponent
{
    public string? Status
    {
        get => Properties.Get<string>(JournalStatus.Key);
        set => Properties.Set(JournalStatus.Key, value);
    }

    public override IEvaluator Evaluator => throw new System.NotImplementedException();

    /// <summary>
    /// Constructs an Journal object, with an iCalObject
    /// (usually an iCalendar object) as its parent.
    /// </summary>
    public Journal()
    {
        Name = JournalStatus.Name;
    }

    protected override void OnDeserializing(StreamingContext context)
    {
        base.OnDeserializing(context);
    }

    protected bool Equals(Journal? other)
    {
        if (Start == null || other?.Start == null)
        {
            return Start == other?.Start; // Both must be null to be considered equal
        }

        return Start.Equals(other.Start) && Equals(other as RecurringComponent);
    }

    public override bool Equals(object? obj)
    {
        if (ReferenceEquals(null, obj)) return false;
        if (ReferenceEquals(this, obj)) return true;
        return obj.GetType() == GetType() && Equals((Journal) obj);
    }

    public override int GetHashCode()
    {
        var hashCode = Start?.GetHashCode() ?? 0;
        hashCode = (hashCode * 397) ^ base.GetHashCode();
        return hashCode;
    }
}
