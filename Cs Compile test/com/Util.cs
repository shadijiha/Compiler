using Cs_Compile_test.com.exceptions;
using Cs_Compile_test.com.nativeTypes;
using System;
using System.Data;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;

namespace Cs_Compile_test.com {
	public static class Util {

		public static string[] RemoveBlanks(this string[] array) {
			return array.Where(e => !string.IsNullOrWhiteSpace(e)).ToArray();
		}

		public static bool EvaluateBoolExpression(string expression) {
			try {
				DataTable dt = new DataTable();
				var output = dt.Compute(expression, "");
				return bool.Parse(output.ToString());
			} catch (Exception) { }
			return false;
		}

		public static string ReplaceLastOccurrence(this string Source, string Find, string Replace) {
			int place = Source.LastIndexOf(Find);

			if (place == -1)
				return Source;

			string result = Source.Remove(place, Find.Length).Insert(place, Replace);
			return result;
		}

		public static string ReplaceFirstOccurrence(this string Source, string Find, string Replace) {
			var regex = new Regex(Regex.Escape(Find));
			return regex.Replace(Source, Replace, 1);
		}

		public static StreamReader GetReadStream(this ShadoFileReader reader, ShadoObject ctx) {
			StreamReader stream = ctx.GetVariable("stream").value as StreamReader;
			if (stream == null)
				throw new RuntimeError("Could not open read stream to " + ctx.GetVariable("filepath"));
			return stream;
		}

		public static StreamWriter GetWriteStream(this ShadoFileWriter reader, ShadoObject ctx) {
			StreamWriter stream = ctx.GetVariable("stream").value as StreamWriter;
			if (stream == null)
				throw new RuntimeError("Could not open write stream to " + ctx.GetVariable("filepath"));
			return stream;
		}

		public static void Close(this ICloseBeforeExit stream, ShadoObject context) {
			context.type.GetMethod("close").Call(context, null);
		}
	}

	public struct ExecutionStatus {
		public Type status { get; set; }
		public object value { get; set; }

		public ExecutionStatus(Type type = Type.OK, object val = null) {
			this.status = type;
			this.value = val;
		}

		public enum Type {
			OK, RETURN, ABORT, BREAK, CONTINUE
		}
	}

	public static class VMSetup {

		public static void SetupBasicMethods(this VM vm) {
			ShadoMethod method_print = new ShadoMethod("print", 1, "void", true, new string[] { "object" });
			method_print.SetCode((context, obj) => {
				foreach (object e in obj) {
					string o = e?.ToString().Trim() ?? "\"null\"";
					if (o.StartsWith("\"") && o.EndsWith("\"")) {
						o = Regex.Unescape(o.Substring(1,
							o.Length - 2)); // Keep the string literal without the quotes ("")
					} else if (context.HasVariable(o) || vm.Get(o) != null) {
						o = context.GetVariable(o)?.ToString() ??
							VM.instance.Get(o)?.ToString(); // Get the value of the variable if it is not a raw string
					}
					// Otherwise try to find the type of the variable or just print the C# type (Mainly the variable is an R-value)
					else {
						ShadoObject temp = new ShadoObject(vm.GetTypeOf(e), e);
						o = temp.ToString() ?? o;
					}

					Console.Write(o);
				}
				Console.WriteLine();
				return null;
			});
			vm.PushVariable(method_print);

			ShadoMethod method_typeof = new ShadoMethod("typeof", 1, "string", new string[] { "object" });
			method_typeof.SetCode((ctx, obj) =>
				ctx.GetVariable(obj[0].ToString())?.type.name ?? vm.Get(obj[0].ToString())?.type.name ?? vm.GetTypeOf(obj[0])?.name ?? "object");
			vm.PushVariable(method_typeof);

			ShadoMethod method_clear = new ShadoMethod("clear", 0, "void");
			method_clear.SetCode((ctx, obj) => { Console.Clear(); return null; });
			vm.PushVariable(method_clear);

			ShadoMethod method_sleep = new ShadoMethod("sleep", 1, "void");
			method_sleep.SetCode((ctx, obj) => { Thread.Sleep(int.Parse(obj[0].ToString())); return null; });
			vm.PushVariable(method_sleep);
		}

		public static void SetupMathMethods(this VM vm) {
			ShadoMethod method_random = new ShadoMethod("random", 0, "double");
			method_random.SetCode((ctx, obj) => vm.random.NextDouble());
			vm.PushVariable(method_random);

			ShadoMethod method_sqrt = new ShadoMethod("sqrt", 1, "double");
			method_sqrt.SetCode((ctx, obj) => Math.Sqrt(double.Parse(obj[0].ToString())));
			vm.PushVariable(method_sqrt);

			ShadoMethod method_pow = new ShadoMethod("pow", 2, "double");
			method_pow.SetCode((ctx, obj) => Math.Pow(double.Parse(obj[0].ToString()), double.Parse(obj[1].ToString())));
			vm.PushVariable(method_pow);
		}

		public static void SetupInspectMethods(this VM vm) {
			ShadoMethod method_print_types = new ShadoMethod("print_all_types", 0, "void");
			method_print_types.SetCode((ctx, obj) => {
				foreach (var shadoClass in VM.instance.GetAllTypes()) {
					Console.WriteLine("Type:\t{0}", shadoClass);
					foreach (var shadoMethod in shadoClass.GetMethods()) {
						Console.WriteLine("\t\t{0}\t{1}", shadoMethod.name, shadoMethod.GetFullType());
					}
				}
				return null;
			});
			vm.PushVariable(method_print_types);

			ShadoMethod print_user_defined_types = new ShadoMethod("print_user_defined_types", 0, "void");
			print_user_defined_types.SetCode((ctx, obj) => {
				foreach (var shadoClass in VM.instance.GetAllTypes()) {
					if (shadoClass.GetType().Namespace.Contains("native"))
						continue;

					Console.WriteLine("Type:\t{0}", shadoClass);
					foreach (var shadoMethod in shadoClass.GetMethods()) {
						Console.WriteLine("\t\t{0}\t{1}", shadoMethod.name, shadoMethod.GetFullType());
					}
				}
				return null;
			});
			vm.PushVariable(print_user_defined_types);

			ShadoMethod inspect_vm = new ShadoMethod("inspect_vm", 0, "void");
			inspect_vm.SetCode((ctx, obj) => {
				StringBuilder builder = new StringBuilder("VM content:\n");

				foreach (var variable in vm.AllVariables()) {
					// See if it is a method, then get all its inner variable
					builder.Append("\t");
					if (variable is ShadoMethod method) {
						builder.Append(method.name).Append(": ").Append(method.GetFullType());
						foreach (var methodVar in method.GetAllVariables()) {
							builder.Append("\t")
								.Append(methodVar.name).Append("\t=>\t").Append(variable.value ?? "null").Append("\n");
						}
					} else {
						// Otherwise
						builder.Append(variable.name).Append("\t=>\t").Append(variable.value ?? "null");
					}

					builder.Append("\n");
				}

				Console.WriteLine(builder);
				return null;
			});
			vm.PushVariable(inspect_vm);

			ShadoMethod inspect_memory = new ShadoMethod("inspect_memory", 0, "void");
			inspect_memory.SetCode((ctx, obj) => {
				Console.WriteLine(MemoryManager.ToString());
				return null;
			});
			vm.PushVariable(inspect_memory);
		}
	}
}
