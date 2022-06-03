using System;
using System.Collections.Generic;
using System.Text;

namespace Cs_Compile_test.com.interfaces
{
    public class Context
    {
        private List<ShadoObject> contexts = new List<ShadoObject>();

        public Context(params ShadoObject[] objects) { 
            foreach (var obj in objects) 
                contexts.Add(obj);
        }

        public Context Attach(ShadoObject obj) { 
            contexts.Add(obj);
            return this;
        }

        public ShadoObject Get(int i) {
            if (i < 0 || i >= contexts.Count)
                return null;
            return contexts[i];
        }

        public ShadoObject GetVariable(string name) {
            foreach (var obj in contexts) {
                if (obj.GetVariable(name) != null)
                    return obj.GetVariable(name);
            }
            return null;
        }

        public ShadoObject GetByAddress(int address)
        {
            foreach (var obj in contexts)
            {
                if (obj.GetByAddress(address) != null)
                    return obj.GetByAddress(address);
            }
            return null;
        }

        public ShadoObject AddOrUpdateVariable(string type, string name, object val) {
            return contexts[0].AddOrUpdateVariable(type, name, val);
        }

        public ShadoObject AddVariable(string type, string name, object val)
        {
            return contexts[0].AddVariable(type, name, val);
        }

        public ShadoObject AddVariable(ShadoObject obj)
        {
            return contexts[0].AddVariable(obj);
        }

        public List<ShadoObject> GetAllVariables() {
            var result = new List<ShadoObject>();
            foreach (var obj in contexts)
                result.AddRange(obj.GetAllVariables());
            return result;
        }

        public bool HasVariable(string s) {
            foreach (var obj in contexts)
            {
                if (obj.HasVariable(s))
                    return true;
            }
            return false;
        }

        public bool IsEmpty() { 
            return contexts.Count == 0;
        }
        public int Size()
        {
            return contexts.Count;
        }

        public override string ToString()
        {
            return !IsEmpty() ? Get(0).ToString() : "null";
        }

        public static explicit operator Context(ShadoObject obj)
        {
            return new Context(obj);
        }
        public static explicit operator ShadoObject(Context c) {
            return c.Get(0);
        }
    }
}
