using Cs_Compile_test.com.exceptions;
using Cs_Compile_test.com.interfaces;
using Cs_Compile_test.com.nativeTypes;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace Cs_Compile_test.com {
	public sealed class VM {
		public static VM instance = new VM();

		public readonly Random random;
		private IList<ShadoClass> classes;
		private IList<ShadoObject> variables;
		private bool hasInitialized = false;

		private VM() {
			classes = new List<ShadoClass>();
			variables = new List<ShadoObject>();
			random = new Random();
		}

		public void Initialize() {
			if (hasInitialized)
				return;
			hasInitialized = true;

			//AddType("object", o=> true);
			AddType("function", o => o == null || o.GetType() == typeof(ShadoMethod));
			AddType(new ShadoInt());
			AddType(new ShadoFloat());
			AddType(new ShadoDouble());
			AddType(new ShadoBool());
			AddType(new ShadoString());
			AddType(new ShadoChar());
			AddType(new ShadoVoid());
			AddType(new ShadoFileReader());
			AddType(new ShadoFileWriter());


			this.SetupBasicMethods();
			this.SetupMathMethods();
			this.SetupInspectMethods();

			// Setup the dynamic import function
			ShadoMethod import = new ShadoMethod("import", 1,  "object");
			import.SetCode((context, args) => {
				var temp = this;
				VM.instance = new VM();
				Compiler c = new Compiler(args[0].ToString());
				c.compile();

				// Parse file name
				string filename = Path.GetFileName(args[0].ToString()).Split(".")[0];
				string moduleType = $"Module<{filename}>";

				ShadoClass clazz = new ShadoClass(moduleType);
				VM.instance.AddType(clazz);
				temp.classes.Add(clazz); // TODO change this

				ShadoObject returnObj = new ShadoObject(clazz, null);
				foreach (var variable in VM.instance.variables) {
					if (variable is ShadoMethod method)	
						clazz.AddMethod(method);
				}

				return returnObj;
			});
			this.PushVariable(import);
		}

		public void Shutdown() {
			foreach (var variable in variables) {
				// See if it is a method, then get all its inner variable
				if (variable is ShadoMethod method) {
					foreach (var methodVar in method.GetAllVariables()) {
						// If the method is auto closable
						if (methodVar.type is ICloseBeforeExit closable) {
							closable.Close(methodVar);
						}
					}
				} else {
					// Otherwise see if the variable is auto closable
					if (variable.type is ICloseBeforeExit closable) {
						closable.Close(variable);
					}
				}
			}

			classes.Clear();
			variables.Clear();
			hasInitialized = false;
		}

		public string InvokeMain(int argc = 0, string[] argv = null) {
			ShadoMethod main = Get("main") as ShadoMethod;
			if (main == null)
				throw new RuntimeError("Method main not found!");

			main.optionalArgs = true;
			return main.Call((Context)ShadoObject.Global, new object[] { argc, argv.Cast<object>().ToList() })?.ToString() ?? "0";
		}

		public bool IsValidType(ShadoClass clazz, Object value) {
			if (clazz == null)
				throw new RuntimeError("A type does not exist");

			return clazz.IsValid(value) || clazz.name == "object" || clazz.name == "object*";
		}

		public bool HasType(ShadoClass clazz) {
			return classes.Any(e => e.Equals(clazz));
		}

		public bool HasType(string type) {
			return classes.Any(e => e.name == type);
		}

		public void AddType(ShadoClass clazz) {

			// If type already exists
			if (classes.Contains(clazz)) {
				classes.Remove(clazz);
				classes.Remove(new ShadoClass(clazz.name + "*"));
				classes.Remove(classes.Single(e => e.name == clazz.name + "[]"));
			}

			classes.Add(clazz);

			// Add pointer and reference type
			classes.Add(new ShadoClass(clazz.name + "*", new TypeValidator(clazz.name + "*", o => {

				// if o is a variable
				ShadoObject var = VM.instance.Get(o.ToString());
				if (var != null)
					return var.type.name == clazz.name + "*";

				// If o is an integer
				if (int.TryParse(o.ToString(), out _))
					return true;

				// Otherwise
				string rvalue = o.ToString().Trim();
				if (rvalue.StartsWith("&"))
				{
					// Get type of variable without &
					rvalue = rvalue.Substring(1);
					var = VM.instance.Get(rvalue);
					if (var == null)
						throw new RuntimeError("Cannot get the pointer of a null variable");

					return clazz.name == var.type.name;
				}
				
				// Otherwise it is ok
				return true;
			})));


			// Add array type
			classes.Add(new ShadoArray(clazz.name + "[]")); ;
		}

		public void AddType(string type, Predicate<Object> validator) {
			AddType(new ShadoClass(type, new TypeValidator(type, validator)));
		}

		public void PushVariable(ShadoObject obj) {
			if (Get(obj.name) != null)
				throw new CompilationError("variable {0} already exists", obj.name);

			if (!HasType(obj.type))
				throw new CompilationError("type {0} does not exist", obj.type);

			// Verify type compatibility
			bool valid = IsValidType(obj.type, obj.value);
			if (!valid)
				throw new CompilationError("Cannot assign {0} to a variable of type {1}", obj.ToString(), obj.type);

			// Verify if it is a pointer
			/*if (obj.IsPointer()) {
				obj.value = obj.GetHashCode();
			}*/

			variables.Add(obj);
		}

		public ShadoObject Get(string varName) {
			return variables.Where(e => e.name == varName).FirstOrDefault() ?? ShadoObject.Global.GetVariable(varName);
		}

		public ShadoObject GetOrThrow(string varName) {

			ShadoObject obj = Get(varName);
			if (obj != null)
				return obj;

			throw new CompilationError($"Variable {varName} does not exist");
		}

		public ShadoClass GetClass(string name) {
			foreach (ShadoClass klass in classes)
				if (klass.name == name)
					return klass;
			return null;
		}

		public ShadoClass? GetTypeOf(object o) {
			foreach (ShadoClass clazz in GetAllTypes()) {
				if (clazz.IsValid(o) && clazz.name != "object" && clazz.name != "object*" && clazz.name != "object[]")
					return clazz;
			}

			return GetClass("object");
		}

		public static ShadoClass GetSuperType() {
			return instance.GetClass("object");
		}

		public List<ShadoObject> AllVariables() {
			var temp = new List<ShadoObject>(variables);
			temp.AddRange(ShadoObject.Global.GetAllVariables());
			return temp;
		}

		public IList<ShadoClass> GetAllTypes() {
			return new List<ShadoClass>(classes);
		}

		public static VM GetInstance() {
			return instance;
		}
	}
}
