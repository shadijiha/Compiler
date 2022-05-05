using Cs_Compile_test.com.exceptions;
using Cs_Compile_test.com.interfaces;
using System.Collections.Generic;
using System.Linq;

namespace Cs_Compile_test.com {
	[System.Serializable]
	public class ShadoObject {
		public static readonly ShadoObject Global = new ShadoObject("object", "GLOBAL", "GLOBAL");

		public ShadoClass type { get; set; }
		public string name { get; set; }
		private object _value;
		private int id;
		protected IList<ShadoObject> instanceVariables;

		public ShadoObject(ShadoClass clazz, string name, object value) {
			this.type = clazz;
			this.name = name;
			this.value = value;
			// Keep the string literal without the quotes ("")
			if (type != null && type.name == "string")
				this.value = value?.ToString()
					.ReplaceFirstOccurrence("\"", "")
					.ReplaceLastOccurrence("\"", "") ?? "";

			this.id = VM.instance.random.Next(int.MaxValue);
			this.instanceVariables = new List<ShadoObject>();

			MemoryManager.AddVariable(this);
		}

		public ShadoObject(string type, string name, object value)
			: this(VM.GetInstance().GetClass(type), name, value) {
		}

		public ShadoObject(string type, object value)
			: this(type, "", value) {
			name = "temp_" + GetHashCode();
		}

		public ShadoObject(ShadoClass clazz, object value)
			: this(clazz, "", value) {
			name = "temp_" + GetHashCode();
		}

		public object value {
			get => _value;
			set {
				if (value != null && !(type?.IsValid(value) ?? true))
					throw new RuntimeError("Cannot assign {0} to an object of type {1}", value.ToString(), type.name);
				_value = value;
			}
		}

		public ShadoObject AddVariable(ShadoObject obj) {
			instanceVariables.Add(obj);
			return this;
		}

		public ShadoObject AddVariable(string type, string name, object value = null) {
			return AddVariable(new ShadoObject(VM.instance.GetClass(type), name, value));
		}

		public ShadoObject AddOrUpdateVariable(string type, string name, object value = null) {
			var o = GetVariable(name);
			if (o == null)
				AddVariable(type, name, value);
			else
				o.value = value;
			return this;
		}

		public ShadoObject GetVariable(string name) {
			return instanceVariables.Where(e => e.name == name).FirstOrDefault();
		}

		public ShadoObject GetByAddress(int hashCode) {
			var it = from obj in instanceVariables
					 where obj.GetHashCode() == hashCode
					 select obj;

			return it.First();
		}

		public List<ShadoObject> GetAllVariables() {
			return instanceVariables.Where(e => !string.IsNullOrEmpty(e.name)).ToList();
		}

		public bool HasVariable(string name) {
			return GetVariable(name) != null;
		}

		public virtual bool IsMethod() {
			return false;
		}

		public virtual bool IsPointer() {
			return type.name.Trim().EndsWith("*");
		}

		public bool IsReference() {
			return type.name.Trim().EndsWith("&");
		}

		public bool IsValue() {
			return !IsPointer() && !IsReference();
		}

		public override int GetHashCode() {
			return id;
		}

		public override string ToString() {
			return type?.GetMethod("toString")?.Call((Context)this, null)?.ToString() ?? value?.ToString() ?? "undefined@" + id;
		}
	}
}
