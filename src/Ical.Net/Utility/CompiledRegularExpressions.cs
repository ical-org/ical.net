using System.Text.RegularExpressions;

// ReSharper disable InconsistentNaming

namespace Ical.Net.Utility;

#if SUPPORTS_REGEX_CODEGEN
internal static partial class CompiledRegularExpressions
#else
internal static class CompiledRegularExpressions
#endif
{
    private const string NarrowRequestPattern = @"(.*?[^\\]);(.*?[^\\]);(.+)";
    private const RegexOptions NarrowRequestOptions = RegexOptions.Compiled;
#if SUPPORTS_REGEX_CODEGEN
    [GeneratedRegex(NarrowRequestPattern, NarrowRequestOptions)]
    private static partial Regex NarrowRequestRegex();
    internal static Regex NarrowRequest => NarrowRequestRegex();
#else
    internal static readonly Regex NarrowRequest = new(NarrowRequestPattern, NarrowRequestOptions);
#endif

    private const string BroadRequestPattern = @"(.*?[^\\]);(.+)";
    private const RegexOptions BroadRequestOptions = RegexOptions.Compiled;
#if SUPPORTS_REGEX_CODEGEN
    [GeneratedRegex(BroadRequestPattern, BroadRequestOptions)]
    private static partial Regex BroadRequestRegex();
    internal static Regex BroadRequest => BroadRequestRegex();
#else
    internal static readonly Regex BroadRequest = new(BroadRequestPattern, BroadRequestOptions);
#endif

    private const string NormalizeToCrLfPattern = @"((\r(?=[^\n]))|((?<=[^\r])\n))";
    private const RegexOptions NormalizeToCrLfOptions = RegexOptions.Compiled;
#if SUPPORTS_REGEX_CODEGEN
    [GeneratedRegex(NormalizeToCrLfPattern, NormalizeToCrLfOptions)]
    private static partial Regex NormalizeToCrLfRegex();
    internal static Regex NormalizeToCrLf = NormalizeToCrLfRegex();
#else
    internal static readonly Regex NormalizeToCrLf = new(NormalizeToCrLfPattern, NormalizeToCrLfOptions);
#endif

    private const string NewLinePattern = @"(\r\n[ \t])";
    private const RegexOptions NewLineOptions = RegexOptions.Compiled;
#if SUPPORTS_REGEX_CODEGEN
    [GeneratedRegex(NewLinePattern, NewLineOptions)]
    private static partial Regex NewLineRegex();
    internal static Regex NewLine => NewLineRegex();
#else
    internal static readonly Regex NewLine = new(NewLinePattern, NewLineOptions);
#endif

    private const string DateOnlyPattern = @"^((\d{4})(\d{2})(\d{2}))?$";
    private const RegexOptions DateOnlyOptions = RegexOptions.Compiled | RegexOptions.IgnoreCase;
#if SUPPORTS_REGEX_CODEGEN
    [GeneratedRegex(DateOnlyPattern, DateOnlyOptions)]
    private static partial Regex DateOnlyRegex();
    internal static Regex DateOnly => DateOnlyRegex();
#else
    internal static readonly Regex DateOnly = new(DateOnlyPattern, DateOnlyOptions);
#endif

    private const string FullDateTimePattern = @"^((\d{4})(\d{2})(\d{2}))T((\d{2})(\d{2})(\d{2})(Z)?)$";
    private const RegexOptions FullDateTimeOptions = RegexOptions.Compiled | RegexOptions.IgnoreCase;
#if SUPPORTS_REGEX_CODEGEN
    [GeneratedRegex(FullDateTimePattern, FullDateTimeOptions)]
    private static partial Regex FullDateTimeRegex();
    internal static Regex FullDateTime = FullDateTimeRegex();
#else
    internal static readonly Regex FullDateTime = new(FullDateTimePattern, FullDateTimeOptions);
#endif

    private const string SingleBackslashPattern = @"(?<!\\)\\(?!\\)";
    private const RegexOptions SingleBackslashOptions = RegexOptions.Compiled;
#if SUPPORTS_REGEX_CODEGEN
    [GeneratedRegex(SingleBackslashPattern, SingleBackslashOptions)]
    private static partial Regex SingleBackslashRegex();
    internal static Regex SingleBackslash => SingleBackslashRegex();
#else
    internal static readonly Regex SingleBackslash = new(SingleBackslashPattern, SingleBackslashOptions);
#endif

    private const string OtherIntervalPattern =
        @"every\s+(?<Interval>other|\d+)?\w{0,2}\s*(?<Freq>second|minute|hour|day|week|month|year)s?,?\s*(?<More>.+)";

    private const RegexOptions OtherIntervalOptions = RegexOptions.Compiled | RegexOptions.IgnoreCase;

#if SUPPORTS_REGEX_CODEGEN
    [GeneratedRegex(OtherIntervalPattern, OtherIntervalOptions)]
    private static partial Regex OtherIntervalRegex();
    internal static Regex OtherInterval => OtherIntervalRegex();
#else
    internal static readonly Regex OtherInterval = new(OtherIntervalPattern, OtherIntervalOptions);
#endif

    private const string AdverbFrequenciesPattern =
        @"FREQ=(SECONDLY|MINUTELY|HOURLY|DAILY|WEEKLY|MONTHLY|YEARLY);?(.*)";

    private const RegexOptions AdverbFrequenciesOptions = RegexOptions.Compiled | RegexOptions.IgnoreCase;

#if SUPPORTS_REGEX_CODEGEN
    [GeneratedRegex(AdverbFrequenciesPattern, AdverbFrequenciesOptions)]
    private static partial Regex AdverbFrequenciesRegex();
    internal static Regex AdverbFrequencies => AdverbFrequenciesRegex();
#else
    internal static readonly Regex AdverbFrequencies = new(AdverbFrequenciesPattern, AdverbFrequenciesOptions);
#endif

    private const string NumericTemporalUnitsPattern = @"(?<Num>\d+)\w\w\s+(?<Type>second|minute|hour|day|week|month)";
    private const RegexOptions NumericTemporalUnitsOptions = RegexOptions.Compiled | RegexOptions.IgnoreCase;
#if SUPPORTS_REGEX_CODEGEN
    [GeneratedRegex(NumericTemporalUnitsPattern, NumericTemporalUnitsOptions)]
    private static partial Regex NumericTemporalUnitsRegex();
    internal static Regex NumericTemporalUnits = NumericTemporalUnitsRegex();
#else
    internal static readonly Regex NumericTemporalUnits =
        new(NumericTemporalUnitsPattern, NumericTemporalUnitsOptions);
#endif

    private const string TemporalUnitTypePattern = @"(?<Type>second|minute|hour|day|week|month)\s+(?<Num>\d+)";
    private const RegexOptions TemporalUnitTypeOptions = RegexOptions.Compiled | RegexOptions.IgnoreCase;
#if SUPPORTS_REGEX_CODEGEN
    [GeneratedRegex(TemporalUnitTypePattern, TemporalUnitTypeOptions)]
    private static partial Regex TemporalUnitTypeRegex();
    internal static Regex TemporalUnitType => TemporalUnitTypeRegex();
#else
    internal static readonly Regex TemporalUnitType = new(TemporalUnitTypePattern, TemporalUnitTypeOptions);
#endif

    private const string RelativeDaysOfWeekPattern =
        @"(?<Num>\d+\w{0,2})?(\w|\s)+?(?<First>first)?(?<Last>last)?\s*((?<Day>sunday|monday|tuesday|wednesday|thursday|friday|saturday)\s*(and|or)?\s*)+";

    private const RegexOptions RelativeDaysOfWeekOptions = RegexOptions.Compiled | RegexOptions.IgnoreCase;

#if SUPPORTS_REGEX_CODEGEN
    [GeneratedRegex(RelativeDaysOfWeekPattern, RelativeDaysOfWeekOptions)]
    private static partial Regex RelativeDaysOfWeekRegex();
    internal static Regex RelativeDaysOfWeek = RelativeDaysOfWeekRegex();
#else
    internal static readonly Regex RelativeDaysOfWeek = new(RelativeDaysOfWeekPattern, RelativeDaysOfWeekOptions);
#endif

    private const string TimePattern =
        @"at\s+(?<Hour>\d{1,2})(:(?<Minute>\d{2})((:|\.)(?<Second>\d{2}))?)?\s*(?<Meridian>(a|p)m?)?";

    private const RegexOptions TimeOptions = RegexOptions.Compiled | RegexOptions.IgnoreCase;
#if SUPPORTS_REGEX_CODEGEN
    [GeneratedRegex(TimePattern, TimeOptions)]
    private static partial Regex TimeRegex();
    internal static Regex Time => TimeRegex();
#else
    internal static readonly Regex Time = new(TimePattern, TimeOptions);
#endif

    private const string RecurUntilPattern = @"^\s*until\s+(?<DateTime>.+)$";
    private const RegexOptions RecurUntilOptions = RegexOptions.Compiled | RegexOptions.IgnoreCase;
#if SUPPORTS_REGEX_CODEGEN
    [GeneratedRegex(RecurUntilPattern, RecurUntilOptions)]
    private static partial Regex RecurUntilRegex();
    internal static Regex RecurUntil => RecurUntilRegex();
#else
    internal static readonly Regex RecurUntil = new(RecurUntilPattern, RecurUntilOptions);
#endif

    private const string SpecificRecurrenceCountPattern = @"^\s*for\s+(?<Count>\d+)\s+occurrences\s*$";
    private const RegexOptions SpecificRecurrenceCountOptions = RegexOptions.Compiled | RegexOptions.IgnoreCase;
#if SUPPORTS_REGEX_CODEGEN
    [GeneratedRegex(SpecificRecurrenceCountPattern, SpecificRecurrenceCountOptions)]
    private static partial Regex SpecificRecurrenceCountRegex();
    internal static Regex SpecificRecurrenceCount => SpecificRecurrenceCountRegex();
#else
    internal static readonly Regex SpecificRecurrenceCount =
        new(SpecificRecurrenceCountPattern, SpecificRecurrenceCountOptions);
#endif

    private const string StatusCodePattern = @"\d(\.\d+)*";
    private const RegexOptions StatusCodePatternOptions = RegexOptions.Compiled | RegexOptions.CultureInvariant;
#if SUPPORTS_REGEX_CODEGEN
    [GeneratedRegex(StatusCodePattern, StatusCodePatternOptions)]
    private static partial Regex StatusCodeRegex();
    internal static Regex StatusCode => StatusCodeRegex();
#else
    internal static readonly Regex StatusCode = new(StatusCodePattern, StatusCodePatternOptions);
#endif

    private const string DayOfWeekPattern = @"(\+|-)?(\d{1,2})?(\w{2})";
    private const RegexOptions DayOfWeekPatternOptions = RegexOptions.Compiled | RegexOptions.IgnoreCase;
#if SUPPORTS_REGEX_CODEGEN
    [GeneratedRegex(DayOfWeekPattern, DayOfWeekPatternOptions)]
    private static partial Regex DayOfWeekRegex();
    internal static Regex DayOfWeek => DayOfWeekRegex();
#else
    internal static readonly Regex DayOfWeek = new(DayOfWeekPattern, DayOfWeekPatternOptions);
#endif

    private const string UnescapedCommasPattern = @"(?<!\\),";
    private const RegexOptions UnescapedCommasOptions = RegexOptions.Compiled;
#if SUPPORTS_REGEX_CODEGEN
    [GeneratedRegex(UnescapedCommasPattern, UnescapedCommasOptions)]
    private static partial Regex UnescapedCommasRegex();
    internal static Regex UnescapedCommas => UnescapedCommasRegex();
#else
    internal static readonly Regex UnescapedCommas = new(UnescapedCommasPattern, UnescapedCommasOptions);
#endif

    private const string TimespanPattern =
        @"^(?<sign>\+|-)?P(((?<week>\d+)W)|(?<main>((?<day>\d+)D)?(?<time>T((?<hour>\d+)H)?((?<minute>\d+)M)?((?<second>\d+)S)?)?))$";

    private const RegexOptions TimespanOptions = RegexOptions.Compiled | RegexOptions.IgnoreCase;
#if SUPPORTS_REGEX_CODEGEN
    [GeneratedRegex(TimespanPattern, TimespanOptions)]
    private static partial Regex TimespanRegex();
    internal static Regex Timespan => TimespanRegex();
#else
    internal static readonly Regex Timespan = new(TimespanPattern, TimespanOptions);
#endif

    private const string DecodeOffsetPattern = @"(\+|-)(\d{2})(\d{2})(\d{2})?";
    private const RegexOptions DecodeOffsetOptions = RegexOptions.Compiled;
#if SUPPORTS_REGEX_CODEGEN
    [GeneratedRegex(DecodeOffsetPattern, DecodeOffsetOptions)]
    private static partial Regex DecodeOffsetRegex();
    private static Regex DecodeOffset => DecodeOffsetRegex();
#else
    internal static readonly Regex DecodeOffset = new(DecodeOffsetPattern, DecodeOffsetOptions);
#endif


    internal const string ContentLineNameGroup = "name";
    internal const string ContentLineValueGroup = "value";
    internal const string ContentLineParamNameGroup = "paramName";
    internal const string ContentLineParamValueGroup = "paramValue";

    // name          = iana-token / x-name
    // iana-token    = 1*(ALPHA / DIGIT / "-")
    // x-name        = "X-" [vendorid "-"] 1*(ALPHA / DIGIT / "-")
    // vendorid      = 3*(ALPHA / DIGIT)
    // Add underscore to match behavior of bug 2033495
    private const string ContentLineIdentifier = "[-A-Za-z0-9_]+";

    // param-value   = paramtext / quoted-string
    // paramtext     = *SAFE-CHAR
    // quoted-string = DQUOTE *QSAFE-CHAR DQUOTE
    // QSAFE-CHAR    = WSP / %x21 / %x23-7E / NON-US-ASCII
    // ; Any character except CONTROL and DQUOTE
    // SAFE-CHAR     = WSP / %x21 / %x23-2B / %x2D-39 / %x3C-7E
    //               / NON-US-ASCII
    // ; Any character except CONTROL, DQUOTE, ";", ":", ","
    private const string ContentLineParamValue =
        $"((?<{ContentLineParamValueGroup}>[^\\x00-\\x08\\x0A-\\x1F\\x7F\";:,]*)|\"(?<{ContentLineParamValueGroup}>[^\\x00-\\x08\\x0A-\\x1F\\x7F\"]*)\")";

    // param         = param-name "=" param-value *("," param-value)
    // param-name    = iana-token / x-name
    private const string ContentLineParamName = $"(?<{ContentLineParamNameGroup}>{ContentLineIdentifier})";

    private const string ContentLineParam =
        $"{ContentLineParamName}={ContentLineParamValue}(,{ContentLineParamValue})*";

    // contentline   = name *(";" param ) ":" value CRLF
    private const string ContentLineName = $"(?<{ContentLineNameGroup}>{ContentLineIdentifier})";

    // value         = *VALUE-CHAR
    private const string ContentLineValue = $"(?<{ContentLineValueGroup}>[^\\x00-\\x08\\x0E-\\x1F\\x7F]*)";
    private const RegexOptions ContentLineOptions = RegexOptions.Compiled;
    private const string ContentLinePattern = $"^{ContentLineName}(;{ContentLineParam})*:{ContentLineValue}$";

#if SUPPORTS_REGEX_CODEGEN
    [GeneratedRegex(ContentLinePattern, ContentLineOptions)]
    private static partial Regex ContentLineRegex();
    internal static Regex ContentLine => ContentLineRegex();
#else
    internal static readonly Regex ContentLine = new(ContentLinePattern, ContentLineOptions);
#endif
}