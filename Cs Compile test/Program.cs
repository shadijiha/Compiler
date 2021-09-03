using Cs_Compile_test.com;
using System;
using System.IO;

namespace Cs_Compile_test {
	public class Program {
		public static string PATH = Directory.GetParent(Environment.CurrentDirectory).Parent.Parent.FullName;
		public static string FULL_FILE_PATH = PATH + "\\test.sscript";

		public static int Main(string[] args) {
			try {

				// Handle a call using args if not in debug
				// Example of args: --filepath FILE_PATH_HERE
				if (args.Length > 0 && args[0].ToLower() == "--filepath") {
					FULL_FILE_PATH = args[1]
						.Replace("\"", "")
						.Trim();
				}

				Compiler compiler =
					new Compiler(FULL_FILE_PATH);
				compiler.compile();

				string result = VM.instance.InvokeMain(args.Length, args);
				return int.Parse(result);

			} catch (Exception e) {
				Console.Error.WriteLine(e.Message);
				return -1;
			} finally {
				VM.instance.Shutdown();
			}

			return 0;
		}
	}
}
