using System;
using System.Collections.Generic;
using System.IO;
using System.Runtime.Serialization;
using System.Text;
using System.Text.RegularExpressions;
using Ical.Net.CalendarComponents;
using Ical.Net.Utility;

namespace Ical.Net.Serialization
{
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

        public static readonly SimpleDeserializer Default = new SimpleDeserializer(
            new DataTypeMapper(),
            new SerializerFactory(),
            new CalendarComponentFactory());

        private readonly DataTypeMapper _dataTypeMapper;
        private readonly ISerializerFactory _serializerFactory;
        private readonly CalendarComponentFactory _componentFactory;
        
        public IEnumerable<ICalendarComponent> Deserialize(TextReader reader)
        {
            var context = new SerializationContext();
            var stack = new Stack<ICalendarComponent>();
            var current = default(ICalendarComponent);
            foreach (var contentLineString in GetContentLines(reader))
            {
                var contentLine = ParseContentLine(context, contentLineString);
                if (string.Equals(contentLine.Name, "BEGIN", StringComparison.OrdinalIgnoreCase))
                {
                    stack.Push(current);
                    current = _componentFactory.Build((string)contentLine.Value);
                    SerializationUtil.OnDeserializing(current);
                }
                else
                {
                    if (current == null)
                    {
                        throw new SerializationException($"Expected 'BEGIN', found '{contentLine.Name}'");
                    }
                    if (string.Equals(contentLine.Name, "END", StringComparison.OrdinalIgnoreCase))
                    {
                        if (!string.Equals((string)contentLine.Value, current.Name, StringComparison.OrdinalIgnoreCase))
                        {
                            throw new SerializationException($"Expected 'END:{current.Name}', found 'END:{contentLine.Value}'");
                        }
                        SerializationUtil.OnDeserialized(current);
                        var finished = current;
                        current = stack.Pop();
                        if (current == null)
                        {
                            yield return finished;
                        }
                        else
                        {
                            current.Children.Add(finished);
                        }
                    }
                    else
                    {
                        current.Properties.Add(contentLine);
                    }
                }
            }
            if (current != null)
            {
                throw new SerializationException($"Unclosed component {current.Name}");
            }
        }

        private CalendarProperty ParseContentLine(SerializationContext context, string input)
        {
            var match = CompiledRegularExpressions.ContentLine.Match(input);
            if (!match.Success)
            {
                throw new SerializationException($"Could not parse line: '{input}'");
            }
            var name = match.Groups[CompiledRegularExpressions.ContentLineNameGroup].Value;
            var value = match.Groups[CompiledRegularExpressions.ContentLineValueGroup].Value;
            var paramNames = match.Groups[CompiledRegularExpressions.ContentLineParamNameGroup].Captures;
            var paramValues = match.Groups[CompiledRegularExpressions.ContentLineParamValueGroup].Captures;

            var property = new CalendarProperty(name.ToUpperInvariant());
            context.Push(property);
            SetPropertyParameters(property, paramNames, paramValues);
            SetPropertyValue(context, property, value);
            context.Pop();
            return property;
        }

        private static void SetPropertyParameters(CalendarProperty property, CaptureCollection paramNames, CaptureCollection paramValues)
        {
            var paramValueIndex = 0;
            for (var paramNameIndex = 0; paramNameIndex < paramNames.Count; paramNameIndex++)
            {
                var paramName = paramNames[paramNameIndex].Value;
                var parameter = new CalendarParameter(paramName);
                var nextParamIndex = paramNameIndex + 1 < paramNames.Count ? paramNames[paramNameIndex + 1].Index : int.MaxValue;
                while (paramValueIndex < paramValues.Count && paramValues[paramValueIndex].Index < nextParamIndex)
                {
                    var paramValue = paramValues[paramValueIndex].Value;
                    parameter.AddValue(paramValue);
                    paramValueIndex++;
                }
                property.AddParameter(parameter);
            }
        }

        private void SetPropertyValue(SerializationContext context, CalendarProperty property, string value)
        {
            var type = _dataTypeMapper.GetPropertyMapping(property) ?? typeof(string);
            var serializer = (SerializerBase)_serializerFactory.Build(type, context);
            using (var valueReader = new StringReader(value))
            {
                var propertyValue = serializer.Deserialize(valueReader);
                var propertyValues = propertyValue as IEnumerable<string>;
                if (propertyValues != null)
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
        }

        private static IEnumerable<string> GetContentLines(TextReader reader)
        {
            var currentLine = new StringBuilder();
            while (true)
            {
                var nextLine = reader.ReadLine();
                if (nextLine == null)
                {
                    break;
                }

                if (nextLine.Length <= 0)
                {
                    continue;
                }

                if ((nextLine[0] == ' ' || nextLine[0] == '\t'))
                {
                    currentLine.Append(nextLine, 1, nextLine.Length - 1);
                }
                else
                {
                    if (currentLine.Length > 0)
                    {
                        yield return currentLine.ToString();
                    }
                    currentLine.Clear();
                    currentLine.Append(nextLine);
                }
            }
            if (currentLine.Length > 0)
            {
                yield return currentLine.ToString();
            }
        }
    }
}
