
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
        public ReflectionType GetTypeObject(System.Type t)
        {
            ReflectionType type = null;
            if (typeCache.TryGetValue(t.FullName, out type))
            {
                return type;
            }
            type = new ReflectionType(t);
            typeCache.Add(t.FullName, type);
            return type;
        }

        public object CreateInstance(System.Type t)
        {
            return System.Activator.CreateInstance(t);
        }
    }
    public class ReflectionType
    {

        public class CopyFieldInfo
        {
            public FieldInfo sourceField;
            public FieldInfo destField;
            public CopyFieldInfo(FieldInfo src, FieldInfo dst)
            {
                this.sourceField = src;
                this.destField = dst;
            }
        }

        private System.Type type;
        private Dictionary<string, FieldInfo> cacheCopyFieldInfo;
        private Dictionary<System.Type, List<CopyFieldInfo>> copyDestFieldsByType;
        const BindingFlags copyBindingFlag = BindingFlags.Public | BindingFlags.Instance;

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
        public MethodInfo GetStaticMethodInfo(string m)
        {
            var method = this.type.GetMethod(m, BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.Static);
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

        public System.Type GetRawType()
        {
            return this.type;
        }
        public List<CopyFieldInfo> GetCopyFieldInfoList(System.Type dstType)
        {
            if (copyDestFieldsByType == null)
            {
                copyDestFieldsByType = new Dictionary<System.Type, List<CopyFieldInfo>>();
            }
            List<CopyFieldInfo> copyInfoList = null;
            if (copyDestFieldsByType.TryGetValue(dstType, out copyInfoList))
            {
                return copyInfoList;
            }
            copyInfoList = new List<CopyFieldInfo>();
            var dstFields = dstType.GetFields(copyBindingFlag);
            var ownPublicFields = GetPublicInstanceField();

            foreach (var field in dstFields)
            {
                FieldInfo sourceField = null;

                if (ownPublicFields.TryGetValue(field.Name, out sourceField))
                {
                    var copyInfo = new CopyFieldInfo(sourceField, field);
                    copyInfoList.Add(copyInfo);
                }
            }

            copyDestFieldsByType.Add(dstType, copyInfoList);
            return copyInfoList;
        }

        public Dictionary<string, FieldInfo> GetPublicInstanceField()
        {
            if (cacheCopyFieldInfo != null)
            {
                return cacheCopyFieldInfo;
            }
            var originFields = this.type.GetFields(copyBindingFlag);
            cacheCopyFieldInfo = new Dictionary<string, FieldInfo>(originFields.Length);
            foreach (var field in originFields)
            {
                cacheCopyFieldInfo.Add(field.Name, field);
            }
            return cacheCopyFieldInfo;
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

        public void CopyFieldsToObjectByVarName<T>(ref T dest)
        {
            var copyInfoList = type.GetCopyFieldInfoList(dest.GetType());
            object box = dest;
            foreach ( var info in copyInfoList)
            {
                var obj = info.sourceField.GetValue(this.target);
                info.destField.SetValue(box, obj);
            }
            dest = (T)box;
        }


        public static List<T> CopyToListFromArray<T>(ReflectionCache cache, System.Array arr)
        {
            if( arr == null) { return null; }
            List<T> list = new List<T>(arr.Length);
            if(arr.Length <= 0) { return list; }

            foreach (var obj in arr)
            {
                T dest = (T)System.Activator.CreateInstance(typeof(T));
                var reflectionType = cache.GetTypeObject(obj.GetType());
                var reflectionClassWithObject = new ReflectionClassWithObject(reflectionType, obj);
                reflectionClassWithObject.CopyFieldsToObjectByVarName(ref dest);
                list.Add(dest);
            }
            return list;
        }
    }

}