
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using System.Reflection;

namespace UTJ.FrameDebugSave
{
    public class ReflectionCache
    {
        private Dictionary<string, ReflectionType> typeCache = new Dictionary<string, ReflectionType>();

        public ReflectionType GetTypeObject(string fullname)
        {
            ReflectionType type = null;
            if (typeCache.TryGetValue(fullname, out type))
            {
                return type;
            }
            var asms = System.AppDomain.CurrentDomain.GetAssemblies();
            foreach (var asm in asms)
            {
                var t = asm.GetType(fullname);
                if (t != null)
                {
                    type = new ReflectionType(t);
                    typeCache.Add(fullname, type);
                    return type;
                }
            }
            return null;
        }

        public object CreateInstance(System.Type t)
        {
            return System.Activator.CreateInstance(t);
        }
    }
    public class ReflectionType
    {
        public System.Type type;
        public ReflectionType(System.Type t)
        {
            this.type = t;
        }
        public object CreateInstance()
        {
            return System.Activator.CreateInstance(type);
        }


        public FieldInfo GetField( string f)
        {
            var field = this.type.GetField(f, BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.Instance | BindingFlags.Static);
            return field;
        }
        public void SetFieldValue(string f, object obj, object val)
        {
            var field = GetField(f);
            field.SetValue(obj, val);
        }
        public T GetFieldValue<T>( string f, object obj)
        {
            var field = GetField(f);
            return (T)field.GetValue(obj);
        }

        public T CallMethod<T>(string m, object obj, object[] args)
        {
            var method = GetMethodInfo( m);
            T val = (T)method.Invoke(obj, args);
            return val;
        }

        public MethodInfo GetMethodInfo( string m)
        {
            var method = this.type.GetMethod(m, BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.Instance | BindingFlags.Static);
            return method;
        }

        public PropertyInfo GetPropertyInfo( string p)
        {
            var prop = this.type.GetProperty(p, BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.Instance | BindingFlags.Static);
            return prop;
        }

        public void SetPropertyValue(string p, object target, object val)
        {
            var prop = GetPropertyInfo(p);
            prop.SetValue(target, val);
        }
        public T GetPropertyValue<T>( string p, object target)
        {
            var prop = GetPropertyInfo( p);
            return (T)prop.GetValue(target);
        }
    }

    public class ReflectionClassWithObject
    {
        private ReflectionType type;
        private object target;

        public ReflectionClassWithObject(ReflectionType t , object obj)
        {
            this.type = t;
            this.target = obj;
        }
        public T GetPropertyValue<T>( string p)
        {
            return type.GetPropertyValue<T>(p, this.target);
        }
        public T GetFieldValue<T>( string f)
        {
            return type.GetFieldValue<T>(f, this.target);
        }


        public MethodInfo GetMethodInfo(string m)
        {
            return this.type.GetMethodInfo(m);
        }
        public T CallMethod<T>(string m,  object[] args)
        {
            return this.type.CallMethod<T>(m,this.target,args);
        }
    }

}