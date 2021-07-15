using System;
using System.Collections.Generic;
using System.Text;
using Cs_Compile_test.com;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Cs_Compiler_unit_tests {

	[TestClass]
	public class ObjectPtrTest : BaseTest {

		[TestMethod]
		public void Object_Hash_Code() {
			string code = TestUtil.MainWithCore("object o = new object()", "object* ptr = o.hashCode();"); 
			TestUtil.CompileAndRun(code);

			ShadoObject ptr = VM.instance.Get("main").GetVariable("ptr");

			Assert.IsTrue(int.TryParse(ptr.value.ToString(), out _));
		}

		[TestMethod]
		public void Object_Ptr_Pound_Op() {
			string code = TestUtil.MainWithCore("object o = new object()", "object* ptr = &o;");
			TestUtil.CompileAndRun(code);

			ShadoObject ptr = VM.instance.Get("main").GetVariable("ptr");

			Assert.IsTrue(int.TryParse(ptr.value.ToString(), out _));
		}

		[TestMethod]
		public void Derefrance_With_Star() {
			string code = TestUtil.MainWithCore("int temp = 10;", "int* ptr = &temp;", "*ptr = 20;");
			TestUtil.CompileAndRun(code);

			ShadoObject temp = VM.instance.Get("main").GetVariable("temp");

			Assert.AreEqual("20", temp.value.ToString());
		}
	}
}
