using System.Text;

namespace antlr
{

	/* ANTLR Translator Generator
	 * Project led by Terence Parr at http://www.jGuru.com
	 * Software rights: http://www.antlr.org/license.html
	 */

	//
	// ANTLR C# Code Generator by Micheal Jordan
	//                            Kunle Odutola       : kunle UNDERSCORE odutola AT hotmail DOT com
	//                            Anthony Oguntimehin
	//
    
    public class ParseTreeToken : ParseTree
	{
		protected IToken token;

		public ParseTreeToken(IToken token) 
		{
			this.token = token;
		}

		protected internal override int getLeftmostDerivation(StringBuilder buf, int step) 
		{
			buf.Append(' ');
			buf.Append(ToString());
			return step; // did on replacements
		}

		public override string ToString()
		{
			if ( token != null ) 
			{
				return token.getText();
			}
			return "<missing token>";
		}
	}
}
