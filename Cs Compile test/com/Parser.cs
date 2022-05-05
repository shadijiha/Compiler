using Cs_Compile_test.com.exceptions;
using Cs_Compile_test.com.expr;
using Cs_Compile_test.com.interfaces;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Text.RegularExpressions;

namespace Cs_Compile_test.com {
	public static class Parser {
		private static List<string> linesStartingGlobal = new List<string>();

		/// <summary>
		/// Parses the code and adds all the extacted methods to the class
		/// </summary>
		/// <param name="code">The code to parse</param>
		/// <param name="clazz">The class to add the method to, if it is null, the methods are added to the global scope</param>
		public static void ExtractMethods(string code, ShadoClass clazz = null, ShadoObject ctorReturn = null) {

			var blocks = ExtractBlocks(code);

			foreach (string block in blocks) {
				string[] lines = block.Split("\n");

				// If this is not a method definition
				if (!IsMethodDefinition(lines[0]) && !IsConstructor(lines[0], clazz))
					continue;

				// Extract method data
				MethodInfo info = ExtractMethodInfo(lines[0]);
				if (IsConstructor(lines[0], clazz))
					info.name = clazz.name;
			

				// Add it to the global scope
				ShadoMethod method = new ShadoMethod(info.name, info.args.Length, info.returnType);
				method.AddAttribute(info.attributes);

				List<AbstractExpression> methodCodeLines = new List<AbstractExpression>();

				// Add all args to method scope
				for (int i = 0; i < info.args.Length; i++) {
					var temp = Regex.Split(info.args[i], "\\s+")
						.Where(e => !string.IsNullOrEmpty(e)).ToArray();
					method.AddVariable(temp[0], temp[1]);
				}

				// If method is C# method
				if (method.Is(ShadoMethod.Attributes.C_SHARP)) {
					var CScode = string.Join('\n', lines[1..^0]);
					methodCodeLines.Add(new CSharpExpression("C#{\n" + CScode, method));
					goto AfterLoop;
				}

				// Parse method lines
				for (int i = 1; i < lines.Length - 1; i++) {
					Context context = new Context(method);
					if (ctorReturn != null)
						context.Attach(ctorReturn);

					// Check if the statement is an if statement
					if (ConditionalExpression.IsIfStatement(lines[i])) {
						Range range = new Range(i, lines.Length);
						var ifBlock = ExtractBlocks(string.Join('\n', lines[range]));
						methodCodeLines.Add(new ConditionalExpression(ifBlock.First(), lines[i], context));

						// Do not add the if statement content for parsing
						i += ifBlock.First().Split("\n").Length - 1;
					}
					// Else check if it is a for loop
					else if (LoopExpression.IsLoopStatement(lines[i])) {
						Range range = new Range(i, lines.Length);
						var loopBlock = ExtractBlocks(string.Join('\n', lines[i..lines.Length]));
						methodCodeLines.Add(new LoopExpression(loopBlock.First(), lines[i], context));

						// Do not add the if statement content for parsing
						i += loopBlock.First().Split("\n").Length - 1;
					} 
					// Else check if it is a C# block
					else if (CSharpExpression.IsCSharpStatement(lines[i])) {
						var CSBlock = ExtractBlocks(string.Join('\n', lines[i..lines.Length]));
						methodCodeLines.Add(new CSharpExpression(CSBlock.First(), context));

						// Do not add the if statement content for parsing
						i += CSBlock.First().Split("\n").Length - 1;
					}
					else {
						// Otherwise just push an Expression
						methodCodeLines.Add(new Expression(lines[i], context));
					}
				}

			AfterLoop:
				ShadoMethod.MethodCall lambda = (ctx, args) =>
				{
					var status = new ExecutionStatus();

					int i = 0;
					while (i < methodCodeLines.Count && status.status == ExecutionStatus.Type.OK)
					{
						try
						{
							methodCodeLines[i++].Execute(ref status);
						}
						catch (Exception e)
						{
							throw new Exception(e.Message + $"\n\t @ line: {i + 1}\n" + $"--->\t{lines[i]}");
						}
					}

					return status.value;
				};
				method.SetCode(lambda);

				// If it is a constructor the method needs to return an object
				if (IsConstructor(lines[0], clazz)) {
					method.SetCode((ctx, args) => {
						lambda(  ctx, args);						
						return ctorReturn;
					});
				}

				if (clazz == null)
					VM.instance.PushVariable(method);
				else
					clazz.AddMethod(method);
			}
		}

		public static void ExtractClasses(string code) {
			var blocks = ExtractBlocks(code);

			foreach (string block in blocks) {
				string[] lines = block.Split("\n");

				// If this is a class definition
				if (!IsClassDefinition(lines[0]))
					continue;

				// Extract class data
				string line = lines[0];
				string[] tokens = Regex.Split(line, "\\s+").Where(e => !string.IsNullOrEmpty(e)).ToArray();

				string className = "";
				for (uint i = 0; i < tokens.Length; i++) {
					if (tokens[i] == "class")
						try { className = tokens[i + 1]; } catch (Exception) { throw new SyntaxError("Class does not have a name"); }
				}


				// Register this class
				ShadoClass clazz = new ShadoClass(className);
				clazz.AddParentClass(VM.GetSuperType());


				// This is the object the constructor will return
				ShadoObject constructorReturn = new ShadoObject(clazz, null);

				// Extract all instance variables
				foreach (var l in lines) {
					if (IsInstanceVariable(l)) {
						ExecutionStatus dummy = new ExecutionStatus();
						new Expression(l, constructorReturn).Execute(ref dummy);
					}
				}

				constructorReturn.AddVariable("string", "class", className);

				// Add constructor and the class() method
				clazz.AddMethod(new ShadoMethod(className, 0, className).SetCode((ctx, args) => {

					// Call constructor of all parent
					foreach (var parent in clazz.GetParentClasses())
						parent.GetConstructor().Call(ctx, args);

					return constructorReturn;
				}));
				clazz.AddMethod(new ShadoMethod("class", 0, "string")
					.SetCode((ctx, args) => className)
					.AddAttribute(ShadoMethod.Attributes.STATIC));

				VM.instance.AddType(clazz);

				// Extract methods
				Range r = new Range(1, lines.Length - 1);
				ExtractMethods(string.Join('\n', lines[r]), clazz, constructorReturn);
			}
		}

		private static List<string> ExtractBlocks(string code) {
			List<string> blocks = new List<string>();

			Stack<string> stack = new Stack<string>();
			StringBuilder buffer = new StringBuilder();

			string[] lines = code.Split("\n");
			foreach (string line in lines) {

				// If it is a global declerations
				if (line.Trim().StartsWith("global") && line.Trim().Length > 5)
					linesStartingGlobal.Add(line);

				if (line.Contains("{")) {
					stack.Push("{");
				}

				if (line.Contains("}")) {
					try { stack.Pop(); } catch (Exception) { }

					if (stack.Count == 0) {
						blocks.Add(buffer.ToString() + "}");
						buffer.Clear();
					}
				}

				if (stack.Count != 0)
					buffer.Append(line).Append("\n");
			}

			return blocks;
		}

		public static void ProcessGlobalAllocations() {
            foreach (var line in linesStartingGlobal)
            {
				string trimmed = line.Trim().Substring(6);
				ExecutionStatus status = new ExecutionStatus
				{
					status = ExecutionStatus.Type.OK
				};
				new Expression(trimmed, ShadoObject.Global).Execute(ref status);
			}
		}
		
		private static string Clean(string input, string regex = @"\(|\)") {
			return Regex.Replace(input, regex, "");
		}

		private static bool IsMethodDefinition(string line) {
			return new ExpressionSyntax("TYPE IDENTIFIER(ANY)ANY{").Matches(line);
		}

		private static bool IsClassDefinition(string line) {
			return new ExpressionSyntax("ANYclassANY{").Matches(line) ||
				new ExpressionSyntax("ANYstructANY{").Matches(line);
		}

		private static bool IsInstanceVariable(string line) {
			return new ExpressionSyntax("ANYTYPE IDENTIFIER = ANY").Matches(line) ||
			       new ExpressionSyntax("ANYTYPE IDENTIFIER;").Matches(line);
		}

		private static bool IsConstructor(string line, ShadoClass clazz) {
			if (clazz == null)
				return false;
			return new ExpressionSyntax($"ANY{clazz.name}(ANY)ANY{{").Matches(line);
		}
		private static MethodInfo ExtractMethodInfo(string signature) {

			string[] tokens = Regex.Split(signature, "\\s+");
			MethodInfo info = new MethodInfo();

			// Extract the type
			foreach (var token in tokens) {
				if (VM.instance.HasType(token)) {
					info.returnType = token;
					break;
				}
			}

			// Extract all attributes
			foreach (var token in tokens) {
				foreach (var att in Enum.GetValues(typeof(ShadoMethod.Attributes))) {
					if (token.ToLower() == att.ToString().ToLower()) {
						info.attributes |= (ShadoMethod.Attributes)att;
					}
				}
			}

			if (signature.Contains("C#")) {
				info.attributes |= ShadoMethod.Attributes.C_SHARP;
			}

			foreach (var token in tokens) {
				// If the token contains a ( then it means that the name is NAME(rest
				if (token.Contains("(")) {
					info.name = Clean(token.Split("(")[0]);
					break;
				}
			}

			info.args = Clean(Regex.Match(signature, "\\(.*\\)").Value)
				.Split(",").Where(x => !string.IsNullOrEmpty(x))
				.ToArray();

			return info;
		}

		private struct MethodInfo {
			public string name { get; set; }
			public string returnType { get; set; }
			public ShadoMethod.Attributes attributes { get; set; }
			public string[] args { get; set; }
		}
	}
}
