using Cs_Compile_test.com.exceptions;
using Cs_Compile_test.com.interfaces;
using Cs_Compile_test.com.nativeTypes;
using System;

namespace Cs_Compile_test.com {
	[Serializable]
	public class ShadoMethod : ShadoObject {

		[Flags]
		public enum Attributes {
			PUBLIC, PRIVATE, PROTECTED, STATIC, NATIVE, C_SHARP
		}

		public delegate object MethodCall(Context context, object[] args);

		protected int argCount;
		protected string returnType;
		protected string[] argTypes;
		[field: NonSerialized]  protected MethodCall code;
		public bool optionalArgs;
		protected string fullType;
		protected Attributes attributes;

		protected bool callBeforeAndAfter = true;

		public ShadoMethod(string name, int argCount, string returnType, bool optionalArgs, string[] argTypes)
			: base("function", name, null) {
			this.argCount = argCount;
			this.returnType = returnType;
			this.argTypes = argTypes;
			this.optionalArgs = optionalArgs;

			// Compute type
			string method_full_type = returnType + "(";
			int i = 1;
			foreach (string rawArg in argTypes) {
				method_full_type += rawArg + (i++ < argCount ? "," : "");
			}
			method_full_type += ")";
			this.fullType = method_full_type;

			//VM.getInstance().addType(method_full_type, o -> true);
			//this.type = VM.getInstance().getClass(method_full_type);

			// Call the constructor if any (this allows you to override the default function constructor)
			try
			{
				var funcClass = (ShadoFunction)VM.instance.GetClass("function");
				ShadoFunction.currentCallee = this;
				var childExtendsFunction = VM.instance.GetChildrenOf(funcClass);
				foreach (var child in childExtendsFunction)
				{
					child.GetConstructor()?.Call(new Context(Global).Attach(this), null);
				}
			}
			catch (Exception e)
            {
            }		
		}

		public ShadoMethod(string name, int argCount, string returnType, string[] argTypes)
			: this(name, argCount, returnType, false, argTypes) {

		}

		public ShadoMethod(String name, int argCount, String returnType)
			: this(name, argCount, returnType, new string[argCount]) {
			for (int i = 0; i < argCount; i++)
				argTypes[i] = "object";
		}

		public ShadoMethod(ShadoMethod o)
			: this(o.name, o.argCount, o.returnType, o.optionalArgs, o.argTypes) {
			code = o.code;
		}

		/// <summary>
		/// Calls the code of the method
		/// </summary>
		/// <param name="context">The object or the method to which the method belogs to</param>
		/// <param name="args">The method arguments</param>
		/// <returns></returns>
		public object Call(Context context, Object[] args) {
			if (context == null)
				throw new RuntimeError("Null pointer exception: Cannot call a method on null");

			int c = args?.Length ?? 0;
			if (!optionalArgs && c != argCount)
				throw new RuntimeError("method arguments mismatch (call for {0})", this.name);

			// Bind args value to names
			for (int i = 0; i < c; i++) {
				string argType = i >= argTypes.Length ? "object" : argTypes[i];
				string name = i >= instanceVariables.Count ? "temp_" + GetHashCode() : instanceVariables[i].name;
				AddOrUpdateVariable(argType, name, args[i]);
			}

			// Call the beforeCall function if exists
			/*if (callBeforeAndAfter) {
				var funcClass = (ShadoFunction)VM.instance.GetClass("function");
				ShadoFunction.currentCallee = this;
				var childExtendsFunction = VM.instance.GetChildrenOf(funcClass);
				foreach (var child in childExtendsFunction)
				{
					var m = child.GetMethod("beforeCall");
					if (m == null)
						continue;
					m.optionalArgs = true;
					m.DisableBeforeAfterCall();
					m.Call(new Context(Global).Attach(this), args);
				}
			}*/


			return code(context, args);
		}

		/// <summary>
		/// Changes the code of the method
		/// </summary>
		/// <param name="code">The method code</param>
		/// <returns></returns>
		public ShadoMethod SetCode(MethodCall code) {
			this.code = code;
			return this;
		}

		/// <summary>
		/// Adds an attribute to the function
		/// </summary>
		/// <param name="attributes">The attribute to add</param>
		/// <returns></returns>
		public ShadoMethod AddAttribute(Attributes attributes) {
			this.attributes |= attributes;
			return this;
		}

		public ShadoMethod DisableBeforeAfterCall() {
			callBeforeAndAfter = false;
			return this;
		}

		/// <summary>
		/// Returns the returns type and all the argument types of the method
		/// </summary>
		/// <returns></returns>
		public String GetFullType() {
			return fullType;
		}

		/// <summary>
		/// Returns if the method complies to the attributes passed
		/// </summary>
		/// <param name="attributes">The attributes passed</param>
		/// <returns></returns>
		public bool Is(Attributes attributes) {
			return (this.attributes & attributes) == attributes;
		}

		/// <summary>
		/// Returns if the object is a method
		/// </summary>
		/// <returns></returns>
		public override bool IsMethod() {
			return true;
		}

		/**
		 * Method is always a pointer
		 * @return
		 */
		public override bool IsPointer() {
			return true;
		}

		/// <summary>
		/// Returns the type of the arguments the method takes
		/// </summary>
		/// <returns></returns>
		public string[] GetArgTypes() {
			return argTypes;
		}

		public int GetArgCount() {
			return argCount;
		}

		public string GetReturnType() { 
			return returnType;
		}

		public bool ArgCountEquals(int count) { 
			return optionalArgs ? true : argCount == count;
		}

		/// <summary>
		/// Compares 2 functions by name and arg count
		/// </summary>
		/// <param name="obj"></param>
		/// <returns></returns>
		public override bool Equals(object obj) {
			if (obj == null || obj.GetType() != GetType())
				return false;
			else if (obj == this)
				return true;
			else {
				var o = (ShadoMethod)obj;
				return name == o.name && argCount == o.argCount;
			}
		}

		public override string ToString() {
			return name;
		}
	}
}
