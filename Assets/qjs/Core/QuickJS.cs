using System;
using System.Collections;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using AOT;
using Unity.Collections;
using UnityEngine;

namespace qjs
{
    class Api
    {
#if UNITY_IPHONE
        public const string dll_name = "__Internal";
#else
        public const string dll_name = "qjs";
#endif

        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        public delegate void QJS_PrintHandlerDelegate(int type, string str);
        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        public delegate void QJS_ActionHandlerDelegate(
            int handler,
            IntPtr ctx,
            int class_id,
            IntPtr ids,
            int length,
            int type,
            int argc);
        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        public delegate void QJS_DeleteObjectHandlerDelegate(int handler, int id);
        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        public delegate void QJS_LoadModuleHandlerDelegate(
            int handler,
            IntPtr ctx,
            string module_name);
        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        public delegate void QJS_LoadClassHandlerDelegate(
            int handler,
            IntPtr ctx,
            string class_name);
        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        public delegate IntPtr QJS_ModuleNameHandlerDelegate(int handler,
            IntPtr ctx,
            string base_name,
            string name);

        [StructLayout(LayoutKind.Sequential)]
        public struct QJS_Handles
        {
            public int handler;
            [MarshalAs(UnmanagedType.FunctionPtr)]
            public QJS_PrintHandlerDelegate print;
            [MarshalAs(UnmanagedType.FunctionPtr)]
            public QJS_ActionHandlerDelegate action;
            [MarshalAs(UnmanagedType.FunctionPtr)]
            public QJS_DeleteObjectHandlerDelegate delete_object;
            [MarshalAs(UnmanagedType.FunctionPtr)]
            public QJS_LoadModuleHandlerDelegate load_module;
            [MarshalAs(UnmanagedType.FunctionPtr)]
            public QJS_LoadClassHandlerDelegate load_class;
            [MarshalAs(UnmanagedType.FunctionPtr)]
            public QJS_ModuleNameHandlerDelegate module_name;
        }

        public const int TYPE_NEW_OBJECT = 0;
        public const int TYPE_INVOKE = 1;
        public const int TYPE_GET_FIELD = 2;
        public const int TYPE_SET_FIELD = 3;
        public const int TYPE_GET_PROPERTY = 4;
        public const int TYPE_SET_PROPERTY = 5;
        public const int TYPE_STATIC_INVOKE = 6;
        public const int TYPE_STATIC_GET = 7;
        public const int TYPE_STATIC_SET = 8;
        public const int TYPE_ADD_DFIELD = 9;
        public const int TYPE_GET_DFIELD = 10;
        public const int TYPE_SET_DFIELD = 11;
        public const int TYPE_GENERIC_CLASS = 12;
        public const int TYPE_ARRAY_CLASS = 13;
        public const int TYPE_NEW_ARRAY = 14;
        public const int TYPE_CALL_DELEGATE = 15;
        public const int TYPE_STATIC_PGET = 16;
        public const int TYPE_STATIC_PSET = 17;
        public const int TYPE_CREATE_WORKER = 18;
        public const int TYPE_PROMISE_TRANS = 19;

        [DllImport(dll_name)]
        public static extern IntPtr QJS_RetainInstance(IntPtr ctx, IntPtr ptr);

        [DllImport(dll_name)]
        public static extern IntPtr QJS_RetainClass(IntPtr ctx, IntPtr ptr);

        [DllImport(dll_name)]
        public static extern IntPtr QJS_RetainValue(IntPtr ctx, IntPtr ptr, ref int tras_id, ref int sharp_type);

        [DllImport(dll_name)]
        public static extern void QJS_ReleaseJSValue(IntPtr ctx, IntPtr ptr);

        [DllImport(dll_name, CallingConvention = CallingConvention.Cdecl)]
        public static extern IntPtr QJS_Setup(QJS_Handles handlers, IntPtr argumentsPtr, IntPtr resultsPtr);

        [DllImport(dll_name)]
        public static extern void QJS_Shutdown(IntPtr runtime);

        [DllImport(dll_name)]
        public static extern IntPtr QJS_NewContext(IntPtr runtime);

        [DllImport(dll_name)]
        public static extern IntPtr QJS_RegisterClass(IntPtr ctx, string name, int id);

        [DllImport(dll_name)]
        public static extern void QJS_SubmitClass(IntPtr ctx, IntPtr clazz);

        [DllImport(dll_name)]
        public static extern void QJS_AddConstructor(IntPtr clazz, int id, int argv_min, int argv_max);

        [DllImport(dll_name)]
        public static extern void QJS_AddField(IntPtr clazz, string name, int id, bool is_static);

        [DllImport(dll_name)]
        public static extern void QJS_AddFunction(IntPtr clazz, string name, int id, bool is_static, int argv_min, int argv_max);

        [DllImport(dll_name)]
        public static extern void QJS_AddProperty(IntPtr clazz, string name, int id, int mask);

        [DllImport(dll_name)]
        public static extern void QJS_AddEnum(IntPtr clazz, string name, int value);

        [DllImport(dll_name)]
        public static extern IntPtr QJS_NewInstance(IntPtr ctx, IntPtr clazz, int id);

        [DllImport(dll_name)]
        public static extern IntPtr QJS_NewFunction(IntPtr ctx, IntPtr clazz, int id);

        [DllImport(dll_name)]
        public static extern void QJS_DeleteContext(IntPtr ctx);

        [DllImport(dll_name)]
        public static extern IntPtr QJS_ToStringPtr(IntPtr ctx, IntPtr val);

        [DllImport(dll_name)]
        public static extern void QJS_FreeStringPtr(IntPtr ctx, IntPtr ptr);

        [DllImport(dll_name)]
        public static extern void QJS_ClearResults(IntPtr ctx);

        [DllImport(dll_name)]
        public static extern int QJS_Action(IntPtr ctx, int type, int argc);

        [DllImport(dll_name)]
        public static extern void QJS_Eval(
            IntPtr ctx,
            string code,
            string filename);

        [DllImport(dll_name)]
        public static extern void QJS_Load(
            IntPtr ctx,
            string code,
            string filename);

        [DllImport(dll_name)]
        public static extern uint QJS_NewAtom(IntPtr ctx, string str);

        [DllImport(dll_name)]
        public static extern void QJS_FreeAtom(IntPtr ctx, uint atom);

        [DllImport(dll_name)]
        public static extern void QJS_ToString(IntPtr ctx, IntPtr value, StringBuilder output);

        [DllImport(dll_name)]
        public static extern bool QJS_Equals(IntPtr value1, IntPtr value2);

        public const int SHARP_CALL = 0;
        public const int SHARP_GET = 1;
        public const int SHARP_SET = 2;
        public const int SHARP_CALL_ATOM = 3;
        public const int SHARP_GET_ATOM = 4;
        public const int SHARP_SET_ATOM = 5;
        public const int SHARP_INVOKE = 6;
        public const int SHARP_NEW = 7;
        public const int SHARP_NEW_ARRAY = 8;
        public const int SHARP_NEW_OBJECT = 9;
        public const int SHARP_NEW_BIND = 10;
        public const int SHARP_GET_INDEX = 11;
        public const int SHARP_SET_INDEX = 12;
        public const int SHARP_PROMISE_COMPLETE = 13;

        [DllImport(dll_name)]
        public static extern bool QJS_IsInt8Array(IntPtr ctx, IntPtr ptr);

        [DllImport(dll_name)]
        public static extern bool QJS_IsUint8Array(IntPtr ctx, IntPtr ptr);

        [DllImport(dll_name)]
        public static extern bool QJS_IsInt16Array(IntPtr ctx, IntPtr ptr);

        [DllImport(dll_name)]
        public static extern bool QJS_IsUint16Array(IntPtr ctx, IntPtr ptr);

        [DllImport(dll_name)]
        public static extern bool QJS_IsInt32Array(IntPtr ctx, IntPtr ptr);

        [DllImport(dll_name)]
        public static extern bool QJS_IsUint32Array(IntPtr ctx, IntPtr ptr);

        [DllImport(dll_name)]
        public static extern bool QJS_IsInt64Array(IntPtr ctx, IntPtr ptr);

        [DllImport(dll_name)]
        public static extern bool QJS_IsUint64Array(IntPtr ctx, IntPtr ptr);

        [DllImport(dll_name)]
        public static extern bool QJS_IsFloat32Array(IntPtr ctx, IntPtr ptr);

        [DllImport(dll_name)]
        public static extern bool QJS_IsFloat64Array(IntPtr ctx, IntPtr ptr);

        [DllImport(dll_name)]
        public static extern bool QJS_IsFunction(IntPtr ctx, IntPtr ptr);

        [DllImport(dll_name)]
        public static extern bool QJS_IsFunctionPtr(IntPtr ctx, IntPtr ptr);

        [DllImport(dll_name)]
        public static extern uint QJS_GetTypedArrayLength(IntPtr ctx, IntPtr ptr);

        [DllImport(dll_name)]
        public static extern bool QJS_CopyArrayBuffer(IntPtr ctx, IntPtr ptr, IntPtr dis, uint offset, uint length);

        [DllImport(dll_name)]
        public static extern int QJS_GetFunctionLength(IntPtr ctx, IntPtr ptr);

        [DllImport(dll_name)]
        public static extern int QJS_GetFunctionLengthPtr(IntPtr ctx, IntPtr ptr);

        [DllImport(dll_name)]
        public static extern void QJS_NewPromise(IntPtr ctx, int promise);

        [DllImport(dll_name)]
        public static extern void QJS_Step(IntPtr ctx);

        [DllImport(dll_name)]
        public static extern IntPtr QJS_Malloc(IntPtr ctx, int size);

        public const UInt16 ITEM_TYPE_INT = 0;
        public const UInt16 ITEM_TYPE_LONG = 1;
        public const UInt16 ITEM_TYPE_DOUBLE = 2;
        public const UInt16 ITEM_TYPE_BOOL = 3;
        public const UInt16 ITEM_TYPE_STRING = 4;
        // QJS_Instance ptr
        public const UInt16 ITEM_TYPE_OBJECT = 5;
        // QJS_Class ptr
        public const UInt16 ITEM_TYPE_CLASS = 6;
        // QJS_Value ptr
        public const UInt16 ITEM_TYPE_VALUE = 7;
        public const UInt16 ITEM_TYPE_PROMISE = 8;
        public const UInt16 ITEM_TYPE_NULL = 9;
        // instance id
        public const UInt16 ITEM_TYPE_JS_OBJECT = 10;
        // class id
        public const UInt16 ITEM_TYPE_JS_CLASS = 11;
        // raw ptr
        public const UInt16 ITEM_TYPE_JS_VALUE = 12;
        // raw ptr
        public const UInt16 ITEM_TYPE_JS_STRING = 13;
        
        [StructLayout(LayoutKind.Sequential)]
        public struct QJS_Item
        {
            [StructLayout(LayoutKind.Explicit)]
            public struct Union
            {
                [FieldOffset(0)]
                public Int32 i;
                [FieldOffset(0)]
                public Int64 l;
                [FieldOffset(0)]
                public Double d;
                [FieldOffset(0)]
                public Boolean b;
                [FieldOffset(0)]
                public IntPtr p;
            }

            public UInt16 type;
            public Union val;

            public void SetNull()
            {
                type = Api.ITEM_TYPE_NULL;
            }
            public void SetItem(int v)
            {
                type = Api.ITEM_TYPE_INT;
                val.i = v;
            }
            public void SetItem(long v)
            {
                type = Api.ITEM_TYPE_LONG;
                val.l = v;
            }
            public void SetItem(double v)
            {
                type = Api.ITEM_TYPE_DOUBLE;
                val.d = v;
            }
            public void SetItem(bool v)
            {
                type = Api.ITEM_TYPE_BOOL;
                val.b = v;
            }
            public void SetItem(JSValue v, Stack<IntPtr> cache)
            {
                switch (v.Type)
                {
                    case JSValueType.Int:
                        {
                            type = Api.ITEM_TYPE_INT;
                            val.i = (int)v.Value;
                            break;
                        }
                    case JSValueType.Long:
                        {
                            type = Api.ITEM_TYPE_LONG;
                            val.l = (long)v.Value;
                            break;
                        }
                    case JSValueType.Double:
                        {
                            type = Api.ITEM_TYPE_DOUBLE;
                            val.d = (double)v.Value;
                            break;
                        }
                    case JSValueType.Bool:
                        {
                            type = Api.ITEM_TYPE_BOOL;
                            val.b = (bool)v.Value;
                            break;
                        }
                    case JSValueType.String:
                        {
                            type = Api.ITEM_TYPE_STRING;
                            val.p = Marshal.StringToHGlobalAuto((string)v.Value);
                            cache.Push(val.p);
                            break;
                        }
                    case JSValueType.Object:
                        {
                            type = Api.ITEM_TYPE_OBJECT;
                            val.p = v.GetObject().ptr;
                            break;
                        }
                    case JSValueType.Class:
                        {
                            type = Api.ITEM_TYPE_CLASS;
                            val.p = v.GetClass().ptr;
                            break;
                        }
                    case JSValueType.JSObject:
                        {
                            type = Api.ITEM_TYPE_VALUE;
                            val.p = v.GetPtr();
                            break;
                        }
                    default:
                        {
                            type = Api.ITEM_TYPE_NULL;
                            break;
                        }
                }
            }
            public void SetItem(string v, Stack<IntPtr> cache)
            {
                type = Api.ITEM_TYPE_STRING;
                val.p = Marshal.StringToHGlobalAuto(v);
                cache.Push(val.p);
            }

            public void SetPromise(int v)
            {
                type = Api.ITEM_TYPE_PROMISE;
                val.i = v;
            }

            public void SetValue(IntPtr v)
            {
                type = Api.ITEM_TYPE_VALUE;
                val.p = v;
            }

            public void SetItem(Class clazz)
            {
                type = Api.ITEM_TYPE_CLASS;
                val.p = clazz.ptr;
            }

            public string ToString(QuickJS quickJS)
            {
                if (type == ITEM_TYPE_JS_STRING)
                {
                    IntPtr ptr = QJS_ToStringPtr(quickJS.ctx, val.p);
                    string ret = Marshal.PtrToStringAnsi(ptr);
                    QJS_FreeStringPtr(quickJS.ctx, ptr);
                    return ret;
                } else if (type == ITEM_TYPE_STRING)
                {
                    return Marshal.PtrToStringAnsi(val.p);
                } else
                {
                    return null;
                }
            }

            public JSValue ToValue(QuickJS quickJS)
            {
                switch (type)
                {
                    case Api.ITEM_TYPE_INT:
                        return new JSValue(val.i);
                    case Api.ITEM_TYPE_LONG:
                        return new JSValue(val.l);
                    case Api.ITEM_TYPE_DOUBLE:
                        return new JSValue(val.d);
                    case Api.ITEM_TYPE_BOOL:
                        return val.b ? JSValue.True : JSValue.False;
                    case Api.ITEM_TYPE_STRING:
                        return new JSValue(Marshal.PtrToStringAnsi(val.p));
                    case Api.ITEM_TYPE_JS_OBJECT:
                        {
                            Instance instance;
                            if (quickJS.instances.items.TryGetValue(val.i, out instance))
                            {
                                return new JSValue(quickJS, QJS_RetainInstance(quickJS.ctx, instance.ptr), instance);
                            }
                            else
                            {
                                return JSValue.Zero;
                            }
                        }
                    case Api.ITEM_TYPE_JS_CLASS:
                        {
                            if (quickJS.instances.classes.Count > val.i)
                            {
                                var clazz = quickJS.instances.classes[val.i];
                                return new JSValue(quickJS, QJS_RetainClass(quickJS.ctx, clazz.ptr), clazz);
                            }
                            else
                            {
                                return JSValue.Zero;
                            }
                        }
                    case Api.ITEM_TYPE_JS_VALUE:
                        {
                            int id = 0;
                            int type = 0;
                            IntPtr ptr = QJS_RetainValue(quickJS.ctx, val.p, ref id, ref type);
                            switch (type)
                            {
                                case 1:
                                    {
                                        Instance instance;
                                        if (quickJS.instances.items.TryGetValue(id, out instance))
                                        {
                                            return new JSValue(quickJS, ptr, instance);
                                        }
                                        else
                                        {
                                            return new JSValue(quickJS, ptr);
                                        }
                                    }
                                case 2:
                                    {
                                        if (quickJS.instances.classes.Count > id)
                                        {
                                            return new JSValue(quickJS, ptr, quickJS.instances.classes[id]);
                                        } else
                                        {
                                            return new JSValue(quickJS, ptr);
                                        }
                                    }
                                default:
                                    {
                                        return new JSValue(quickJS, ptr);
                                    }
                            }
                        }
                    case Api.ITEM_TYPE_JS_STRING:
                        return new JSValue(ToString(quickJS));
                    default:
                        return JSValue.Zero;
                }
            }

            public void SetAny(QuickJS quickJS, object obj, Stack<IntPtr> cache)
            {
                Instance instance;
                var ctx = quickJS.ctx;
                var instances = quickJS.instances;
                Type oType = obj == null ? null : obj.GetType();
                if (obj == null)
                {
                    type = ITEM_TYPE_NULL;
                }
                //else if (obj is YieldInstruction)
                //{
                //    var beh = quickJS.instances.currentBehaviour;
                //    if (beh == null)
                //    {
                //        Debug.LogWarning("No current behaviour, can not make Promise");
                //        type = ITEM_TYPE_NULL;
                //    }
                //    else
                //    {
                //        type = ITEM_TYPE_PROMISE;
                //        val.i = QuickJS.newPromise(beh, quickJS, obj as YieldInstruction);
                //    }
                //}
                else if (Utils.IsIntType(oType))
                {
                    if (oType == typeof(long) || oType == typeof(ulong))
                    {
                        type = ITEM_TYPE_LONG;
                        val.l = Convert.ToInt64(obj);
                    }
                    else
                    {
                        type = ITEM_TYPE_INT;
                        val.i = Convert.ToInt32(obj);
                    }
                }
                else if (Utils.IsDoubleType(oType))
                {
                    type = ITEM_TYPE_DOUBLE;
                    val.d = Convert.ToDouble(obj);
                }
                else if (oType == typeof(bool))
                {
                    type = ITEM_TYPE_BOOL;
                    val.b = Convert.ToBoolean(obj);
                }
                else if (oType == typeof(string))
                {
                    type = ITEM_TYPE_STRING;
                    val.p = Marshal.StringToHGlobalAuto((string)obj);
                    cache.Push(val.p);

                }
                else if (instances.index.TryGetValue(obj, out instance))
                {
                    type = ITEM_TYPE_OBJECT;
                    val.p = instance.ptr;
                }
                else if (obj is Delegate)
                {
                    Type delegateType = typeof(Delegate);
                    Class clazz;
                    if (instances.classesIndex.TryGetValue(delegateType, out clazz))
                    {
                        type = ITEM_TYPE_OBJECT;
                        val.p = QuickJS.createFunction(instances, ctx, clazz, obj as Delegate);
                    }
                    else
                    {
                        Debug.LogWarning("Can not make function because Delegate is not registered.");
                        type = ITEM_TYPE_NULL;
                    }
                }
                else if (oType == typeof(Instance))
                {
                    var ins = obj as Instance;
                    type = ITEM_TYPE_OBJECT;
                    val.p = ins.ptr;
                }
                else if (oType == typeof(JSValue))
                {
                    JSValue jsValue = obj as JSValue;
                    if (jsValue.Type >= JSValueType.ObjectIndex)
                    {
                        type = ITEM_TYPE_VALUE;
                        val.p = jsValue.GetPtr();
                    }
                    else
                        SetItem(jsValue, cache);
                }
                else
                {
                    Class clazz;
                    if (instances.classesIndex.TryGetValue(oType, out clazz))
                    {
                        type = ITEM_TYPE_OBJECT;
                        val.p = QuickJS.createObject(instances, ctx, clazz, obj);
                    }
                    else
                    {
                        if (!oType.IsGenericType)
                            Debug.LogWarning(string.Format("{0} is registered dynamically, which would be incomplete after public.", oType));
                        clazz = QuickJS.RegisterClass(instances, ctx, oType);
                        type = ITEM_TYPE_OBJECT;
                        val.p = QuickJS.createObject(instances, ctx, clazz, obj);
                    }
                }
            }
        }
    }

    public interface RequireLoader
    {
        // Load content of filename
        string Load(string filename);
        // Test if file exist
        bool Test(string filename);
    }

    public class QuickJS
    {
        static QuickJS instance;
        public static QuickJS Instance
        {
            get
            {
                if (instance == null)
                {
                    instance = new QuickJS(true);
                }
                return instance;
            }
        }

        internal IntPtr ctx;

        private bool disabled = false;

        public bool Disabled {
            get
            {
                return disabled;
            }
        }

        private IntPtr runtime;

        private RequireLoader requireLoader;
        public RequireLoader Loader {
            get
            {
                return requireLoader;
            }
            set
            {
                requireLoader = value;
            }
        }

        internal class Instances
        {

            class ArgumentsHeap
            {
                public List<object> arguments = new List<object>();
                public MonoBehaviour behaviour;

                public void Clear()
                {
                    arguments.Clear();
                    behaviour = null;
                }
            }

            public WeakReference<QuickJS> reference;

            public int handler;
            public int instanceCount = 0;

            public int promiseCount = 0;

            public Dictionary<int, Instance> items = new Dictionary<int, Instance>();
            public Dictionary<object, Instance> index = new Dictionary<object, Instance>();

            public Dictionary<Type, Class> classesIndex = new Dictionary<Type, Class>();
            public List<Class> classes = new List<Class>();

            public Api.QJS_Item[] arguments = new Api.QJS_Item[16];
            public IntPtr argumentsPtr;
            public Api.QJS_Item[] results = new Api.QJS_Item[8];
            public IntPtr resultsPtr;

            public Stack<IntPtr> argumentCache = new Stack<IntPtr>(8);
            public Stack<IntPtr> resultCache = new Stack<IntPtr>(4);

            public Instances()
            {
                argumentsPtr = Marshal.UnsafeAddrOfPinnedArrayElement(arguments, 0);
                resultsPtr = Marshal.UnsafeAddrOfPinnedArrayElement(results, 0);
            }

            public MonoBehaviour currentBehaviour;

            
        }

        internal Instances instances;
        private static ConcurrentDictionary<int, Instances> insCache = new ConcurrentDictionary<int, Instances>();
        private static int handlerCount = 0;

        private bool needUpdate = false;

        public QuickJS(bool full = false)
        {
            needUpdate = full;
            instances = new Instances();

            lock (instances)
            {
                Api.QJS_Handles handles = new Api.QJS_Handles();
                handles.print = _print;
                handles.action = _action;
                handles.delete_object = _deleteObject;
                handles.load_module = _loadModule;
                handles.load_class = _loadClass;
                handles.module_name = _moduleName;
                lock (insCache)
                {
                    handles.handler = handlerCount++;
                }
                runtime = Api.QJS_Setup(handles, instances.argumentsPtr, instances.resultsPtr);
                instances.reference = new WeakReference<QuickJS>(this);
                instances.handler = handles.handler;
                insCache[handles.handler] = instances;
                ctx = Api.QJS_NewContext(runtime);
            }
            if (full)
            {
                RegisterClass<FieldType>();
                RegisterClass<BoundsInt>();
                RegisterClass<Bounds>();
                RegisterClass<Color>();
                RegisterClass<Quaternion>();
                RegisterClass<RectInt>();
                RegisterClass<Rect>();
                RegisterClass<Vector2Int>();
                RegisterClass<Vector2>();
                RegisterClass<Vector3Int>();
                RegisterClass<Vector3>();
                RegisterClass<Vector4>();
                RegisterClass<Matrix4x4>();

                RegisterClass<sbyte>();
                RegisterClass<byte>();
                RegisterClass<Int16>();
                RegisterClass<UInt16>();
                RegisterClass<Int32>();
                RegisterClass<UInt32>();
                RegisterClass<Int64>();
                RegisterClass<UInt64>();
                RegisterClass<float>();
                RegisterClass<double>();

                RegisterClass(typeof(List<>));
                RegisterClass(typeof(Dictionary<,>));
                RegisterClass<Delegate>();
                RegisterClass<Worker>();
                RegisterClass<MonoBehaviour>();
                RegisterClass<Transform>();
                DefaultConfigure.Register();
            }
        }

        ~QuickJS()
        {
            lock(instances)
            {
                destroy();
            }
        }

        private void destroy()
        {
            if (!disabled)
            {
                Api.QJS_DeleteContext(ctx);
                disabled = true;
                Instances ins;
                insCache.TryRemove(instances.handler, out ins);
                Api.QJS_Shutdown(runtime);
                runtime = IntPtr.Zero;
            }
        }

        public void Destroy()
        {
            lock (instances)
            {
                destroy();
            }
        }

        static RangeInt argumentsRange(MethodBase method)
        {
            RangeInt range = new RangeInt(0, 0);
            foreach (var arg in method.GetParameters())
            {
                if (arg.HasDefaultValue)
                {
                    range.length++;
                } else
                {
                    range.start++;
                }
            }
            return range;
        }

        const int GETTER_MASK = (1 << 1);
        const int SETTER_MASK = (1 << 2);
        const int STATIC_MASK = (1 << 3);

        public void RegisterClass(Type type)
        {
            lock (instances)
            {
                if (!supportTypes.Contains(type))
                {
                    supportTypes.Add(type);
                }
                RegisterClass(instances, ctx, type);
            }
        }

        internal static Class RegisterClass(Instances instances, IntPtr ctx, Type type)
        {
            Class clazz;
            if (instances.classesIndex.TryGetValue(type, out clazz))
            {
                clazz = instances.classesIndex[type];
            } else {
                clazz = new Class(type);
                instances.classesIndex[type] = clazz;
                int id = instances.classes.Count;
                instances.classes.Add(clazz);
                clazz.id = id;

                clazz.ptr = Api.QJS_RegisterClass(ctx, type.FullName, id);

                //Debug.Log("BEGIN " + type.FullName);

                if (type.IsEnum)
                {
                    foreach (var enu in type.GetEnumValues())
                    {
                        Api.QJS_AddEnum(clazz.ptr, type.GetEnumName(enu), (int)enu);
                    }
                } else
                {
                    var constructors = new List<ConstructorInfo>();
                    foreach (var constructor in type.GetConstructors())
                    {
                        if (constructor.IsPublic)
                        {
                            //Debug.Log(" --- [C] " + constructor.GetParameters().Length);
                            var range = argumentsRange(constructor);
                            Api.QJS_AddConstructor(clazz.ptr, constructors.Count, range.start, range.end);
                            constructors.Add(constructor);
                        }
                    }
                    clazz.constructors = constructors.ToArray();

                    var fields = new List<FieldInfo>();
                    foreach (var field in type.GetFields())
                    {
                        if (field.IsPublic)
                        {
                            //Debug.Log(" --- [F] " + field.Name);
                            Api.QJS_AddField(clazz.ptr, field.Name, fields.Count, field.IsStatic);
                            fields.Add(field);
                        }
                    }
                    clazz.fields = fields.ToArray();

                    var methods = new List<MethodInfo>();
                    foreach (var method in type.GetMethods())
                    {
                        if (method.IsPublic)
                        {
                            if (method.Name.StartsWith("get_") || method.Name.StartsWith("set_")) continue;
                            //Debug.Log(" --- [M] " + method.Name + " static " + method.IsStatic);
                            var range = argumentsRange(method);
                            Api.QJS_AddFunction(clazz.ptr, method.Name, methods.Count, method.IsStatic, range.start, range.end);
                            methods.Add(method);
                        }
                    }
                    clazz.methods = methods.ToArray();

                    var properties = new List<PropertyInfo>();
                    foreach (var property in type.GetProperties())
                    {
                        int mask = 0;
                        MethodInfo method = property.GetGetMethod();
                        if (method != null)
                        {
                            mask |= GETTER_MASK;
                            if (method.IsStatic) mask |= STATIC_MASK;
                        }
                        method = property.GetSetMethod();
                        if (method != null)
                        {
                            mask |= SETTER_MASK;
                            if (method.IsStatic) mask |= STATIC_MASK;
                        }
                        if (mask > 0)
                        {
                            //Debug.Log(" --- [P] " + property.Name + " mask " + mask + " on " + type.FullName);
                            Api.QJS_AddProperty(clazz.ptr, property.Name, properties.Count, mask);
                            properties.Add(property);
                        }
                    }
                    clazz.properties = properties.ToArray();
                }

                //Debug.Log("END " + type.FullName);

                Api.QJS_SubmitClass(ctx, clazz.ptr);
            }
            return clazz;
        }

        public void RegisterClass<T>()
        {
            RegisterClass(typeof(T));
        }


        [MonoPInvokeCallback(typeof(Api.QJS_PrintHandlerDelegate))]
        private static void _print(int type, string str)
        {
            switch (type)
            {
                case 0:
                    Debug.Log(str);
                    break;
                case 1:
                    Debug.LogWarning(str);
                    break;
                case 2:
                    Debug.LogError(str);
                    break;
            }
        }

        private static IEnumerator promiseCallback(YieldInstruction yieldInstruction, Action success)
        {
            yield return yieldInstruction;
            success();
        }

        private static IEnumerator promiseCallback(IEnumerator enumerator, Action success)
        {
            yield return enumerator;
            success();
        }

        internal static int newPromise(MonoBehaviour behaviour, QuickJS quickJS, YieldInstruction yieldInstruction)
        {
            var instances = quickJS.instances;
            int promiseId = ++instances.promiseCount;
            behaviour.StartCoroutine(promiseCallback(yieldInstruction, () =>
            {
                if (!quickJS.Disabled)
                {
                    lock (instances)
                    {
                        var context = quickJS.ctx;
                        instances.arguments[0].SetItem(promiseId);
                        instances.arguments[1].SetItem(1);

                        instances.currentBehaviour = behaviour;
                        Api.QJS_Action(context, Api.SHARP_PROMISE_COMPLETE, 2);
                    }
                }
            }));
            Api.QJS_NewPromise(quickJS.ctx, promiseId);
            return promiseId;
        }

        internal static int newPromise(MonoBehaviour behaviour, QuickJS quickJS, IEnumerator enumerator)
        {
            var instances = quickJS.instances;
            int promiseId = ++instances.promiseCount;
            behaviour.StartCoroutine(promiseCallback(enumerator, () =>
            {
                if (!quickJS.Disabled)
                {
                    lock (instances)
                    {
                        var context = quickJS.ctx;
                        instances.arguments[0].SetItem(promiseId);
                        instances.arguments[1].SetItem(1);

                        instances.currentBehaviour = behaviour;
                        Api.QJS_Action(context, Api.SHARP_PROMISE_COMPLETE, 2);
                    }
                }
            }));
            Api.QJS_NewPromise(quickJS.ctx, promiseId);
            return promiseId;
        }

        internal static IntPtr createObject(Instances instances, IntPtr ctx, Class clazz, object obj)
        {
            int insId = ++instances.instanceCount;
            IntPtr ptr = Api.QJS_NewInstance(ctx, clazz.ptr, insId);
            instances.items[insId] = instances.index[obj] = new Instance()
            {
                //clazz = clazz,
                id = insId,
                ptr = ptr,
                target = obj
            };
            return ptr;
        }

        internal static IntPtr createFunction(Instances instances, IntPtr ctx, Class clazz, Delegate del)
        {
            int insId = ++instances.instanceCount;
            IntPtr ptr = Api.QJS_NewFunction(ctx, clazz.ptr, insId);
            instances.items[insId] = instances.index[del] = new Instance()
            {
                id = insId,
                ptr = ptr,
                target = del
            };
            return ptr;
        }

        internal static void clearCache(Stack<IntPtr> cache)
        {
            if (cache.Count > 0)
            {
                foreach (var ptr in cache)
                {
                    Marshal.FreeHGlobal(ptr);
                }
                cache.Clear();
            }
        }

        static readonly int item_length = Marshal.SizeOf<Api.QJS_Item>();
        private bool isWorker = false;

        [MonoPInvokeCallback(typeof(Api.QJS_ActionHandlerDelegate))]
        private static void _action(int handler,
            IntPtr ctx,
            int class_id,
            IntPtr ids,
            int length,
            int type,
            int argc)
        {
            Instances instances = null;
            try
            {
                QuickJS quickJS;
                if (insCache.TryGetValue(handler, out instances) && instances.reference.TryGetTarget(out quickJS))
                {
                    clearCache(instances.resultCache);
                    Api.QJS_Item[] argv = instances.arguments;
                    Api.QJS_Item[] results = instances.results;
                    switch (type)
                    {
                        case Api.TYPE_NEW_OBJECT:
                            {
                                if (length > 0)
                                {
                                    Class clazz = instances.classes[class_id];
                                    int len = length;
                                    long longPtr = argv[0].val.l;

                                    object[] ps = Utils.ProcessArguments(quickJS, argv, 1, argc);
                                    var constructor = Utils.FindMethod(quickJS, clazz.constructors, ids, length, ps);
                                    if (constructor != null)
                                    {
                                        var obj = Activator.CreateInstance(clazz.target, ps);

                                        //if (obj is YieldInstruction)
                                        //{
                                        //    var beh = instances.currentBehaviour;
                                        //    if (beh == null)
                                        //    {
                                        //        Debug.LogWarning("No current behaviour, can not make Promise");
                                        //        results[0].SetItem(2);
                                        //    }
                                        //    else
                                        //    {
                                        //        int promiseId = newPromise(beh, quickJS, obj as YieldInstruction);
                                        //        results[0].SetItem(1);
                                        //        results[1].SetPromise(promiseId);
                                        //    }
                                        //}
                                        //else
                                        {
                                            Instance ins = new Instance();
                                            ins.target = obj;
                                            ins.ptr = new IntPtr(longPtr);
                                            int insId = ++instances.instanceCount;
                                            ins.id = insId;
                                            instances.items[insId] = instances.index[obj] = ins;
                                            results[0].SetItem(0);
                                            results[1].SetItem(insId);
                                        }

                                        return;
                                    }
                                    Debug.LogError("Can not found match constructor. " + clazz.target);
                                    results[0].SetNull();
                                }

                                break;
                            }
                        case Api.TYPE_INVOKE:
                            {
                                if (length > 0)
                                {
                                    Class clazz = instances.classes[class_id];
                                    Instance instance;
                                    if (argv[0].type == Api.ITEM_TYPE_JS_OBJECT && instances.items.TryGetValue(argv[0].val.i, out instance))
                                    {
                                        var tar = instance.target;
                                        var ps = Utils.ProcessArguments(quickJS, argv, 1, argc);

                                        var method = Utils.FindMethod(quickJS, clazz.methods, ids, length, ps);
                                        if (method != null)
                                        {
                                            object ret = method.Invoke(tar, ps);
                                            results[0].SetAny(quickJS, ret, instances.resultCache);
                                        } else
                                        {
                                            Debug.LogError("Can not found match method. ");
                                            results[0].SetNull();
                                        }
                                        return;
                                    }
                                    Debug.LogError("Instance can not be empty.");
                                    results[0].SetNull();
                                    return;
                                }
                                break;
                            }
                        case Api.TYPE_GET_FIELD:
                            {
                                Class clazz = instances.classes[class_id];
                                Instance instance;
                                if (argv[0].type == Api.ITEM_TYPE_JS_OBJECT && instances.items.TryGetValue(argv[0].val.i, out instance))
                                {
                                    var field = clazz.fields[Marshal.ReadInt32(ids)];
                                    results[0].SetAny(quickJS, field.GetValue(instance.target), instances.resultCache);
                                    return;
                                }
                                Debug.LogError("No target.");
                                break;
                            }
                        case Api.TYPE_SET_FIELD:
                            {
                                Class clazz = instances.classes[class_id];
                                Instance instance;
                                if (argv[0].type == Api.ITEM_TYPE_JS_OBJECT && instances.items.TryGetValue(argv[0].val.i, out instance))
                                {
                                    var field = clazz.fields[Marshal.ReadInt32(ids)];
                                    object val = Utils.ProcessObject(quickJS, ref argv[1]);
                                    field.SetValue(instance.target, Utils.ConvertType(val, field.FieldType));
                                    return;
                                }
                                Debug.LogError("No target.");
                                break;
                            }
                        case Api.TYPE_GET_PROPERTY:
                            {
                                Class clazz = instances.classes[class_id];
                                Instance instance;
                                if (argv[0].type == Api.ITEM_TYPE_JS_OBJECT && instances.items.TryGetValue(argv[0].val.i, out instance))
                                {
                                    var property = clazz.properties[Marshal.ReadInt32(ids)];
                                    var obj = property.GetValue(instance.target);
                                    results[0].SetAny(quickJS, obj, instances.resultCache);
                                    return;
                                }
                                Debug.LogError("No target.");
                                break;
                            }
                        case Api.TYPE_SET_PROPERTY:
                            {
                                Class clazz = instances.classes[class_id];
                                Instance instance;
                                if (argv[0].type == Api.ITEM_TYPE_JS_OBJECT && instances.items.TryGetValue(argv[0].val.i, out instance))
                                {
                                    var property = clazz.properties[Marshal.ReadInt32(ids)];
                                    var val = Utils.ProcessObject(quickJS, ref argv[1]);
                                    property.SetValue(instance.target, Utils.ConvertType(val, property.PropertyType));
                                    return;
                                }
                                Debug.LogError("No target.");
                                break;
                            }
                        case Api.TYPE_STATIC_INVOKE:
                            {
                                Class clazz = instances.classes[class_id];
                                var ps = Utils.ProcessArguments(quickJS, argv, 0, argc);

                                var method = Utils.FindMethod(quickJS, clazz.methods, ids, length, ps);
                                if (method != null)
                                {
                                    object ret = method.Invoke(clazz.target, ps);
                                    results[0].SetAny(quickJS, ret, instances.resultCache);
                                    return;
                                }

                                Debug.LogError("Can not found match method.");
                                results[0].SetNull();

                                break;
                            }
                        case Api.TYPE_STATIC_GET:
                            {
                                Class clazz = instances.classes[class_id];
                                var field = clazz.fields[Marshal.ReadInt32(ids)];
                                results[0].SetAny(quickJS, field.GetValue(clazz.target), instances.resultCache);
                                return;
                            }
                        case Api.TYPE_STATIC_SET:
                            {
                                Class clazz = instances.classes[class_id];
                                var field = clazz.fields[Marshal.ReadInt32(ids)];
                                if (argc > 0)
                                {
                                    var val = Utils.ProcessObject(quickJS, ref argv[0]);
                                    field.SetValue(clazz.target, Utils.ConvertType(val, field.FieldType));
                                }
                                return;
                            }
                        //case Api.TYPE_ADD_DFIELD:
                        //    {
                        //        if (argc >= 2)
                        //        {
                        //            Class clazz = instances.classes[class_id];
                        //            if (clazz.annotations == null)
                        //            {
                        //                clazz.annotations = new List<Annotation>();
                        //            }
                        //            try
                        //            {
                        //                Annotation annotation = new Annotation();
                        //                annotation.name = argv[0].ToString(quickJS);
                        //                int t = argv[1].val.i;
                        //                annotation.type = (FieldType)t;
                        //                if (argc > 2)
                        //                {
                        //                    var val = Utils.ProcessObject(quickJS, ref argv[2]);
                        //                    annotation.defaultValue = (val is JSValue) ? ((JSValue)val).Value : val;
                        //                }
                        //                int count = clazz.annotations.Count;
                        //                clazz.annotations.Add(annotation);
                        //                results[0].SetItem(count);
                        //                return;
                        //            }
                        //            catch (Exception e)
                        //            {
                        //                Debug.LogError("add field failed " + e);
                        //            }
                        //        }
                        //        results[0].SetNull();
                        //        return;
                        //    }
                        case Api.TYPE_GET_DFIELD:
                            {
                                if (argc >= 2)
                                {
                                    Instance instance;
                                    if (argv[0].type == Api.ITEM_TYPE_JS_OBJECT && instances.items.TryGetValue(argv[0].val.i, out instance))
                                    {
                                        if (instance.container != null)
                                        {
                                            string str = argv[1].ToString(quickJS);
                                            if (str != null)
                                            {
                                                var attrs = instance.container.GetAttributes();
                                                foreach (var attr in attrs)
                                                {
                                                    if (attr.key == str)
                                                    {
                                                        results[0].SetAny(quickJS, attr.value.Value, instances.resultCache);
                                                        return;
                                                    }
                                                }
                                            }
                                        }
                                    } 
                                }
                                results[0].SetNull();
                                return;
                            }
                        case Api.TYPE_SET_DFIELD:
                            {
                                if (argc >= 3)
                                {
                                    Instance instance;
                                    if (argv[0].type == Api.ITEM_TYPE_JS_OBJECT && instances.items.TryGetValue(argv[0].val.i, out instance))
                                    {
                                        if (instance.container != null)
                                        {
                                            string str = argv[1].ToString(quickJS);
                                            if (str != null)
                                            {
                                                var attrs = instance.container.GetAttributes();
                                                foreach (var attr in attrs)
                                                {
                                                    if (attr.key == str)
                                                    {
                                                        var val = Utils.ProcessObject(quickJS, ref argv[2]);
                                                        attr.value.Value = (val is JSValue) ? ((JSValue)val).Value : val;
                                                        break;
                                                    }
                                                }
                                            }
                                        }
                                    }
                                }
                                return;
                            }
                        case Api.TYPE_GENERIC_CLASS:
                            {
                                Class clazz = instances.classes[class_id];

                                Type[] types = new Type[argc];
                                for (int i = 0; i < argc; ++i)
                                {
                                    types[i] = Utils.ProcessObject(quickJS, ref argv[i]) as Type;
                                }
                                Type newType = clazz.target.MakeGenericType(types);
                                Class newClazz;
                                if (!instances.classesIndex.TryGetValue(newType, out newClazz))
                                {
                                    newClazz = QuickJS.RegisterClass(instances, ctx, newType);
                                }
                                results[0].SetItem(newClazz);
                                return;
                            }
                        case Api.TYPE_ARRAY_CLASS:
                            {
                                Class clazz = instances.classes[class_id];

                                Type newType = clazz.target.MakeArrayType();
                                Class newClazz;
                                if (!instances.classesIndex.TryGetValue(newType, out newClazz))
                                {
                                    newClazz = QuickJS.RegisterClass(instances, ctx, newType);
                                }
                                results[0].SetItem(newClazz);
                                return;
                            }
                        case Api.TYPE_NEW_ARRAY:
                            {
                                Class clazz = instances.classes[class_id];
                                if (argv[0].type == Api.ITEM_TYPE_INT && argv[1].type == Api.ITEM_TYPE_LONG)
                                {
                                    var arrayType = clazz.target.MakeArrayType();
                                    int len = argv[0].val.i;
                                    IntPtr ptr = new IntPtr(argv[1].val.l);

                                    Array arr = (Array)Activator.CreateInstance(arrayType, len);
                                    for (int i = 0; i < len; ++i)
                                    {
                                        Api.QJS_Item item = Marshal.PtrToStructure<Api.QJS_Item>(ptr);
                                        arr.SetValue(Utils.ConvertType(Utils.ProcessObject(quickJS, ref item), clazz.target), i);
                                        ptr = ptr + item_length;
                                    }
                                    results[0].SetAny(quickJS, arr, instances.resultCache);
                                } else
                                {
                                    results[0].SetNull();
                                }

                                return;
                            }
                        case Api.TYPE_CALL_DELEGATE:
                            {
                                Instance instance;
                                if (argv[0].type == Api.ITEM_TYPE_INT && instances.items.TryGetValue(argv[0].val.i, out instance))
                                {
                                    if (instance.target is Delegate)
                                    {
                                        Delegate del = instance.target as Delegate;
                                        var method = del.GetMethodInfo();

                                        object[] ps = Utils.ProcessArguments(quickJS, argv, 1, argc);
                                        var parameters = method.GetParameters();
                                        if (Utils.TestArguments(quickJS, method, parameters, ps) > 0)
                                        {
                                            for (int i = 0, t = ps.Length; i < t; ++i)
                                            {
                                                ps[i] = Utils.ConvertType(ps[i], parameters[i].ParameterType);
                                            }
                                            object ret = del.DynamicInvoke(ps);
                                            results[0].SetAny(quickJS, ret, instances.resultCache);
                                            return;
                                        }
                                    }
                                }
                                results[0].SetNull();
                                return;
                            }
                        case Api.TYPE_STATIC_PGET:
                            {
                                Class clazz = instances.classes[class_id];
                                var property = clazz.properties[Marshal.ReadInt32(ids)];
                                results[0].SetAny(quickJS, property.GetValue(clazz.target), instances.resultCache);
                                return;
                            }
                        case Api.TYPE_STATIC_PSET:
                            {
                                Class clazz = instances.classes[class_id];
                                var property = clazz.properties[Marshal.ReadInt32(ids)];
                                if (argc > 0)
                                {
                                    property.SetValue(clazz.target,
                                        Utils.ConvertType(Utils.ProcessObject(quickJS, ref argv[0]), property.PropertyType));
                                }
                                return;
                            }
                        case Api.TYPE_CREATE_WORKER:
                            {
                                if (!quickJS.isWorker)
                                {
                                    string base_name = argv[0].ToString(quickJS);
                                    string name = argv[1].ToString(quickJS);
                                    string targetPath = Path.GetFullPath(Path.Combine("/", base_name, "..", name)).Substring(1);
                                    if (quickJS.Loader.Test(targetPath))
                                    {
                                        QuickJS newQuickJS = new QuickJS();
                                        newQuickJS.isWorker = true;
                                        newQuickJS.Loader = quickJS.Loader;
                                        foreach (var sType in quickJS.supportTypes)
                                        {
                                            newQuickJS.RegisterClass(sType);
                                        }

                                        Worker worker = new Worker(newQuickJS, quickJS, targetPath);
                                        results[0].SetAny(quickJS, worker, instances.resultCache);
                                        return;
                                    } else
                                    {
                                        Debug.LogError("Not found script for worker (" + targetPath + ")");
                                    }
                                } else
                                {
                                    Debug.LogError("Can not create worker in worker.");
                                }
                                results[0].SetNull();
                                break;
                            }
                        case Api.TYPE_PROMISE_TRANS:
                            {
                                Instance instance;
                                if (argv[0].type == Api.ITEM_TYPE_JS_OBJECT && instances.items.TryGetValue(argv[0].val.i, out instance))
                                {
                                    var obj = instance.target;
                                    var beh = instances.currentBehaviour;
                                    if (beh == null)
                                    {
                                        Debug.LogWarning("No current behaviour, can not make Promise");
                                        results[0].SetNull();
                                    }
                                    else
                                    {
                                        if (obj is YieldInstruction)
                                        {
                                            int promiseId = newPromise(beh, quickJS, obj as YieldInstruction);
                                            results[0].SetPromise(promiseId);
                                        } else if (obj is IEnumerator)
                                        {
                                            int promiseId = newPromise(beh, quickJS, obj as IEnumerator);
                                            results[0].SetPromise(promiseId);
                                        } else
                                        {
                                            results[0].SetNull();
                                        }
                                    }
                                } else
                                {
                                    results[0].SetNull();
                                }
                                break;
                            }
                        default: break;
                    }
                }
            } catch (Exception e)
            {
                Debug.LogError(e);
                if (instances != null)
                {
                    instances.results[0].SetNull();
                }
            }
        }

        [MonoPInvokeCallback(typeof(Api.QJS_DeleteObjectHandlerDelegate))]
        private static void _deleteObject(int handler, int obj)
        {
            Instances instances;
            if (insCache.TryGetValue(handler, out instances))
            {
                Instance instance;
                if (instances.items.TryGetValue(obj, out instance))
                {
                    instances.items.Remove(obj);
                    Instance other;
                    if (instances.index.TryGetValue(instance.target, out other) && other == instance)
                    {
                        instances.index.Remove(instance.target);
                    }
                    instance.disabled = true;
                    if (instance.target is IDisposable)
                    {
                        ((IDisposable)instance.target).Dispose();
                    }
                }
            }
        }

        private Dictionary<string, JSONNode> packageCache = new Dictionary<string, JSONNode>();
        private string testFile(string filename)
        {
            if (Loader == null) return null;
            if (filename[0] == '/') filename = filename.Substring(1);
            if (Loader.Test(filename))
            {
                return filename;
            }
            string jsstr = filename + ".js";
            if (Loader.Test(jsstr))
            {
                return jsstr;
            }
            string jsonstr = filename + ".json";
            if (Loader.Test(jsonstr))
            {
                return jsonstr;
            }
            string packstr = filename + "/package.json";
            if (Loader.Test(packstr))
            {
                JSONNode json;
                if (!packageCache.TryGetValue(packstr, out json))
                {
                    string content = Loader.Load(packstr);
                    json = JSON.Parse(content);
                    packageCache.Add(packstr, json);
                }
                if (json.IsObject)
                {
                    if (json.HasKey("main"))
                    {
                        string main = json["main"];
                        return Path.GetFullPath(Path.Combine("/", filename, main)).Substring(1);
                    }
                }
            }
            return null;
        }

        private static string popSeg(string str)
        {
            int len = str.IndexOf('/');
            return str.Substring(0, len < 0 ? 0 : len);
        } 

        [MonoPInvokeCallback(typeof(Api.QJS_ModuleNameHandlerDelegate))]
        private static IntPtr _moduleName(
            int handler,
            IntPtr ctx,
            string module_base_name,
            string module_name)
        {
            Instances instances;
            if (insCache.TryGetValue(handler, out instances))
            {
                clearCache(instances.resultCache);
                QuickJS quickJS;
                if (instances.reference.TryGetTarget(out quickJS))
                {
                    string spec_name = null;
                    if (module_name[0] == '/')
                    {
                        spec_name = quickJS.testFile(module_name);
                    }
                    else if (module_name[0] == '.')
                    {
                        spec_name = quickJS.testFile(Path.GetFullPath(Path.Combine("/", module_base_name, "..", module_name)).Substring(1));
                    }
                    else
                    {
                        string base_name = popSeg(module_base_name);
                        while (true)
                        {
                            string tarPath = Path.GetFullPath(Path.Combine("/", base_name, "node_modules", module_name)).Substring(1);
                            spec_name = quickJS.testFile(tarPath);
                            if (spec_name != null) break;
                            if (base_name.Length == 0) break;
                            base_name = popSeg(base_name);
                        }
                    }

                    if (spec_name == null)
                    {
                        return IntPtr.Zero;
                    }
                    byte[] buf = Encoding.UTF8.GetBytes(spec_name);
                    IntPtr ptr = Api.QJS_Malloc(ctx, buf.Length + 1);
                    Marshal.Copy(buf, 0, ptr, buf.Length);
                    Marshal.WriteByte(ptr, buf.Length, 0);

                    return ptr;
                }
            }

            return IntPtr.Zero;
        }

        [MonoPInvokeCallback(typeof(Api.QJS_LoadModuleHandlerDelegate))]
        private static void _loadModule(
            int handler,
            IntPtr ctx,
            string module_name)
        {
            Instances instances;
            if (insCache.TryGetValue(handler, out instances))
            {
                clearCache(instances.resultCache);
                QuickJS quickJS;
                if (instances.reference.TryGetTarget(out quickJS))
                {
                    string ext = Path.GetExtension(module_name);
                    if (ext.Length == 0)
                    {
                        string try_name = module_name + ".js";
                        string res = null;
                        if (quickJS.Loader != null)
                        {
                            res = quickJS.Loader.Load(try_name);
                            if (res == null)
                            {
                                try_name = module_name + ".json";
                                res = quickJS.Loader.Load(try_name);
                            }
                            if (res == null)
                                res = quickJS.Loader.Load(module_name);
                        }
                        if (res != null)
                        {
                            instances.results[0].SetItem(res, instances.resultCache);
                            return;
                        }
                    }
                    else
                    {
                        string res = null;
                        if (quickJS.Loader != null)
                            res = quickJS.Loader.Load(module_name);
                        if (res != null)
                        {
                            instances.results[0].SetItem(res, instances.resultCache);
                            return;
                        }
                    }
                }
            }
            instances.results[0].SetNull();
        }

        private static Type GetTypeByName(string name)
        {
            foreach (Assembly assembly in AppDomain.CurrentDomain.GetAssemblies())
            {
                foreach (Type type in assembly.GetTypes())
                {
                    if (type.FullName == name)
                        return type;
                }
            }

            return null;
        }

        [MonoPInvokeCallback(typeof(Api.QJS_LoadClassHandlerDelegate))]
        private static void _loadClass(int handler,
            IntPtr ctx,
            string class_name)
        {
            Instances instances;
            if (insCache.TryGetValue(handler, out instances))
            {
                Type type = Type.GetType(class_name);
                if (type == null)
                    type = Type.GetType(class_name + "," + class_name.Substring(0, class_name.IndexOf(".")));
                if (type == null)
                    type = GetTypeByName(class_name);
                if (type != null)
                {
                    Class clazz = QuickJS.RegisterClass(instances, ctx, type);
                    instances.results[0].SetItem(clazz);
                    return;
                }
                else
                {
                    Debug.LogError("Can not dynamic load " + class_name);
                }
            }
            instances.results[0].SetNull();
        }

        HashSet<Type> supportTypes = new HashSet<Type>();

        public Type[] GetSupportTypes() {
            Type[] types = new Type[supportTypes.Count];
            supportTypes.CopyTo(types, 0);
            return types;
        }

        private bool isInitialized = false;

        public JSValue Eval(string code, string path = "<inline>")
        {
            lock (instances)
            {
                if (!isInitialized)
                    setupStep();
                instances.results[0].SetItem((long)669);
                Api.QJS_Eval(ctx, code, path);
                var ret = instances.results[0].ToValue(this);
                Api.QJS_ClearResults(ctx);
                return ret;
            }
        }

        public JSValue Load(string code, string module)
        {
            lock (instances)
            {
                if (!isInitialized)
                    setupStep();
                Api.QJS_Load(ctx, code, module);
                var ret = instances.results[0].ToValue(this);
                Api.QJS_ClearResults(ctx);
                return ret;
            }
        }

        public JSValue Load(string path)
        {
            lock (instances)
            {
                if (!isInitialized)
                    setupStep();
                if (Loader != null && Loader.Test(path))
                {
                    string code = Loader.Load(path);
                    return Load(code, path);
                } else
                {
                    Debug.LogError("Can not load:" + path);
                }
                return JSValue.Zero;
            }
        }

        private class StepBeh : MonoBehaviour
        {
            internal QuickJS quickJS;

            private void FixedUpdate()
            {
                if (quickJS != null)
                {
                    if (quickJS.disabled)
                    {
                        Destroy(this);
                    } else
                    {

                        quickJS.Step();
                    }
                }
            }
        }

        private void setupStep()
        {
            isInitialized = true;
            if (needUpdate && Application.isPlaying)
            {
                GameObject gameObject = new GameObject();
                var beh = gameObject.AddComponent<StepBeh>();
                gameObject.name = "qjs_step";
                beh.quickJS = this;
                GameObject.DontDestroyOnLoad(gameObject);
            }
        }

        ConcurrentQueue<Action> tasks = new ConcurrentQueue<Action>();

        public void Step()
        {
            Action action;
            while (tasks.TryDequeue(out action))
            {
                action();
            }
            if (!disabled)
                Api.QJS_Step(ctx);
        }

        public void EnqueueTask(Action task)
        {
            tasks.Enqueue(task);
        }

        public JSAtom NewAtom(string str)
        {
            uint atom = Api.QJS_NewAtom(ctx, str);
            return new JSAtom(this, atom);
        }

        /**
         * Create a []
         */
        public JSValue NewArray()
        {
            lock (instances)
            {
                int ret = Api.QJS_Action(ctx, Api.SHARP_NEW_ARRAY, 0);
                var res = ret > 0 ? instances.results[0].ToValue(this) : JSValue.Zero;
                Api.QJS_ClearResults(ctx);
                return res;
            }
        }

        /**
         * Create a {}
         */
        public JSValue NewObject()
        {
            lock (instances)
            {
                int ret = Api.QJS_Action(ctx, Api.SHARP_NEW_OBJECT, 0);
                var res = ret > 0 ? instances.results[0].ToValue(this) : JSValue.Zero;
                Api.QJS_ClearResults(ctx);
                return res;
            }
        }

        /**
         * Create a Object and bind to a c# object.
         */
        public JSValue NewBind(JSValue value, object target)
        {
            lock (instances)
            {
                instances.arguments[0].SetItem(value, instances.argumentCache);
                Instance ins = new Instance();
                if (target is Container)
                {
                    Container container = target as Container;
                    ins.target = container.target;
                    ins.container = container;
                } else
                {
                    ins.target = target;
                }
                int insId = ++instances.instanceCount;
                ins.id = insId;
                instances.items[insId] = instances.index[target] = ins;
                instances.arguments[1].SetItem(insId);

                int len = Api.QJS_Action(ctx, Api.SHARP_NEW_BIND, 2);
                clearCache(instances.argumentCache);

                JSValue ret;
                if (len >= 2)
                {
                    ins.ptr = new IntPtr(instances.results[1].val.l);
                    ret = instances.results[0].ToValue(this);
                } else
                {
                    instances.items.Remove(insId);
                    if (instances.index[target] == ins)
                    {
                        instances.index.Remove(target);
                    }
                    ret = JSValue.Zero;
                }
                Api.QJS_ClearResults(ctx);

                return ret;
            }
        }

        internal bool isInt8Array(IntPtr value)
        {
            return Api.QJS_IsInt8Array(ctx, value);
        }
        internal bool isUint8Array(IntPtr value)
        {
            return Api.QJS_IsUint8Array(ctx, value);
        }
        internal bool isInt16Array(IntPtr value)
        {
            return Api.QJS_IsInt16Array(ctx, value);
        }
        internal bool isUint16Array(IntPtr value)
        {
            return Api.QJS_IsUint16Array(ctx, value);
        }
        internal bool isInt32Array(IntPtr value)
        {
            return Api.QJS_IsInt32Array(ctx, value);
        }
        internal bool isUint32Array(IntPtr value)
        {
            return Api.QJS_IsUint32Array(ctx, value);
        }
        internal bool isInt64Array(IntPtr value)
        {
            return Api.QJS_IsInt64Array(ctx, value);
        }
        internal bool isUint64Array(IntPtr value)
        {
            return Api.QJS_IsUint64Array(ctx, value);
        }
        internal bool isFloat32Array(IntPtr value)
        {
            return Api.QJS_IsFloat32Array(ctx, value);
        }
        internal bool isFloat64Array(IntPtr value)
        {
            return Api.QJS_IsFloat64Array(ctx, value);
        }
        internal bool isFunction(IntPtr value)
        {
            return Api.QJS_IsFunctionPtr(ctx, value);
        }
        internal int getFunctionLength(IntPtr value)
        {
            return Api.QJS_GetFunctionLengthPtr(ctx, value);
        }
        internal uint getTypedArrayLength(IntPtr value)
        {
            return Api.QJS_GetTypedArrayLength(ctx, value);
        }
        internal bool copyArrayBuffer(IntPtr ptr, IntPtr dis, uint offset, uint length)
        {
            return Api.QJS_CopyArrayBuffer(ctx, ptr, dis, offset, length);
        }
    }

    public enum JSValueType
    {
        Bool,
        Int,
        Long,
        Double,
        String,
        Null,

        ObjectIndex,
        Object = ObjectIndex,
        JSObject,
        Class
    };

    public class JSAtom
    {
        uint target;
        QuickJS context;

        internal uint Target
        {
            get
            {
                return target;
            }
        }

        internal QuickJS Context
        {
            get
            {
                return context;
            }
        }

        internal JSAtom(QuickJS context, uint target)
        {
            this.context = context;
            this.target = target;
        }

        ~JSAtom()
        {
            if (context != null && !context.Disabled)
            {
                Api.QJS_FreeAtom(context.ctx, target);
            }
        }

        public void Free()
        {
            if (context != null && !context.Disabled)
            {
                Api.QJS_FreeAtom(context.ctx, target);
            }
            context = null;
        }
    }

    public class AwaitYield : CustomYieldInstruction
    {
        private bool waiting = true;
        private bool success;
        private JSValue result;

        public override bool keepWaiting => waiting;

        public bool Success
        {
            get => success;
        }
        public JSValue Result
        {
            get => success ? result : null;
        }
        public JSValue Error
        {
            get => success ? null : result;
        }

        internal void Complete(bool success, JSValue result)
        {
            this.success = success;
            this.result = result;
            waiting = false;
        }
    }

    public class JSValue : IEquatable<JSValue>, IConvertible
    {
        internal struct JSValueContext
        {
            public QuickJS quickJS;
            public Instance instance;
            public Class clazz;

            public IntPtr ctx
            {
                get
                {
                    return quickJS.ctx;
                }
            }

            public bool Disabled
            {
                get
                {
                    return quickJS.Disabled;
                }
            }
        }
        internal JSValueContext context;
        private JSValueType type;

        public JSValueType Type {
            get
            {
                return type;
            }
        }

        private object value;

        public object Value {
            get
            {
                switch (type)
                {
                    case JSValueType.Object:
                        return context.instance == null ? null : context.instance.target;
                    case JSValueType.JSObject:
                        return this;
                    case JSValueType.Class:
                        return context.clazz.target;
                    default:
                        return value;
                }
            }
        }

        public static readonly JSValue Zero = new JSValue();
        public static readonly JSValue True = new JSValue(true);
        public static readonly JSValue False = new JSValue(false);

        internal Instance GetObject()
        {
            if (type == JSValueType.Object)
            {
                return context.instance;
            }
            return null;
        }

        private IntPtr stringPtr;

        internal IntPtr GetPtr()
        {
            if (type >= JSValueType.ObjectIndex)
            {
                return (IntPtr)value;
            } else if (type == JSValueType.String)
            {
                if (stringPtr == IntPtr.Zero)
                {
                    stringPtr = Marshal.StringToHGlobalAuto((string)value);
                }
                return stringPtr;
            }
            return IntPtr.Zero;
        }

        internal Class GetClass()
        {
            return context.clazz;
        }

        public JSValue(bool v)
        {
            type = JSValueType.Bool;
            value = v;
        }

        public JSValue(int v)
        {
            type = JSValueType.Int;
            value = v;
        }

        public JSValue(long v)
        {
            type = JSValueType.Long;
            value = v;
        }

        public JSValue(double v)
        {
            type = JSValueType.Double;
            value = v;
        }

        public JSValue(string v)
        {
            type = JSValueType.String;
            value = v;
        }

        public JSValue()
        {
            type = JSValueType.Null;
        }

        internal JSValue(QuickJS quickJS, IntPtr jsvalue, Instance instance)
        {
            type = JSValueType.Object;
            value = jsvalue;
            context = new JSValueContext() {
                quickJS = quickJS,
                instance = instance
            };
        }

        internal JSValue(QuickJS quickJS, IntPtr jsvalue, Class clazz)
        {
            type = JSValueType.Class;
            value = jsvalue;
            context = new JSValueContext()
            {
                quickJS = quickJS,
                clazz = clazz
            };
        }

        internal JSValue(QuickJS quickJS, IntPtr jsvalue)
        {
            type = JSValueType.JSObject;
            value = jsvalue;
            context = new JSValueContext()
            {
                quickJS = quickJS,
            };
        }

        ~JSValue()
        {
            if (type >= JSValueType.ObjectIndex)
            {
                lock (context.quickJS.instances)
                {
                    free();
                }
            } else if (type == JSValueType.String)
            {
                if (stringPtr != IntPtr.Zero)
                    Marshal.FreeHGlobal(stringPtr);
            }
        }

        private void free()
        {
            if (context.Disabled) return;
            Api.QJS_ReleaseJSValue(context.ctx, (IntPtr)value);
        }

        public void Free()
        {
            if (type >= JSValueType.ObjectIndex)
            {
                lock (context.quickJS.instances)
                {
                    free();
                }
                value = null;
                type = JSValueType.Null;
            }  else if (type == JSValueType.String)
            {
                if (stringPtr != IntPtr.Zero)
                {
                    Marshal.FreeHGlobal(stringPtr);
                    stringPtr = IntPtr.Zero;
                }
            }
        }

        private bool isBehaviour()
        {
            return context.instance != null && context.instance.target is MonoBehaviour;
        }

        public JSValue Call(string name, params object[] argv)
        {
            if (Type < JSValueType.ObjectIndex)
            {
                Debug.LogWarning("Can not call with (" + Type + ")");
                return Zero;
            }
            var instances = context.quickJS.instances;
            lock (instances)
            {
                if (context.Disabled) return null;

                instances.arguments[0].SetValue((IntPtr)value);
                instances.arguments[1].SetItem(name, instances.argumentCache);

                for (int i = 0, t = argv.Length; i < t; ++i)
                {
                    instances.arguments[2 + i].SetAny(context.quickJS, argv[i], instances.argumentCache);
                }
                if (isBehaviour())
                    instances.currentBehaviour = context.instance.target as MonoBehaviour;
                int len = Api.QJS_Action(context.ctx, Api.SHARP_CALL, argv.Length + 2);
                QuickJS.clearCache(instances.argumentCache);

                var ret = len > 0 ? instances.results[0].ToValue(context.quickJS) : Zero;
                Api.QJS_ClearResults(context.ctx);
                return ret;
            }
        }

        public JSValue Call(JSAtom atom, params object[] argv)
        {
            if (Type < JSValueType.ObjectIndex)
            {
                Debug.LogWarning("Can not call with (" + Type + ")");
                return Zero;
            }

            if (atom.Context != context.quickJS)
            {
                Debug.LogWarning("Atom context not match");
                return Zero;
            }
            var instances = context.quickJS.instances;
            lock (instances)
            {
                if (context.Disabled)
                    return Zero;

                instances.arguments[0].SetValue((IntPtr)value);
                instances.arguments[1].SetItem(unchecked((int)atom.Target));
                for (int i = 0, t = argv.Length; i < t; ++i)
                {
                    instances.arguments[2 + i].SetAny(context.quickJS, argv[i], instances.argumentCache);
                }

                if (isBehaviour())
                    instances.currentBehaviour = context.instance.target as MonoBehaviour;
                int len = Api.QJS_Action(context.ctx, Api.SHARP_CALL_ATOM, argv.Length + 2);
                QuickJS.clearCache(instances.argumentCache);

                var ret = len > 0 ? instances.results[0].ToValue(context.quickJS) : Zero;
                Api.QJS_ClearResults(context.ctx);
                return ret;
            }
        }

        public JSValue Get(string name)
        {
            if (Type < JSValueType.ObjectIndex)
            {
                Debug.LogWarning("Can not get with (" + Type + ")");
                return Zero;
            }

            var instances = context.quickJS.instances;
            lock (instances)
            {
                if (context.Disabled) return Zero;

                instances.arguments[0].SetValue((IntPtr)value);
                instances.arguments[1].SetItem(name, instances.argumentCache);

                if (isBehaviour())
                    instances.currentBehaviour = context.instance.target as MonoBehaviour;
                int len = Api.QJS_Action(context.ctx, Api.SHARP_GET, 2);
                QuickJS.clearCache(instances.argumentCache);

                var ret = len > 0 ? instances.results[0].ToValue(context.quickJS) : Zero;
                Api.QJS_ClearResults(context.ctx);
                return ret;
            }
        }

        public JSValue Get(JSAtom atom)
        {
            if (Type < JSValueType.ObjectIndex)
            {
                Debug.LogWarning("Can not get with (" + Type + ")");
                return Zero;
            }

            if (atom.Context != context.quickJS)
            {
                Debug.LogWarning("Atom context not match");
                return Zero;
            }
            var instances = context.quickJS.instances;
            lock (instances)
            {
                if (context.Disabled) return Zero;
                instances.arguments[0].SetValue((IntPtr)value);
                instances.arguments[1].SetItem(unchecked((int)atom.Target));

                if (isBehaviour())
                    instances.currentBehaviour = context.instance.target as MonoBehaviour;
                int len = Api.QJS_Action(context.ctx, Api.SHARP_GET_ATOM, 2);

                var ret = len > 0 ? instances.results[0].ToValue(context.quickJS) : Zero;
                Api.QJS_ClearResults(context.ctx);
                return ret;
            }
        }

        public JSValue Get(int index)
        {
            if (Type < JSValueType.ObjectIndex)
            {
                Debug.LogWarning("Can not get with (" + Type + ")");
                return Zero;
            }

            var instances = context.quickJS.instances;
            lock (instances)
            {
                if (context.Disabled) return Zero;
                instances.arguments[0].SetValue((IntPtr)value);
                instances.arguments[1].SetItem(index);

                if (isBehaviour())
                    instances.currentBehaviour = context.instance.target as MonoBehaviour;
                int len = Api.QJS_Action(context.ctx, Api.SHARP_GET_INDEX, 2);

                var ret = len > 0 ? instances.results[0].ToValue(context.quickJS) : Zero;
                Api.QJS_ClearResults(context.ctx);
                return ret;
            }
        }

        public void Set(string name, object value)
        {
            if (Type < JSValueType.ObjectIndex)
            {
                Debug.LogWarning("Can not set with (" + Type + ")");
                return;
            }

            var instances = context.quickJS.instances;
            lock (instances)
            {
                if (context.Disabled) return;
                instances.arguments[0].SetValue((IntPtr)this.value);
                instances.arguments[1].SetItem(name, instances.argumentCache);
                instances.arguments[2].SetAny(context.quickJS, value, instances.argumentCache);

                if (isBehaviour())
                    instances.currentBehaviour = context.instance.target as MonoBehaviour;
                Api.QJS_Action(context.ctx, Api.SHARP_SET, 3);
                QuickJS.clearCache(instances.argumentCache);
            }
        }

        public void Set(JSAtom atom, object value)
        {
            if (Type < JSValueType.ObjectIndex)
            {
                Debug.LogWarning("Can not set with (" + Type + ")");
                return;
            }

            if (atom.Context != context.quickJS)
            {
                Debug.LogWarning("Atom context not match");
                return;
            }

            var instances = context.quickJS.instances;
            lock (instances)
            {
                if (context.Disabled) return;
                instances.arguments[0].SetValue((IntPtr)this.value);
                instances.arguments[1].SetItem(unchecked((int)atom.Target));
                instances.arguments[2].SetAny(context.quickJS, value, instances.argumentCache);

                if (isBehaviour())
                    instances.currentBehaviour = context.instance.target as MonoBehaviour;
                Api.QJS_Action(context.ctx, Api.SHARP_SET_ATOM, 3);
                QuickJS.clearCache(instances.argumentCache);
            }
        }

        public void Set(int index, object value)
        {
            if (Type < JSValueType.ObjectIndex)
            {
                Debug.LogWarning("Can not set with (" + Type + ")");
                return;
            }

            var instances = context.quickJS.instances;
            lock (instances)
            {
                if (context.Disabled) return;
                instances.arguments[0].SetValue((IntPtr)this.value);
                instances.arguments[1].SetItem(index);
                instances.arguments[2].SetAny(context.quickJS, value, instances.argumentCache);

                if (isBehaviour())
                    instances.currentBehaviour = context.instance.target as MonoBehaviour;
                Api.QJS_Action(context.ctx, Api.SHARP_SET_INDEX, 3);
                QuickJS.clearCache(instances.argumentCache);
            }
        }

        /**
         * Call this object as a function.
         */
        public JSValue Invoke(JSValue this_value, params object[] argv)
        {
            if (Type < JSValueType.ObjectIndex)
            {
                Debug.LogWarning("Can not invoke with (" + Type + ")");
                return Zero;
            }

            var instances = context.quickJS.instances;
            lock (instances)
            {
                instances.arguments[0].SetValue((IntPtr)value);
                if (this_value.Type >= JSValueType.ObjectIndex)
                {
                    instances.arguments[1].SetValue((IntPtr)this_value.value);
                }
                else
                {
                    instances.arguments[1].SetNull();
                }
                for (int i = 0, t = argv.Length; i < t; ++i)
                {
                    instances.arguments[2 + i].SetAny(context.quickJS, argv[i], instances.argumentCache);
                }

                if (isBehaviour())
                    instances.currentBehaviour = context.instance.target as MonoBehaviour;
                int len = Api.QJS_Action(context.ctx, Api.SHARP_INVOKE, 2 + argv.Length);
                QuickJS.clearCache(instances.argumentCache);

                var ret = len > 0 ? instances.results[0].ToValue(context.quickJS) : Zero;
                Api.QJS_ClearResults(context.ctx);
                return ret;
            }
        }

        public override string ToString()
        {
            if (Type == JSValueType.JSObject)
            {
                var instances = context.quickJS.instances;
                lock (instances)
                {
                    StringBuilder stringBuilder = new StringBuilder(256);

                    if (isBehaviour())
                        instances.currentBehaviour = context.instance.target as MonoBehaviour;
                    Api.QJS_ToString(context.ctx, (IntPtr)value, stringBuilder);
                    return stringBuilder.ToString();
                }
            }
            if (Value == null)
            {
                return "null";
            }
            else
            {
                return Value.ToString();
            }
        }

        /**
         * Create a object by this constructor.
         * If this is not a constructor JSValue.Zero would be returned.
         */
        public JSValue New(params object[] argv)
        {
            if (Type == JSValueType.JSObject)
            {
                var instances = context.quickJS.instances;
                lock (instances)
                {
                    instances.arguments[0].SetValue((IntPtr)value);
                    for (int i = 0, t = argv.Length; i < t; ++i)
                    {
                        instances.arguments[1 + i].SetAny(context.quickJS, argv[i], instances.argumentCache);
                    }

                    if (isBehaviour())
                        instances.currentBehaviour = context.instance.target as MonoBehaviour;
                    int len = Api.QJS_Action(context.ctx, Api.SHARP_NEW, 1 + argv.Length);
                    QuickJS.clearCache(instances.argumentCache);

                    var ret = len > 0 ? instances.results[0].ToValue(context.quickJS) : Zero;
                    Api.QJS_ClearResults(context.ctx);
                    return ret;
                }
            }
            else
            {
                Debug.LogError("Only JSObject could new a value. " + Type);
                return Zero;
            }
        }

        public bool IsInt8Array()
        {
            if (Type == JSValueType.JSObject)
            {
                return context.quickJS.isInt8Array((IntPtr)value);
            }
            return false;
        }
        public bool IsUint8Array()
        {
            if (Type == JSValueType.JSObject)
            {
                return context.quickJS.isUint8Array((IntPtr)value);
            }
            return false;
        }

        public bool IsInt16Array()
        {
            if (Type == JSValueType.JSObject)
            {
                return context.quickJS.isInt16Array((IntPtr)value);
            }
            return false;
        }
        public bool IsUint16Array()
        {
            if (Type == JSValueType.JSObject)
            {
                return context.quickJS.isUint16Array((IntPtr)value);
            }
            return false;
        }

        public bool IsInt32Array()
        {
            if (Type == JSValueType.JSObject)
            {
                return context.quickJS.isInt32Array((IntPtr)value);
            }
            return false;
        }
        public bool IsUint32Array()
        {
            if (Type == JSValueType.JSObject)
            {
                return context.quickJS.isUint32Array((IntPtr)value);
            }
            return false;
        }

        public bool IsInt64Array()
        {
            if (Type == JSValueType.JSObject)
            {
                return context.quickJS.isInt64Array((IntPtr)value);
            }
            return false;
        }
        public bool IsUint64Array()
        {
            if (Type == JSValueType.JSObject)
            {
                return context.quickJS.isUint64Array((IntPtr)value);
            }
            return false;
        }

        public bool IsFloat32Array()
        {
            if (Type == JSValueType.JSObject)
            {
                return context.quickJS.isFloat32Array((IntPtr)value);
            }
            return false;
        }
        public bool IsFloat64Array()
        {
            if (Type == JSValueType.JSObject)
            {
                return context.quickJS.isFloat64Array((IntPtr)value);
            }
            return false;
        }
        public bool IsFunction()
        {
            if (Type == JSValueType.JSObject)
            {
                return Api.QJS_IsFunction(context.quickJS.ctx, (IntPtr)value);
            }
            return false;
        }

        public Delegate MakeDelegate(Type type)
        {
            if(IsFunction())
            {
                object ret = null;
                if (Utils.ConvertDelegate(this, type, ref ret))
                {
                    return ret as Delegate;
                }
            }
            return null;
        }

        public T MakeDelegate<T>() where T : Delegate
        {
            return MakeDelegate(typeof(T)) as T;
        }

        public uint TypedArrayLength()
        {
            if (Type == JSValueType.JSObject)
            {
                return Api.QJS_GetTypedArrayLength(context.ctx, (IntPtr)value);
            }
            return 0;
        }

        public int GetLength()
        {
            if (Type == JSValueType.JSObject)
            {
                return Api.QJS_GetFunctionLength(context.ctx, (IntPtr)value);
            }
            return 0;
        }

        public bool CopyArrayBuffer(IntPtr dis, uint offset, uint length)
        {
            if (Type == JSValueType.JSObject)
            {
                return Api.QJS_CopyArrayBuffer(context.ctx, (IntPtr)value, dis, offset, length);
            }
            return false;
        }

        public bool Equals(JSValue other)
        {
            if (System.Object.ReferenceEquals(other, null))
            {
                return false;
            }

            // Optimization for a common success case.
            if (System.Object.ReferenceEquals(this, other))
            {
                return true;
            }

            if (other.Type == Type)
            {
                if (Type == JSValueType.JSObject)
                {
                    return context.ctx == other.context.ctx && Api.QJS_Equals((IntPtr)value, (IntPtr)other.value);
                }
                else if (Type == JSValueType.Object) {
                    return context.ctx == other.context.ctx && context.instance == other.context.instance;
                }
                else
                {
                    return value == other.value;
                }
            }
            return false;
        }

        public override bool Equals(object other)
        {
            if (other is JSValue)
            {
                return Equals(other as JSValue);
            }
            return false;
        }

        public override int GetHashCode()
        {
            int hashCode = 1148455455;
            hashCode = hashCode * -1521134295 + type.GetHashCode();
            hashCode = hashCode * -1521134295 + EqualityComparer<object>.Default.GetHashCode(value);
            return hashCode;
        }

        public static bool operator ==(JSValue v1, JSValue v2)
        {
            // Check for null on left side.
            if (System.Object.ReferenceEquals(v1, null))
            {
                if (System.Object.ReferenceEquals(v2, null))
                {
                    // null == null = true.
                    return true;
                }

                // Only the left side is null.
                return false;
            }
            // Equals handles case of null on right side.
            return v1.Equals(v2);
        }

        public static bool operator !=(JSValue v1, JSValue v2)
        {
            return !(v1 == v2);
        }

        public bool IsNull
        {
            get
            {
                return Type == JSValueType.Null;
            }
        }

        bool IConvertible.ToBoolean(IFormatProvider provider)
        {
            switch (Type)
            {
                case JSValueType.Bool:
                    return (bool)value;
                case JSValueType.Int:
                    return (int)value != 0;
                case JSValueType.Long:
                    return (long)value != 0;
                case JSValueType.Double:
                    return (double)value != 0;
                case JSValueType.Null:
                    return false;
                default:
                    return true;
            }
        }

        byte IConvertible.ToByte(IFormatProvider provider)
        {
            return Convert.ToByte(value);
        }

        char IConvertible.ToChar(IFormatProvider provider)
        {
            return Convert.ToChar(value);
        }

        DateTime IConvertible.ToDateTime(IFormatProvider provider)
        {
            return Convert.ToDateTime(value);
        }

        decimal IConvertible.ToDecimal(IFormatProvider provider)
        {
            return Convert.ToDecimal(value);
        }

        double IConvertible.ToDouble(IFormatProvider provider)
        {
            return Convert.ToDouble(value);
        }

        short IConvertible.ToInt16(IFormatProvider provider)
        {
            return Convert.ToInt16(value);
        }

        int IConvertible.ToInt32(IFormatProvider provider)
        {
            return Convert.ToInt32(value);
        }

        long IConvertible.ToInt64(IFormatProvider provider)
        {
            return Convert.ToInt64(value);
        }

        sbyte IConvertible.ToSByte(IFormatProvider provider)
        {
            return Convert.ToSByte(value);
        }

        float IConvertible.ToSingle(IFormatProvider provider)
        {
            return Convert.ToSingle(value);
        }

        string IConvertible.ToString(IFormatProvider provider)
        {
            return ToString();
        }

        object IConvertible.ToType(Type conversionType, IFormatProvider provider)
        {
            object obj;
            switch (Type)
            {
                case JSValueType.Object:
                    {
                        obj = context.instance.target;
                        break;
                    }
                case JSValueType.Class:
                    {
                        obj = context.clazz.target;
                        break;
                    }
                default:
                    obj = value;
                    break;
            }
            return Convert.ChangeType(obj, conversionType);
        }

        ushort IConvertible.ToUInt16(IFormatProvider provider)
        {
            return Convert.ToUInt16(value);
        }

        uint IConvertible.ToUInt32(IFormatProvider provider)
        {
            return Convert.ToUInt32(value);
        }

        ulong IConvertible.ToUInt64(IFormatProvider provider)
        {
            return Convert.ToUInt64(value);
        }

        public TypeCode GetTypeCode()
        {
            switch (Type)
            {
                case JSValueType.Bool: return TypeCode.Boolean;
                case JSValueType.Int: return TypeCode.Int32;
                case JSValueType.Long: return TypeCode.Int64;
                case JSValueType.Double: return TypeCode.Double;
                case JSValueType.String: return TypeCode.String;
                case JSValueType.Class:
                case JSValueType.Object:
                case JSValueType.JSObject:
                    return TypeCode.Object;
                default:
                    return TypeCode.DBNull;
            }
        }

        public AwaitYield Await()
        {
            AwaitYield await = new AwaitYield();
            System.Action<JSValue> success = (value) => {
                await.Complete(true, value);
            };
            Call("then", new object[] { success });
            System.Action<JSValue> reject = (value) => {
                await.Complete(false, value);
            };
            Call("catch", new object[] { reject });
            return await;
        }
    }

}