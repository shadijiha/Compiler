using Cs_Compile_test.com.interfaces;
using System.Data;
using System.Linq;

namespace Cs_Compile_test.com.expr {

	public class ConditionalExpression : AbstractExpression {
		private string block;
		private ShadoObject scope;
		private string condition;

		public ConditionalExpression(string block, string condition, ShadoObject scope) {
			this.block = block;
			this.scope = scope;
			this.condition = condition;
		}

		public object Execute(ref ExecutionStatus status) {

			if (Evaluate()) {
				var lines = block.Split("\n");
				lines = Clean(lines);

				for (int i = 1; i < lines.Length - 1; i++) {
					new Expression(lines[i], scope).Execute(ref status);
				}

				return new Expression(lines[^1], scope).Execute(ref status);
			}

			return null;
		}

		private bool Evaluate() {
			string line = condition.Replace("if", "").Trim();
			line = line.Substring(0, line.Length - 1);

			foreach (var variable in scope.GetAllVariables()) {
				line = line.Replace(variable.name, variable.value?.ToString());
			}

			return Util.EvaluateBoolExpression(line);
		}

		private string[] Clean(string[] lines) {
			return lines.Where(e => !string.IsNullOrEmpty(e) && e.Trim() != "{" && e.Trim() != "}").ToArray();
		}

		public static bool IsIfStatement(string line) => line.Trim().StartsWith("if");
	}
}
