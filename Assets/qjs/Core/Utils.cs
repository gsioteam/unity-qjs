
using System;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Threading;
using UnityEditor;
using UnityEngine;

namespace qjs
{
    class Utils
    {
        public static bool IsIntType(Type type)
        {
            return type == typeof(char) ||
                type == typeof(byte) ||
                type == typeof(short) ||
                type == typeof(ushort) ||
                type == typeof(int) ||
                type == typeof(uint) ||
                type == typeof(long) ||
                type == typeof(ulong);
        }

        public static bool IsDoubleType(Type type)
        {
            return type == typeof(float) || type == typeof(double);
        }

        private static bool copyArray<T>(JSValue obj, Type pType, ref object ret)
        {
            if (pType == typeof(T[]))
            {
                uint len = obj.TypedArrayLength();
                var arr = new T[len];
                IntPtr ptr = Marshal.UnsafeAddrOfPinnedArrayElement(arr, 0);
                obj.CopyArrayBuffer(ptr, 0, len);
                ret = arr;
                return true;
            }
            return false;
        }

        public static bool ConvertDelegate(QuickJS quickJS, object obj, Type pType, ref object ret)
        {
            if (obj is JSValue)
            {
                JSValue value = (JSValue)obj;
                return ConvertDelegate(value, pType, ref ret);
            }
            return false;
        }

        public static bool ConvertDelegate(JSValue obj, Type pType, ref object ret)
        {
            if (typeof(Delegate).IsAssignableFrom(pType) && obj.IsFunction())
            {
                var method = pType.GetMethod("Invoke");
                FunctionMaker functionMaker = null;
                if (method.ReturnType == typeof(void))
                {
                    var ps = method.GetParameters();
                    if (ps.Length > 8)
                    {
                        Debug.LogWarning("Not support delegate when arguments count > 8");
                    } else
                    {
                        Type[] types = new Type[ps.Length];
                        for (int i = 0, t = ps.Length; i < t; ++i)
                        {
                            types[i] = ps[i].ParameterType;
                        }
                        switch (ps.Length)
                        {
                            case 0:
                                functionMaker = new FunctionAction()
                                {
                                    target = obj
                                };
                                break;
                            case 1:
                                functionMaker = (FunctionMaker)Activator.CreateInstance(typeof(FunctionAction<>).MakeGenericType(types));
                                functionMaker.target = obj;
                                break;
                            case 2:
                                functionMaker = (FunctionMaker)Activator.CreateInstance(typeof(FunctionAction<,>).MakeGenericType(types));
                                functionMaker.target = obj;
                                break;
                            case 3:
                                functionMaker = (FunctionMaker)Activator.CreateInstance(typeof(FunctionAction<,,>).MakeGenericType(types));
                                functionMaker.target = obj;
                                break;
                            case 4:
                                functionMaker = (FunctionMaker)Activator.CreateInstance(typeof(FunctionAction<,,,>).MakeGenericType(types));
                                functionMaker.target = obj;
                                break;
                            case 5:
                                functionMaker = (FunctionMaker)Activator.CreateInstance(typeof(FunctionAction<,,,,>).MakeGenericType(types));
                                functionMaker.target = obj;
                                break;
                            case 6:
                                functionMaker = (FunctionMaker)Activator.CreateInstance(typeof(FunctionAction<,,,,,>).MakeGenericType(types));
                                functionMaker.target = obj;
                                break;
                            case 7:
                                functionMaker = (FunctionMaker)Activator.CreateInstance(typeof(FunctionAction<,,,,,,>).MakeGenericType(types));
                                functionMaker.target = obj;
                                break;
                            case 8:
                                functionMaker = (FunctionMaker)Activator.CreateInstance(typeof(FunctionAction<,,,,,,,>).MakeGenericType(types));
                                functionMaker.target = obj;
                                break;
                        }
                    }
                } else
                {
                    var ps = method.GetParameters();
                    if (ps.Length > 8)
                    {
                        Debug.LogWarning("Not support delegate when arguments count > 8");
                    }
                    else
                    {
                        Type[] types = new Type[ps.Length + 1];
                        for (int i = 0, t = ps.Length; i < t; ++i)
                        {
                            types[i] = ps[i].ParameterType;
                        }
                        types[ps.Length] = method.ReflectedType;
                        switch (ps.Length)
                        {
                            case 0:
                                functionMaker = (FunctionMaker)Activator.CreateInstance(typeof(FunctionFunc<>).MakeGenericType(types));
                                functionMaker.target = obj;
                                break;
                            case 1:
                                functionMaker = (FunctionMaker)Activator.CreateInstance(typeof(FunctionFunc<,>).MakeGenericType(types));
                                functionMaker.target = obj;
                                break;
                            case 2:
                                functionMaker = (FunctionMaker)Activator.CreateInstance(typeof(FunctionFunc<,,>).MakeGenericType(types));
                                functionMaker.target = obj;
                                break;
                            case 3:
                                functionMaker = (FunctionMaker)Activator.CreateInstance(typeof(FunctionFunc<,,,>).MakeGenericType(types));
                                functionMaker.target = obj;
                                break;
                            case 4:
                                functionMaker = (FunctionMaker)Activator.CreateInstance(typeof(FunctionFunc<,,,,>).MakeGenericType(types));
                                functionMaker.target = obj;
                                break;
                            case 5:
                                functionMaker = (FunctionMaker)Activator.CreateInstance(typeof(FunctionFunc<,,,,,>).MakeGenericType(types));
                                functionMaker.target = obj;
                                break;
                            case 6:
                                functionMaker = (FunctionMaker)Activator.CreateInstance(typeof(FunctionFunc<,,,,,,>).MakeGenericType(types));
                                functionMaker.target = obj;
                                break;
                            case 7:
                                functionMaker = (FunctionMaker)Activator.CreateInstance(typeof(FunctionFunc<,,,,,,,>).MakeGenericType(types));
                                functionMaker.target = obj;
                                break;
                            case 8:
                                functionMaker = (FunctionMaker)Activator.CreateInstance(typeof(FunctionFunc<,,,,,,,,>).MakeGenericType(types));
                                functionMaker.target = obj;
                                break;
                        }
                    }
                }
                if (functionMaker != null)
                {
                    ret = functionMaker.get(pType);
                    return true;
                }
            }
            return false;
        }

        public static object[] CovertArguments(List<JSValue> argv, ParameterInfo[] parameterInfos)
        {
            object[] ret = new object[argv.Count];
            for (int i = 0, t = argv.Count; i < t; ++i)
            {
                var obj = argv[i];
                Type pType = parameterInfos[i].ParameterType;
                if (copyArray<sbyte>(obj, pType, ref ret[i]))
                {
                }
                else if (copyArray<byte>(obj, pType, ref ret[i]))
                {
                }
                else if (copyArray<Int16>(obj, pType, ref ret[i]))
                {
                }
                else if (copyArray<UInt16>(obj, pType, ref ret[i]))
                {
                }
                else if (copyArray<Int32>(obj, pType, ref ret[i]))
                {
                }
                else if (copyArray<UInt32>(obj, pType, ref ret[i]))
                {
                }
                else if (copyArray<Int64>(obj, pType, ref ret[i]))
                {
                }
                else if (copyArray<UInt64>(obj, pType, ref ret[i]))
                {
                }
                else if (copyArray<float>(obj, pType, ref ret[i]))
                {
                }
                else if (copyArray<double>(obj, pType, ref ret[i]))
                {
                }
                else if (ConvertDelegate(obj, pType, ref ret[i]))
                {
                }
                else
                {
                    Type type = obj.Value.GetType();
                    ret[i] = obj.Value;
                    if (type != pType)
                    {
                        ret[i] = Convert.ChangeType(obj.Value, pType);
                    }
                }
            }
            return ret;
        }

        private static bool copyArray<T>(QuickJS quickJS, object obj, Type pType, ref object ret)
        {
            if (pType == typeof(T[]) && obj is JSValue)
            {
                JSValue value = (JSValue)obj;
                uint len = (uint)value.GetLength();
                var arr = new T[len];
                IntPtr ptr = Marshal.UnsafeAddrOfPinnedArrayElement(arr, 0);
                value.CopyArrayBuffer(ptr, 0, len);
                ret = arr;
                return true;
            }
            return false;
        }

        private static bool isTypeMatch(object obj, Type target)
        {
            if (obj == null)
            {
                return target.IsClass;
            }
            Type aType = obj.GetType();
            if (Utils.IsIntType(aType) && Utils.IsIntType(target))
            {
                return true;
            }
            else if ((Utils.IsIntType(aType) || Utils.IsDoubleType(aType)) && Utils.IsDoubleType(target))
            {
                return true;
            }
            else 
                return target.IsAssignableFrom(obj.GetType());
        }

        private static float isJSValueMatch(JSValue value, Type target)
        {
            if (target == typeof(sbyte[]) && value.IsInt8Array())
            {
                return 1;
            }
            else if (target == typeof(byte[]) && value.IsUint8Array())
            {
                return 1;
            }
            else if (target == typeof(Int16[]) && value.IsInt16Array())
            {
                return 1;
            }
            else if (target == typeof(UInt16[]) && value.IsUint16Array())
            {
                return 1;
            }
            else if (target == typeof(Int32[]) && value.IsInt32Array())
            {
                return 1;
            }
            else if (target == typeof(UInt32[]) && value.IsUint32Array())
            {
                return 1;
            }
            else if (target == typeof(Int64[]) && value.IsInt64Array())
            {
                return 1;
            }
            else if (target == typeof(UInt64[]) && value.IsUint64Array())
            {
                return 1;
            }
            else if (target == typeof(float[]) && value.IsFloat32Array())
            {
                return 1;
            }
            else if (target == typeof(double[]) && value.IsFloat64Array())
            {
                return 1;
            }
            else if (typeof(Delegate).IsAssignableFrom(target) && value.IsFunction())
            {
                if (target.GetMethod("Invoke").GetParameters().Length == value.GetLength())
                {
                    return 1;
                } else
                {
                    return 0.5f;
                }
            }
            return 0;
        }

        public static object[] ProcessArguments(QuickJS quickJS, Api.QJS_Item[] argv, int argStart, int argEnd)
        {
            object[] results = new object[argEnd - argStart];
            for (int i = 0, t = results.Length; i < t; ++i)
            {
                results[i] = ProcessObject(quickJS, ref argv[i + argStart]);
            }
            return results;
        }

        public static object ProcessObject(QuickJS quickJS, ref Api.QJS_Item item)
        {
            switch (item.type)
            {
                case Api.ITEM_TYPE_INT:
                    return item.val.i;
                case Api.ITEM_TYPE_LONG:
                    return item.val.l;
                case Api.ITEM_TYPE_DOUBLE:
                    return item.val.d;
                case Api.ITEM_TYPE_BOOL:
                    return item.val.b;
                case Api.ITEM_TYPE_STRING:
                    return Marshal.PtrToStringAuto(item.val.p);
                case Api.ITEM_TYPE_JS_OBJECT:
                    {
                        Instance instance;
                        if (quickJS.instances.items.TryGetValue(item.val.i, out instance))
                        {
                            return instance.target;
                        }
                        break;
                    }
                case Api.ITEM_TYPE_JS_CLASS:
                    {
                        Class clazz = quickJS.instances.classes[item.val.i];
                        return clazz.target;
                    }
                case Api.ITEM_TYPE_JS_VALUE:
                    {
                        return item.ToValue(quickJS);
                    }
                case Api.ITEM_TYPE_JS_STRING:
                    return item.ToString(quickJS);
            }
            return null;
        }

        public static float TestArguments(QuickJS quickJS, MethodBase method, ParameterInfo[] parameters, object[] argv)
        {
            int i = 0;
            float minus = 0;

            if (argv.Length > parameters.Length) return 0;
            for (int l = 0, t = argv.Length; l < t; ++l)
            {
                Type pType = parameters[i].ParameterType;
                var obj = argv[l];

                if (obj is JSValue)
                {
                    float match = isJSValueMatch((JSValue)obj, pType);
                    if (match == 1) ++i;
                    else if (match == 0) return 0;
                    else
                    {
                        ++i;
                        minus += (1 - match);
                    }
                } else
                {
                    bool match = isTypeMatch(obj, pType);
                    if (match)
                        ++i;
                    else return 0;
                }
            }
            for (int t = parameters.Length; i < t; ++i)
            {
                if (!parameters[i].HasDefaultValue) return 0;
            }
            if (parameters.Length == 0) return 1;
            else
                return (i - minus) / parameters.Length;
        }

        public static T FindMethod<T>(QuickJS quickJS, T[] methods, IntPtr indexes, int length, object[] argv) where T : MethodBase
        {
            float max = 0;
            T ret = null;
            ParameterInfo[] parameters = null;
            for (int i = 0; i < length; ++i)
            {
                int idx = Marshal.ReadInt32(indexes, i * 4);
                var method = methods[idx];
                var ps = method.GetParameters();
                float v = TestArguments(quickJS, method, ps, argv);
                if (v == 1)
                {
                    parameters = ps;
                    ret = method;
                    break;
                }
                else if (v > 0)
                {
                    if (v > max)
                    {
                        parameters = ps;
                        ret = method;
                        max = v;
                    }
                }
            }
            if (parameters != null)
            {
                for (int i = 0, t = argv.Length; i < t; ++i)
                {
                    argv[i] = ConvertType(quickJS, argv[i], parameters[i].ParameterType);
                }
            }
            return ret;
        }

        public static object ConvertType(QuickJS quickJS, object obj, Type type)
        {
            object ret = null;
            if (copyArray<sbyte>(quickJS, obj, type, ref ret))
            {
            }
            else if (copyArray<byte>(quickJS, obj, type, ref ret))
            {
            }
            else if (copyArray<Int16>(quickJS, obj, type, ref ret))
            {
            }
            else if (copyArray<UInt16>(quickJS, obj, type, ref ret))
            {
            }
            else if (copyArray<Int32>(quickJS, obj, type, ref ret))
            {
            }
            else if (copyArray<UInt32>(quickJS, obj, type, ref ret))
            {
            }
            else if (copyArray<Int64>(quickJS, obj, type, ref ret))
            {
            }
            else if (copyArray<UInt64>(quickJS, obj, type, ref ret))
            {
            }
            else if (copyArray<float>(quickJS, obj, type, ref ret))
            {
            }
            else if (copyArray<double>(quickJS, obj, type, ref ret))
            {
            }
            else if (ConvertDelegate(quickJS, obj, type, ref ret))
            {
            }
            else
            {
                ret = ConvertType(obj, type);
            }
            return ret;
        }

        public static object ConvertType(JSValue obj, Type type)
        {
            object ret = null;
            if (copyArray<sbyte>(obj, type, ref ret))
            {
            }
            else if (copyArray<byte>(obj, type, ref ret))
            {
            }
            else if (copyArray<Int16>(obj, type, ref ret))
            {
            }
            else if (copyArray<UInt16>(obj, type, ref ret))
            {
            }
            else if (copyArray<Int32>(obj, type, ref ret))
            {
            }
            else if (copyArray<UInt32>(obj, type, ref ret))
            {
            }
            else if (copyArray<Int64>(obj, type, ref ret))
            {
            }
            else if (copyArray<UInt64>(obj, type, ref ret))
            {
            }
            else if (copyArray<float>(obj, type, ref ret))
            {
            }
            else if (copyArray<double>(obj, type, ref ret))
            {
            }
            else if (ConvertDelegate(obj, type, ref ret))
            {
            }
            else
            {
                object val = obj.Value;
                if (val is JSValue)
                {
                    ret = Convert.ChangeType(obj, type);
                } else
                {
                    ret = ConvertType(val, type);
                }
            }
            return ret;
        }

        public static object ConvertType(object obj, Type type)
        {
            if (type.IsAssignableFrom(obj.GetType()))
            {
                return obj;
            }
            else if (obj is JSValue)
            {
                return ConvertType((JSValue)obj, type);
            }
            else
            {
                return Convert.ChangeType(obj, type);
            }
        }

        public static T ConvertType<T>(object obj)
        {
            return (T)ConvertType(obj, typeof(T));
        }
    }
}