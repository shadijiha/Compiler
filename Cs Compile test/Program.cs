using Cs_Compile_test.com;
using System;
using System.IO;

namespace Cs_Compile_test {
	public class Program {
		public static string PATH = Directory.GetParent(Environment.CurrentDirectory).Parent.Parent.FullName;
		public static string FULL_FILE_PATH = PATH + "\\test2.sscript";

		public static int Main(string[] args) {
			try {
				var options = Util.getCurrentCompilationFileFromArgs(FULL_FILE_PATH);

				Compiler compiler =
					new Compiler(options.filepath, options.output);
				compiler.compile();

				string result = VM.instance.InvokeMain(args.Length, args);
				return int.Parse(result);

			} catch (Exception e) {
#if DEBUG
				Console.Error.WriteLine(e.StackTrace);
#else
				Console.Error.WriteLine(e.Message);
#endif
				return -1;
			} finally {
				VM.instance.Shutdown();
			}

			return 0;
		}
	}
}
