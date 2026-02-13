//
// Copyright ical.net project maintainers and contributors.
// Licensed under the MIT license.
//

using System;

namespace Ical.Net.Serialization.DataTypes;

[Obsolete("Use RecurrenceRuleSerializer instead.")]
public class RecurrencePatternSerializer : RecurrenceRuleSerializer
{
    public RecurrencePatternSerializer() { }

    public RecurrencePatternSerializer(SerializationContext ctx) : base(ctx) { }
}
