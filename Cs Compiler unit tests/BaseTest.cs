using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using Cs_Compile_test.com;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Cs_Compiler_unit_tests {
	public class BaseTest {
		protected StringWriter console;

		[TestInitialize]
		public void Setup() {
			console = new StringWriter();
			Console.SetOut(console);
			Console.SetError(console);
		}

		[TestCleanup]
		public void CleanUp() {
			// Now you have to restore default output stream
			var standardOutput = new StreamWriter(Console.OpenStandardOutput());
			standardOutput.AutoFlush = true;
			Console.SetOut(standardOutput);

			// Clear VM
			VM.instance.Shutdown();
		}
	}
}
