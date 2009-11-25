header
{    
    using DDay.iCal.Components; 
    using DDay.iCal.Serialization;
    using System.Text;   
}

options
{    
	language = "CSharp";
	namespace = "DDay.iCal.Serialization.iCalendar";
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
icalobject[ISerializationContext ctx] returns [DDay.iCal.iCalendar iCal = (DDay.iCal.iCalendar)Activator.CreateInstance(iCalendarType);]
:	
    ((CRLF)* BEGIN COLON VCALENDAR (CRLF)* icalbody[ctx, iCal] END COLON VCALENDAR (CRLF)*)* {iCal.OnLoaded(EventArgs.Empty);}
;

icalbody[ISerializationContext ctx, DDay.iCal.iCalendar iCal]: (calprop[iCal])* (component[ctx, iCal])?;
component[ISerializationContext ctx, iCalObject o] returns [ComponentBase c = null;]: (c=x_comp[ctx, o] | c=iana_comp[ctx, o])+;
iana_comp[ISerializationContext ctx, iCalObject o] returns [ComponentBase c = null;]: BEGIN COLON n:IANA_TOKEN {c = o.iCalendar.Create(o, n.getText().ToLower()); c.Line = n.getLine(); c.Column = n.getColumn(); } (CRLF)* (calendarline[ctx, c])* END COLON IANA_TOKEN (CRLF)* { c.OnLoaded(EventArgs.Empty); };
x_comp[ISerializationContext ctx, iCalObject o] returns [ComponentBase c = null;]: BEGIN COLON n:X_NAME {c = o.iCalendar.Create(o, n.getText().ToLower()); c.Line = n.getLine(); c.Column = n.getColumn(); } (CRLF)* (calendarline[ctx, c])* END COLON X_NAME (CRLF)* { c.OnLoaded(EventArgs.Empty); };

// iCalendar Properties
calprop[iCalObject o]: x_prop[o] | iana_prop[o];

// NOTE: RFC2445 states that for properties, the value should be a "text" value, not a "value" value.
// Outlook 2007, however, uses a "value" value in some instances, and will require this for proper
// parsing.  NOTE: Fixes bug #1874977 - X-MS-OLK-WKHRDAYS won't parse correctly
iana_prop[iCalObject o] {Property p; string v;}: n:IANA_TOKEN { p = new Property(o, n.getLine(), n.getColumn()); } (SEMICOLON (param[p] | xparam[p]))* COLON v=value {p.Name = n.getText().ToUpper(); p.Value = v; p.AddToParent(); } (CRLF)*;
x_prop[iCalObject o] {Property p; string v;}: n:X_NAME { p = new Property(o, n.getLine(), n.getColumn()); } (SEMICOLON (param[p] | xparam[p]))* COLON v=value {p.Name = n.getText().ToUpper(); p.Value = v; p.AddToParent(); } (CRLF)*;

// Content Line
calendarline[ISerializationContext ctx, iCalObject o]: contentline[ctx, o] | component[ctx, o]; // Allow for embedded components here (VALARM)
contentline[ISerializationContext ctx, iCalObject o]
{
	IToken currToken = LT(0);
    ContentLine c = new ContentLine(o, currToken.getLine() + 1, 0);
    string n;
    string v;
}: n=name {c.Name = n;} (SEMICOLON (param[c] | xparam[c]))* COLON v=value {c.Value = v; DDay.iCal.Serialization.iCalendar.Components.ContentLineSerializer.DeserializeToObject(c, o, ctx); } (CRLF)*;

name returns [string s = string.Empty;]: x:X_NAME {s = x.getText();} | i:IANA_TOKEN {s = i.getText();};
param[iCalObject o] {Parameter p; string n; string v;}: n=param_name {p = new Parameter(o, n);} EQUAL v=param_value {p.Values.Add(v);} (COMMA v=param_value {p.Values.Add(v);})*;
param_name returns [string n = string.Empty;]: i:IANA_TOKEN {n = i.getText();};
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

// Parameters
xparam[iCalObject o] { Parameter p; string v;}: n:X_NAME {p = new Parameter(o, n.getText());} EQUAL v=param_value {p.Values.Add(v);} (COMMA v=param_value {p.Values.Add(v);})*;

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
protected UNICODE: '\u0100'..'\uFFFE';
protected SPECIAL: '\u0021' | '\u0023'..'\u002b' | '\u003c' | '\u003e'..'\u0040' | '\u005b' | '\u005d'..'\u0060' | '\u007b'..'\u007e' | '\u0080'..'\u00ff';
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
IANA_TOKEN: (ALPHA | DIGIT | DASH | SPECIAL | UNICODE)+
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