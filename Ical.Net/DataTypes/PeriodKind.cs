// Copyright ical.net project maintainers and contributors.
// Licensed under the MIT license.

#nullable enable
namespace Ical.Net.DataTypes;

/// <summary>
/// The kind of <see cref="DataTypes.Period"/>s that can be added to a <see cref="PeriodList"/>.
/// </summary>
internal enum PeriodKind
{
    /// <summary>
    /// The period kind is undefined.
    /// </summary>
    Undefined,
    /// <summary>
    /// A date-time kind.
    /// </summary>
    DateTime,
    /// <summary>
    /// A date-only kind.
    /// </summary>
    DateOnly,
    /// <summary>
    /// A period that has a <see cref="Duration"/>.
    /// </summary>
    Period
}
