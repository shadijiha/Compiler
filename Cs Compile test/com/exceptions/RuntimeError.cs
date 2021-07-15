/**
 *
 */

using System;

namespace Cs_Compile_test.com.exceptions
{
	public class RuntimeError : Exception
	{
		public RuntimeError(string messageFormat, params object[] args)
			: base("RuntimeError Error: " + string.Format(messageFormat, args))
		{
		}

		public RuntimeError() {
		}
	}
}