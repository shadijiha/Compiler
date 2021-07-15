using Cs_Compile_test.com.exceptions;
using System;
using System.Collections.Generic;
using System.Text;

namespace Cs_Compile_test.com {
	public static class MemoryManager {

		private static readonly Dictionary<int, ShadoObject> objects = new Dictionary<int, ShadoObject>();

		public static void AddVariable(ShadoObject obj) {
			objects.Add(obj.GetHashCode(), obj);
		}

		public static ShadoObject GetByAddress(int address) {
			try {
				return objects[address];
			} catch (Exception e) {
				throw new RuntimeError("Cannot find the variable with address {0}", address);
			}
		}

		public static string ToString() {

			StringBuilder builder = new StringBuilder();

			foreach (KeyValuePair<int, ShadoObject> obj in objects) {
				builder.Append(obj.Key).Append("\t")
					.Append(obj.Value?.name).Append(": ").Append(obj.Value?.type?.name)
					.Append("\t=>\t")
					.Append(obj.Value.value).Append("\n");
			}

			return builder.ToString();
		}
	}
}
