using Cs_Compile_test.com;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.IO;

namespace Cs_Compiler_unit_tests {
	[TestClass]
	public class FileIOTest : BaseTest {

		[TestMethod]
		public void File_Reader_Read_Line() {
			string path = Environment.CurrentDirectory + "\\test3.txt";

			using (StreamWriter writer = new StreamWriter(path)) {
				writer.WriteLine("This is a test line");
				writer.WriteLine("This should be hidden");
				writer.Close();
			}

			string code = TestUtil.MainTemplate(
				$"FileReader reader = new FileReader(\"{path}\");",
							"reader.open();",
							"string line = reader.readLine();",
							"reader.close();");

			TestUtil.CompileAndRun(code);

			ShadoObject line = VM.instance.Get("main").GetVariable("line");

			File.Delete(path);
			Assert.AreEqual("This is a test line", line.ToString());
		}

		[TestMethod]
		public void File_Reader_Read_To_End() {
			string path = Environment.CurrentDirectory + "\\test3.txt";

			using (StreamWriter writer = new StreamWriter(path)) {
				writer.WriteLine("This is a test line");
				writer.WriteLine("another string");
				writer.Close();
			}

			string code = TestUtil.MainTemplate(
				$"FileReader reader = new FileReader(\"{path}\");",
				"reader.open();",
				"string all = reader.readToEnd();",
				"reader.close();");

			TestUtil.CompileAndRun(code);

			ShadoObject line = VM.instance.Get("main").GetVariable("all");

			File.Delete(path);
			Assert.AreEqual("This is a test line\r\nanother string", line.ToString());
		}

		[TestMethod]
		public void File_Reader_Has_Next() {
			string path = Environment.CurrentDirectory + "\\test15.txt";

			using (StreamWriter writer = new StreamWriter(path)) {
				writer.WriteLine("This is a test line");
				writer.Close();
			}

			string code = TestUtil.MainTemplate(
				$"FileReader reader = new FileReader(\"{path}\");",
				"reader.open();",
				"bool a = reader.hasNext();",
				"reader.readLine();",
				"bool b = reader.hasNext();",
				"reader.close();");

			TestUtil.CompileAndRun(code);

			ShadoObject a = VM.instance.Get("main").GetVariable("a");
			ShadoObject b = VM.instance.Get("main").GetVariable("b");

			File.Delete(path);
			Assert.AreEqual("True", a.ToString());
			Assert.AreEqual("False", b.ToString());
		}

		[TestMethod]
		public void File_Reader_Write() {
			string path = Environment.CurrentDirectory + "\\test.txt";
			string code = TestUtil.MainTemplate(
				$"FileWriter reader = new FileWriter(\"{path}\");",
						"reader.open();",
						"reader.write(\"Hello from test!\")",
						"reader.close();"
				);
			TestUtil.CompileAndRun(code);

			// Open that file and see if it has executed
			string filecontent = File.ReadAllText(path);

			// Delete file
			File.Delete(path);

			Assert.AreEqual("Hello from test!", filecontent);
		}

		[TestMethod]
		public void File_Writer_Append_Mode() {

			// Write test data to see if it stays
			string path = Environment.CurrentDirectory + "\\test2.txt";
			using (StreamWriter writer = new StreamWriter(path)) {
				writer.WriteLine("This text should remain");
				writer.Close();
			}

			string code = TestUtil.MainTemplate(
				$"FileWriter writer = new FileWriter(\"{path}\", true);",
				"writer.open();",
				"writer.write(\"some text\");",
				"writer.close();");
			TestUtil.CompileAndRun(code);

			Assert.AreEqual("This text should remain\r\nsome text", File.ReadAllText(path));

			// Delete the file
			File.Delete(path);
		}

		[TestMethod]
		public void File_Writer_Non_Append_Mode() {

			// Write test data to see if it stays
			string path = Environment.CurrentDirectory + "\\test2.txt";
			using (StreamWriter writer = new StreamWriter(path)) {
				writer.WriteLine("This text should go");
				writer.Close();
			}

			string code = TestUtil.MainTemplate(
				$"FileWriter writer = new FileWriter(\"{path}\");",
				"writer.open();",
				"writer.write(\"some text\");",
				"writer.close();");
			TestUtil.CompileAndRun(code);

			Assert.AreEqual("some text", File.ReadAllText(path));

			// Delete the file
			File.Delete(path);
		}
	}
}
