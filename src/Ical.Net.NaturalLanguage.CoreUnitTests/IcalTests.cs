using Ical.Net.DataTypes;

namespace Ical.Net.NaturalLanguage.CoreUnitTests;

public class IcalTests {
    [Theory]
    [InlineData("Every day", "RRULE:FREQ=DAILY")]
    [InlineData("Every day at 10, 12 and 17", "RRULE:FREQ=DAILY;BYHOUR=10,12,17")]
    [InlineData("Every week", "RRULE:FREQ=WEEKLY")]
    [InlineData("Every hour", "RRULE:FREQ=HOURLY")]
    [InlineData("Every 4 hours", "RRULE:FREQ=HOURLY;INTERVAL=4")]
    [InlineData("Every week on Tuesday", "RRULE:FREQ=WEEKLY;BYDAY=TU")]
    [InlineData("Every week on Monday, Wednesday", "RRULE:FREQ=WEEKLY;BYDAY=MO,WE", "Every week on Monday and Wednesday")]
    [InlineData("Every week on Monday and Wednesday", "RRULE:FREQ=WEEKLY;BYDAY=MO,WE")]
    [InlineData("Every weekday", "RRULE:FREQ=WEEKLY;BYDAY=MO,TU,WE,TH,FR")]
    [InlineData("Every 2 weeks", "RRULE:FREQ=WEEKLY;INTERVAL=2")]
    [InlineData("Every month", "RRULE:FREQ=MONTHLY")]
    [InlineData("Every 6 months", "RRULE:FREQ=MONTHLY;INTERVAL=6")]
    [InlineData("Every year", "RRULE:FREQ=YEARLY")]
    [InlineData("Every year on the 1st Friday", "RRULE:FREQ=YEARLY;BYDAY=1FR")]
    [InlineData("Every year on the 13th Friday", "RRULE:FREQ=YEARLY;BYDAY=13FR")]
    [InlineData("Every month on the 4th", "RRULE:FREQ=MONTHLY;BYMONTHDAY=4")]
    [InlineData("Every month on the 4th last", "RRULE:FREQ=MONTHLY;BYMONTHDAY=-4")]
    [InlineData("Every month on the 3rd Tuesday", "RRULE:FREQ=MONTHLY;BYDAY=3TU")]
    [InlineData("Every month on the 3rd last Tuesday", "RRULE:FREQ=MONTHLY;BYDAY=-3TU")]
    [InlineData("Every month on the last Monday", "RRULE:FREQ=MONTHLY;BYDAY=-1MO")]
    [InlineData("Every month on the 2nd last Friday", "RRULE:FREQ=MONTHLY;BYDAY=-2FR")]
    [InlineData("Every week for 20 times", "RRULE:FREQ=WEEKLY;COUNT=20")]
    public void TestToAndFromText(string text, string rpStr, string? returnText = null) {
        returnText ??= text;
        var rp = Parser.ParseText(text);
        Assert.Equal(rpStr["RRULE:".Length..], rp?.ToString());
        var rp2 = new RecurrencePattern(rpStr);
        var friendlyName = ToText.ToFriendlyText(rp2);
        Assert.Equal(returnText, friendlyName);
    }
}
