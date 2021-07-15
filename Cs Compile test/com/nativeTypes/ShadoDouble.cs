/**
 *
 */

using System;
using System.Collections.Generic;
using System.Text;

namespace Cs_Compile_test.com.nativeTypes {
	public class ShadoDouble : ShadoClass {
		public ShadoDouble()
			: base("double", new TypeValidator("double", o => {
					try {
						double.Parse(o.ToString());
						return true;
					} catch (Exception) { 
						return false;
					}
				}
				)) {}
	}
}
