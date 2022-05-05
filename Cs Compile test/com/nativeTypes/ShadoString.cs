/**
 *
 */

using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Cs_Compile_test.com.nativeTypes {
	[System.Serializable]
	public class ShadoString : ShadoClass {
		public ShadoString()
			: base("string", new TypeValidator("string", o => true))
		{}

		protected override void initializeMethods() {
			base.initializeMethods();

			methods.Add(new ShadoMethod("length", 0, "int").SetCode((ctx, objects) => ctx.Get(0).value.ToString().Length));
			methods.Add(new ShadoMethod("charAt", 1, "char").SetCode((ctx, args) => ctx.Get(0).value.ToString()[int.Parse(args[0].ToString())]));
			methods.Add(new ShadoMethod("toCharArray", 0, "char[]")
				.SetCode((ctx, args) => ctx.Get(0).value.ToString().ToCharArray().Cast<object>().ToList()));

			methods.Add(new ShadoMethod("concat", 1, "string")
				.SetCode((ctx, args) => ctx.Get(0).value.ToString() + args[0].ToString()));
			methods.Add(new ShadoMethod("contains", 1, "bool")
				.SetCode((ctx, args) => ctx.Get(0).value.ToString().Contains(args[0].ToString())));

			methods.Add(new ShadoMethod("startsWith", 1, "bool")
				.SetCode((ctx, args) => ctx.ToString().StartsWith(args[0].ToString())));
			methods.Add(new ShadoMethod("endsWith", 1, "bool")
				.SetCode((ctx, args) => ctx.ToString().EndsWith(args[0].ToString())));

			methods.Add(new ShadoMethod("equals", 1, "bool")
				.SetCode((ctx, args) => ctx.ToString() == args[0].ToString()));
			methods.Add(new ShadoMethod("equalsIgnoreCase", 1, "bool")
				.SetCode((ctx, args) => ctx.ToString().ToLower() == args[0].ToString().ToLower()));

			methods.Add(new ShadoMethod("toLowerCase", 0, "void")
				.SetCode((ctx, args) => ctx.ToString().ToLower()));
			methods.Add(new ShadoMethod("toUpperCase", 0, "void")
				.SetCode((ctx, args) => ctx.ToString().ToUpper()));

			methods.Add(new ShadoMethod("indexOf", 1, "int")
				.SetCode((ctx, args) => ctx.ToString().IndexOf(args[0].ToString())));

			methods.Add(new ShadoMethod("isEmpty", 0, "bool")
				.SetCode((ctx, args) => ctx.ToString() == ""));

			methods.Add(new ShadoMethod("trim", 0, "string")
				.SetCode((ctx, args) => ctx.ToString().Trim()));

			methods.Add(new ShadoMethod("replace", 2, "string")
				.SetCode((ctx, args) => ctx.ToString().Replace(args[0].ToString(), args[1].ToString())));

			methods.Add(new ShadoMethod("split", 1, "string[]")
				.SetCode((ctx, args) => { return 
					ctx.ToString().Split(args[0].ToString().Replace("\"", "")).Cast<object>().ToList();
				}));
		}
	}
}
