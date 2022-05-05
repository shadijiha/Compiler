using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Cs_Compile_test.com.exceptions;

namespace Cs_Compile_test.com {
	[Serializable]
	public class ShadoClass
	{
		protected IList<ShadoClass> parents;
		protected TypeValidator validator;
		[NonSerialized] protected IList<ShadoMethod> methods;
		public string name { get; }

		public ShadoClass(string name, TypeValidator validator) {
			this.name = name;
			parents = new List<ShadoClass>();
			methods = new List<ShadoMethod>();
			this.validator = validator;
			initializeMethods();
		}

		public ShadoClass(string name)
			: this(name, new TypeValidator(name, o => true)) {
			
		}

		public bool IsValid(object value) {
			return validator.validator(value);
		}

		public bool IsValidType(string name) {
			return name == this.name || parents.Where(e => e.IsValidType(name)).FirstOrDefault() != null;
		}

		public ShadoClass GetUnitType() {
			string t = name.Replace("[", "").Replace("]", "").Replace("*", "").Trim();
			return VM.instance.GetClass(t);
		}

		public ShadoMethod GetMethod(string name) {

			ShadoMethod method = methods.FirstOrDefault(e => e.name == name);
			if (method == null) {
				// Check parents
				foreach (ShadoClass parent in parents) {
					method = parent.GetMethod(name);
				}
			}

			// If it is still null then throw exception
			if (method == null)
				throw new CompilationError("Method {0} does not exist on type {1}", name, this.name);
			return method;
		}

		public ShadoMethod[] GetMethods() {
			return methods.ToArray();
		}

		public ShadoMethod GetConstructor(int argCount = 0) {

			foreach(var method in methods)	{
				if (method.name == name && method.GetArgCount() == argCount)
					return method;
			}

			throw new RuntimeError($"Could not get constructor {name} with {argCount} args!");
		}

		public ShadoClass[] GetParentClasses() {
			return parents.ToArray();
		}

		public void AddMethod(ShadoMethod method) {
			// If method already exists we override it
			if (methods.Contains(method)) {
				methods.Remove(method);
			}
			methods.Add(method);
		}

		public void AddParentClass(ShadoClass clzz) {
			if (this.Equals(VM.GetSuperType()) || clzz == null)
				return;

			parents.Add(clzz);
		}

		protected virtual void initializeMethods() {
			var toString = new ShadoMethod("toString", 0, "string");
			toString.SetCode((ctx, objects) => {
				return ctx.Get(0).value;
			});
			AddMethod(toString);

			var hashcode = new ShadoMethod("hashCode", 0, "int");
			hashcode.SetCode((ctx, objects)=> {
				int temp = ctx.GetHashCode();
				return temp;
			});
			AddMethod(hashcode);
		}

		public override string ToString() {
			return name;
		}

		public override bool Equals(object o) {
			if (o == this)
				return true;
			else if (o == null || o.GetType() != GetType())
				return false;
			return name == o.ToString();
		}
	}
}
