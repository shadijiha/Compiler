using Cs_Compile_test.com;
using System;
using System.IO;

namespace Cs_Compile_test {
	public class Program {
		public static string PATH = Directory.GetParent(Environment.CurrentDirectory).Parent.Parent.FullName;
		public static string FULL_FILE_PATH = PATH + "\\test2.sscript";

		public static void Main(string[] args) {
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
				VM.instance.InvokeMain(args.Length, args);

			} catch (Exception e) {
				Console.Error.WriteLine(e.Message);
			} finally {
				VM.instance.Shutdown();
			}
		}
	}
}
