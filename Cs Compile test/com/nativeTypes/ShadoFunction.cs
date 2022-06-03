using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Cs_Compile_test.com.nativeTypes
{
    public class ShadoFunction : ShadoClass
    {
		public static ShadoMethod currentCallee = null;
		public ShadoFunction()
			: base("function", new TypeValidator("function", o => o == null || o.GetType() == typeof(ShadoMethod)
				))
			{ }

		protected override void initializeMethods() { 
			base.initializeMethods();

			AddMethod(new ShadoMethod("name", 0, "string").SetCode((ctx, args) => {
				return currentCallee?.name;
			}).DisableBeforeAfterCall());

			AddMethod(new ShadoMethod("args_count", 0, "int").SetCode((ctx, args) => {
				return currentCallee?.GetArgCount();
			}).DisableBeforeAfterCall());

			AddMethod(new ShadoMethod("return_type", 0, "string").SetCode((ctx, args) => {
				return currentCallee?.GetReturnType();
			}).DisableBeforeAfterCall());
		}
	}
}
