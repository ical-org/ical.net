﻿//
// Copyright ical.net project maintainers and contributors.
// Licensed under the MIT license.
//

using System;
using System.IO;
using Ical.Net.Serialization.DataTypes;

namespace Ical.Net.DataTypes;

/// <summary>
/// A class that is used to specify exactly when an <see cref="Components.Alarm"/> component will trigger.
/// Usually this date/time is relative to the component to which the Alarm is associated.
/// </summary>
public class Trigger : EncodableDataType
{
    private CalDateTime _mDateTime;
    private Duration? _mDuration;
    private string _mRelated = TriggerRelation.Start;

    public virtual CalDateTime DateTime
    {
        get => _mDateTime;
        set
        {
            _mDateTime = value;
            if (_mDateTime == null)
            {
                return;
            }

            // NOTE: this, along with the "Duration" setter, fixes the bug tested in
            // TODO11(), as well as this thread: https://sourceforge.net/forum/forum.php?thread_id=1926742&forum_id=656447

            // DateTime and Duration are mutually exclusive
            Duration = null;

            // Ensure date/time has a time part
            if (!_mDateTime.HasTime)
            {
                _mDateTime = new CalDateTime(_mDateTime.Date, new TimeOnly(), _mDateTime.TzId);
            }
        }
    }

    public virtual Duration? Duration
    {
        get => _mDuration;
        set
        {
            _mDuration = value;
            if (_mDuration != null)
            {
                // NOTE: see above.

                // DateTime and Duration are mutually exclusive
                DateTime = null;
            }
        }
    }

    public virtual string Related
    {
        get => _mRelated;
        set => _mRelated = value;
    }

    public virtual bool IsRelative => _mDuration != null;

    public Trigger() { }

    public Trigger(Duration ts)
    {
        Duration = ts;
    }

    public Trigger(string value) : this()
    {
        var serializer = new TriggerSerializer();
        CopyFrom(serializer.Deserialize(new StringReader(value)) as ICopyable);
    }

    /// <inheritdoc/>
    public override void CopyFrom(ICopyable obj)
    {
        base.CopyFrom(obj);
        if (obj is not Trigger t)
        {
            return;
        }

        DateTime = t.DateTime?.Copy();
        Duration = t.Duration;
        Related = t.Related;
    }

    protected bool Equals(Trigger other) => Equals(_mDateTime, other._mDateTime) && _mDuration.Equals(other._mDuration) && _mRelated == other._mRelated;

    public override bool Equals(object obj)
    {
        if (ReferenceEquals(null, obj))
        {
            return false;
        }
        if (ReferenceEquals(this, obj))
        {
            return true;
        }
        if (obj.GetType() != GetType())
        {
            return false;
        }
        return Equals((Trigger)obj);
    }

    public override int GetHashCode()
    {
        unchecked
        {
            var hashCode = _mDateTime?.GetHashCode() ?? 0;
            hashCode = (hashCode * 397) ^ _mDuration.GetHashCode();
            hashCode = (hashCode * 397) ^ _mRelated?.GetHashCode() ?? 0;
            return hashCode;
        }
    }
}
