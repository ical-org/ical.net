//
// Copyright ical.net project maintainers and contributors.
// Licensed under the MIT license.
//

using System.Collections;
using System.Linq;
using System.Text;
using Ical.Net.Utility;
using NUnit.Framework;

namespace Ical.Net.Tests;

public class TextUtilTests
{
    [Test, TestCaseSource(nameof(FoldLines_TestCases))]
    public string FoldLines_Tests(string input)
    {
        var sb = new StringBuilder(input.Length);
        sb.FoldLines(input);
        return sb.ToString();
    }

    public static IEnumerable FoldLines_TestCases()
    {
        yield return new TestCaseData("No folding")
            .Returns("No folding" + SerializationConstants.LineBreak)
            .SetName("Short string remains unfolded");

        const string exactly85OctetsReturns =
            "HelloWorldHelloWorldHelloWorldHelloWorldHelloWorldHelloWorldHelloWorldHello" + SerializationConstants.LineBreak
            + " WorldHello" + SerializationConstants.LineBreak;

        yield return new TestCaseData("HelloWorldHelloWorldHelloWorldHelloWorldHelloWorldHelloWorldHelloWorldHelloWorldHello")
            .Returns(exactly85OctetsReturns)
            .SetName("10 Octets remainder gets folded");

        yield return new TestCaseData(
                "           HelloWorldHelloWorldHelloWorldHelloWorldHelloWorldHelloWorld              ")
            .Returns("           HelloWorldHelloWorldHelloWorldHelloWorldHelloWorldHelloWorld    " +
                     SerializationConstants.LineBreak + "           " + SerializationConstants.LineBreak)
            .SetName("85 Octets long string with leading and trailing whitespace is folded without trimming");

        var exactly310Octets = string.Concat(Enumerable.Repeat("HelloWorld", 31));

        const string exactly310OctetsReturns =
            "HelloWorldHelloWorldHelloWorldHelloWorldHelloWorldHelloWorldHelloWorldHello" + SerializationConstants.LineBreak +
            " WorldHelloWorldHelloWorldHelloWorldHelloWorldHelloWorldHelloWorldHelloWorl" + SerializationConstants.LineBreak +
            " dHelloWorldHelloWorldHelloWorldHelloWorldHelloWorldHelloWorldHelloWorldHel" + SerializationConstants.LineBreak +
            " loWorldHelloWorldHelloWorldHelloWorldHelloWorldHelloWorldHelloWorldHelloWo" + SerializationConstants.LineBreak +
            " rldHelloWorld" + SerializationConstants.LineBreak;

        yield return new TestCaseData(exactly310Octets)
            .Returns(exactly310OctetsReturns)
            .SetName("310 Octets long string is split onto multiple lines at a width of 75 octets");

        const string exactly75Octets = "HelloWorldHelloWorldHelloWorldHelloWorldHelloWorldHelloWorldHelloWorldHello";
        const string exactly75OctetsReturns = "HelloWorldHelloWorldHelloWorldHelloWorldHelloWorldHelloWorldHelloWorldHello" + SerializationConstants.LineBreak;

        yield return new TestCaseData(exactly75Octets)
            .Returns(exactly75OctetsReturns)
            .SetName("String with exactly 75 octets remains unfolded");

        // String containing multi-byte characters with 210 Octets
        // (Chinese "Hello World" repeated 10 times)
        const string multiByteString210Octets = "こんにちは世界こんにちは世界こんにちは世界こんにちは世界こんにちは世界こんにちは世界こんにちは世界こんにちは世界こんにちは世界こんにちは世界";
        const string multiByteString210OctetsReturns =
            "こんにちは世界こんにちは世界こんにちは世界こんにち" + SerializationConstants.LineBreak +
            " は世界こんにちは世界こんにちは世界こんにちは世界" + SerializationConstants.LineBreak +
            " こんにちは世界こんにちは世界こんにちは世界" + SerializationConstants.LineBreak;

        yield return new TestCaseData(multiByteString210Octets)
            .Returns(multiByteString210OctetsReturns)
            .SetName("String with multi-byte characters exceeding 75 octets is folded correctly");
    }
}
