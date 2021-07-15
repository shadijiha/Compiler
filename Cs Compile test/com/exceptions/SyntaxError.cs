/**
 *
 */

using System;

namespace Cs_Compile_test.com.exceptions
{
	public class SyntaxError : Exception
	{
		public SyntaxError(string messageFormat, params object[] args)
			: base("RuntimeError Error: " + string.Format(messageFormat, args))
		{
		}

		public SyntaxError() {
		}
	}
}