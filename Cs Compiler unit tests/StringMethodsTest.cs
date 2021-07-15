using System;
using System.Collections.Generic;
using System.Text;
using Cs_Compile_test.com;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Cs_Compiler_unit_tests {
	[TestClass]
	public class StringMethodsTest: BaseTest {

		[TestMethod]
		public void String_Length() {
			string code = TestUtil.MainTemplate("string s = \"?XD\";", "int l = s.length();");
			TestUtil.CompileAndRun(code);

			ShadoObject s = VM.instance.Get("main").GetVariable("l");

			Assert.AreEqual("3", s.ToString());
		}

		[TestMethod]
		public void String_Chat_At() {
			string code = TestUtil.MainTemplate("string s = \"?XD\";", "char c = s.charAt(0);");
			TestUtil.CompileAndRun(code);

			ShadoObject c = VM.instance.Get("main").GetVariable("c");

			Assert.AreEqual("?", c.ToString());
		}

		[TestMethod]
		public void String_To_Char_Array() {
			string code = TestUtil.MainTemplate("string s = \"?XD\";", "char[] arr = s.toCharArray();");
			TestUtil.CompileAndRun(code);

			ShadoObject arr = VM.instance.Get("main").GetVariable("arr");

			Assert.AreEqual("[?, X, D]", arr.ToString());
		}

		[TestMethod]
		public void String_Concat() {
			string code = TestUtil.MainTemplate("string s = \"?XD\";", "string temp = s.concat(\"Hehexd\");");
			TestUtil.CompileAndRun(code);

			ShadoObject temp = VM.instance.Get("main").GetVariable("temp");

			Assert.AreEqual("?XDHehexd", temp.ToString());
		}

		[TestMethod]
		public void String_Contains() {
			string code = TestUtil.MainTemplate("string s = \"?XD\";", "bool b = s.contains(\"X\")", "bool c = s.contains(\"LULW\");");
			TestUtil.CompileAndRun(code);

			ShadoObject b = VM.instance.Get("main").GetVariable("b");
			ShadoObject c = VM.instance.Get("main").GetVariable("c");

			Assert.AreEqual("True", b.ToString());
			Assert.AreEqual("False", c.ToString());
		}

		[TestMethod]
		public void String_Start_With() {
			string code = TestUtil.MainTemplate("string s = \"?XD\";", "bool b = s.startsWith(\"?\")", "bool c = s.startsWith(\"D\");");
			TestUtil.CompileAndRun(code);

			ShadoObject b = VM.instance.Get("main").GetVariable("b");
			ShadoObject c = VM.instance.Get("main").GetVariable("c");

			Assert.AreEqual("True", b.ToString());
			Assert.AreEqual("False", c.ToString());
		}

		[TestMethod]
		public void String_Start_Ends() {
			string code = TestUtil.MainTemplate("string s = \"?XD\";", "bool b = s.endsWith(\"?\")", "bool c = s.endsWith(\"D\");");
			TestUtil.CompileAndRun(code);

			ShadoObject b = VM.instance.Get("main").GetVariable("b");
			ShadoObject c = VM.instance.Get("main").GetVariable("c");

			Assert.AreEqual("False", b.ToString());
			Assert.AreEqual("True", c.ToString());
		}

		[TestMethod]
		public void String_Equals() {
			string code = TestUtil.MainTemplate("string s = \"?XD\";", "bool b = s.equals(\"?\")", "bool c = s.equals(\"?XD\");");
			TestUtil.CompileAndRun(code);

			ShadoObject b = VM.instance.Get("main").GetVariable("b");
			ShadoObject c = VM.instance.Get("main").GetVariable("c");

			Assert.AreEqual("False", b.ToString());
			Assert.AreEqual("True", c.ToString());
		}

		[TestMethod]
		public void String_Equals_Ignore_Case() {
			string code = TestUtil.MainTemplate("string s = \"?XD\";", "bool b = s.equalsIgnoreCase(\"?XD\")", "bool c = s.equalsIgnoreCase(\"?xd\");");
			TestUtil.CompileAndRun(code);

			ShadoObject b = VM.instance.Get("main").GetVariable("b");
			ShadoObject c = VM.instance.Get("main").GetVariable("c");

			Assert.AreEqual("True", b.ToString());
			Assert.AreEqual("True", c.ToString());
		}

		[TestMethod]
		public void String_To_Lower_Case() {
			string code = TestUtil.MainTemplate("string s = \"?XD\";", "string b = s.toLowerCase();");
			TestUtil.CompileAndRun(code);

			ShadoObject b = VM.instance.Get("main").GetVariable("b");

			Assert.AreEqual("?xd", b.ToString());
		}

		[TestMethod]
		public void String_To_Upper_Case() {
			string code = TestUtil.MainTemplate("string s = \"This is a test\";", "string b = s.toUpperCase();");
			TestUtil.CompileAndRun(code);

			ShadoObject b = VM.instance.Get("main").GetVariable("b");

			Assert.AreEqual("This is a test".ToUpper(), b.ToString());
		}

		[TestMethod]
		public void String_Index_Of() {
			string code = TestUtil.MainTemplate("string s = \"This is a test\";", "int b = s.indexOf(\"i\");");
			TestUtil.CompileAndRun(code);

			ShadoObject b = VM.instance.Get("main").GetVariable("b");

			Assert.AreEqual("2", b.ToString());
		}

		[TestMethod]
		public void String_Is_Empty() {
			string code = TestUtil.MainTemplate("string s = \"\";", "string c = \"H\"", "bool d = s.isEmpty();", "bool e = c.isEmpty();"); 
			TestUtil.CompileAndRun(code);

			ShadoObject d = VM.instance.Get("main").GetVariable("d");
			ShadoObject e = VM.instance.Get("main").GetVariable("e");

			Assert.AreEqual("True", d.ToString());
			Assert.AreEqual("False", e.ToString());
		}

		[TestMethod]
		public void String_Trim() {
			string code = TestUtil.MainTemplate("string s = \"		Hehexd	 \";", "string c = s.trim();");
			TestUtil.CompileAndRun(code);

			ShadoObject c = VM.instance.Get("main").GetVariable("c");

			Assert.AreEqual("Hehexd", c.ToString());
		}

		[TestMethod]
		public void String_Replace() {
			string code = TestUtil.MainTemplate("string s = \"Hehexd\";", "string c = s.replace(\"he\", \"zi\");");
			TestUtil.CompileAndRun(code);

			ShadoObject c = VM.instance.Get("main").GetVariable("c");

			Assert.AreEqual("Hezixd", c.ToString());
		}

		[TestMethod]
		public void String_Split() {
			string code = TestUtil.MainTemplate("string s = \"He he xd\";", "string[] c = s.split(\" \");");
			TestUtil.CompileAndRun(code);

			ShadoObject c = VM.instance.Get("main").GetVariable("c");

			Assert.AreEqual("[He, he, xd]", c.ToString());
		}
	}
}
