//
// Copyright ical.net project maintainers and contributors.
// Licensed under the MIT license.
//

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Runtime.Serialization;
using Ical.Net.CalendarComponents;
using Ical.Net.Utility;

namespace Ical.Net.Serialization;

/// <summary>
/// Provides functionality to deserialize iCalendar data streams into
/// calendar component objects according to the RFC 5545 specification.
/// </summary>
/// <remarks>
/// The serializer parses unfolded iCalendar content lines and constructs a hierarchy of
/// calendar components and their properties. It supports standard iCalendar line folding
/// and parameter parsing rules.
/// <para/>
/// This class is thread-safe.
/// Use the static Default instance for typical deserialization scenarios.
/// </remarks>
public class SimpleDeserializer
{
    internal SimpleDeserializer(
        DataTypeMapper dataTypeMapper,
        ISerializerFactory serializerFactory,
        CalendarComponentFactory componentFactory)
    {
        _dataTypeMapper = dataTypeMapper;
        _serializerFactory = serializerFactory;
        _componentFactory = componentFactory;
    }

    public static readonly SimpleDeserializer Default =
        new(new DataTypeMapper(),
            new SerializerFactory(),
            new CalendarComponentFactory());

    private readonly DataTypeMapper _dataTypeMapper;
    private readonly ISerializerFactory _serializerFactory;
    private readonly CalendarComponentFactory _componentFactory;

    /// <summary>
    /// Deserializes iCalendar data from the specified text reader
    /// and returns the top-level calendar components as an enumerable collection.
    /// <para/>
    /// Line endings are handled as follows:<br/>
    /// * RFC-aware - Properly handles CRLF as mandated<br/>
    /// * Accepts LF-only for lenient interoperability<br/>
    /// * Accepts bare CR for compatibility with TextReader.ReadLine()<br/>
    /// </summary>
    /// <param name="reader">A <see cref="TextReader"/> that provides the iCalendar data to deserialize. The reader must supply data in a valid
    /// iCalendar format and will be read to the end of the stream.
    /// </param>
    /// <returns>
    /// An enumerable collection of <see cref="ICalendarComponent"/> objects representing
    /// the top-level components found in the input data.
    /// Each component may contain child components and properties.
    /// </returns>
    public IEnumerable<ICalendarComponent> Deserialize(TextReader reader)
    {
        var context = new SerializationContext();
        var stack = new Stack<ICalendarComponent?>();
        ICalendarComponent? currentComponent = null;

        foreach (var calProperty in DeserializeContentLines(reader, context))
        {
            if (string.Equals(calProperty.Name, "BEGIN", StringComparison.OrdinalIgnoreCase))
            {
                stack.Push(currentComponent); // The first component pushed is null
                EnsureValueIsString(calProperty, out var calPropertyValue);

                currentComponent = _componentFactory.Build(calPropertyValue);
                SerializationUtil.OnDeserializing(currentComponent);
            }
            else
            {
                // Ensure currentComponent is not null for non-BEGIN lines
                ThrowIfNull(currentComponent, calProperty);

                if (string.Equals(calProperty.Name, "END", StringComparison.OrdinalIgnoreCase))
                {
                    EnsureValueIsString(calProperty, out var calPropertyValue);
                    EnsureExpectedEnd(calPropertyValue, currentComponent!);

                    SerializationUtil.OnDeserialized(currentComponent!);
                    var latestFinished = currentComponent!;

                    currentComponent = stack.Pop();
                    // The last component popped is null
                    if (currentComponent == null)
                    {
                        yield return latestFinished;
                    }
                    else
                    {
                        currentComponent.Children.Add(latestFinished);
                    }
                }
                else
                {
                    currentComponent!.Properties.Add(calProperty);
                }
            }
        }

        // Check for unclosed component
        ThrowIfUnclosed(currentComponent);
    }

    private static void EnsureValueIsString(CalendarProperty calProperty, out string value)
    {
        if (calProperty.Value is not string val)
        {
            // This should never happen
            Debug.Assert(calProperty.Value is string, "CalendarProperty.Value must be of type string.");
            value = string.Empty;
        }
        else
        {
            value = val;
        }
    }

    private static void EnsureExpectedEnd(string calPropertyValue, ICalendarComponent currentComponentName)
    {
        if (!string.Equals(calPropertyValue, currentComponentName.Name, StringComparison.OrdinalIgnoreCase))
        {
            throw new SerializationException($"Expected 'END:{currentComponentName.Name}', found 'END:{calPropertyValue}'");
        }
    }

    private static void ThrowIfNull(ICalendarComponent? currentComponent, CalendarProperty calProperty)
    {
        if (currentComponent == null)
        {
            throw new SerializationException($"Expected 'BEGIN', found '{calProperty.Name}'");
        }
    }

    private static void ThrowIfUnclosed(ICalendarComponent? currentComponent)
    {
        if (currentComponent != null)
        {
            throw new SerializationException($"Unclosed component {currentComponent.Name}");
        }
    }

    private CalendarProperty DeserializeContentLine(SerializationContext context, ReadOnlySpan<char> input)
    {
        const string nameEndChars = ":;";

        // 1. Identify the Name (up to the first ';' or ':')
        var nameEnd = input.IndexOfAny(nameEndChars);
        ThrowIfNameEndMissing(input, nameEnd);

        var name = input.Slice(0, nameEnd).ToString().ToUpperInvariant();
        var property = new CalendarProperty(name);
        context.Push(property);

        var remaining = input.Slice(nameEnd);

        // 2. Parse Parameters if they exist (identified by ';')
        while (IsInParameters(remaining))
        {
            remaining = remaining.Slice(1); // Consume ';'

            var equalsIdx = remaining.IndexOf('=');
            if (equalsIdx == -1) break;

            var paramName = remaining.Slice(0, equalsIdx).ToString();
            var parameter = new CalendarParameter(paramName);
            remaining = remaining.Slice(equalsIdx + 1);

            // Parse one or more comma-separated values (handling quoted strings)
            var indexAfterParsing = ParseValues(remaining, parameter, nameEndChars, input);

            property.AddParameter(parameter);
            remaining = remaining.Slice(indexAfterParsing);
            // Note: The loop continues if 'remaining' starts with ';'
        }

        // 3. Ensure we have a ':' before the Property Value
        ThrowIfMalformed(remaining, input);

        // 4. Set the Property Value
        var propertyValue = remaining.Slice(1);
        SetPropertyValue(context, property, propertyValue);

        context.Pop();
        return property;
    }

    private static int ParseValues(ReadOnlySpan<char> remaining, CalendarParameter parameter, string nameEndChars, ReadOnlySpan<char> fullLine)
    {
        var i = 0;
        var inQuotes = false;
        var valueStart = 0;

        for (; i < remaining.Length; i++)
        {
            var c = remaining[i];
            // Skip delimiter checks while inside quotes so values
            // containing ,;: aren’t split prematurely
            // Note: Quotes are expected to be balanced
            inQuotes = ToggleInQuotes(c, inQuotes);
            if (inQuotes) continue;
            
            // Wait for a delimiter to extract the value
            if (",;:".IndexOf(c) == -1) continue; // netstandard2.0 compatible

            // Extract the value
            var pValueSpan = remaining.Slice(valueStart, i - valueStart);

            // Unquote the value if necessary
            pValueSpan = Unquote(pValueSpan);

            parameter.AddValue(pValueSpan.ToString());
            valueStart = i + 1;

            // If we hit (another) ';' or ':', we are done with this parameter
            if (nameEndChars.IndexOf(c) != -1) break; // netstandard2.0 compatible
        }

        // Check for unbalanced quotes at the end of the line
        if (inQuotes)
        {
            throw new SerializationException($"Unbalanced quotes in line '{fullLine.ToString()}'");
        }

        return i;
    }

    private static bool IsInParameters(ReadOnlySpan<char> remaining)
        => remaining.Length > 0 && remaining[0] == ';';

    private static void ThrowIfNameEndMissing(ReadOnlySpan<char> input, int nameEnd)
    {
        if (nameEnd == -1)
        {
            throw new SerializationException($"Could not parse line (missing name): '{input.ToString()}'");
        }
    }

    private static bool ToggleInQuotes(char c, bool inQuotes)
    {
        if (c == '"') inQuotes = !inQuotes;
        return inQuotes;
    }

    private static ReadOnlySpan<char> Unquote(ReadOnlySpan<char> pValueSpan)
    {
        if (pValueSpan.Length >= 2 && pValueSpan[0] == '"' && pValueSpan[pValueSpan.Length - 1] == '"')
        {
            pValueSpan = pValueSpan.Slice(1, pValueSpan.Length - 2);
        }

        return pValueSpan;
    }

    private static void ThrowIfMalformed(ReadOnlySpan<char> remaining, ReadOnlySpan<char> input)
    {
        if (remaining.Length == 0 || remaining[0] != ':')
        {
            // If there was no colon, the line is technically malformed according to RFC 5545
            throw new SerializationException($"Could not parse malformed line (missing colon): '{input.ToString()}'");
        }
    }

    /// <summary>
    /// Reads and parses unfolded content lines from the specified text reader
    /// according to RFC 5545 line folding rules, yielding CalendarProperty objects.
    /// </summary>
    /// <remarks>
    /// This method processes input according to the iCalendar (RFC 5545) specification, combining
    /// folded lines into single logical lines and parsing them into CalendarProperty objects.
    /// RFC 5545 mandates CRLF (\\r\\n) line endings, but this implementation accepts standalone 
    /// LF (\\n) and bare CR (\\r) for practical interoperability.
    /// The caller is responsible for disposing the provided reader if necessary.
    /// </remarks>
    /// <param name="reader">The text reader from which to read the content lines. Cannot be null.</param>
    /// <param name="context">The serialization context for parsing properties.</param>
    /// <returns>
    /// An enumerable collection of CalendarProperty objects representing each parsed content line.
    /// The collection is empty if no lines are present.</returns>
    private IEnumerable<CalendarProperty> DeserializeContentLines(TextReader reader, SerializationContext context)
    {
        var buffer = System.Buffers.ArrayPool<char>.Shared.Rent(8192);

        // Use PooledCharBuffer as a buffer for the unfolded content, using zero-allocation streaming.
        // The buffer is reused (reset) after each line is yielded.
        var unfoldedBuffer = new PooledCharBuffer(PooledCharBuffer.DefaultBufferCapacity);

        var state = ParserState.Normal;

        try
        {
            int charsRead;
            while ((charsRead = reader.Read(buffer, 0, buffer.Length)) > 0)
            {
                // character-by-character loop is the most maintainable
                for (var i = 0; i < charsRead; i++)
                {
                    var c = buffer[i];
                    var shouldYield = false;

                    state = state switch
                    {
                        ParserState.FoldCheck => ProcessFoldCheck(c, ref unfoldedBuffer, ref i, out shouldYield),
                        ParserState.SawCr => ProcessSawCr(c, ref i),
                        ParserState.Normal => ProcessNormal(buffer, charsRead, ref unfoldedBuffer, ref i),
                        _ => state
                    };

                    if (shouldYield && unfoldedBuffer.Length > 0) //NOSONAR - false positive 'always evaluates to false'
                    {
                        // Parse the current line immediately before the buffer is reused
                        yield return DeserializeContentLine(context, unfoldedBuffer.Span);

                        // Reset the buffer for reuse with the next line
                        unfoldedBuffer.Reset();
                    }
                }
            }

            // Parse and yield the final line if the buffer isn't empty
            if (unfoldedBuffer.Length > 0)
            {
                yield return DeserializeContentLine(context, unfoldedBuffer.Span);
            }
        }
        finally
        {
            // Must be explicitly disposed, because 'using'
            // is not allowed when passing the array by ref to methods.
            unfoldedBuffer.Dispose();
            System.Buffers.ArrayPool<char>.Shared.Return(buffer, clearArray: true);
        }
    }

    private static ParserState ProcessFoldCheck(
        char c,
        ref PooledCharBuffer unfoldedBuffer,
        ref int i,
        out bool shouldYield)
    {
        /*
        Handling folded lines per RFC 5545:
        Example: "SUMMARY:This is a very lo\r\n ng line"
                                           |   | 
                                     newline   space (fold marker)
        State transitions:
           1.	Reading "lo" => Normal
           2.	Hit \r => SawCr
           3.	Hit \n => FoldCheck
           4.	Hit (space) => Fold detected
             a) The space is discarded (not written to buffer)
             b) Return to 'Normal' to continue reading "ng line"
                as part of the same logical line
        */

        shouldYield = false;

        // If the char after a newline is Space or Tab, it's a fold.
        if (c == ' ' || c == '\t')
        {
            return ParserState.Normal;
        }

        // Not a fold. Signal that the current line should be yielded.
        shouldYield = unfoldedBuffer.Length > 0;

        // We must re-evaluate this character in the 'Normal' state context.
        i--;
        return ParserState.Normal;
    }

    private static ParserState ProcessSawCr(char c, ref int i)
    {
        // RFC 5545 mandates CRLF line endings, but we accept standalone LF and bare CR for compatibility.
        // If CR is followed by LF, consume both. Otherwise, treat CR as a line ending.
        if (c == '\n')
        {
            // CRLF sequence - consume the LF and proceed to fold check
            return ParserState.FoldCheck;
        }

        // Bare CR - treat as line ending but re-process current character
        i--;
        return ParserState.FoldCheck;
    }

    private static ParserState ProcessNormal(
        char[] buffer,
        int charsRead,
        ref PooledCharBuffer unfoldedBuffer,
        ref int i)
    {
        // Look ahead to the next newline character so we can write chunks.
        // This search is more efficient than writing one char at a time.
        var searchSpan = buffer.AsSpan(i, charsRead - i);
        var newlineIndex = searchSpan.IndexOfAny('\r', '\n');

        if (newlineIndex == -1)
        {
            // No newlines found, write all remaining characters
            unfoldedBuffer.Write(searchSpan);
            i = charsRead - 1; // Set to end, loop will increment to charsRead
            return ParserState.Normal;
        }

        if (newlineIndex > 0)
        {
            // Write characters up to the newline
            unfoldedBuffer.Write(searchSpan.Slice(0, newlineIndex));
        }

        // Move to the newline character and handle state transition
        i += newlineIndex;

        return buffer[i] == '\r' ? ParserState.SawCr : ParserState.FoldCheck;
    }

    private void SetPropertyValue(SerializationContext context, CalendarProperty property, ReadOnlySpan<char> value)
    {
        var type = _dataTypeMapper.GetPropertyMapping(property) ?? typeof(string);
        var serializer = _serializerFactory.Build(type, context) as SerializerBase;

        var propertyValue = serializer?.Deserialize(value);

        if (propertyValue is IEnumerable<string> propertyValues)
        {
            foreach (var singlePropertyValue in propertyValues)
            {
                property.AddValue(singlePropertyValue);
            }
        }
        else
        {
            property.AddValue(propertyValue);
        }
    }

    /// <remarks>
    /// State Transitions explained:<br/>
    /// Input Sequence	State Transitions	                                              Result<br/>
    /// A\r\n B         Normal(A) => SawCr(\r) => FoldCheck(\n) => Normal(B)	          AB (Folded)<br/>
    /// A\r\nB          Normal(A) => SawCr(\r) => FoldCheck(\n) => Yield(A) => Normal(B)  A, B (Two lines)<br/>
    /// A\nB            Normal(A) => FoldCheck(\n) => Yield(A) => Normal(B)               A, B (Two lines)<br/>
    /// A\rB            Normal(A) => SawCr(\r) => FoldCheck(reprocess B) => Yield(A) => Normal(B)  A, B (Two lines)<br/>
    /// </remarks>
    private enum ParserState
    {
        Normal,
        SawCr,
        FoldCheck
    }
}
