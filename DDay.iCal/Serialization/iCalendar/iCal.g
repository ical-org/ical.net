// The following is a grammar file to parse
// iCalendar (.ics) files.  This grammar is
// designed for ANTLR 2.7.6.
//
// Copyright (c) 2010 Douglas Day
//
header
{
    using System.Text;
    using System.IO;
    using System.Collections.Generic;  
    using System.Runtime.Serialization;
    using DDay.iCal.Serialization;
    using DDay.iCal.Serialization.iCalendar;
}

options
{    
	language = "CSharp";
	namespace = "DDay.iCal";
}

//
// iCalParser
//
class iCalParser extends Parser;
options
{
    k=3;
    defaultErrorHandler=false;
}

// iCalendar object
icalendar[ISerializationContext ctx] returns [IICalendarCollection iCalendars = new iCalendarCollection()]
:	
{
	SerializationUtil.OnDeserializing(iCalendars);

	IICalendar iCal = null;
	ISerializationSettings settings = ctx.GetService(typeof(ISerializationSettings)) as ISerializationSettings;
}
    (
		(CRLF)* BEGIN COLON VCALENDAR (CRLF)*
		{			
			ISerializationProcessor<IICalendar> processor = ctx.GetService(typeof(ISerializationProcessor<IICalendar>)) as ISerializationProcessor<IICalendar>;
			
			// Do some pre-processing on the calendar:
			if (processor != null)
				processor.PreDeserialization(iCal);
		
			iCal = (IICalendar)SerializationUtil.GetUninitializedObject(settings.iCalendarType);			
			SerializationUtil.OnDeserializing(iCal);
			
			// Push the iCalendar onto the serialization context stack
			ctx.Push(iCal);
		}
		icalbody[ctx, iCal]
		END COLON VCALENDAR (CRLF)*
		{
			// Do some final processing on the calendar:
			if (processor != null)
				processor.PostDeserialization(iCal);
		
			// Notify that the iCalendar has been loaded
			iCal.OnLoaded();
			iCalendars.Add(iCal);
			
			SerializationUtil.OnDeserialized(iCal);
			
			// Pop the iCalendar off the serialization context stack
			ctx.Pop();
		}
	)* 	
	{
		SerializationUtil.OnDeserialized(iCalendars);
	}
;

// iCalendar body
// NOTE: We allow intermixture of calendar components and calendar properties
icalbody[ISerializationContext ctx, IICalendar iCal]:
{
	ISerializerFactory sf = ctx.GetService(typeof(ISerializerFactory)) as ISerializerFactory;
	ICalendarComponentFactory cf = ctx.GetService(typeof(ICalendarComponentFactory)) as ICalendarComponentFactory;
	ICalendarComponent c;
	ICalendarProperty p;
}
(
	property[ctx, iCal] |
	component[ctx, sf, cf, iCal]
)*;

// iCalendar components
component[
	ISerializationContext ctx,
	ISerializerFactory sf,
	ICalendarComponentFactory cf,
	ICalendarObject o
] returns [ICalendarComponent c = null;]:
BEGIN COLON
(
	n:IANA_TOKEN { c = cf.Build(n.getText().ToLower(), true); } | 
	m:X_NAME { c = cf.Build(m.getText().ToLower(), true); }
)
{
	ISerializationProcessor<ICalendarComponent> processor = ctx.GetService(typeof(ISerializationProcessor<ICalendarComponent>)) as ISerializationProcessor<ICalendarComponent>;
	// Do some pre-processing on the component
	if (processor != null)
		processor.PreDeserialization(c);

	SerializationUtil.OnDeserializing(c);

	// Push the component onto the serialization context stack
	ctx.Push(c);

	if (o != null)
	{
		// Add the component as a child immediately, in case
		// embedded components need to access this component,
		// or the iCalendar itself.
		o.AddChild(c); 
	}
	
	c.Line = n.getLine();
	c.Column = n.getColumn();
}
(CRLF)*
(
	// Components can have properties, and other embedded components.
	// (i.e. VALARM)	
	property[ctx, c] |
	component[ctx, sf, cf, c]
)*
END COLON IANA_TOKEN (CRLF)*
{	
	// Do some final processing on the component
	if (processor != null)
		processor.PostDeserialization(c);

	// Notify that the component has been loaded
	c.OnLoaded();
	
	SerializationUtil.OnDeserialized(c);
	
	// Pop the component off the serialization context stack
	ctx.Pop();
};

//
// Properties
//
// NOTE: RFC5545 states that for properties, the value should be a "text" value, not a "value" value.
// Outlook 2007, however, uses a "value" value in some instances, and will require this for proper
// parsing.  NOTE: Fixes bug #1874977 - X-MS-OLK-WKHRDAYS won't parse correctly
//
property[
	ISerializationContext ctx,	
	ICalendarPropertyListContainer c
] returns [ICalendarProperty p = null;]
{
	string v;
}:
(
	n:IANA_TOKEN
	{
		p = new CalendarProperty(n.getLine(), n.getColumn());
		p.Name = n.getText().ToUpper();
	} |
	m:X_NAME
	{
		p = new CalendarProperty(m.getLine(), m.getColumn());
		p.Name = m.getText().ToUpper();
	}
)
{
	ISerializationProcessor<ICalendarProperty> processor = ctx.GetService(typeof(ISerializationProcessor<ICalendarProperty>)) as ISerializationProcessor<ICalendarProperty>;
	// Do some pre-processing on the property
	if (processor != null)
		processor.PreDeserialization(p);

	if (c != null)
	{
		// Add the property to the container, as the parent object(s)
		// may be needed during deserialization.
		c.Properties.Add(p);
	}

	// Push the property onto the serialization context stack
	ctx.Push(p);
	IStringSerializer dataMapSerializer = new DataMapSerializer(ctx);
}
(SEMICOLON parameter[ctx, p])* COLON
v=value
{
	// Deserialize the value of the property
	// into a concrete iCalendar data type,
	// or string value.
	p.Value = dataMapSerializer.Deserialize(new StringReader(v));
} (CRLF)*
{
	// Do some final processing on the property:
	if (processor != null)
		processor.PostDeserialization(p);

	// Notify that the property has been loaded
	p.OnLoaded();
	
	// Pop the property off the serialization context stack
	ctx.Pop();
};

//
// Parameters
//
parameter[
	ISerializationContext ctx,
	ICalendarParameterListContainer container
] returns [ICalendarParameter p = null;]
{
	string v;
	List<string> values = new List<string>();
}:
(
	n:IANA_TOKEN { p = new CalendarParameter(n.getText()); } |
	m:X_NAME { p = new CalendarParameter(m.getText()); }
)
{
	// Push the parameter onto the serialization context stack
	ctx.Push(p);
}
EQUAL v=param_value { values.Add(v); }
(
	COMMA v=param_value { values.Add(v); }
)*
{
	p.Values = values.ToArray();
	
	if (container != null)
	{
		container.Parameters.Add(p);
	}
	
	// Notify that the parameter has been loaded
	p.OnLoaded();
	
	// Pop the parameter off the serialization context stack
	ctx.Pop();
};

// Value productions
param_value returns [string v = string.Empty;]: v=paramtext | v=quoted_string;
paramtext returns [string s = null;] {StringBuilder sb = new StringBuilder(); string c;}: (c=safe_char {sb.Append(c);})* { s = sb.ToString(); };
value returns [string v = string.Empty] {StringBuilder sb = new StringBuilder(); string c;}: (c=value_char {sb.Append(c);})* { v = sb.ToString(); };
quoted_string returns [string s = string.Empty] {StringBuilder sb = new StringBuilder(); string c;}: DQUOTE (c=qsafe_char {sb.Append(c);})* DQUOTE {s = sb.ToString(); };

// Character Productions
qsafe_char returns [string c = string.Empty]: a:~(CTL | DQUOTE | CRLF) {c = a.getText();};
safe_char returns [string c = string.Empty]: a:~(CTL | DQUOTE | SEMICOLON | COLON | COMMA | CRLF) {c = a.getText();};
value_char returns [string c = string.Empty]: a:~(CTL | CRLF) {c = a.getText();};
tsafe_char returns [string s = string.Empty]: a:~(CTL | DQUOTE | SEMICOLON | COLON | BACKSLASH | COMMA | CRLF) {s = a.getText();};
text_char returns [string s = string.Empty]: a:~(CTL | BACKSLASH | CRLF) {s = a.getText();}; // NOTE: Fixes bug #1975624 - Spaces in ProdID break
text returns [string s = string.Empty] {string t;}: (t=text_char {s += t;})*;

// Number handling
number returns [string s = string.Empty]: n1:NUMBER { s += n1.getText(); } (DOT { s += "."; } n2:NUMBER {s += n2.getText();})?;
version_number returns [string s = string.Empty] {string t;}: t=number {s += t;} (SEMICOLON {s += ";";} t=number {s += t;})?;

//
// iCalLexer
//
class iCalLexer extends Lexer;
options
{
    k=3; // k=2 for CRLF and ESCAPED_CHAR, k=3 to handle LINEFOLDER
    charVocabulary = '\u0000'..'\ufffe';
}

protected CR: '\u000d';
LF: '\u000a' {$setType(Token.SKIP);};
protected ALPHA: '\u0041'..'\u005a' | '\u0061'..'\u007a';
protected DIGIT: '\u0030'..'\u0039';
protected DASH: '\u002d';
protected UNDERSCORE: '\u005F';
protected UNICODE: '\u0100'..'\uFFFE';
protected SPECIAL: '\u0021' | '\u0023'..'\u002b' | '\u003c' | '\u003e'..'\u0040' | '\u005b' | '\u005d'..'\u005e' | '\u0060' | '\u007b'..'\u007e' | '\u0080'..'\u00ff';
SPACE: '\u0020';
HTAB: '\u0009';
COLON: '\u003a';
SEMICOLON: '\u003b';
COMMA: '\u002c';
DOT: '\u002e';
EQUAL: '\u003d';
BACKSLASH: '\u005c';
SLASH: '\u002f';
DQUOTE: '\u0022';
CRLF: CR LF { newline(); };
CTL: '\u0000'..'\u0008' | '\u000b'..'\u001F' | '\u007F';
ESCAPED_CHAR: BACKSLASH (BACKSLASH | DQUOTE | SEMICOLON | COMMA | "N" | "n");
IANA_TOKEN: (ALPHA | DIGIT | DASH | UNDERSCORE | SPECIAL | UNICODE)+
{ 
    string s = $getText;
    int val;
    if (int.TryParse(s, out val))
        $setType(NUMBER);
    else
    {
        switch(s.ToUpper())
        {
            case "BEGIN": $setType(BEGIN); break;
            case "END": $setType(END); break;
            case "VCALENDAR": $setType(VCALENDAR); break;            
            default: 
                if (s.Length > 2 && s.Substring(0,2).Equals("X-"))
                    $setType(X_NAME);
                break;                
        }
    }
};

LINEFOLDER: CRLF (SPACE | HTAB) {$setType(Token.SKIP);};