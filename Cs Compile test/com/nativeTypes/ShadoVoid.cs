/**
 *
 */

using System;
using System.Collections.Generic;
using System.Text;

namespace Cs_Compile_test.com.nativeTypes {
	public class ShadoVoid : ShadoClass {
		public ShadoVoid()
			: base("void", new TypeValidator("void", o => true
				)) {}
	}
}
