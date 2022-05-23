using Cs_Compile_test.com.exceptions;
using Cs_Compile_test.com.nativeTypes;
using System;
using System.Data;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using Cs_Compile_test.com.interfaces;
using System.Collections.Generic;
using System.Runtime.Serialization.Formatters.Binary;

namespace Cs_Compile_test.com {
	public static class Util {

		public static string[] RemoveBlanks(this string[] array) {
			return array.Where(e => !string.IsNullOrWhiteSpace(e)).ToArray();
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
			context.type.GetMethodOrThrow("close").Call((Context)context, null);
		}

		public static string[] Clean(this AbstractExpression expr, string[] lines) {
			return lines.Where(e => !string.IsNullOrEmpty(e) && e.Trim() != "{" && e.Trim() != "}").ToArray();
		}

		public static string getFullIncludePath(string includeStr) {
			string file = getCurrentCompilationFileFromArgs().filepath;
			string coreLib = Directory.GetParent(System.Reflection.Assembly.GetEntryAssembly().Location).Parent.Parent.Parent.FullName;

			// Check if the file is in the same dir as the compling file
			string compiledFilePath = file == null ? Path.GetDirectoryName(includeStr) : Directory.GetParent(file).Name;
			if (File.Exists(compiledFilePath + "/" + includeStr)) {
				return compiledFilePath + "/" + includeStr;
			}
			if (File.Exists(coreLib + "/" + includeStr)) {
				return coreLib + "/" + includeStr;
			}

			return includeStr;
		}

		public static (string filepath, bool output) getCurrentCompilationFileFromArgs(string defaultval = null) {
			// Handle a call using args if not in debug
			// Example of args: --filepath FILE_PATH_HERE
			var args = Environment.GetCommandLineArgs();
			
			string val = defaultval;
			if (args.Length > 1 && args[1].ToLower() == "--filepath") {
				val = args[2]
					.Replace("\"", "")
					.Trim();
			}

			bool output = false;
			foreach (var arg in args)
			{
				if (arg == "--output")
					output = true;
			}

			return (val, output);
		}

		public static T DeepClone<T>(this T obj)
		{
			using (MemoryStream stream = new MemoryStream())
			{
				BinaryFormatter formatter = new BinaryFormatter();
				formatter.Serialize(stream, obj);
				stream.Position = 0;

				return (T)formatter.Deserialize(stream);
			}
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

		private static readonly DateTime Jan1st1970 = new DateTime
			(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc);

		public static void SetupBasicMethods(this VM vm) {
			ShadoMethod method_print = new ShadoMethod("print", 1, "void", true, new string[] { "object" });
			method_print.SetCode((context, obj) => {
				foreach (object e in obj) {
					string o = e?.ToString() ?? "\"null\"";
					if (o.Contains("\"")) {
						Console.WriteLine("Printed string!");
						o = Regex.Unescape(o
							.ReplaceFirstOccurrence("\"", "")
							.ReplaceLastOccurrence("\"", "")); // Keep the string literal without the quotes ("")

					} else if (context.HasVariable(o) || vm.Get(o) != null) {
						o = context.GetVariable(o)?.ToString() ??
							VM.instance.Get(o)?.ToString(); // Get the value of the variable if it is not a raw string
					}
					// Otherwise try to find the type of the variable or just print the C#
					// type (Mainly the variable is an R-value)
					else {
						ShadoObject temp = new ShadoObject(vm.GetTypeOf(e), e);
						o = temp.ToString() ?? o;
					}

					Console.Write(o);
				}
				return null;
			});
			vm.PushVariable(method_print);

			ShadoMethod method_println = new ShadoMethod("println", 1, "void", true, new[] { "object" });
			method_println.SetCode((context, obj) => {
				(vm.Get("print") as ShadoMethod).Call(context, obj);
				Console.WriteLine();
				return null;
			});
			vm.PushVariable(method_println);

			ShadoMethod method_typeof = new ShadoMethod("typeof", 1, "string", new string[] { "object" });
			method_typeof.SetCode((ctx, obj) => (obj[0] as ShadoObject)?.type.name ?? "object");
			vm.PushVariable(method_typeof);

			ShadoMethod method_clear = new ShadoMethod("clear", 0, "void");
			method_clear.SetCode((ctx, obj) => { Console.Clear(); return null; });
			vm.PushVariable(method_clear);

			ShadoMethod method_sleep = new ShadoMethod("sleep", 1, "void");
			method_sleep.SetCode((ctx, obj) => { Thread.Sleep(int.Parse(obj[0].ToString())); return null; });
			vm.PushVariable(method_sleep);

			ShadoMethod method_input = new ShadoMethod("input", 0, "string");
			method_input.SetCode((ctx, obj) => { return Console.ReadLine(); });
			vm.PushVariable(method_input);

			ShadoMethod method_time = new ShadoMethod("time", 0, "int");
			method_time.SetCode((ctx, obj) => VMSetup.CurrentTimeMillis());
			vm.PushVariable(method_time);
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

			ShadoMethod method_floot = new ShadoMethod("floor", 1, "int");
			method_floot.SetCode((ctx, obj) => Math.Floor(double.Parse(obj[0].ToString())));
			vm.PushVariable(method_floot);
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
				string output = "VM content: \n" + variablesToString(vm.AllVariables());
				Console.WriteLine(output);
				return null;
			});
			vm.PushVariable(inspect_vm);

			ShadoMethod inspect_global_scope = new ShadoMethod("inspect_global_scope", 0, "void");
			inspect_global_scope.SetCode((ctx, obj) => {
				string output = "Global scope content: \n" + variablesToString(ShadoObject.Global.GetAllVariables());
				Console.WriteLine(output);
				return null;
			});
			vm.PushVariable(inspect_global_scope);


			ShadoMethod inspect_memory = new ShadoMethod("inspect_memory", 0, "void");
			inspect_memory.SetCode((ctx, obj) => {
				Console.WriteLine(MemoryManager.ToString());
				return null;
			});
			vm.PushVariable(inspect_memory);
		}

		private static long CurrentTimeMillis() {
			return (long)(DateTime.UtcNow - Jan1st1970).TotalMilliseconds;
		}

		private static string variablesToString(IList<ShadoObject> variables) {

			StringBuilder builder = new StringBuilder();

			foreach (var variable in variables)
			{
				// See if it is a method, then get all its inner variable
				builder.Append("\t");
				if (variable is ShadoMethod method)
				{
					builder.Append(method.name).Append(": ").Append(method.GetFullType());
					foreach (var methodVar in method.GetAllVariables())
					{
						builder.Append("\t")
							.Append(methodVar.name).Append("\t=>\t").Append(variable.ToString()).Append("\n");
					}
				}
				else
				{
					// Otherwise
					builder.Append(variable.name).Append("\t=>\t").Append(variable.ToString());
				}

				builder.Append("\n");
			}

			return builder.ToString();
		}
	}
}
