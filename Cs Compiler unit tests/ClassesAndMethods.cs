using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;

namespace Cs_Compiler_unit_tests {
	[TestClass]
	public class ClassesAndMethods : BaseTest {

		[TestMethod]
		public void Call_On_Public_Method() {

			string code = "class MyTest {\n" +
			"	public void test()	{\n" +
			"		print(\"Hello world!\")\n" +
			"	}\n" +
			"}\n\n" +
			TestUtil.MainTemplate("MyTest test = new MyTest();", "test.test();");

			TestUtil.CompileAndRun(code);

			Assert.AreEqual("Hello world!", console.ToString());
		}

		[TestMethod]
		public void Call_On_Private_Method() {

			string code = "class MyTest {\n" +
						  "	private void test()	{\n" +
						  "		print(\"Hello world!\")\n" +
						  "	}\n" +
						  "}\n\n" +
						  TestUtil.MainTemplate("MyTest test = new MyTest();", "test.test();");

			Assert.ThrowsException<Exception>(() => {
				TestUtil.CompileAndRun(code);
			});
		}

		[TestMethod]
		public void Call_On_Public_Static_Method() {

			string code = "class MyTest {\n" +
						  "	public static void test()	{\n" +
						  "		print(\"Hello world!\")\n" +
						  "	}\n" +
						  "}\n\n" +
						  TestUtil.MainTemplate("MyTest test = new MyTest();", "MyTest.test();");

			TestUtil.CompileAndRun(code);

			Assert.AreEqual("Hello world!", console.ToString());
		}

		[TestMethod]
		public void Call_On_Private_Static_Method() {

			string code = "class MyTest {\n" +
						  "	private static void test()	{\n" +
						  "		print(\"Hello world!\")\n" +
						  "	}\n" +
						  "}\n\n" +
						  TestUtil.MainTemplate("MyTest test = new MyTest();", "MyTest.test();");

			Assert.ThrowsException<Exception>(() => TestUtil.CompileAndRun(code));
		}

	}
}
