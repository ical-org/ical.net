using System;
using antlr.collections;

namespace antlr
{
	/* ANTLR Translator Generator
	 * Project led by Terence Parr at http://www.jGuru.com
	 * Software rights: http://www.antlr.org/license.html
	 *
	 * $Id:$
	 */

	//
	// ANTLR C# Code Generator by Micheal Jordan
	//                            Kunle Odutola       : kunle UNDERSCORE odutola AT hotmail DOT com
	//                            Anthony Oguntimehin
	//
	// With many thanks to Eric V. Smith from the ANTLR list.
	//
	
	/// <summary>
	/// Summary description for DumpASTVisitor.
	/// </summary>
	/** Simple class to dump the contents of an AST to the output */
	public class DumpASTVisitor : ASTVisitor 
	{
		protected int level;


		private void tabs() 
		{
			for (int i = 0; i < level; i++) 
			{
				Console.Out.Write("   ");
			}
		}

		public void visit(AST node) 
		{
			// Flatten this level of the tree if it has no children
		    AST node2;
			for (node2 = node; node2 != null; node2 = node2.getNextSibling()) 
			{
				if (node2.getFirstChild() != null) 
				{
				    break;
				}
			}

			for (node2 = node; node2 != null; node2 = node2.getNextSibling()) 
			{
				if (node2 == node) 
				{
					tabs();
				}
			    Console.Out.Write(node2.getText() ?? "nil");

			    Console.Out.Write(" [" + node2.Type + "] ");
				Console.Out.WriteLine("");

				if (node2.getFirstChild() != null) 
				{
					level++;
					visit(node2.getFirstChild());
					level--;
				}
			}
		}
	}  
}


