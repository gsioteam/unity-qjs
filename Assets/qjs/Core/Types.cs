using System;
using System.Collections;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Globalization;
using System.Reflection;
using System.Runtime.Serialization;
using System.Threading;
using UnityEditor;
using UnityEngine;

namespace qjs
{
    public enum FieldType
    {
        Bool = 0,
        BoundsInt,
        Bounds,
        Color,
        Double,
        Long,
        Quaternion,
        RectInt,
        Rect,
        String,
        Vector2Int,
        Vector2,
        Vector3Int,
        Vector3,
        Vector4,
        Object,
        Array,
        Unkown,
    }

    public struct Annotation
    {
        public string name;
        public FieldType type;
        public object defaultValue;
        public Type objectType;

        public static Annotation From(JSValue value)
        {
            int len = value.GetLength();
            Annotation annotation = new Annotation();
            annotation.name = value.Get(0).ToString();
            annotation.type = (FieldType)Convert.ToInt32(value.Get(1));
            if (len > 2)
            {
                var val = value.Get(2);
                annotation.defaultValue = val.Value;
            }
            if (annotation.type == FieldType.Object || annotation.type == FieldType.Array)
            {
                while (true)
                {
                    if (len > 3)
                    {
                        var typeValue = value.Get(3);
                        if (typeValue.Type == JSValueType.Class)
                        {
                            annotation.objectType = typeValue.Value as Type;
                            break;
                        }
                    }
                    annotation.objectType = typeof(UnityEngine.Object);
                    break;
                }
            }
            return annotation;
        }
    }

    class Instance
    {
        // public Class clazz;
        public int id;
        public IntPtr ptr;
        public object target;
        public Container container;

        public bool disabled = false;
    }

    class Class
    {
        public Type target;
        public IntPtr ptr;
        public int id;

        public MethodBase[] constructors;
        public FieldInfo[] fields;
        public MethodBase[] methods;
        public PropertyInfo[] properties;

        public Class(Type type)
        {
            target = type;
        }
    }

    public delegate object InvokeHandler<T>(T target, object[] argv);

    public struct ClassInfo
    {
        private Class clazz;

        internal ClassInfo(Class clazz)
        {
            this.clazz = clazz;
        }

        public void RegisterMethod<T>(MethodInfo methodInfo, InvokeHandler<T> invoke)
        {
            for (int i = 0, t = clazz.methods.Length; i < t; ++i)
            {
                var method = clazz.methods[i];
                if (method == methodInfo)
                {
                    FixedMethodInfo<T> fixedMethod = new FixedMethodInfo<T>(methodInfo, invoke);
                    clazz.methods[i] = fixedMethod;
                    break;
                }
            }
        }
    }

    class CachedMethodInfo : MethodBase
    {
        private MethodBase method;
        public CachedMethodInfo(MethodBase methodInfo)
        {
            method = methodInfo;
        }

        public override MethodAttributes Attributes => method.Attributes;

        public override RuntimeMethodHandle MethodHandle => method.MethodHandle;

        public override Type DeclaringType => method.DeclaringType;

        public override MemberTypes MemberType => method.MemberType;

        public override string Name => method.Name;

        public override Type ReflectedType => method.ReflectedType;

        public override object[] GetCustomAttributes(bool inherit)
        {
            return method.GetCustomAttributes(inherit);
        }

        public override object[] GetCustomAttributes(Type attributeType, bool inherit)
        {
            return method.GetCustomAttributes(attributeType, inherit);
        }

        public override MethodImplAttributes GetMethodImplementationFlags()
        {
            return method.GetMethodImplementationFlags();
        }

        private ParameterInfo[] parameters;
        public override ParameterInfo[] GetParameters()
        {
            if (parameters == null)
            {
                parameters = method.GetParameters();
            }
            return parameters;
        }

        public override object Invoke(object obj, BindingFlags invokeAttr, Binder binder, object[] parameters, CultureInfo culture)
        {
            return method.Invoke(obj, invokeAttr, binder, parameters, culture);
        }

        public override bool IsDefined(Type attributeType, bool inherit)
        {
            return method.IsDefined(attributeType, inherit);
        }
    }

    class FixedMethodInfo<T> : MethodBase
    {
        private MethodBase method;
        private InvokeHandler<T> invoke;

        public FixedMethodInfo(MethodBase methodInfo, InvokeHandler<T> invoke)
        {
            this.method = methodInfo;
            this.invoke = invoke;
        }

        public override MethodAttributes Attributes => method.Attributes;

        public override RuntimeMethodHandle MethodHandle => method.MethodHandle;

        public override Type DeclaringType => method.DeclaringType;

        public override MemberTypes MemberType => method.MemberType;

        public override string Name => method.Name;

        public override Type ReflectedType => method.ReflectedType;

        public override object[] GetCustomAttributes(bool inherit)
        {
            return method.GetCustomAttributes(inherit);
        }

        public override object[] GetCustomAttributes(Type attributeType, bool inherit)
        {
            return method.GetCustomAttributes(attributeType, inherit);
        }

        public override MethodImplAttributes GetMethodImplementationFlags()
        {
            return method.GetMethodImplementationFlags();
        }

        public override ParameterInfo[] GetParameters()
        {
            return method.GetParameters();
        }

        public override object Invoke(object obj, BindingFlags invokeAttr, Binder binder, object[] parameters, CultureInfo culture)
        {
            return invoke((T)obj, parameters);
        }

        public override bool IsDefined(Type attributeType, bool inherit)
        {
            return method.IsDefined(attributeType, inherit);
        }
    }

    abstract class FunctionMaker
    {
        public JSValue target;
        public Delegate get(Type type)
        {
            return Delegate.CreateDelegate(type, this, "Invoke");
        }
    }

    class FunctionAction : FunctionMaker
    {
        public void Invoke()
        {
            this.target.Invoke(JSValue.Zero);
        }
    }
    class FunctionAction<T1> : FunctionMaker
    {
        public void Invoke(T1 t1)
        {
            this.target.Invoke(JSValue.Zero, t1);
        }
    }
    class FunctionAction<T1, T2> : FunctionMaker
    {
        public void Invoke(T1 t1, T2 t2)
        {
            this.target.Invoke(JSValue.Zero, t1, t2);
        }
    }
    class FunctionAction<T1, T2, T3> : FunctionMaker
    {
        public void Invoke(T1 t1, T2 t2, T3 t3)
        {
            this.target.Invoke(JSValue.Zero, t1, t2, t3);
        }
    }
    class FunctionAction<T1, T2, T3, T4> : FunctionMaker
    {
        public void Invoke(T1 t1, T2 t2, T3 t3, T4 t4)
        {
            this.target.Invoke(JSValue.Zero, t1, t2, t3, t4);
        }
    }
    class FunctionAction<T1, T2, T3, T4, T5> : FunctionMaker
    {
        public void Invoke(T1 t1, T2 t2, T3 t3, T4 t4, T5 t5)
        {
            this.target.Invoke(JSValue.Zero, t1, t2, t3, t4, t5);
        }
    }
    class FunctionAction<T1, T2, T3, T4, T5, T6> : FunctionMaker
    {
        public void Invoke(T1 t1, T2 t2, T3 t3, T4 t4, T5 t5, T6 t6)
        {
            this.target.Invoke(JSValue.Zero, t1, t2, t3, t4, t5, t6);
        }
    }
    class FunctionAction<T1, T2, T3, T4, T5, T6, T7> : FunctionMaker
    {
        public void Invoke(T1 t1, T2 t2, T3 t3, T4 t4, T5 t5, T6 t6, T7 t7)
        {
            this.target.Invoke(JSValue.Zero, t1, t2, t3, t4, t5, t6, t7);
        }
    }
    class FunctionAction<T1, T2, T3, T4, T5, T6, T7, T8> : FunctionMaker
    {
        public void Invoke(T1 t1, T2 t2, T3 t3, T4 t4, T5 t5, T6 t6, T7 t7, T8 t8)
        {
            this.target.Invoke(JSValue.Zero, t1, t2, t3, t4, t5, t6, t7, t8);
        }
    }
    class FunctionFunc<R> : FunctionMaker
    {
        public R Invoke()
        {
            JSValue ret = this.target.Invoke(JSValue.Zero);
            return Utils.ConvertType<R>(ret);
        }
    }
    class FunctionFunc<T1, R> : FunctionMaker
    {
        public R Invoke(T1 t1)
        {
            JSValue ret = this.target.Invoke(JSValue.Zero, t1);
            return Utils.ConvertType<R>(ret);
        }
    }
    class FunctionFunc<T1, T2, R> : FunctionMaker
    {
        public R Invoke(T1 t1, T2 t2)
        {
            JSValue ret = this.target.Invoke(JSValue.Zero, t1, t2);
            return Utils.ConvertType<R>(ret);
        }
    }
    class FunctionFunc<T1, T2, T3, R> : FunctionMaker
    {
        public R Invoke(T1 t1, T2 t2, T3 t3)
        {
            JSValue ret = this.target.Invoke(JSValue.Zero, t1, t2, t3);
            return Utils.ConvertType<R>(ret);
        }
    }
    class FunctionFunc<T1, T2, T3, T4, R> : FunctionMaker
    {
        public R Invoke(T1 t1, T2 t2, T3 t3, T4 t4)
        {
            JSValue ret = this.target.Invoke(JSValue.Zero, t1, t2, t3, t4);
            return Utils.ConvertType<R>(ret);
        }
    }
    class FunctionFunc<T1, T2, T3, T4, T5, R> : FunctionMaker
    {
        public R Invoke(T1 t1, T2 t2, T3 t3, T4 t4, T5 t5)
        {
            JSValue ret = this.target.Invoke(JSValue.Zero, t1, t2, t3, t4, t5);
            return Utils.ConvertType<R>(ret);
        }
    }
    class FunctionFunc<T1, T2, T3, T4, T5, T6, R> : FunctionMaker
    {
        public R Invoke(T1 t1, T2 t2, T3 t3, T4 t4, T5 t5, T6 t6)
        {
            JSValue ret = this.target.Invoke(JSValue.Zero, t1, t2, t3, t4, t5, t6);
            return Utils.ConvertType<R>(ret);
        }
    }
    class FunctionFunc<T1, T2, T3, T4, T5, T6, T7, R> : FunctionMaker
    {
        public R Invoke(T1 t1, T2 t2, T3 t3, T4 t4, T5 t5, T6 t6, T7 t7)
        {
            JSValue ret = this.target.Invoke(JSValue.Zero, t1, t2, t3, t4, t5, t6, t7);
            return Utils.ConvertType<R>(ret);
        }
    }
    class FunctionFunc<T1, T2, T3, T4, T5, T6, T7, T8, R> : FunctionMaker
    {
        public R Invoke(T1 t1, T2 t2, T3 t3, T4 t4, T5 t5, T6 t6, T7 t7, T8 t8)
        {
            JSValue ret = this.target.Invoke(JSValue.Zero, t1, t2, t3, t4, t5, t6, t7, t8);
            return Utils.ConvertType<R>(ret);
        }
    }

    class WorkerInner
    {
        QuickJS quickJS;
        Thread thread;
        Semaphore semaphore;
        bool validate = true;
        WeakReference<Worker> worker;
        JSValue self;
        string codePath;

        ConcurrentQueue<string> messages = new ConcurrentQueue<string>();

        const string script = "self = global;\n" +
"self.postMessage = function(data) {\n " +
"    this.__postMessage(JSON.stringify({data, type: 'message'}));\n" +
"}\n" +
"self.__onmessage = function(message) {\n" +
"    if (this.onmessage) this.onmessage(JSON.parse(message));\n" +
"}; self;";

        public WorkerInner(QuickJS quickJS, Worker worker, string path)
        {
            semaphore = new Semaphore(0, 2);
            this.quickJS = quickJS;
            self = quickJS.Eval(script);
            self.Set("__postMessage", new Action<string>(onmessage));
            codePath = path;
            this.worker = new WeakReference<Worker>(worker);
            thread = new Thread(main);
            thread.Start();
        }

        public void main()
        {
            quickJS.Eval(quickJS.Loader.Load(codePath), codePath);
            while (validate)
            {
                string message;
                while (messages.TryDequeue(out message))
                {
                    self.Call("__onmessage", message);
                }
                quickJS.Step();
                semaphore.WaitOne();
            }
        }

        public void Stop()
        {
            validate = false;
            semaphore.Release();
        }

        public void postMessage(string message)
        {
            messages.Enqueue(message);
            semaphore.Release();
        }

        public void onmessage(string message)
        {
            Worker worker;
            if (this.worker.TryGetTarget(out worker))
            {
                worker.onmessage(message);
            }
        }
    }


    class Worker
    {
        QuickJS quickJS;
        WorkerInner inner;
        public Action<string> onGetMessage;
        WeakReference<QuickJS> parent;
        bool alive = true;

        public Worker(QuickJS quickJS, QuickJS parent, string codePath)
        {
            this.quickJS = quickJS;
            this.parent = new WeakReference<QuickJS>(parent);
            inner = new WorkerInner(quickJS, this, codePath);
        }

        public void postMessage(string message)
        {
            if (!alive) return;
            inner.postMessage(message);
        }

        internal void onmessage(string message)
        {
            if (!alive) return;
            QuickJS parent;
            if (this.parent.TryGetTarget(out parent))
            {
                parent.EnqueueTask(()=> {
                    onGetMessage(message);
                });
            }
        }

        public void terminate()
        {
            if (alive)
            {
                alive = false;
                quickJS.Destroy();
                inner.Stop();
            }
        }

        ~Worker()
        {
            if (alive)
            {
                alive = false;
                quickJS.Destroy();
                inner.Stop();
            }
        }
    }
}