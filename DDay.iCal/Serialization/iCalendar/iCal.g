// The following is a grammar file to parse
// iCalendar (.ics) files.  This grammar is
// designed for ANTLR 2.7.6.
//
// Copyright (c) 2010 Douglas Day
//
header
{
    using System.Text;
    using System.Collections.Generic;  
}

options
{    
	language = "CSharp";
	namespace = "DDay.iCal.Serialization";
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
icalendar[ISerializationContext ctx] returns [iCalendarCollection iCalendars = new iCalendarCollection();]
:	
{
	IICalendar iCal = null;
	ISerializationSettings settings = ctx.GetService(typeof(ISerializationSettings)) as ISerializationSettings;
}
    (
		(CRLF)* BEGIN COLON VCALENDAR (CRLF)* {iCal = (IICalendar)Activator.CreateInstance(settings.iCalendarType);}
		icalbody[ctx, iCal]
		END COLON VCALENDAR (CRLF)*
		{
			iCal.OnLoaded();
			iCalendars.Add(iCal);
		}
	)* 	
;

// iCalendar body
// NOTE: We allow intermixture of calendar components and calendar properties
icalbody[ISerializationContext ctx, IICalendar iCal]:
{
	ICalendarComponentFactory cf = ctx.GetService(typeof(ICalendarComponentFactory)) as ICalendarComponentFactory;
	IStringParserFactory spf = ctx.GetService(typeof(IStringParserFactory)) as IStringParserFactory;
	ICalendarComponent c;
	ICalendarProperty p;
}
(
	property[ctx, spf, iCal] |
	component[ctx, spf, cf, iCal]
)*;

// iCalendar components
component[
	ISerializationContext ctx,
	IStringParserFactory spf,
	ICalendarComponentFactory cf,
	ICalendarObject o
] returns [ICalendarComponent c = null;]:
BEGIN COLON
(
	n:IANA_TOKEN { c = cf.Create(n.getText().ToLower()); } | 
	m:X_NAME { c = cf.Create(m.getText().ToLower()); }
)
{
	// Add the component as a child immediately, in case
	// embedded components need to access this component,
	// or the iCalendar itself.
	o.AddChild(c); 
	
	c.Line = n.getLine();
	c.Column = n.getColumn();
}
(CRLF)*
(
	// Components can have properties, and other embedded components.
	// (i.e. VALARM)	
	property[ctx, spf, c] |
	component[ctx, spf, cf, c]
)*
END COLON IANA_TOKEN (CRLF)* { c.OnLoaded(); };

//
// Properties
//
// NOTE: RFC5545 states that for properties, the value should be a "text" value, not a "value" value.
// Outlook 2007, however, uses a "value" value in some instances, and will require this for proper
// parsing.  NOTE: Fixes bug #1874977 - X-MS-OLK-WKHRDAYS won't parse correctly
//
property[
	ISerializationContext ctx,
	IStringParserFactory spf,
	ICalendarPropertyListContainer c
]
{
	ICalendarProperty p;
	string v;
	IStringParser sp = spf.Create(typeof(ICalendarProperty));
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
(SEMICOLON parameter[ctx, p])* COLON
v=value
{
	// Parse the value of the property
	sp.Parse(v, p);
	
	// Add the property to the container	
	c.Properties.Add(p);
} (CRLF)*;

//
// Parameters
//
parameter[ISerializationContext ctx, ICalendarProperty prop]
{
	ICalendarParameter p;	
	string v;
	List<string> values = new List<string>();
}:
(
	n:IANA_TOKEN { p = new CalendarParameter(n.getText()); } |
	m:X_NAME { p = new CalendarParameter(m.getText()); }
)
EQUAL v=param_value { values.Add(v); }
(
	COMMA v=param_value { values.Add(v); }
)*
{
	p.Values = values.ToArray();
	prop.Parameters.Add(p);
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