/**
 *
 */

using System;
using System.Collections.Generic;
using System.Text;

namespace Cs_Compile_test.com.nativeTypes {
	[System.Serializable]
	public class ShadoChar : ShadoClass {
		public ShadoChar()
			: base("char", new TypeValidator("char", o => 
				int.TryParse(o.ToString(), out _) || o.ToString().Replace("'", "").Length <= 1)) {}
	}
}
