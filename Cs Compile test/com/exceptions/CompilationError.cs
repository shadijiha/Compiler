/**
 *
 */

using System;

namespace Cs_Compile_test.com.exceptions
{
	public class CompilationError : Exception
	{
		public CompilationError(string messageFormat, params object[] args)
			: base("Compilation Error: " + string.Format(messageFormat, args))
		{
		}

		public CompilationError() {
		}
	}
}