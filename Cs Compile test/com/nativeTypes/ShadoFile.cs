using System.IO;

namespace Cs_Compile_test.com.nativeTypes {

	public interface ICloseBeforeExit { }

	public class ShadoFileReader : ShadoClass, ICloseBeforeExit {

		public ShadoFileReader() : base("FileReader", new TypeValidator("FileReader", o => true)) { }

		protected override void initializeMethods() {
			base.initializeMethods();

			ShadoMethod constructor = new ShadoMethod("FileReader", 1, "FileReader");
			constructor.SetCode((ctx, args) => new ShadoObject(this, null).AddOrUpdateVariable("string", "filepath", args[0]));
			AddMethod(constructor);

			ShadoMethod open = new ShadoMethod("open", 0, "void");
			open.SetCode((ctx, args) => {
				string filepath = ctx.GetVariable("filepath").value.ToString().Trim();
				ctx.AddOrUpdateVariable("object", "stream", new StreamReader(filepath));
				return null;
			});
			AddMethod(open);

			ShadoMethod close = new ShadoMethod("close", 0, "void");
			close.SetCode((ctx, args) => {
				this.GetReadStream(ctx).Close();
				return null;
			});
			AddMethod(close);

			ShadoMethod readline = new ShadoMethod("readLine", 0, "string");
			readline.SetCode((ctx, args) => this.GetReadStream(ctx).ReadLine());
			AddMethod(readline);

			ShadoMethod hasnext = new ShadoMethod("hasNext", 0, "bool");
			hasnext.SetCode((ctx, args) => !this.GetReadStream(ctx).EndOfStream);
			AddMethod(hasnext);

			ShadoMethod readToEnd = new ShadoMethod("readToEnd", 0, "string");
			readToEnd.SetCode((ctx, args) => this.GetReadStream(ctx).ReadToEnd());
			AddMethod(readToEnd);
		}
	}

	public class ShadoFileWriter : ShadoClass, ICloseBeforeExit {
		public ShadoFileWriter() : base("FileWriter", new TypeValidator("FileWriter", o => true)) { }

		protected override void initializeMethods() {
			base.initializeMethods();

			ShadoMethod constructor = new ShadoMethod("FileWriter", 2, "FileWriter");
			constructor.optionalArgs = true;
			constructor.SetCode((ctx, args) =>
				new ShadoObject(this, null)
					.AddOrUpdateVariable("string", "filepath", args[0])  // Todo: This much be an evaluated expression in the future
					.AddOrUpdateVariable("bool", "append", args.Length >= 2 ? args[1] : false)
				);
			AddMethod(constructor);

			ShadoMethod open = new ShadoMethod("open", 0, "void");
			open.SetCode((ctx, args) => {
				string filepath = ctx.GetVariable("filepath").value.ToString().Trim();

				bool append = false;
				bool.TryParse(ctx.GetVariable("append").value.ToString().Trim(), out append);

				ctx.AddVariable("object", "stream",
						append ? File.AppendText(filepath) : new StreamWriter(filepath)
						);

				return null;
			});
			AddMethod(open);

			ShadoMethod close = new ShadoMethod("close", 0, "void");
			close.SetCode((ctx, args) => {
				this.GetWriteStream(ctx).Close();
				return null;
			});
			AddMethod(close);

			ShadoMethod write = new ShadoMethod("write", 1, "void");
			write.SetCode((ctx, args) => {
				this.GetWriteStream(ctx).Write(args[0].ToString());
				return null;
			});
			AddMethod(write);

			ShadoMethod writeln = new ShadoMethod("writeln", 1, "void");
			writeln.SetCode((ctx, args) => {
				args[0] = args[0] + "\n";
				ctx.type.GetMethod("write").Call(ctx, args);
				return null;
			});
			AddMethod(writeln);

			ShadoMethod flush = new ShadoMethod("flush", 0, "void");
			flush.SetCode((ctx, args) => {
				this.GetWriteStream(ctx).Flush();
				return null;
			});
			AddMethod(flush);
		}
	}
}
