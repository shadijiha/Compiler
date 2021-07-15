using Cs_Compile_test.com;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.IO;

namespace Cs_Compiler_unit_tests {
	[TestClass]
	public class ConsoleModuleTest : BaseTest {

		[TestMethod]
		public void Print_Simple_String() {
			string code = TestUtil.MainTemplate("print(\"Hello world!\");");
			TestUtil.CompileAndRun(code);

			Assert.AreEqual("Hello world!\r\n", console.ToString());
		}

		[TestMethod]
		public void Print_String_Variable() {
			string code = TestUtil.MainTemplate("string temp = \"variable!!!\";", "print(temp);");
			TestUtil.CompileAndRun(code);

			Assert.AreEqual("variable!!!\r\n", console.ToString());
		}

		[TestMethod]
		public void Print_Array_Variable() {
			string code = TestUtil.MainTemplate("int[] arr = {1, 2, 3, 4, 5, 6};", "print(arr)");
			TestUtil.CompileAndRun(code);

			Assert.AreEqual("[1, 2, 3, 4, 5, 6]\r\n", console.ToString());
		}

		[TestMethod]
		public void Print_Multible_Args() {
			string code = TestUtil.MainTemplate("print(\"Hehexd\", 123);");
			TestUtil.CompileAndRun(code);

			Assert.AreEqual("Hehexd123\r\n", console.ToString());
		}
	}
}
