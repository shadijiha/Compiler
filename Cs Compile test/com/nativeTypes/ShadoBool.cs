/**
 *
 */

using System;
using System.Collections.Generic;
using System.Text;

namespace Cs_Compile_test.com.nativeTypes {
	public class ShadoBool : ShadoClass {
		public ShadoBool()
			: base("bool", new TypeValidator("bool", o => {
					try {
						bool.Parse(o.ToString());
						return true;
					} catch (Exception) { 
						return false;
					}
				}
				)) {}
	}
}
