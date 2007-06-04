header
{
    using DDay.iCal.Components;    
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
icalobject returns [iCalendar iCal = (iCalendar)Activator.CreateInstance(iCalendarType);]
:
    (BEGIN COLON VCALENDAR CRLF icalbody[iCal] END COLON VCALENDAR CRLF)* {iCal.OnLoad(EventArgs.Empty);}
;
    
icalbody[iCalendar iCal]: (calprops[iCal])? (component[iCal])?;
component[iCalObject o] returns [ComponentBase c = null;]: (c=iana_comp[o] | c=x_comp[o])+;
iana_comp[iCalObject o] returns [ComponentBase c = null;]: BEGIN COLON n:IANA_TOKEN {c = o.iCalendar.Create(o, n.getText().ToLower());} CRLF (calendarline[c])+ END COLON IANA_TOKEN CRLF { c.OnLoad(EventArgs.Empty); };
x_comp[iCalObject o] returns [ComponentBase c = null;]: BEGIN COLON n:X_NAME {c = o.iCalendar.Create(o, n.getText().ToLower());} CRLF (calendarline[c])+ END COLON X_NAME CRLF { c.OnLoad(EventArgs.Empty); };

// iCalendar Properties
calprops[iCalendar iCal]: calprop[iCal] (calprop[iCal])+;
calprop[iCalObject o]: prodid[o] | version[o] | calscale[o] | method[o] | iana_prop[o] | x_prop[o];
prodid[iCalObject o] {Property p; string v;}: n:PRODID { p = new Property(o); } (SEMICOLON xparam[p])* COLON v=text {p.Name = n.getText(); p.Value = v; p.AddToParent(); } CRLF;
version[iCalObject o] {Property p; string v;}: n:VERSION { p = new Property(o); } (SEMICOLON xparam[p])* COLON v=version_number {p.Name = n.getText(); p.Value = v; p.AddToParent(); } CRLF;
calscale[iCalObject o] {Property p;}: n:CALSCALE { p = new Property(o); } (SEMICOLON xparam[p])* COLON v:IANA_TOKEN {p.Name = n.getText(); p.Value = v.getText(); p.AddToParent(); } CRLF;
method[iCalObject o] {Property p;}: n:METHOD { p = new Property(o); } (SEMICOLON xparam[p])* COLON v:IANA_TOKEN {p.Name = n.getText(); p.Value = v.getText(); p.AddToParent(); } CRLF;
iana_prop[iCalObject o] {Property p; string v;}: n:IANA_TOKEN { p = new Property(o); } (SEMICOLON xparam[p])* COLON v=text {p.Name = n.getText(); p.Value = v; p.AddToParent(); } CRLF;
x_prop[iCalObject o] {Property p; string v;}: n:X_NAME { p = new Property(o); } (SEMICOLON xparam[p])* COLON v=text {p.Name = n.getText(); p.Value = v; p.AddToParent(); } CRLF;

// Content Line
calendarline[iCalObject o]: contentline[o] | component[o]; // Allow for embedded components here (VALARM)
contentline[iCalObject o]
{
    ContentLine c = new ContentLine(o);
    string n;
    string v;
}: n=name {c.Name = n;} (SEMICOLON param[c])* COLON v=value {c.Value = v; DDay.iCal.Serialization.iCalendar.Components.ContentLineSerializer.DeserializeToObject(c, o); } CRLF;

name returns [string s = string.Empty;]: x:X_NAME {s = x.getText();} | i:IANA_TOKEN {s = i.getText();};
param[iCalObject o] {Parameter p; string n; string v;}: n=param_name {p = new Parameter(o, n);} EQUAL v=param_value {p.Values.Add(v);} (COMMA v=param_value {p.Values.Add(v);})*;
param_name returns [string n = string.Empty;]: i:IANA_TOKEN {n = i.getText();} | x:X_NAME {n = x.getText();}; // FIXME: in rfc 32445, X_NAME was x-token, but x-token was not defined.
param_value returns [string v = string.Empty;]: v=paramtext | v=quoted_string;
paramtext returns [string t = string.Empty] {string c;}: (c=safe_char {t += c;})*;
value returns [string v = string.Empty] {string c;}: (c=value_char {v += c;})*;
quoted_string returns [string s = string.Empty] {string c;}: DQUOTE (c=qsafe_char {s += c;})* DQUOTE;

// Character Productions
qsafe_char returns [string c = string.Empty]: a:~(CTL | DQUOTE | CRLF) {c = a.getText();};
safe_char returns [string c = string.Empty]: a:~(CTL | DQUOTE | SEMICOLON | COLON | COMMA | CRLF) {c = a.getText();};
value_char returns [string c = string.Empty]: a:~(CTL | CRLF) {c = a.getText();};
tsafe_char returns [string s = string.Empty]: a:~(CTL | DQUOTE | SEMICOLON | COLON | BACKSLASH | COMMA | CRLF) {s = a.getText();};
text_char returns [string s = string.Empty]: s=tsafe_char | c:COLON { s = c.getText(); } | q:DQUOTE { s = q.getText(); } | e:ESCAPED_CHAR { s = e.getText(); };
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
    k=3; // k=2 for CRLF, k=3 to handle LINEFOLDER        
    charVocabulary = '\u0000'..'\ufffe';    
}

protected CR: '\u000d';
LF: '\u000a' {$setType(Token.SKIP);};
protected ALPHA: '\u0041'..'\u005a' | '\u0061'..'\u007a';
protected DIGIT: '\u0030'..'\u0039';
protected DASH: '\u002d';
SPECIAL: '\u0021' | '\u0023'..'\u002b' | '\u003c' | '\u003e'..'\u0040' | '\u005b' | '\u005d'..'\u0060' | '\u007b'..'\u00ff';
UNICODE: '\u0100'..'\uFFFE';
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
CRLF: CR LF {newline();};
ESCAPED_CHAR: BACKSLASH (BACKSLASH | DQUOTE | SEMICOLON | COMMA | "N" | "n");
IANA_TOKEN: (ALPHA | DIGIT | DASH)+
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
            case "PRODID": $setType(PRODID); break;
            case "VERSION": $setType(VERSION); break;
            case "CALSCALE": $setType(CALSCALE); break;
            case "METHOD": $setType(METHOD); break;
            default: 
                if (s.Length > 2 && s.Substring(0,2).Equals("X-"))
                    $setType(X_NAME);
                break;                
        }        
    }
};
LINEFOLDER: CRLF (SPACE | HTAB) {$setType(Token.SKIP);};
