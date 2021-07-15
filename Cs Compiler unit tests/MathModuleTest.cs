using Cs_Compile_test.com;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;

namespace Cs_Compiler_unit_tests {
	[TestClass]
	public class MathModuleTest : BaseTest {

		[TestMethod]
		public void Min() {
			string code = TestUtil.MainWithCore("int abc = min(6, 2);", "int b = min(1, 6)");
			TestUtil.CompileAndRun(code);

			ShadoObject a = VM.instance.Get("main").GetVariable("abc");
			ShadoObject b = VM.instance.Get("main").GetVariable("b");

			Assert.AreEqual("2", a.ToString());
			Assert.AreEqual("1", b.ToString());
		}

		[TestMethod]
		public void Max() {
			string code = TestUtil.MainWithCore(
				"int r = max(6, 2);",
							"int e = max(1, 6)");
			TestUtil.CompileAndRun(code);

			ShadoObject r = VM.instance.Get("main").GetVariable("r");
			ShadoObject e = VM.instance.Get("main").GetVariable("e");

			Assert.AreEqual("6", r.ToString());
			Assert.AreEqual("6", e.ToString());
		}

		[TestMethod]
		public void Factorial() {
			string code = TestUtil.MainWithCore(
				"int f = factorial(5);");
			TestUtil.CompileAndRun(code);

			ShadoObject f = VM.instance.Get("main").GetVariable("f");

			Assert.AreEqual("120", f.ToString());
		}

		[TestMethod]
		public void Pi() {
			string code = TestUtil.MainWithCore(
				"double p = pi();");
			TestUtil.CompileAndRun(code);

			ShadoObject p = VM.instance.Get("main").GetVariable("p");

			Assert.AreEqual(Math.PI, double.Parse(p.ToString()), 0.0001);
		}

		[TestMethod]
		public void Random_Range() {

			string code = TestUtil.MainWithCore("double r = randomRange(10, 32);");
			TestUtil.CompileAndRun(code);

			ShadoObject r = VM.instance.Get("main").GetVariable("r");

			Assert.That.IsBetweenRange(double.Parse(r.ToString()), 10D, 32D);
		}

		[TestMethod]
		public void Pow() {

			string code = TestUtil.MainWithCore("double r = pow(2, 3);");
			TestUtil.CompileAndRun(code);

			ShadoObject r = VM.instance.Get("main").GetVariable("r");

			Assert.AreEqual(8, double.Parse(r.ToString()));
		}

		[TestMethod]
		public void Sqrt() {

			string code = TestUtil.MainWithCore("double r = sqrt(9);");
			TestUtil.CompileAndRun(code);

			ShadoObject r = VM.instance.Get("main").GetVariable("r");

			Assert.AreEqual(3, double.Parse(r.ToString()));
		}
	}
}
