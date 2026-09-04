//
// Copyright ical.net project maintainers and contributors.
// Licensed under the MIT license.
//

using Ical.Net.DataTypes;
using NUnit.Framework;

namespace Ical.Net.Tests;

[TestFixture]
public class GeographicLocationTests
{

    /// <summary>
    /// Ensures that events can be properly added to a calendar.
    /// </summary>
    [Test, Category("GeographicLocation")]
    public void ContstuctFromString()
    {
        var loc = new GeographicLocation("37.386013;-122.082932");
        Assert.That(loc.Latitude, Is.EqualTo(37.386013));
        Assert.That(loc.Longitude, Is.EqualTo(-122.082932));
    }

    [Test, Category("GeographicLocation")]
    public void ContstuctNoArguments()
    {
        var loc = new GeographicLocation();
        Assert.That(loc.Latitude, Is.EqualTo(0));
        Assert.That(loc.Longitude, Is.EqualTo(0));
    }

    [Test, Category("GeographicLocation")]
    public void ContstuctFromDoubles()
    {
        var loc = new GeographicLocation(37.386013, -122.082932);
        Assert.That(loc.Latitude, Is.EqualTo(37.386013));
        Assert.That(loc.Longitude, Is.EqualTo(-122.082932));
    }
}
