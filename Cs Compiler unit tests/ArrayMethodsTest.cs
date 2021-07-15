using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using Cs_Compile_test.com;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Cs_Compiler_unit_tests {
	[TestClass]
	public class ArrayMethodsTest : BaseTest {

		[TestMethod]
		public void Array_Length() {

			string code = TestUtil.MainTemplate("int[] arr = {1, 2, 3, 4, 5, 6};", "int len = arr.length();");
			TestUtil.CompileAndRun(code);	

			// Get the variable len from VM
			ShadoObject value = VM.instance.Get("main").GetVariable("len");

			Assert.AreEqual("6", value.ToString());
		}

		[TestMethod]
		public void Array_At() {

			// Testing using rvalue and lvalue
			string code = TestUtil.MainTemplate("int[] arr = {1, 2, 3, 4, 5, 6};", "int e = arr.at(2);", "int element = arr.at(e);");
			TestUtil.CompileAndRun(code);

			// Get the variable len from VM
			ShadoObject e = VM.instance.Get("main").GetVariable("e");
			ShadoObject element = VM.instance.Get("main").GetVariable("element");

			Assert.AreEqual("3", e.ToString());
			Assert.AreEqual("4", element.ToString());
		}

		[TestMethod]
		public void Array_Add() {

			// Testing using and rvalue
			string code = TestUtil.MainTemplate("int[] arr = {1, 2, 3, 4, 5, 6};", "arr.add(200);");
			TestUtil.CompileAndRun(code);

			// Get the variable len from VM
			ShadoObject value = VM.instance.Get("main").GetVariable("arr");

			Assert.AreEqual("[1, 2, 3, 4, 5, 6, 200]", value.ToString());
		}

		[TestMethod]
		public void Array_Add_At() {

			// Testing using and rvalue
			string code = TestUtil.MainTemplate("int[] arr = {1, 2, 3, 4, 5, 6};", "arr.addAt(0, 200);");
			TestUtil.CompileAndRun(code);

			// Get the variable len from VM
			ShadoObject value = VM.instance.Get("main").GetVariable("arr");

			Assert.AreEqual("[200, 1, 2, 3, 4, 5, 6]", value.ToString());
		}

		[TestMethod]
		public void Array_Pop() {

			// Testing using and rvalue
			string code = TestUtil.MainTemplate("int[] arr = {1, 2, 3, 4, 5, 6};", "arr.pop();");
			TestUtil.CompileAndRun(code);

			// Get the variable len from VM
			ShadoObject value = VM.instance.Get("main").GetVariable("arr");

			Assert.AreEqual("[1, 2, 3, 4, 5]", value.ToString());
		}

		[TestMethod]
		public void Array_Contains() {

			// Testing using and rvalue
			string code = TestUtil.MainTemplate("int[] arr = {1, 2, 3, 4, 5, 6};", "bool a = arr.contains(3)", "bool b = arr.contains(200)");
			TestUtil.CompileAndRun(code);

			// Get the variable len from VM
			ShadoObject a = VM.instance.Get("main").GetVariable("a");
			ShadoObject b = VM.instance.Get("main").GetVariable("b");

			Assert.AreEqual("True", a.ToString());
			Assert.AreEqual("False", b.ToString());
		}

		[TestMethod]
		public void Array_To_String() {
			// Testing using and rvalue
			string code = TestUtil.MainTemplate("int[] arr = {1, 2, 3, 4, 5, 6};");
			TestUtil.CompileAndRun(code);

			// Get the variable len from VM
			ShadoObject arr = VM.instance.Get("main").GetVariable("arr");

			Assert.AreEqual("[1, 2, 3, 4, 5, 6]", arr.ToString());
		}
	}
}
