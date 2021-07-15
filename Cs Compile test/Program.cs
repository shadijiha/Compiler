using Cs_Compile_test.com;
using System;
using System.IO;

namespace Cs_Compile_test {
	public class Program {
		public static readonly string PATH = Directory.GetParent(Environment.CurrentDirectory).Parent.Parent.FullName;

		public static void Main(string[] args) {
			try {

				Compiler compiler =
					new Compiler(PATH + "\\test.sscript");
				compiler.compile();
				VM.instance.InvokeMain(args.Length, args);

			} catch (Exception e) {
				Console.WriteLine();
				Console.WriteLine(e.Message);
			} finally {
				VM.instance.Shutdown();
			}
		}
	}
}
