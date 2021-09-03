using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace Cs_Compile_test.com {
	public class PreprocessorCommand {
		public readonly static List<PreprocessorCommand> commands = new List<PreprocessorCommand>();
		public readonly static IDictionary<string, Func<Compiler, object>> constants = new Dictionary<string, Func<Compiler, object>>();

		public string name { get; }
		public uint argCount { get; }
		private Run code;

		public delegate object Run(string[] args);

		public PreprocessorCommand(string name, uint argCount, Run code) {
			this.name = name;
			this.argCount = argCount;
			this.code = code;
		}

		static PreprocessorCommand() {

			var include = new PreprocessorCommand("include", 1,
				filename => {
					// Compile
					new Compiler(filename[0].Trim().Replace("\"", "").Replace("<", "").Replace(">", "")
					).compile();

					return "";
				});
			commands.Add(include);
			
			var define = new PreprocessorCommand("define", 2, args => {
				try {
					constants.Add(args[0], compiler => args.Length >= 2 ? args[1] : "");
				}
				catch (Exception e) {
#if DEBUG
					Console.WriteLine($"[WARNNING]: {args[0]} Macro already exists!");
#endif
				}
				return "";
			});
			commands.Add(define);

			// Constants
			constants.Add("__LINE__", compiler => compiler.lineNumber);
			constants.Add("__FILE__", compiler => compiler.filename);
			constants.Add("__PATH__", compiler => !string.IsNullOrEmpty(compiler.filename) ?
				Directory.GetParent(compiler.filename) : new DirectoryInfo(Environment.CurrentDirectory));
			constants.Add("__PATH_TO_CORE__", compiler => @"D:\Code\Projects\Compiler\Cs Compile test");
		}

		public T Execute<T>(string[] args) {
			return (T)code(args);
		}

		public static PreprocessorCommand Get(string name) {
			return (from command in commands
					where command.name == name
					select command).First();
		}
	}
}
