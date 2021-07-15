/**
 *
 */

using Cs_Compile_test.com;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;

namespace Cs_Compile_test {
	public class Compiler {
		private string filecontent;
		public string filename { get; private set; }
		public uint lineNumber { get; set; }

		public Compiler(string filename) {
			if (filename != null) {
				this.filename = filename;
				StringBuilder builder = new StringBuilder();

				using StreamReader reader = new StreamReader(filename);
				while (!reader.EndOfStream) {
					builder.Append(reader.ReadLine()).Append("\n");
				}

				filecontent = builder.ToString();
			} else {
				filename = "";
				filecontent = "";
				lineNumber = 0;
			}
		}

		public void compile() {
			VM.instance.Initialize();

			// Run the preprocessor
			preprocessor();

			// Remove multiline comments
			while (Regex.IsMatch(filecontent, @"\/\*[^*]*\*+([^/][^*]*\*+)*\/"))
				filecontent = Regex.Replace(filecontent, @"\/\*[^*]*\*+([^/][^*]*\*+)*\/", "");

			// Run the class definer
			Parser.ExtractClasses(filecontent);

			// Run the method definer
			Parser.ExtractMethods(filecontent);

		}

		public void preprocessor() {
			string[] lines = filecontent.Split("\n");
			for (uint i = 0; i < lines.Length; i++) {
				lineNumber = i + 1;
				string line = lines[i].Trim();

				// Replace prepressor constant
				foreach (var constant in PreprocessorCommand.constants) {
					line = line.Replace(constant.Key, constant.Value(this)?.ToString() ?? "");
					lines[i] = line;
				}

				if (line.StartsWith("#")) {
					// Extract command name
					string[] tokens = Expression.SplitByExceptQuotes(line.Substring(1), " |	").ToArray();
					PreprocessorCommand command = PreprocessorCommand.Get(tokens[0]);

					lines[i] = command.Execute<string>(tokens.Skip(1).ToArray());

					// Go back to preprocess the file that just got included
					lines = string.Join('\n', lines).Split('\n');
					i = 0;
				}

			}

			filecontent = string.Join('\n', lines);
		}

		public static void compile(string code) {
			Compiler compiler = new Compiler(null);
			compiler.filecontent = code;
			compiler.compile();
		}
	}
}
