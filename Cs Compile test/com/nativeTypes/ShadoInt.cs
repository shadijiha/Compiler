/**
 *
 */

using System;
using System.Collections.Generic;
using System.Text;

namespace Cs_Compile_test.com.nativeTypes {
	[System.Serializable]
	public class ShadoInt : ShadoClass {
		public ShadoInt()
			: base("int", new TypeValidator("int", o => {
					try {
						int.Parse(o.ToString());
						return true;
					} catch (Exception) { 
						return false;
					}
				}
				)) {}
	}
}
