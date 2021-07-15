/**
 *
 */

using System;
using System.Collections.Generic;
using System.Text;

namespace Cs_Compile_test.com.nativeTypes {
	public class ShadoFloat : ShadoClass {
		public ShadoFloat()
			: base("float", new TypeValidator("float", o => {
					try {
						float.Parse(o.ToString());
						return true;
					} catch (Exception) { 
						return false;
					}
				}
				)) {}
	}
}
