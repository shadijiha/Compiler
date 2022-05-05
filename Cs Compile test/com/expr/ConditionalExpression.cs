using Cs_Compile_test.com.interfaces;
using System.Data;
using System.Linq;

namespace Cs_Compile_test.com.expr {

	public class ConditionalExpression : AbstractExpression {
		private string block;
		private string condition;

		public ConditionalExpression(string block, string condition, Context scope) {
			this.block = block;
			this.scope = scope;
			this.condition = condition;
		}

		public ConditionalExpression(string block, string condition, ShadoObject scope)
			: this(block, condition, (Context)scope)
		{
		}

		public override object Execute(ref ExecutionStatus status) {

			if (Evaluate(ref status)) {
				var lines = block.Split("\n");
				lines = this.Clean(lines);

				for (int i = 1; i < lines.Length - 1; i++) {
					new Expression(lines[i], scope).Execute(ref status);
				}

				return new Expression(lines[^1], scope).Execute(ref status);
			}

			return null;
		}

		private bool Evaluate(ref ExecutionStatus status) {
			string line = condition.Replace("if", "").Trim();
			line = line.ReplaceFirstOccurrence("(", "").ReplaceLastOccurrence(")", "").Replace("{", "").Replace("}", "").Trim();

			bool.TryParse(new Expression(line, scope).Execute(ref status).ToString(), out var result);

			return result;
		}

		public static bool IsIfStatement(string line) => line.Trim().StartsWith("if");
    }
}
