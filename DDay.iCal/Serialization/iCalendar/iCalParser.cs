// $ANTLR 2.7.6 (20061021): "iCal.g" -> "iCalParser.cs"$
        
    using System.Text;   

namespace DDay.iCal.Serialization
{
	// Generate the header common to all output files.
	using System;
	
	using TokenBuffer              = antlr.TokenBuffer;
	using TokenStreamException     = antlr.TokenStreamException;
	using TokenStreamIOException   = antlr.TokenStreamIOException;
	using ANTLRException           = antlr.ANTLRException;
	using LLkParser = antlr.LLkParser;
	using Token                    = antlr.Token;
	using IToken                   = antlr.IToken;
	using TokenStream              = antlr.TokenStream;
	using RecognitionException     = antlr.RecognitionException;
	using NoViableAltException     = antlr.NoViableAltException;
	using MismatchedTokenException = antlr.MismatchedTokenException;
	using SemanticException        = antlr.SemanticException;
	using ParserSharedInputState   = antlr.ParserSharedInputState;
	using BitSet                   = antlr.collections.impl.BitSet;
	
	public partial class iCalParser : antlr.LLkParser
	{
		public const int EOF = 1;
		public const int NULL_TREE_LOOKAHEAD = 3;
		public const int CRLF = 4;
		public const int BEGIN = 5;
		public const int COLON = 6;
		public const int VCALENDAR = 7;
		public const int END = 8;
		public const int IANA_TOKEN = 9;
		public const int X_NAME = 10;
		public const int SEMICOLON = 11;
		public const int EQUAL = 12;
		public const int COMMA = 13;
		public const int DQUOTE = 14;
		public const int CTL = 15;
		public const int BACKSLASH = 16;
		public const int NUMBER = 17;
		public const int DOT = 18;
		public const int CR = 19;
		public const int LF = 20;
		public const int ALPHA = 21;
		public const int DIGIT = 22;
		public const int DASH = 23;
		public const int UNDERSCORE = 24;
		public const int UNICODE = 25;
		public const int SPECIAL = 26;
		public const int SPACE = 27;
		public const int HTAB = 28;
		public const int SLASH = 29;
		public const int ESCAPED_CHAR = 30;
		public const int LINEFOLDER = 31;
		
		
		protected void initialize()
		{
			tokenNames = tokenNames_;
		}
		
		
		protected iCalParser(TokenBuffer tokenBuf, int k) : base(tokenBuf, k)
		{
			initialize();
		}
		
		public iCalParser(TokenBuffer tokenBuf) : this(tokenBuf,3)
		{
		}
		
		protected iCalParser(TokenStream lexer, int k) : base(lexer,k)
		{
			initialize();
		}
		
		public iCalParser(TokenStream lexer) : this(lexer,3)
		{
		}
		
		public iCalParser(ParserSharedInputState state) : base(state,3)
		{
			initialize();
		}
		
	public iCalendarCollection  icalobject(
		ISerializationContext ctx
	) //throws RecognitionException, TokenStreamException
{
		iCalendarCollection iCalendars = new iCalendarCollection();;
		
		
		IICalendar iCal = null;
		{    // ( ... )*
			for (;;)
			{
				if ((LA(1)==CRLF||LA(1)==BEGIN))
				{
					{    // ( ... )*
						for (;;)
						{
							if ((LA(1)==CRLF))
							{
								match(CRLF);
							}
							else
							{
								goto _loop4_breakloop;
							}
							
						}
_loop4_breakloop:						;
					}    // ( ... )*
					match(BEGIN);
					match(COLON);
					match(VCALENDAR);
					{    // ( ... )*
						for (;;)
						{
							if ((LA(1)==CRLF))
							{
								match(CRLF);
							}
							else
							{
								goto _loop6_breakloop;
							}
							
						}
_loop6_breakloop:						;
					}    // ( ... )*
					iCal = (IICalendar)Activator.CreateInstance(iCalendarType);
					icalbody(ctx, iCal);
					match(END);
					match(COLON);
					match(VCALENDAR);
					{    // ( ... )*
						for (;;)
						{
							if ((LA(1)==CRLF) && (LA(2)==EOF||LA(2)==CRLF||LA(2)==BEGIN) && (tokenSet_0_.member(LA(3))))
							{
								match(CRLF);
							}
							else
							{
								goto _loop8_breakloop;
							}
							
						}
_loop8_breakloop:						;
					}    // ( ... )*
					
								if (iCal != null)
								{
									iCal.OnLoaded();
									iCalendars.Add(iCal);
								}
							
				}
				else
				{
					goto _loop9_breakloop;
				}
				
			}
_loop9_breakloop:			;
		}    // ( ... )*
		return iCalendars;
	}
	
	public void icalbody(
		ISerializationContext ctx, IICalendar iCal
	) //throws RecognitionException, TokenStreamException
{
		
		
		{    // ( ... )*
			for (;;)
			{
				if ((LA(1)==IANA_TOKEN||LA(1)==X_NAME))
				{
					calprop(iCal);
				}
				else
				{
					goto _loop12_breakloop;
				}
				
			}
_loop12_breakloop:			;
		}    // ( ... )*
		{
			switch ( LA(1) )
			{
			case BEGIN:
			{
				component(ctx, iCal);
				break;
			}
			case END:
			{
				break;
			}
			default:
			{
				throw new NoViableAltException(LT(1), getFilename());
			}
			 }
		}
	}
	
	public void calprop(
		IICalendar iCal
	) //throws RecognitionException, TokenStreamException
{
		
		
		switch ( LA(1) )
		{
		case X_NAME:
		{
			x_prop(iCal);
			break;
		}
		case IANA_TOKEN:
		{
			iana_prop(iCal);
			break;
		}
		default:
		{
			throw new NoViableAltException(LT(1), getFilename());
		}
		 }
	}
	
	public ICalendarComponent  component(
		ISerializationContext ctx, ICalendarObject o
	) //throws RecognitionException, TokenStreamException
{
		ICalendarComponent c = null;;
		
		
		{ // ( ... )+
			int _cnt16=0;
			for (;;)
			{
				if ((LA(1)==BEGIN) && (LA(2)==COLON) && (LA(3)==X_NAME))
				{
					c=x_comp(ctx, o);
				}
				else if ((LA(1)==BEGIN) && (LA(2)==COLON) && (LA(3)==IANA_TOKEN)) {
					c=iana_comp(ctx, o);
				}
				else
				{
					if (_cnt16 >= 1) { goto _loop16_breakloop; } else { throw new NoViableAltException(LT(1), getFilename());; }
				}
				
				_cnt16++;
			}
_loop16_breakloop:			;
		}    // ( ... )+
		return c;
	}
	
	public ICalendarComponent  x_comp(
		ISerializationContext ctx, ICalendarObject o
	) //throws RecognitionException, TokenStreamException
{
		ICalendarComponent c = null;;
		
		IToken  n = null;
		
		match(BEGIN);
		match(COLON);
		n = LT(1);
		match(X_NAME);
		c = o.Calendar.ComponentFactory.Create(n.getText().ToLower()); c.Line = n.getLine(); c.Column = n.getColumn();
		{    // ( ... )*
			for (;;)
			{
				if ((LA(1)==CRLF))
				{
					match(CRLF);
				}
				else
				{
					goto _loop26_breakloop;
				}
				
			}
_loop26_breakloop:			;
		}    // ( ... )*
		{    // ( ... )*
			for (;;)
			{
				if ((LA(1)==BEGIN||LA(1)==IANA_TOKEN||LA(1)==X_NAME))
				{
					calendarline(ctx, c);
				}
				else
				{
					goto _loop28_breakloop;
				}
				
			}
_loop28_breakloop:			;
		}    // ( ... )*
		match(END);
		match(COLON);
		match(X_NAME);
		{    // ( ... )*
			for (;;)
			{
				if ((LA(1)==CRLF))
				{
					match(CRLF);
				}
				else
				{
					goto _loop30_breakloop;
				}
				
			}
_loop30_breakloop:			;
		}    // ( ... )*
		o.AddChild(c); c.OnLoaded();
		return c;
	}
	
	public ICalendarComponent  iana_comp(
		ISerializationContext ctx, ICalendarObject o
	) //throws RecognitionException, TokenStreamException
{
		ICalendarComponent c = null;;
		
		IToken  n = null;
		
		match(BEGIN);
		match(COLON);
		n = LT(1);
		match(IANA_TOKEN);
		c = o.Calendar.ComponentFactory.Create(n.getText().ToLower()); c.Line = n.getLine(); c.Column = n.getColumn();
		{    // ( ... )*
			for (;;)
			{
				if ((LA(1)==CRLF))
				{
					match(CRLF);
				}
				else
				{
					goto _loop19_breakloop;
				}
				
			}
_loop19_breakloop:			;
		}    // ( ... )*
		{    // ( ... )*
			for (;;)
			{
				if ((LA(1)==BEGIN||LA(1)==IANA_TOKEN||LA(1)==X_NAME))
				{
					calendarline(ctx, c);
				}
				else
				{
					goto _loop21_breakloop;
				}
				
			}
_loop21_breakloop:			;
		}    // ( ... )*
		match(END);
		match(COLON);
		match(IANA_TOKEN);
		{    // ( ... )*
			for (;;)
			{
				if ((LA(1)==CRLF))
				{
					match(CRLF);
				}
				else
				{
					goto _loop23_breakloop;
				}
				
			}
_loop23_breakloop:			;
		}    // ( ... )*
		o.AddChild(c); c.OnLoaded();
		return c;
	}
	
	public void calendarline(
		ISerializationContext ctx, ICalendarObject o
	) //throws RecognitionException, TokenStreamException
{
		
		
		switch ( LA(1) )
		{
		case IANA_TOKEN:
		case X_NAME:
		{
			contentline(ctx, o);
			break;
		}
		case BEGIN:
		{
			component(ctx, o);
			break;
		}
		default:
		{
			throw new NoViableAltException(LT(1), getFilename());
		}
		 }
	}
	
	public void x_prop(
		IICalendar iCal
	) //throws RecognitionException, TokenStreamException
{
		
		IToken  n = null;
		CalendarProperty p; string v;
		
		n = LT(1);
		match(X_NAME);
        p = new CalendarProperty(n.getLine(), n.getColumn());
		{    // ( ... )*
			for (;;)
			{
				if ((LA(1)==SEMICOLON))
				{
					match(SEMICOLON);
					{
						switch ( LA(1) )
						{
						case IANA_TOKEN:
						{
							param(p);
							break;
						}
						case X_NAME:
						{
							xparam(p);
							break;
						}
						default:
						{
							throw new NoViableAltException(LT(1), getFilename());
						}
						 }
					}
				}
				else
				{
					goto _loop41_breakloop;
				}
				
			}
_loop41_breakloop:			;
		}    // ( ... )*
		match(COLON);
		v=value();
		p.Name = n.getText().ToUpper(); p.Value = v; iCal.Properties.Add(p);
		{    // ( ... )*
			for (;;)
			{
				if ((LA(1)==CRLF))
				{
					match(CRLF);
				}
				else
				{
					goto _loop43_breakloop;
				}
				
			}
_loop43_breakloop:			;
		}    // ( ... )*
	}
	
	public void iana_prop(
		IICalendar iCal
	) //throws RecognitionException, TokenStreamException
{
		
		IToken  n = null;
        CalendarProperty p; string v;
		
		n = LT(1);
		match(IANA_TOKEN);
        p = new CalendarProperty(n.getLine(), n.getColumn());
		{    // ( ... )*
			for (;;)
			{
				if ((LA(1)==SEMICOLON))
				{
					match(SEMICOLON);
					{
						switch ( LA(1) )
						{
						case IANA_TOKEN:
						{
							param(p);
							break;
						}
						case X_NAME:
						{
							xparam(p);
							break;
						}
						default:
						{
							throw new NoViableAltException(LT(1), getFilename());
						}
						 }
					}
				}
				else
				{
					goto _loop35_breakloop;
				}
				
			}
_loop35_breakloop:			;
		}    // ( ... )*
		match(COLON);
		v=value();
		p.Name = n.getText().ToUpper(); p.Value = v; iCal.Properties.Add(p);
		{    // ( ... )*
			for (;;)
			{
				if ((LA(1)==CRLF))
				{
					match(CRLF);
				}
				else
				{
					goto _loop37_breakloop;
				}
				
			}
_loop37_breakloop:			;
		}    // ( ... )*
	}
	
	public void param(
		ICalendarObject o
	) //throws RecognitionException, TokenStreamException
{
		
		Parameter p; string n; string v;
		
		n=param_name();
		p = new Parameter(n);
		match(EQUAL);
		v=param_value();
		p.Values.Add(v);
		{    // ( ... )*
			for (;;)
			{
				if ((LA(1)==COMMA))
				{
					match(COMMA);
					v=param_value();
					p.Values.Add(v);
				}
				else
				{
					goto _loop54_breakloop;
				}
				
			}
_loop54_breakloop:			;
		}    // ( ... )*
		o.Parameters.Add(p);
	}
	
	public void xparam(
		ICalendarObject o
	) //throws RecognitionException, TokenStreamException
{
		
		IToken  n = null;
		Parameter p; string v;
		
		n = LT(1);
		match(X_NAME);
		p = new Parameter(n.getText());
		match(EQUAL);
		v=param_value();
		p.Values.Add(v);
		{    // ( ... )*
			for (;;)
			{
				if ((LA(1)==COMMA))
				{
					match(COMMA);
					v=param_value();
					p.Values.Add(v);
				}
				else
				{
					goto _loop85_breakloop;
				}
				
			}
_loop85_breakloop:			;
		}    // ( ... )*
		o.Parameters.Add(p);
	}
	
	public string  value() //throws RecognitionException, TokenStreamException
{
		string v = string.Empty;
		
		StringBuilder sb = new StringBuilder(); string c;
		
		{    // ( ... )*
			for (;;)
			{
				if ((tokenSet_1_.member(LA(1))) && (tokenSet_2_.member(LA(2))) && (tokenSet_2_.member(LA(3))))
				{
					c=value_char();
					sb.Append(c);
				}
				else
				{
					goto _loop62_breakloop;
				}
				
			}
_loop62_breakloop:			;
		}    // ( ... )*
		v = sb.ToString();
		return v;
	}
	
	public void contentline(
		ISerializationContext ctx, ICalendarObject o
	) //throws RecognitionException, TokenStreamException
{
		
		
			IToken currToken = LT(0);
		ContentLine c = new ContentLine(currToken.getLine() + 1, 0);
		string n;
		string v;
		
		
		n=name();
		c.Name = n;
		{    // ( ... )*
			for (;;)
			{
				if ((LA(1)==SEMICOLON))
				{
					match(SEMICOLON);
					{
						switch ( LA(1) )
						{
						case IANA_TOKEN:
						{
							param(c);
							break;
						}
						case X_NAME:
						{
							xparam(c);
							break;
						}
						default:
						{
							throw new NoViableAltException(LT(1), getFilename());
						}
						 }
					}
				}
				else
				{
					goto _loop48_breakloop;
				}
				
			}
_loop48_breakloop:			;
		}    // ( ... )*
		match(COLON);
		v=value();
		c.Value = v; DDay.iCal.Serialization.iCalendar.Components.ContentLineSerializer.DeserializeToObject(c, o, ctx);
		{    // ( ... )*
			for (;;)
			{
				if ((LA(1)==CRLF))
				{
					match(CRLF);
				}
				else
				{
					goto _loop50_breakloop;
				}
				
			}
_loop50_breakloop:			;
		}    // ( ... )*
	}
	
	public string  name() //throws RecognitionException, TokenStreamException
{
		string s = string.Empty;;
		
		IToken  x = null;
		IToken  i = null;
		
		switch ( LA(1) )
		{
		case X_NAME:
		{
			x = LT(1);
			match(X_NAME);
			s = x.getText();
			break;
		}
		case IANA_TOKEN:
		{
			i = LT(1);
			match(IANA_TOKEN);
			s = i.getText();
			break;
		}
		default:
		{
			throw new NoViableAltException(LT(1), getFilename());
		}
		 }
		return s;
	}
	
	public string  param_name() //throws RecognitionException, TokenStreamException
{
		string n = string.Empty;;
		
		IToken  i = null;
		
		i = LT(1);
		match(IANA_TOKEN);
		n = i.getText();
		return n;
	}
	
	public string  param_value() //throws RecognitionException, TokenStreamException
{
		string v = string.Empty;;
		
		
		switch ( LA(1) )
		{
		case BEGIN:
		case COLON:
		case VCALENDAR:
		case END:
		case IANA_TOKEN:
		case X_NAME:
		case SEMICOLON:
		case EQUAL:
		case COMMA:
		case BACKSLASH:
		case NUMBER:
		case DOT:
		case CR:
		case LF:
		case ALPHA:
		case DIGIT:
		case DASH:
		case UNDERSCORE:
		case UNICODE:
		case SPECIAL:
		case SPACE:
		case HTAB:
		case SLASH:
		case ESCAPED_CHAR:
		case LINEFOLDER:
		{
			v=paramtext();
			break;
		}
		case DQUOTE:
		{
			v=quoted_string();
			break;
		}
		default:
		{
			throw new NoViableAltException(LT(1), getFilename());
		}
		 }
		return v;
	}
	
	public string  paramtext() //throws RecognitionException, TokenStreamException
{
		string s = null;;
		
		StringBuilder sb = new StringBuilder(); string c;
		
		{    // ( ... )*
			for (;;)
			{
				if ((tokenSet_3_.member(LA(1))))
				{
					c=safe_char();
					sb.Append(c);
				}
				else
				{
					goto _loop59_breakloop;
				}
				
			}
_loop59_breakloop:			;
		}    // ( ... )*
		s = sb.ToString();
		return s;
	}
	
	public string  quoted_string() //throws RecognitionException, TokenStreamException
{
		string s = string.Empty;
		
		StringBuilder sb = new StringBuilder(); string c;
		
		match(DQUOTE);
		{    // ( ... )*
			for (;;)
			{
				if ((tokenSet_4_.member(LA(1))))
				{
					c=qsafe_char();
					sb.Append(c);
				}
				else
				{
					goto _loop65_breakloop;
				}
				
			}
_loop65_breakloop:			;
		}    // ( ... )*
		match(DQUOTE);
		s = sb.ToString();
		return s;
	}
	
	public string  safe_char() //throws RecognitionException, TokenStreamException
{
		string c = string.Empty;
		
		IToken  a = null;
		
		{
			a = LT(1);
			match(tokenSet_3_);
		}
		c = a.getText();
		return c;
	}
	
	public string  value_char() //throws RecognitionException, TokenStreamException
{
		string c = string.Empty;
		
		IToken  a = null;
		
		{
			a = LT(1);
			match(tokenSet_1_);
		}
		c = a.getText();
		return c;
	}
	
	public string  qsafe_char() //throws RecognitionException, TokenStreamException
{
		string c = string.Empty;
		
		IToken  a = null;
		
		{
			a = LT(1);
			match(tokenSet_4_);
		}
		c = a.getText();
		return c;
	}
	
	public string  tsafe_char() //throws RecognitionException, TokenStreamException
{
		string s = string.Empty;
		
		IToken  a = null;
		
		{
			a = LT(1);
			match(tokenSet_5_);
		}
		s = a.getText();
		return s;
	}
	
	public string  text_char() //throws RecognitionException, TokenStreamException
{
		string s = string.Empty;
		
		IToken  a = null;
		
		{
			a = LT(1);
			match(tokenSet_6_);
		}
		s = a.getText();
		return s;
	}
	
	public string  text() //throws RecognitionException, TokenStreamException
{
		string s = string.Empty;
		
		string t;
		
		{    // ( ... )*
			for (;;)
			{
				if ((tokenSet_6_.member(LA(1))))
				{
					t=text_char();
					s += t;
				}
				else
				{
					goto _loop78_breakloop;
				}
				
			}
_loop78_breakloop:			;
		}    // ( ... )*
		return s;
	}
	
	public string  number() //throws RecognitionException, TokenStreamException
{
		string s = string.Empty;
		
		IToken  n1 = null;
		IToken  n2 = null;
		
		n1 = LT(1);
		match(NUMBER);
		s += n1.getText();
		{
			switch ( LA(1) )
			{
			case DOT:
			{
				match(DOT);
				s += ".";
				n2 = LT(1);
				match(NUMBER);
				s += n2.getText();
				break;
			}
			case EOF:
			case SEMICOLON:
			{
				break;
			}
			default:
			{
				throw new NoViableAltException(LT(1), getFilename());
			}
			 }
		}
		return s;
	}
	
	public string  version_number() //throws RecognitionException, TokenStreamException
{
		string s = string.Empty;
		
		string t;
		
		t=number();
		s += t;
		{
			switch ( LA(1) )
			{
			case SEMICOLON:
			{
				match(SEMICOLON);
				s += ";";
				t=number();
				s += t;
				break;
			}
			case EOF:
			{
				break;
			}
			default:
			{
				throw new NoViableAltException(LT(1), getFilename());
			}
			 }
		}
		return s;
	}
	
	private void initializeFactory()
	{
	}
	
	public static readonly string[] tokenNames_ = new string[] {
		@"""<0>""",
		@"""EOF""",
		@"""<2>""",
		@"""NULL_TREE_LOOKAHEAD""",
		@"""CRLF""",
		@"""BEGIN""",
		@"""COLON""",
		@"""VCALENDAR""",
		@"""END""",
		@"""IANA_TOKEN""",
		@"""X_NAME""",
		@"""SEMICOLON""",
		@"""EQUAL""",
		@"""COMMA""",
		@"""DQUOTE""",
		@"""CTL""",
		@"""BACKSLASH""",
		@"""NUMBER""",
		@"""DOT""",
		@"""CR""",
		@"""LF""",
		@"""ALPHA""",
		@"""DIGIT""",
		@"""DASH""",
		@"""UNDERSCORE""",
		@"""UNICODE""",
		@"""SPECIAL""",
		@"""SPACE""",
		@"""HTAB""",
		@"""SLASH""",
		@"""ESCAPED_CHAR""",
		@"""LINEFOLDER"""
	};
	
	private static long[] mk_tokenSet_0_()
	{
		long[] data = { 114L, 0L};
		return data;
	}
	public static readonly BitSet tokenSet_0_ = new BitSet(mk_tokenSet_0_());
	private static long[] mk_tokenSet_1_()
	{
		long[] data = { 4294934496L, 0L, 0L, 0L};
		return data;
	}
	public static readonly BitSet tokenSet_1_ = new BitSet(mk_tokenSet_1_());
	private static long[] mk_tokenSet_2_()
	{
		long[] data = { 4294934512L, 0L, 0L, 0L};
		return data;
	}
	public static readonly BitSet tokenSet_2_ = new BitSet(mk_tokenSet_2_());
	private static long[] mk_tokenSet_3_()
	{
		long[] data = { 4294907808L, 0L, 0L, 0L};
		return data;
	}
	public static readonly BitSet tokenSet_3_ = new BitSet(mk_tokenSet_3_());
	private static long[] mk_tokenSet_4_()
	{
		long[] data = { 4294918112L, 0L, 0L, 0L};
		return data;
	}
	public static readonly BitSet tokenSet_4_ = new BitSet(mk_tokenSet_4_());
	private static long[] mk_tokenSet_5_()
	{
		long[] data = { 4294842272L, 0L, 0L, 0L};
		return data;
	}
	public static readonly BitSet tokenSet_5_ = new BitSet(mk_tokenSet_5_());
	private static long[] mk_tokenSet_6_()
	{
		long[] data = { 4294868960L, 0L, 0L, 0L};
		return data;
	}
	public static readonly BitSet tokenSet_6_ = new BitSet(mk_tokenSet_6_());
	
}
}
