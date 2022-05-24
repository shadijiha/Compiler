using Cs_Compile_test.com.exceptions;
using System;
using System.Collections.Generic;
using System.Text;

namespace Cs_Compile_test.com {
	public static class MemoryManager {

		private static readonly Dictionary<int, WeakReference> objects = new Dictionary<int, WeakReference>();

		public static void AddVariable(ShadoObject obj) {
			if (objects.ContainsKey(obj.GetHashCode()))
				return;
			objects.Add(obj.GetHashCode(), new WeakReference(obj));
		}

		public static ShadoObject GetByAddress(int? address) {
			if (address == null)
				return null;

			try {
				var obj =  objects[(int)address];
				return obj.IsAlive ? (ShadoObject)obj.Target : null;
			} catch (Exception e) {
				throw new RuntimeError("Cannot find the variable with address {0}", address);
			}
		}

		public static string ToString() {

			StringBuilder builder = new StringBuilder();

			foreach (KeyValuePair<int, WeakReference> obj in objects) {
				var val = (obj.Value?.Target as ShadoObject);
				builder.Append(obj.Key).Append("\t")
					.Append(val?.name).Append(": ").Append(val?.type?.name)
					.Append("\t=>\t")
					.Append(val?.value).Append("\n");
			}

			return builder.ToString();
		}
	}
}
