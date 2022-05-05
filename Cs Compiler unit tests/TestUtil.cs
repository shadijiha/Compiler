using Cs_Compile_test;
using Cs_Compile_test.com;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.IO;
using System.Linq;
using System.Text;

namespace Cs_Compiler_unit_tests {

	public static class TestUtil {

		//private static readonly string CORE_LIB_LOCATION = Directory.GetParent(Program.PATH).FullName + "\\Cs Compile test\\core.sscript";
		private static readonly string CORE_LIB_LOCATION = TryGetSolutionDirectoryInfo() + "\\Cs Compile test\\core.sscript";

		public static string MainTemplate(params string[] lines) {
			StringBuilder builder = new StringBuilder("int main()	{");

			foreach (var line in lines) {
				builder.Append("\n\t").Append(line);
			}

			builder.Append("\n}");
			return builder.ToString();
		}

		public static string MainWithCore(params string[] lines) {
			return $"#include \"{CORE_LIB_LOCATION}\"\n\n{MainTemplate(lines)}";
		}

		public static void RunMain(string[] args = null) {
			args ??= new string[0];
			VM.instance.InvokeMain(args.Length, args);
		}

		public static void CompileAndRun(string code, string[] args = null) {
			Compiler.compile(code);
			RunMain(args);
		}

		public static void IsBetweenRange(this Assert assert, double actual, double min, double max) {
			if (actual < min || actual > max)
				throw new Exception($"Input {actual} does not comply with range {min}-{max}");
		}

		public static DirectoryInfo TryGetSolutionDirectoryInfo(string currentPath = null)
		{
			var directory = new DirectoryInfo(
				currentPath ?? Directory.GetCurrentDirectory());
			while (directory != null && !directory.GetFiles("*.sln").Any())
			{
				directory = directory.Parent;
			}
			return directory;
		}
	}
}
