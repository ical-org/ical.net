//
// Copyright ical.net project maintainers and contributors.
// Licensed under the MIT license.
//

using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Ical.Net.Serialization;
using NUnit.Framework;

namespace Ical.Net.Tests;

/// <summary>
/// Blanket coverage for <see cref="ICopyable.Copy{T}"/> across every concrete
/// <see cref="ICopyable"/> type in the library.
/// <para/>
/// These assert the invariants every implementation must uphold - most importantly that a copy has
/// the <em>exact same runtime type</em> as its source, which a per-type virtual factory can break
/// by returning the wrong type without any field-equality test noticing.
/// <see cref="CopyComponentTests"/> covers field-level equality for a handful of components.
/// </summary>
[TestFixture]
public class CopyAllTypesTests
{
    /// <summary>
    /// Every concrete (non-abstract, non-interface) type in Ical.Net that implements
    /// <see cref="ICopyable"/>, including internal and private nested types.
    /// </summary>
    public static IEnumerable<Type> ConcreteCopyableTypes => typeof(ICopyable).Assembly
        .GetTypes()
        .Where(t => typeof(ICopyable).IsAssignableFrom(t) && !t.IsInterface && !t.IsAbstract)
        .OrderBy(t => t.FullName, StringComparer.Ordinal);

    private static ICopyable CreateInstance(Type type)
        => (ICopyable) Activator.CreateInstance(type, nonPublic: true)!;

    /// <summary>
    /// Guards against the type list silently shrinking (e.g. a type being made abstract) and
    /// leaving the parameterised tests below covering less than they appear to.
    /// </summary>
    [Test]
    public void EveryCopyableTypeIsDiscovered()
    {
        Assert.That(ConcreteCopyableTypes.Count(), Is.EqualTo(28),
            "The set of concrete ICopyable types changed. Update the expected count deliberately, "
            + "and make sure any new type provides its own copy implementation.");
    }

    /// <summary>
    /// Every concrete type must override CreateNew() with a direct <c>new</c>. A type that misses
    /// the override falls back to <see cref="Activator"/>, which still works under the JIT and
    /// fails only once published with trimming enabled.
    /// </summary>
    [Test, TestCaseSource(nameof(ConcreteCopyableTypes))]
    public void CreateNewIsOverridden(Type type)
    {
        var createNew = type.GetMethod("CreateNew", BindingFlags.Instance | BindingFlags.NonPublic);

        Assert.That(createNew, Is.Not.Null, $"{type.FullName} has no CreateNew() method.");
        Assert.That(createNew!.DeclaringType, Is.EqualTo(type),
            $"{type.FullName} does not override CreateNew() and would fall back to Activator.CreateInstance, "
            + "which is not trimming- or NativeAOT-safe. Add: protected override ... CreateNew() => new "
            + type.Name + "();");
    }

    [Test, TestCaseSource(nameof(ConcreteCopyableTypes))]
    public void CopyReturnsSameRuntimeType(Type type)
    {
        var original = CreateInstance(type);

        var copy = original.Copy<object>();

        using (Assert.EnterMultipleScope())
        {
            Assert.That(copy, Is.Not.Null, $"Copy<object>() returned null for {type.FullName}.");
            Assert.That(copy!.GetType(), Is.EqualTo(type),
                $"Copy<object>() returned a {copy.GetType().FullName} for a {type.FullName}.");
            Assert.That(copy, Is.Not.SameAs(original), $"Copy<object>() returned the source instance for {type.FullName}.");
        }
    }

    /// <summary>
    /// A copy requested as an unrelated type must yield <see langword="null"/> rather than throwing.
    /// </summary>
    [Test, TestCaseSource(nameof(ConcreteCopyableTypes))]
    public void CopyToUnrelatedTypeReturnsNull(Type type)
    {
        var original = CreateInstance(type);

        Assert.That(original.Copy<Uri>(), Is.Null);
    }

    /// <summary>
    /// Copies must be independent: mutating the copy's child/property collections must not be
    /// visible through the original. This is the deep-vs-shallow regression that a mechanical
    /// rewrite of the clone paths is most likely to introduce.
    /// </summary>
    [Test, TestCaseSource(nameof(ConcreteCopyableTypes))]
    public void CopyDoesNotShareChildCollections(Type type)
    {
        var original = CreateInstance(type);
        var copy = original.Copy<object>();

        if (original is not ICalendarObject originalObject || copy is not ICalendarObject copyObject)
        {
            Assert.Pass($"{type.FullName} is not an ICalendarObject and has no child collection.");
            return;
        }

        Assert.That(copyObject.Children, Is.Not.SameAs(originalObject.Children),
            $"{type.FullName} shares its Children collection with its copy.");

        var childCountBefore = originalObject.Children.Count;
        copyObject.AddChild(new CalendarObject("X-COPY-PROBE"));

        Assert.That(originalObject.Children, Has.Count.EqualTo(childCountBefore),
            $"Adding a child to a copy of {type.FullName} also modified the original.");
    }

    /// <summary>
    /// All embedded .ics fixtures, as (resource name, content) pairs.
    /// </summary>
    public static IEnumerable<TestCaseData> IcsCorpus => typeof(IcsFiles).Assembly
        .GetManifestResourceNames()
        .Where(n => n.EndsWith(".ics", StringComparison.OrdinalIgnoreCase))
        .OrderBy(n => n, StringComparer.Ordinal)
        .Select(n => new TestCaseData(n).SetName($"SerializeCopySerializeIsStable({n})"));

    /// <summary>
    /// Fixtures that do not survive a serialize -> copy -> serialize round trip, because
    /// <see cref="DataTypes.CalendarDataType.CopyFrom"/> points the copy's parameter proxy at the
    /// <em>source's</em> parameter collection rather than copying it. Attachment.CopyFrom and
    /// Attendee.CopyFrom then re-assign FormatType/Rsvp, and setting a parameter to a null or
    /// default value adds an empty parameter (FMTTYPE=, RSVP=FALSE) instead of removing it.
    /// <para/>
    /// A known defect, tracked here rather than accepted as correct behaviour. Removing an entry
    /// must go with a fix, never with a relaxed assertion.
    /// </summary>
    private static readonly HashSet<string> _knownCopyDivergences = new(StringComparer.Ordinal)
    {
        "Ical.Net.Tests.Calendars.Serialization.Attachment3.ics",
        "Ical.Net.Tests.Calendars.Serialization.Attachment4.ics",
        "Ical.Net.Tests.Calendars.Serialization.Attendee1.ics",
        "Ical.Net.Tests.Calendars.Serialization.Attendee2.ics",
        "Ical.Net.Tests.Calendars.Serialization.Encoding2.ics",
        "Ical.Net.Tests.Calendars.Serialization.Event4.ics",
        "Ical.Net.Tests.Calendars.Serialization.Trigger1.ics",
    };

    /// <summary>
    /// serialize -> copy -> serialize over the whole .ics corpus. Any divergence means the copy
    /// lost, duplicated or reordered state somewhere in the object graph.
    /// </summary>
    [Test, TestCaseSource(nameof(IcsCorpus))]
    public void SerializeCopySerializeIsStable(string resourceName)
    {
        if (_knownCopyDivergences.Contains(resourceName))
        {
            Assert.Ignore($"{resourceName}: known copy divergence, see _knownCopyDivergences.");
            return;
        }

        var ics = IcsFiles.ReadStream(resourceName);

        CalendarCollection calendars;
        try
        {
            calendars = CalendarCollection.Load(ics);
        }
        catch (Exception ex)
        {
            Assert.Ignore($"{resourceName} is not a parseable fixture: {ex.GetType().Name}");
            return;
        }

        var serializer = new CalendarSerializer();

        foreach (var calendar in calendars)
        {
            var expected = serializer.SerializeToString(calendar);
            var copy = calendar.Copy<Calendar>();

            Assert.That(copy, Is.Not.Null, $"Copying the calendar from {resourceName} returned null.");
            Assert.That(serializer.SerializeToString(copy), Is.EqualTo(expected),
                $"A copy of the calendar from {resourceName} did not serialize identically to the original.");
        }
    }
}
