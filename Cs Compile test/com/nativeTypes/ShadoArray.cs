using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Cs_Compile_test.com.exceptions;

namespace Cs_Compile_test.com.nativeTypes {
	public class ShadoArray : ShadoClass {

		public ShadoArray(string name): base(name, null) {
			this.validator = new TypeValidator(name, o => {
				var type = this.GetUnitType();
				var list = o as List<object>;
				if (list == null)
					return false;

				foreach (var obj in list) {
					if (!type.IsValid(obj))
						return false;
				}

				return true;
			});
		}

		protected override void initializeMethods() {
			base.initializeMethods();

			ShadoMethod toString = new ShadoMethod("toString", 0, "string");
			toString.SetCode((ctx, args) => {
				StringBuilder builder = new StringBuilder();

				builder.Append("[");

				var list = ctx.value as List<object>;
				if (list == null)
					throw new RuntimeError("Cannot call {0}.toString() on a variable of type {1}", this.name, ctx.type.name);

				int i = 0;
				foreach (var o in list) {
					builder.Append(o);
					if (i < list.Count - 1)
						builder.Append(", ");

					i++;
				}
				builder.Append("]");

				return builder.ToString();
			});
			AddMethod(toString);

			// Add length method
			AddMethod(new ShadoMethod("length", 0, "int").SetCode((ctx, args) => (ctx.value as List<object>).Count));
			AddMethod(new ShadoMethod("at", 1, GetUnitType().name)
				.SetCode((ctx, args) => (ctx.value as List<object>)[int.Parse(args[0].ToString())]));

			AddMethod(new ShadoMethod("add", 1, "void")
				.SetCode((ctx, args) => {

					// If the arg is a ShadoObject then add its value
					var list = ctx.value as List<object>;
					if (list == null)
						throw new RuntimeError(name + " is not a list");

					if (args[0] is ShadoObject shadoobj)
						list.Add(shadoobj.value);
					else
						list.Add(args[0]); // Otherwise

					return null;
				}));

			AddMethod(new ShadoMethod("addAt", 2, "void")
				.SetCode((ctx, args) => {
					(ctx.value as List<object>).Insert(int.Parse(args[0].ToString()), args[1]);
					return true;
				}));

			AddMethod(new ShadoMethod("pop", 0, GetUnitType().name)
				.SetCode((ctx, args) => {
					var list = (ctx.value as List<object>);
					var obj = list.Last();
					list.RemoveAt(list.Count - 1);
					return obj;
				}));

			AddMethod(new ShadoMethod("contains", 1, "bool")
				.SetCode((ctx, args) => {
					var list = ctx.value as List<object>;
					foreach (object o in list) {
						if (o.Equals(args[0]) || o.ToString().Equals(args[0].ToString()))
							return true;
					}

					return false;
				}));
		}

	}
}
