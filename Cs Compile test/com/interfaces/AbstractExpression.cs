using System;
using System.Collections.Generic;
using System.Text;

namespace Cs_Compile_test.com.interfaces {
	public interface AbstractExpression
	{
		
		/// <summary>
		/// Executes an expression
		/// </summary>
		/// <returns>The object result of the execution</returns>
		public object Execute(ref ExecutionStatus status);

	}
}
