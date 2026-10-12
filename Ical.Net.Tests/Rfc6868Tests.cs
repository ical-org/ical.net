//
// Copyright ical.net project maintainers and contributors.
// Licensed under the MIT license.
//

using System.Collections.Generic;
using Ical.Net.CalendarComponents;
using Ical.Net.DataTypes;
using Ical.Net.Serialization;
using NUnit.Framework;

namespace Ical.Net.Tests;

[TestFixture]
public class Rfc6868Tests
{
    // ---- Codec unit tests (RFC 6868 §3.1 / §3.2) ----

    [TestCase("plain text", "plain text")]
    [TestCase("a^b", "a^^b")]                 // caret -> ^^
    [TestCase("say \"hi\"", "say ^'hi^'")]    // DQUOTE -> ^'
    [TestCase("line1\nline2", "line1^nline2")] // LF -> ^n
    [TestCase("line1\r\nline2", "line1^nline2")] // CRLF collapses to one ^n
    [TestCase("^n", "^^n")]                   // literal caret+n -> ^^n (the subtle case)
    [TestCase("^^", "^^^^")]
    public void Encode_matches_rfc6868(string raw, string expected)
    {
        Assert.That(CaretEncoding.Encode(raw), Is.EqualTo(expected));
    }

    [TestCase("plain text", "plain text")]
    [TestCase("a^^b", "a^b")]
    [TestCase("say ^'hi^'", "say \"hi\"")]
    [TestCase("line1^nline2", "line1\nline2")]
    [TestCase("^^n", "^n")]                    // ^^ -> ^, then literal n
    [TestCase("^x", "^x")]                     // lone caret before other char left unchanged (§3.2)
    [TestCase("trailing^", "trailing^")]       // dangling caret left unchanged
    public void Decode_matches_rfc6868(string encoded, string expected)
    {
        Assert.That(CaretEncoding.Decode(encoded), Is.EqualTo(expected));
    }

    [Test]
    public void Codec_is_symmetric_for_every_special_case()
    {
        var samples = new[]
        {
            "plain",
            "a^b",           // caret
            "line1\nline2",  // newline
            "John \"JD\" Doe", // DQUOTE
            "^n",            // literal caret+n
            "^^",            // literal double caret
            "^'",            // literal caret+quote
            "mix ^ \" \n end",
            "",
            "^",
        };

        foreach (var v in samples)
        {
            var round = CaretEncoding.Decode(CaretEncoding.Encode(v));
            Assert.That(round, Is.EqualTo(v), $"decode(encode(x)) must equal x for [{v}]");
        }
    }

    // ---- Public-API round-trip self-oracle: Load(Serialize()) ----

    private static string Serialize(Calendar cal) => new CalendarSerializer().SerializeToString(cal)!;

    private static string RoundTripParam(string paramValue)
    {
        var cal = new Calendar();
        var ev = new CalendarEvent { Summary = "s" };
        var att = new Attendee("mailto:a@b.com");
        att.Parameters.Set("CN", paramValue);
        ev.Attendees.Add(att);
        cal.Events.Add(ev);

        var serialized = Serialize(cal);
        var loaded = Calendar.Load(serialized)!; // must not throw
        return loaded.Events[0].Attendees[0].Parameters.Get("CN");
    }

    [TestCase("line1\nline2")]         // newline: threw before the fix (unparseable output)
    [TestCase("John \"JD\" Doe")]      // DQUOTE: was silently stripped before the fix
    [TestCase("50% off ^_^")]          // bare carets: must survive ^ -> ^^ -> ^
    [TestCase("^n")]                   // literal caret+n must not become a newline
    [TestCase("Doe, John")]            // comma value stays intact (still DQUOTE-wrapped)
    [TestCase("a^b,c")]                // caret and comma together
    [TestCase("plain name")]
    public void Param_value_roundtrips_through_public_api(string value)
    {
        Assert.That(RoundTripParam(value), Is.EqualTo(value));
    }

    [Test]
    public void Plain_param_value_is_not_altered()
    {
        // RFC 6868 only transforms ^, DQUOTE and newline; ordinary values are byte-identical.
        var cal = new Calendar();
        var ev = new CalendarEvent { Summary = "s" };
        var att = new Attendee("mailto:a@b.com");
        att.Parameters.Set("CN", "John Doe");
        ev.Attendees.Add(att);
        cal.Events.Add(ev);

        var serialized = Serialize(cal);
        Assert.That(serialized, Does.Contain("CN=John Doe"));
        Assert.That(serialized, Does.Not.Contain("^"));
    }

    [Test]
    public void Encoding_targets_the_value_not_the_parameter_name()
    {
        // A caret in the value is encoded; the structural name/=/: are untouched.
        var encoded = CaretEncoding.Encode("v^v");
        Assert.That(encoded, Is.EqualTo("v^^v"));
        // The serializer only ever feeds VALUES to the codec (see ParameterSerializer).
        var recovered = RoundTripParam("v^v");
        Assert.That(recovered, Is.EqualTo("v^v"));
    }
}
