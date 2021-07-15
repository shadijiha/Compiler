using System;
using System.Collections.Generic;
using System.Text;

namespace Cs_Compile_test.com {
	public sealed class TypeValidator {
		public readonly string type;
		public readonly Predicate<object> validator;

		public TypeValidator(string type, Predicate<object> validator) {
			this.type = type;
			this.validator = validator;
		}
	}
}
