using Cs_Compile_test.com.exceptions;
using Cs_Compile_test.com.interfaces;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;

namespace Cs_Compile_test.com {

	public class ExpressionSyntax {
		public readonly static string type = "TYPE";
		public readonly static string identifier = "IDENTIFIER";
		public readonly static string op = "OPERATOR";
		public readonly static string symbol = "SYMBOL";
		public readonly static string any = "ANY";

		public static readonly ExpressionSyntax ASSIGNEMENT = new ExpressionSyntax($"{identifier} {op} {any}");
		public static readonly ExpressionSyntax DECLARATION = new ExpressionSyntax($"{type} {identifier};");
		public static readonly ExpressionSyntax FULL_DECLARATION = new ExpressionSyntax($"{type} {identifier} = {any}");

		private readonly string pattern;

		public ExpressionSyntax(string pattern) {
			this.pattern = pattern;
		}

		public bool Matches(string str, bool escape = true) {
			string regex = escape ?
				Regex.Escape(pattern).Replace(type, compileTypes())
				: pattern.Replace(type, compileTypes());

			regex = regex.Replace(identifier, compileIdentifiers());
			regex = regex.Replace(any, ".*");
			regex = regex.Replace(op, compileOperators());
			regex = regex.Replace(symbol, compileOperators());
			return Regex.IsMatch(str, regex);
		}

		public static string compileTypes() {
			/*StringBuilder builder = new StringBuilder();
			builder.Append("(");

			var it = VM.instance.GetAllTypes();
			uint count = 0;
			foreach (ShadoClass shadoClass in it) {
				builder.Append(Regex.Escape(shadoClass.name));
				if (count < it.Count - 1)
					builder.Append("|");

				count++;
			}

			builder.Append(")");*/

			return "\\w+(\\*|\\[])?"; //builder.ToString();
		}

		public static string compileIdentifiers() {
			return "\\w+";
		}

		public static string compileOperators() {
			//string[] ops = {"=", "==", "!=", "!", "<", ">", "<=", ">=", "+", "*", "-", "/", "(", ")", "[", "]", "{", "}", "|", "%", "@", "$", "#", "^", "&", ".", "->", ",", ";" };
			string[] ops = { "=", "%=", "+=", "-=", "*=", "/=" };
			StringBuilder builder = new StringBuilder();

			uint count = 0;
			builder.Append("(");
			foreach (string s in ops) {
				builder.Append(Regex.Escape(s));

				if (count < ops.Length - 1)
					builder.Append("|");

				count++;
			}

			builder.Append(")");

			return builder.ToString();
		}
	}

	public class Expression : AbstractExpression {
		public enum Type {
			DECLARATION, ASSIGNMENT, FUNC_CALL, OBJECT_FUNC_CALL, RETURN, POINTER_ASSIGNMENT, OBJECT_INSTATIATION, GET_VALUE
		}

		private string raw;
		private ShadoClass type;
		private string name;
		private string rhs;
		private Type expressionType;
		private ShadoObject scope;

		public Expression(string raw, ShadoObject scope) {
			this.scope = scope;
			this.raw = raw.Trim();
			var parts = SplitByExceptQuotes(raw, " ");

			parseExpression(parts);
		}

		public object Execute(ref ExecutionStatus status) {
			switch (expressionType) {
				case Type.ASSIGNMENT:
				case Type.DECLARATION:
					return executeVariableAssignemt(ref status);
				case Type.FUNC_CALL:
					return executeFuncCall(ref status);
				case Type.OBJECT_FUNC_CALL:
					return executeFuncCallOnObject(ref status);
				case Type.RETURN:
					return executeReturn(ref status);
				case Type.POINTER_ASSIGNMENT:
					return executePointerAssignment(ref status);
				case Type.OBJECT_INSTATIATION:
					return executeObjectInstantiation();
				case Type.GET_VALUE:
					return executeGetValueOf();
				default: return rhs;
			}
		}

		private void parseExpression(List<string> tokens) {

			if (new ExpressionSyntax("return ANY").Matches(raw)) {
				this.name = "";
				this.rhs = raw.Replace("return", "").Trim();
				this.expressionType = Type.RETURN;
			}
			// It is a object initialization
			else if (new ExpressionSyntax("TYPE IDENTIFIER = new TYPE(ANY)").Matches(raw)) {
				this.type = VM.instance.GetClass(tokens[0]);
				this.name = tokens[1];
				this.rhs = string.Join(' ', tokens.GetRange(3, tokens.Count - 3));
				this.expressionType = Type.OBJECT_INSTATIATION;
			}
			// If it is a pointer modification
			else if (new ExpressionSyntax("*IDENTIFIER = ANY").Matches(raw)) {
				this.name = raw.Trim().Substring(1).Split("=")[0].Trim();
				this.rhs = raw.Split("=")[1].Trim();
				this.expressionType = Type.POINTER_ASSIGNMENT;
			}
			// If it is a full declaration
			else if (ExpressionSyntax.FULL_DECLARATION.Matches(raw)) {
				this.type = VM.instance.GetClass(tokens[0]);
				this.name = tokens[1];
				this.rhs = string.Join(' ', tokens.GetRange(3, tokens.Count - 3));
				this.expressionType = Type.ASSIGNMENT;
			}
			// If it is a function call on object
			else if (new ExpressionSyntax("IDENTIFIER.IDENTIFIER(ANY)").Matches(raw) && !raw.Contains("{")) {
				this.type = null;
				this.name = null;
				this.rhs = raw;
				this.expressionType = Type.OBJECT_FUNC_CALL;
			}
			// If it is a function call
			else if (new ExpressionSyntax("IDENTIFIER(ANY)").Matches(raw) && !raw.Contains("{")) {
				this.type = null;
				this.name = null;
				this.rhs = raw;
				this.expressionType = Type.FUNC_CALL;
			}
			// If it is a declaration
			else if (ExpressionSyntax.DECLARATION.Matches(raw)) {
				this.type = VM.instance.GetClass(tokens[0]);
				this.name = tokens[1].Replace(";", "").Replace("\n", "");
				this.expressionType = Type.DECLARATION;
				this.rhs = "";
			}
			// If it is a reassignment
			else if (ExpressionSyntax.ASSIGNEMENT.Matches(raw)) {
				// TODO: The varaibles my be in scope in the future
				this.name = tokens[0];
				this.rhs = string.Join(' ', tokens.GetRange(2, tokens.Count - 2));
				this.type = scope.GetVariable(name)?.type ?? VM.instance.Get(name)?.type;
				this.expressionType = Type.ASSIGNMENT;
			}
			// Get value of the variable
			else if (new ExpressionSyntax("IDENTIFIER").Matches(raw)) {
				name = raw.Trim();
				rhs = raw.Trim();
				expressionType = Type.GET_VALUE;
			} else {
				this.rhs = raw;
			}

			// Remove the new line or the ; at the end
			this.rhs = rhs.Trim();
			if (!string.IsNullOrEmpty(rhs) && rhs[^1] == ';')
				rhs = rhs.Substring(0, rhs.Length - 1);
		}

		private object executeVariableAssignemt(ref ExecutionStatus status) {
			// TODO: this shit should not be here
			rhs = rhs.Trim();

			// Check for pointer
			object value = null;
			ShadoObject pointer = null; // If the current expression is an assignment to clone a method (change its name by variable assignment)

			if (rhs.StartsWith("&")) {
				var vname = rhs.Substring(1);
				var ptr = scope.GetVariable(vname) ?? VM.instance.Get(vname);
				checkVariable(ptr);

				value = ptr.GetHashCode();
			}
			// Check for derefrence
			else if (rhs.StartsWith("*")) {

				var ptr = rhs.Substring(1);
				ShadoObject obj;

				// If it is a int
				if (int.TryParse(ptr, out _))
					obj = scope.GetByAddress(int.Parse(ptr)) ?? MemoryManager.GetByAddress(int.Parse(ptr));
				else
					obj = scope.GetByAddress(int.Parse(scope.GetVariable(ptr).value.ToString()))
						  ?? MemoryManager.GetByAddress(int.Parse(VM.instance.Get(ptr).value.ToString()));

				checkVariable(obj);

				value = obj.value;
				pointer = obj;
			}
			// Math expression
			else if (isMathExpression(rhs, ref value)) { }
			// It is a string assignment
			else if (rhs.StartsWith("\"") && rhs.EndsWith("\"")) {
				value = rhs.ReplaceFirstOccurrence("\"", "").ReplaceLastOccurrence("\"", "");
			}
			// It is an array assignment
			else if (rhs.StartsWith("{") && rhs.EndsWith("}")) {
				value = new List<object>();
				string[] args = SplitByExceptQuotes(rhs.Substring(1, rhs.Length - 2), " |,").ToArray();

				// Insert all args to array
				var t = type?.GetUnitType() ?? VM.GetSuperType();
				foreach (string s in args) {
					if (t.IsValid(s.Trim()))
						(value as List<object>).Add(s);
					else
						throw new RuntimeError("Cannot add {0} to an array of type {1}", s, type.name);
				}
			}
			// Variable to variable assignment
			else if (scope.GetVariable(rhs) != null) {
				var obj = scope.GetVariable(rhs);
				value = obj.value;
				pointer = obj;
			} else if (isFunctionCall(rhs)) {
				value = new Expression(rhs, scope).Execute(ref status);
			}

			// Push new variable or modify the old one
			if (pointer?.IsMethod() ?? false) {
				var method = new ShadoMethod(pointer as ShadoMethod);
				method.name = name;
				VM.instance.PushVariable(method);
			} else {
				// Push The variable to the scope context
				name = name == null ? rhs : name;

				// If the value is a ShadoObject then you need to copy its value
				// To avoid shallow copy bugs
				if (value is ShadoObject sobj)
					value = sobj.value;

				scope.AddOrUpdateVariable(type?.name ?? "object", name, value);
			}

			return value;
		}

		private object executeFuncCall(ref ExecutionStatus status) {

			// See if it is a native function declaration 
			if (new ExpressionSyntax("native ANY;").Matches(raw.Trim()))
				return null;


			// Parse function name
			string functionName = rhs.Split("(")[0].Trim();
			string[] rawargs = SplitByExceptQuotes(rhs.Substring(functionName.Length + 1, rhs.Length - functionName.Length - 2), "+| |,").ToArray();
			object[] args = new object[rawargs.Length];

			// Parse the args
			for (int i = 0; i < rawargs.Length; i++) {
				args[i] = new Expression(rawargs[i], scope).Execute(ref status);
			}

			// Get the function
			ShadoObject method = VM.instance.Get(functionName);
			if (method == null || !method.IsMethod())
				throw new RuntimeError("{0} is not a function", functionName);

			return (method as ShadoMethod).Call(scope, args);
		}

		private object executeFuncCallOnObject(ref ExecutionStatus status) {
			// Parse function name
			string[] tokens = rhs.Split(".", 2);
			string objectName = tokens[0].Trim();
			string functionName = tokens[1].Split("(")[0].Trim();

			char[] argsNoParantheses = tokens[1].Split("(", 2)[1].ToCharArray();
			argsNoParantheses[^1] = ' ';

			string[] rawArgs = SplitByExceptQuotes(new string(argsNoParantheses).Trim(), ",").ToArray();
			object[] args = new object[rawArgs.Length];
			for (int i = 0; i < rawArgs.Length; i++) {
				args[i] = new Expression(rawArgs[i], scope).Execute(ref status);
			}

			// See if it is a static function call
			// Meaning that the object name is a type
			if (VM.instance.HasType(objectName)) {
				// See if the method is static
				ShadoClass clazz = VM.instance.GetClass(objectName);
				ShadoMethod func = clazz.GetMethod(functionName);

				// Check if method is static
				if (!func.Is(ShadoMethod.Attributes.STATIC))
					throw new CompilationError("Cannot invoke a none static method in a static way");

				return func.Call(ShadoObject.Global, args);
			}

			// Get the function
			ShadoObject ctx = scope.GetVariable(objectName) ?? VM.instance.GetOrThrow(objectName);
			ShadoMethod method = ctx.type.GetMethod(functionName);

			// See if the method is not public while calling ouside the class
			if ((method.Is(ShadoMethod.Attributes.PRIVATE)
				|| method.Is(ShadoMethod.Attributes.PROTECTED))
				 && scope.ToString() != ctx.ToString())
				throw new CompilationError("Cannot call a non public method outside of its class");

			return method.Call(ctx, args);
		}

		private object executeReturn(ref ExecutionStatus status) {
			object o = new Expression(rhs, scope).Execute(ref status);
			status.value = o;
			status.status = ExecutionStatus.Type.RETURN;
			return o;
		}

		private object executePointerAssignment(ref ExecutionStatus status) {
			this.type = scope.GetVariable(name).type;

			// See if the rhs is ok
			object val = new Expression(rhs, scope).Execute(ref status);
			if (!type.IsValid(val))
				throw new CompilationError("cannot assign {0} to a variable of type {1}", rhs, type.name);

			string expr = new Expression(name, scope).Execute(ref status).ToString();
			int address;
			if (!int.TryParse(expr, out address))
				throw new RuntimeError("Invalid memory address", expr);

			MemoryManager.GetByAddress(address).value = val;

			return null;
		}

		private object executeObjectInstantiation() {

			string[] expr = Regex.Split(rhs.Trim(), "\\s+");
			expr[0] = "";

			string noNew = string.Join(' ', expr.RemoveBlanks());
			string constructor = noNew.Split("(", 2)[0];
			string rawargs = noNew.Split("(", 2)[1];
			rawargs = rawargs.Substring(0, rawargs.Length - 1);

			var args = rawargs.Split(",").RemoveBlanks();

			// Verify that the construct is ok
			if (!type.IsValidType(constructor))
				throw new CompilationError("Cannot assign {0} to a variable of type {1}", constructor, type.name);


			ShadoObject obj = (ShadoObject)type.GetConstructor().Call(scope, args);
			obj.name = this.name;
			scope.AddVariable(obj);
			return obj;
		}

		private ShadoObject executeGetValueOf() {

			// See if it is a variable in the method scope
			string modifiedExpr = rhs;
			foreach (ShadoObject variable in scope.GetAllVariables()) {
				if (variable.name == rhs)
					return variable;
				else
					modifiedExpr = modifiedExpr.Replace(variable.name, variable.ToString());
			}

			// See if it is a variable in the global scope
			foreach (ShadoObject variable in VM.instance.AllVariables()) {
				if (variable.name == rhs)
					return variable;
				else {
					modifiedExpr = modifiedExpr.Replace(variable.name, variable.ToString());
				}
			}

			// Otherwise see if it is a math expression
			object val = null;
			if (isMathExpression(modifiedExpr, ref val))
				return new ShadoObject(VM.GetSuperType(), val);

			// See if it is a string
			if (rhs.StartsWith("\"") && rhs.EndsWith("\""))
				return new ShadoObject("string", rhs.ReplaceFirstOccurrence("\"", "").ReplaceLastOccurrence("\"", ""));

			// See if it is a char
			if (rhs.StartsWith("'") && rhs.EndsWith("'"))
				return new ShadoObject("char", rhs);

			// Otherwise throw an exception
			throw new RuntimeError("Invalid expression\n\t--> {0}", rhs);
		}

		private bool isMathExpression(string expression, ref object output) {
			try {
				// First replace all the variables with their scope values
				foreach (var variable in scope.GetAllVariables()) {
					expression = expression.Replace(variable.name, variable.value?.ToString());
				}

				// Replace whats left with their global value
				/*foreach (var variable in VM.instance.AllVariables()) {
					expression = expression.Replace(variable.name, variable.ToString());
				}*/

				DataTable dt = new DataTable();
				output = dt.Compute(expression, "");
				return true;
			} catch (Exception) { }
			return false;
		}

		private static bool isFunctionCall(string expression) {
			return new ExpressionSyntax("IDENTIFIER(ANY)").Matches(expression) || new ExpressionSyntax("IDENTIFIER.IDENTIFIER(ANY)").Matches(expression);
		}

		private static void checkVariable(ShadoObject obj) {
			if (obj == null)
				throw new CompilationError("null pointer exception");
		}

		public static List<string> SplitByExceptQuotes(string raw, string splitter) {
			return Regex.Matches(raw, @"[\""].+?[\""]|[^" + splitter + @"]+")
				.Select(m => m.Value)
				.ToList();
		}

		public override string ToString() {
			return $"type: {type}, name: {name}, rhs: {rhs}";
		}
	}
}
