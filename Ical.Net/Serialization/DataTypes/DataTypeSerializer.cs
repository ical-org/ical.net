//
// Copyright ical.net project maintainers and contributors.
// Licensed under the MIT license.
//

using Ical.Net.DataTypes;

namespace Ical.Net.Serialization.DataTypes;

public abstract class DataTypeSerializer : SerializerBase
{
    protected DataTypeSerializer() { }

    protected DataTypeSerializer(SerializationContext ctx) : base(ctx) { }

    /// <summary>
    /// Creates a new, empty instance of <see cref="SerializerBase.TargetType"/>.
    /// </summary>
    /// <remarks>
    /// Serializers whose target is an <see cref="ICalendarDataType"/> must override this with a
    /// direct <c>new</c>, which is what keeps <see cref="CreateAndAssociate"/> free of reflection
    /// and therefore trimming- and NativeAOT-safe.
    /// <para/>
    /// The default returns <see langword="null"/>, for serializers whose target is not an
    /// <see cref="ICalendarDataType"/> (<see cref="string"/>, <see cref="int"/>, an enum, ...).
    /// </remarks>
    protected virtual ICalendarDataType? CreateTargetInstance() => null;

    protected virtual ICalendarDataType? CreateAndAssociate()
    {
        // Create an instance of the object
        if (CreateTargetInstance() is not { } dt)
        {
            return null;
        }

        if (SerializationContext.Peek() is ICalendarObject associatedObject)
        {
            dt.AssociatedObject = associatedObject;
        }

        return dt;
    }
}
