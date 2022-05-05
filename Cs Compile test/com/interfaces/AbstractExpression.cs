using System;
using System.Collections.Generic;
using System.Text;

namespace Cs_Compile_test.com.interfaces {
	public abstract class AbstractExpression
	{
		protected Context scope;
		/// <summary>
		/// Executes an expression
		/// </summary>
		/// <returns>The object result of the execution</returns>
		public abstract object Execute(ref ExecutionStatus status);

		public AbstractExpression SetScope(Context context) { 
			this.scope = context;
			return this;
		}

		public bool ScopeHasChanged(Context context) {
			return scope != context;
		}
	}
}
