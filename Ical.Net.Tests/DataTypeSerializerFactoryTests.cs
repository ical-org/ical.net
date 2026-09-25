//
// Copyright ical.net project maintainers and contributors.
// Licensed under the MIT license.
//

using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Ical.Net.DataTypes;
using Ical.Net.Serialization.DataTypes;
using NUnit.Framework;

namespace Ical.Net.Tests;

/// <summary>
/// DataTypeSerializer.CreateAndAssociate() delegates to a per-serializer CreateTargetInstance()
/// override. A serializer that targets an ICalendarDataType but misses the override silently
/// deserializes to null - or, worse, inherits its base serializer's override and produces the
/// wrong type (FreeBusyEntrySerializer derives from PeriodSerializer, AttendeeSerializer from
/// StringSerializer).
/// </summary>
[TestFixture]
public class DataTypeSerializerFactoryTests
{
    /// <summary>
    /// Every concrete DataTypeSerializer whose TargetType is an ICalendarDataType.
    /// </summary>
    public static IEnumerable<Type> DataTypeSerializers => typeof(DataTypeSerializer).Assembly
        .GetTypes()
        .Where(t => typeof(DataTypeSerializer).IsAssignableFrom(t) && !t.IsAbstract)
        .Where(t => t.GetConstructor(Type.EmptyTypes) != null)
        .Where(t => typeof(ICalendarDataType).IsAssignableFrom(((DataTypeSerializer) Activator.CreateInstance(t)!).TargetType))
        .OrderBy(t => t.FullName, StringComparer.Ordinal);

    [Test, TestCaseSource(nameof(DataTypeSerializers))]
    public void CreateTargetInstanceIsOverridden(Type serializerType)
    {
        var method = serializerType.GetMethod("CreateTargetInstance", BindingFlags.Instance | BindingFlags.NonPublic);

        Assert.That(method, Is.Not.Null, $"{serializerType.Name} has no CreateTargetInstance() method.");
        Assert.That(method!.DeclaringType, Is.EqualTo(serializerType),
            $"{serializerType.Name} does not override CreateTargetInstance(), so it would either return null "
            + "or inherit the wrong type from its base serializer.");
    }

    /// <summary>
    /// The created instance must be exactly TargetType - not a base or derived type.
    /// </summary>
    [Test, TestCaseSource(nameof(DataTypeSerializers))]
    public void CreateTargetInstanceMatchesTargetType(Type serializerType)
    {
        var serializer = (DataTypeSerializer) Activator.CreateInstance(serializerType)!;
        var method = serializerType.GetMethod("CreateTargetInstance", BindingFlags.Instance | BindingFlags.NonPublic)!;

        var instance = method.Invoke(serializer, null);

        Assert.That(instance, Is.Not.Null, $"{serializerType.Name}.CreateTargetInstance() returned null.");
        Assert.That(instance!.GetType(), Is.EqualTo(serializer.TargetType),
            $"{serializerType.Name}.CreateTargetInstance() returned a {instance.GetType().Name} "
            + $"but its TargetType is {serializer.TargetType.Name}.");
    }
}
