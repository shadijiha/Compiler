using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using Cs_Compile_test.com.exceptions;
using Cs_Compile_test.com.interfaces;

namespace Cs_Compile_test.com.expr {

	public class LoopExpression : AbstractExpression {

		public static bool IsLoopStatement(string line) => line.Trim().StartsWith("for") || line.Trim().StartsWith("while");

		private string block;
		private ShadoObject scope;
		private string rawCondition;


		private Type type;
		private string loopVariable;
		private string loopCondition;
		private string loopIncrement = null;

		public LoopExpression(string block, string condition, ShadoObject scope) {
			this.block = block;
			this.scope = scope;
			this.rawCondition = condition.Trim();

			ParseLoopCond();
		}

		public object Execute(ref ExecutionStatus status) {

			// Execute the assignment expression
			if (type == Type.FOR)
				new Expression(rawCondition.Split(";")[0], scope).Execute(ref status);

			string[] lines = block.Split("\n");
			lines = Clean(lines);
			while (EvaluateCondition(loopCondition, ref status)) {

				for (int i = 1; i < lines.Length; i++) {
					new Expression(lines[i], scope).Execute(ref status);
				} 

				if (type == Type.FOR)
					new Expression(loopIncrement, scope).Execute(ref status); // Increment the for loop
			}

			return null;
		}

		private void ParseLoopCond() {

			// If it is a for loop
			if (rawCondition.StartsWith("for")) {

				rawCondition = rawCondition.ReplaceFirstOccurrence("for", "").Trim();   // Remove for word
				rawCondition = rawCondition.Replace("{", "").Replace("}", "");  // Remove { and }
				rawCondition = RemoveParantheses(rawCondition);

				var tokens = rawCondition.Split(";");
				loopVariable = ExtractLoopVariable(tokens);

				loopCondition = tokens[1];
				loopIncrement = tokens[^1];

				type = Type.FOR;
			} else if (rawCondition.StartsWith("while")) {
				rawCondition = rawCondition.Trim().ReplaceFirstOccurrence("while", "");
				rawCondition = RemoveParantheses(rawCondition).Replace("{", "").Replace("}", "").Trim();
				loopCondition = rawCondition;
				type = Type.WHILE;
			}
			else {
				throw new CompilationError("Not a valid loop");
			}

		}

		private string[] Clean(string[] lines) {
			return lines.Where(e => !string.IsNullOrEmpty(e) && e.Trim() != "{" && e.Trim() != "}").ToArray();
		}

		private string RemoveParantheses(string source) {
			return rawCondition.ReplaceFirstOccurrence("(", "").ReplaceLastOccurrence(")", "").Trim();
		}

		private string ExtractLoopVariable(string[] tokens) {
			string[] temp = tokens[0].Split("=");	// Split the variable assignment
			string[] varInfo = Regex.Split(temp[0].Trim(), "\\s+");

			return varInfo[^1];
		}

		private bool EvaluateCondition(string expression, ref ExecutionStatus status) {
			bool.TryParse(new Expression(expression, scope).Execute(ref status).ToString(), out var result);
			return result;
		}

		private enum Type
		{
			FOR, WHILE, FOR_EACH
		}
	}
}
