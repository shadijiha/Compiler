using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Cs_Compile_test.com.interfaces;
using DynamicExpresso;
using Microsoft.CodeAnalysis.CSharp.Scripting;

namespace Cs_Compile_test.com.expr {
	public class CSharpExpression : AbstractExpression {

		private string block;
		private ShadoObject scope;

		public CSharpExpression(string block, ShadoObject scope) {
			this.block = block;
			this.scope = scope;
		}

		public object Execute(ref ExecutionStatus status) {
			string[] lines = block.Split("\n");
			Range range = new Range(1, lines.Length - 1);
			string[] blocks = lines[range];

			object result = CSharpScript.EvaluateAsync(string.Join('\n', blocks)).Result;

			return result;
		}

		public static bool IsCSharpStatement(string line) {
			return line.Trim().StartsWith("C#");
		}
	}
}
