//
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
    private CalDateTime? _mDateTime;
    private Duration? _mDuration;
    private string _mRelated = TriggerRelation.Start;

    public virtual CalDateTime? DateTime
    {
        get => _mDateTime;
        set
        {
            _mDateTime = value;
            if (_mDateTime == null)
            {
                return;
            }

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
        if (serializer.Deserialize(new StringReader(value)) is ICopyable deserializedObject)
        {
            CopyFrom(deserializedObject);
        }
    }

    /// <inheritdoc/>
    public override void CopyFrom(ICopyable obj)
    {
        base.CopyFrom(obj);
        if (obj is not Trigger t)
        {
            return;
        }

        DateTime = t.DateTime;
        Duration = t.Duration;
        Related = t.Related;
    }
}
