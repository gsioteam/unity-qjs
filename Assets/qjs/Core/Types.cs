using System;
using System.Collections;
using System.Collections.Concurrent;
using System.Collections.Generic;
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
        Array,
        Object,
    }

    public class Annotation
    {
        public string name;
        public FieldType type;
        public object defaultValue;
    }

    class Instance
    {
        // public Class clazz;
        public int id;
        public IntPtr ptr;
        public object target;

        public bool disabled = false;

        public List<Annotation> annotations;
    }

    class Class
    {
        public Type target;
        public IntPtr ptr;
        public int id;

        public ConstructorInfo[] constructors;
        public FieldInfo[] fields;
        public MethodInfo[] methods;
        public PropertyInfo[] properties;

        public Class(Type type)
        {
            target = type;
        }
    }

    public struct FieldItem
    {
        public string key;
        public FieldObject value;
    }

    public class FieldObject
    {
        FieldType type;
        public FieldType Type
        {
            get
            {
                return type;
            }
        }

        object value;
        public object Value
        {
            get
            {
                return value;
            }
            set
            {
                if (value is bool)
                {
                    this.Booloon = (bool)value;
                } else if (value is BoundsInt)
                {
                    this.BoundsInt = (BoundsInt)value;
                } else if (value is Bounds)
                {
                    this.Bounds = (Bounds)value;
                } else if (value is Color)
                {
                    this.Color = (Color)value;
                } else if (value is double || value is float)
                {
                    this.Double = Convert.ToDouble(value);
                } else if (Utils.IsIntType(value.GetType()))
                {
                    this.Long = Convert.ToInt64(value);
                } else if (value is Quaternion)
                {
                    this.Quaternion = (Quaternion)value;
                } else if (value is RectInt)
                {
                    this.RectInt = (RectInt)value;
                } else if (value is Rect)
                {
                    this.Rect = (Rect)value;
                } else if (value is string)
                {
                    this.String = (string)value;
                } else if (value is Vector2)
                {
                    this.Vector2 = (Vector2)value;
                } else if (value is Vector2Int)
                {
                    this.Vector2Int = (Vector2Int)value;
                } else if (value is Vector3)
                {
                    this.Vector3 = (Vector3)value;
                } else if (value is Vector3Int)
                {
                    this.Vector3Int = (Vector3Int)value;
                } else if (value is Vector4)
                {
                    this.Vector4 = (Vector4)value;
                } else
                {
                    throw new Exception("Not support " + value);
                }
            }
        }

        JSONObject node;
        public JSONNode Node
        {
            get
            {
                return node;
            }
        }

        public FieldObject(Annotation annotation) {
            type = annotation.type;
            switch (type)
            {
                case FieldType.Bool:
                    {
                        bool val = annotation.defaultValue == null ? false : (bool)annotation.defaultValue;
                        value = val;
                        node = new JSONObject();
                        node["type"] = (int)type;
                        node["value"] = val;
                        break;
                    }
                case FieldType.BoundsInt:
                    {
                        BoundsInt val = annotation.defaultValue == null ? new BoundsInt() : (BoundsInt)annotation.defaultValue;
                        value = val;
                        node = new JSONObject();
                        node["type"] = (int)type;
                        JSONArray array = new JSONArray();
                        array.Add(val.min.x);
                        array.Add(val.min.y);
                        array.Add(val.min.z);
                        array.Add(val.max.x);
                        array.Add(val.max.y);
                        array.Add(val.max.z);
                        node["value"] = array;
                        break;
                    }
                case FieldType.Bounds:
                    {
                        Bounds val = annotation.defaultValue == null ? new Bounds() : (Bounds)annotation.defaultValue;
                        value = val;
                        node = new JSONObject();
                        node["type"] = (int)type;
                        JSONArray array = new JSONArray();
                        array.Add(val.min.x);
                        array.Add(val.min.y);
                        array.Add(val.min.z);
                        array.Add(val.max.x);
                        array.Add(val.max.y);
                        array.Add(val.max.z);
                        node["value"] = array;
                        break;
                    }
                case FieldType.Color:
                    {
                        Color val = annotation.defaultValue == null ? Color.black : (Color)annotation.defaultValue;
                        value = val;
                        node = new JSONObject();
                        node["type"] = (int)type;
                        JSONArray array = new JSONArray();
                        array.Add(val.r);
                        array.Add(val.g);
                        array.Add(val.b);
                        array.Add(val.a);
                        node["value"] = array;
                        break;
                    }
                case FieldType.Double:
                    {
                        double val = annotation.defaultValue == null ? 0 : (double)annotation.defaultValue;
                        value = val;
                        node = new JSONObject();
                        node["type"] = (int)type;
                        node["value"] = val;
                        break;
                    }
                case FieldType.Long:
                    {
                        long val = annotation.defaultValue == null ? 0 : (long)annotation.defaultValue;
                        value = val;
                        node = new JSONObject();
                        node["type"] = (int)type;
                        node["value"] = val;
                        break;
                    }
                case FieldType.Quaternion:
                    {
                        Quaternion val = annotation.defaultValue == null ? Quaternion.identity : (Quaternion)annotation.defaultValue;
                        value = val;
                        node = new JSONObject();
                        node["type"] = (int)type;
                        JSONArray array = new JSONArray();
                        array.Add(val.x);
                        array.Add(val.y);
                        array.Add(val.z);
                        array.Add(val.w);
                        node["value"] = array;
                        break;
                    }
                case FieldType.RectInt:
                    {
                        RectInt val = annotation.defaultValue == null ? new RectInt() : (RectInt)annotation.defaultValue;
                        value = val;
                        node = new JSONObject();
                        node["type"] = (int)type;
                        JSONArray array = new JSONArray();
                        array.Add(val.x);
                        array.Add(val.y);
                        array.Add(val.width);
                        array.Add(val.height);
                        node["value"] = array;
                        break;
                    }
                case FieldType.Rect:
                    {
                        Rect val = annotation.defaultValue == null ? new Rect() : (Rect)annotation.defaultValue;
                        value = val;
                        node = new JSONObject();
                        node["type"] = (int)type;
                        JSONArray array = new JSONArray();
                        array.Add(val.x);
                        array.Add(val.y);
                        array.Add(val.width);
                        array.Add(val.height);
                        node["value"] = array;
                        break;
                    }
                case FieldType.String:
                    {
                        string val = annotation.defaultValue == null ? "" : (string)annotation.defaultValue;
                        value = val;
                        node = new JSONObject();
                        node["type"] = (int)type;
                        node["value"] = val;
                        break;
                    }
                case FieldType.Vector2:
                    {
                        Vector2 val = annotation.defaultValue == null ? Vector2.zero : (Vector2)annotation.defaultValue;
                        value = val;
                        node = new JSONObject();
                        node["type"] = (int)type;
                        JSONArray array = new JSONArray();
                        array.Add(val.x);
                        array.Add(val.y);
                        node["value"] = array;
                        break;
                    }
                case FieldType.Vector2Int:
                    {
                        Vector2Int val = annotation.defaultValue == null ? Vector2Int.zero : (Vector2Int)annotation.defaultValue;
                        value = val;
                        node = new JSONObject();
                        node["type"] = (int)type;
                        JSONArray array = new JSONArray();
                        array.Add(val.x);
                        array.Add(val.y);
                        node["value"] = array;
                        break;
                    }
                case FieldType.Vector3:
                    {
                        Vector3 val = annotation.defaultValue == null ? Vector3.zero : (Vector3)annotation.defaultValue;
                        value = val;
                        node = new JSONObject();
                        node["type"] = (int)type;
                        JSONArray array = new JSONArray();
                        array.Add(val.x);
                        array.Add(val.y);
                        array.Add(val.z);
                        node["value"] = array;
                        break;
                    }
                case FieldType.Vector3Int:
                    {
                        Vector3Int val = annotation.defaultValue == null ? Vector3Int.zero : (Vector3Int)annotation.defaultValue;
                        value = val;
                        node = new JSONObject();
                        node["type"] = (int)type;
                        JSONArray array = new JSONArray();
                        array.Add(val.x);
                        array.Add(val.y);
                        array.Add(val.z);
                        node["value"] = array;
                        break;
                    }
                case FieldType.Vector4:
                    {
                        Vector4 val = annotation.defaultValue == null ? Vector4.zero : (Vector4)annotation.defaultValue;
                        value = val;
                        node = new JSONObject();
                        node["type"] = (int)type;
                        JSONArray array = new JSONArray();
                        array.Add(val.x);
                        array.Add(val.y);
                        array.Add(val.z);
                        array.Add(val.w);
                        node["value"] = array;
                        break;
                    }
                case FieldType.Array:
                    {
                        break;
                    }
                case FieldType.Object:
                    {
                        break;
                    }
            }
        }

        public FieldObject(JSONObject node)
        {
            this.node = node;
            if (!node.IsObject)
            {
                throw new Exception("Node must be a Object");
            }
            type = (FieldType)node["type"].AsInt;
            switch (type)
            {
                case FieldType.Bool:
                    {
                        value = node["value"].AsBool;
                        break;
                    }
                case FieldType.BoundsInt:
                    {
                        JSONArray arr = node["value"].AsArray;
                        value = new BoundsInt(
                            arr[0].AsInt,
                            arr[1].AsInt,
                            arr[2].AsInt,
                            arr[3].AsInt,
                            arr[4].AsInt,
                            arr[5].AsInt);
                        break;
                    }
                case FieldType.Bounds:
                    {
                        JSONArray arr = node["value"].AsArray;
                        value = new Bounds(
                            new Vector3(
                                arr[0].AsFloat,
                                arr[1].AsFloat,
                                arr[2].AsFloat
                                ),
                            new Vector3(
                                arr[0].AsFloat,
                                arr[1].AsFloat,
                                arr[2].AsFloat
                                ));
                        break;
                    }
                case FieldType.Color:
                    {
                        JSONArray arr = node["value"].AsArray;
                        value = new Color(
                            arr[0].AsFloat,
                            arr[1].AsFloat,
                            arr[2].AsFloat,
                            arr[3].AsFloat
                            );
                        break;
                    }
                case FieldType.Double:
                    {
                        value = node["value"].AsDouble;
                        break;
                    }
                case FieldType.Long:
                    {
                        value = node["value"].AsLong;
                        break;
                    }
                case FieldType.Array:
                    {
                        JSONArray array = node["value"].AsArray;
                        List<FieldObject> items = new List<FieldObject>();
                        for (int i = 0, t = array.Count; i < t; ++i)
                        {
                            items.Add(new FieldObject(array[i].AsObject));
                        }
                        value = items.ToArray();
                        break;
                    }
                case FieldType.Object:
                    {
                        JSONArray array = node["value"].AsArray;
                        List<FieldItem> items = new List<FieldItem>();
                        for (int i = 0, t = array.Count; i < t; ++i)
                        {
                            JSONObject obj = array[i].AsObject;
                            FieldItem item = new FieldItem();
                            item.key = obj["key"];
                            item.value = new FieldObject(obj["value"].AsObject);
                            items.Add(item);
                        }
                        value = items.ToArray();
                        break;
                    }
                case FieldType.Quaternion:
                    {
                        JSONArray arr = node["value"].AsArray;
                        value = new Quaternion(
                            arr[0].AsFloat,
                            arr[1].AsFloat,
                            arr[2].AsFloat,
                            arr[3].AsFloat
                            );
                        break;
                    }
                case FieldType.Rect:
                    {
                        JSONArray arr = node["value"].AsArray;
                        value = new Rect(
                            arr[0].AsFloat,
                            arr[1].AsFloat,
                            arr[2].AsFloat,
                            arr[3].AsFloat
                            );
                        break;
                    }
                case FieldType.RectInt:
                    {
                        JSONArray arr = node["value"].AsArray;
                        value = new RectInt(
                            arr[0].AsInt,
                            arr[1].AsInt,
                            arr[2].AsInt,
                            arr[3].AsInt
                            );
                        break;
                    }
                case FieldType.String:
                    {
                        value = (string)node["value"];
                        break;
                    }
                case FieldType.Vector2:
                    {
                        JSONArray arr = node["value"].AsArray;
                        value = new Vector2(
                            arr[0].AsFloat,
                            arr[1].AsFloat
                            );
                        break;
                    }
                case FieldType.Vector2Int:
                    {
                        JSONArray arr = node["value"].AsArray;
                        value = new Vector2(
                            arr[0].AsInt,
                            arr[1].AsInt
                            );
                        break;
                    }
                case FieldType.Vector3:
                    {
                        JSONArray arr = node["value"].AsArray;
                        value = new Vector3(
                            arr[0].AsFloat,
                            arr[1].AsFloat,
                            arr[2].AsFloat
                            );
                        break;
                    }
                case FieldType.Vector3Int:
                    {
                        JSONArray arr = node["value"].AsArray;
                        value = new Vector3Int(
                            arr[0].AsInt,
                            arr[1].AsInt,
                            arr[2].AsInt
                            );
                        break;
                    }
                case FieldType.Vector4:
                    {
                        JSONArray arr = node["value"].AsArray;
                        value = new Vector4(
                            arr[0].AsFloat,
                            arr[1].AsFloat,
                            arr[2].AsFloat,
                            arr[3].AsFloat
                            );
                        break;
                    }
            }
        }

        public bool Booloon
        {
            get
            {
                return (bool)value;
            }
            set
            {
                type = FieldType.Bool;
                this.value = value;
                node["type"] = (int)type;
                node["value"] = value;
            }
        }

        public BoundsInt BoundsInt
        {
            get
            {
                return (BoundsInt)value;
            }
            set
            {
                type = FieldType.BoundsInt;
                this.value = value;
                node["type"] = (int)type;
                JSONArray array = new JSONArray();
                array.Add(value.min.x);
                array.Add(value.min.y);
                array.Add(value.min.z);
                array.Add(value.max.x);
                array.Add(value.max.y);
                array.Add(value.max.z);
                node["value"] = array;
            }
        }

        public Bounds Bounds
        {
            get
            {
                return (Bounds)value;
            }
            set
            {
                type = FieldType.Bounds;
                this.value = value;
                node["type"] = (int)type;
                JSONArray array = new JSONArray();
                array.Add(value.min.x);
                array.Add(value.min.y);
                array.Add(value.min.z);
                array.Add(value.max.x);
                array.Add(value.max.y);
                array.Add(value.max.z);
                node["value"] = array;
            }
        }

        public Color Color
        {
            get
            {
                return (Color)value;
            }
            set
            {
                type = FieldType.Color;
                this.value = value;
                node["type"] = (int)type;
                JSONArray array = new JSONArray();
                array.Add(value.r);
                array.Add(value.g);
                array.Add(value.b);
                array.Add(value.a);
                node["value"] = array;
            }
        }

        public double Double
        {
            get
            {
                return (double)value;
            }
            set
            {
                type = FieldType.Double;
                this.value = value;
                node["type"] = (int)type;
                node["value"] = value;
            }
        }

        public long Long
        {
            get
            {
                return (long)value;
            }
            set
            {
                type = FieldType.Long;
                this.value = value;
                node["type"] = (int)type;
                node["value"] = value;
            }
        }

        public Quaternion Quaternion
        {
            get
            {
                return (Quaternion)value;
            }
            set
            {
                type = FieldType.Quaternion;
                this.value = value;
                node["type"] = (int)type;
                JSONArray array = new JSONArray();
                array.Add(value.x);
                array.Add(value.y);
                array.Add(value.z);
                array.Add(value.w);
                node["value"] = array;
            }
        }

        public Rect Rect
        {
            get
            {
                return (Rect)value;
            }
            set
            {
                type = FieldType.Rect;
                this.value = value;
                node["type"] = (int)type;
                JSONArray array = new JSONArray();
                array.Add(value.x);
                array.Add(value.y);
                array.Add(value.width);
                array.Add(value.height);
                node["value"] = array;
            }
        }

        public RectInt RectInt
        {
            get
            {
                return (RectInt)value;
            }
            set
            {
                type = FieldType.RectInt;
                this.value = value;
                node["type"] = (int)type;
                JSONArray array = new JSONArray();
                array.Add(value.x);
                array.Add(value.y);
                array.Add(value.width);
                array.Add(value.height);
                node["value"] = array;
            }
        }

        public string String
        {
            get
            {
                return (string)value;
            }
            set
            {
                type = FieldType.String;
                this.value = value;
                node["type"] = (int)type;
                node["value"] = value;
            }
        }

        public Vector2 Vector2
        {
            get
            {
                return (Vector2)value;
            }
            set
            {
                type = FieldType.Vector2;
                this.value = value;
                node["type"] = (int)type;
                JSONArray array = new JSONArray();
                array.Add(value.x);
                array.Add(value.y);
                node["value"] = array;
            }
        }

        public Vector2Int Vector2Int
        {
            get
            {
                return (Vector2Int)value;
            }
            set
            {
                type = FieldType.Vector2Int;
                this.value = value;
                node["type"] = (int)type;
                JSONArray array = new JSONArray();
                array.Add(value.x);
                array.Add(value.y);
                node["value"] = array;
            }
        }

        public Vector3 Vector3
        {
            get
            {
                return (Vector3)value;
            }
            set
            {
                type = FieldType.Vector3;
                this.value = value;
                node["type"] = (int)type;
                JSONArray array = new JSONArray();
                array.Add(value.x);
                array.Add(value.y);
                array.Add(value.z);
                node["value"] = array;
            }
        }

        public Vector3Int Vector3Int
        {
            get
            {
                return (Vector3Int)value;
            }
            set
            {
                type = FieldType.Vector3Int;
                this.value = value;
                node["type"] = (int)type;
                JSONArray array = new JSONArray();
                array.Add(value.x);
                array.Add(value.y);
                array.Add(value.z);
                node["value"] = array;
            }
        }

        public Vector4 Vector4
        {
            get
            {
                return (Vector4)value;
            }
            set
            {
                type = FieldType.Vector4;
                this.value = value;
                node["type"] = (int)type;
                JSONArray array = new JSONArray();
                array.Add(value.x);
                array.Add(value.y);
                array.Add(value.z);
                array.Add(value.w);
                node["value"] = array;
            }
        }

        private FieldObject()
        {
        }

        public static FieldObject NewObject(Annotation[] annotations)
        {
            FieldObject obj = new FieldObject();
            if (annotations != null)
                obj.Update(annotations);
            return obj;
        }

        public void Update(Annotation[] annotations) {
            type = FieldType.Object;

            if (node == null)
            {
                node = new JSONObject();
            }
            node["type"] = (int)type;
            Dictionary<string, FieldItem> oldObjects = new Dictionary<string, FieldItem>();
            if (value is FieldItem[])
            {
                foreach (var item in (value as FieldItem[])) {
                    oldObjects[item.key] = item;
                }
            }

            List<FieldItem> items = new List<FieldItem>();
            JSONArray array = new JSONArray();
            foreach (var annotation in annotations)
            {
                FieldItem item;
                if (oldObjects.ContainsKey(annotation.name))
                {
                    var old = oldObjects[annotation.name];
                    if (old.value.type == annotation.type)
                    {
                        item = old;
                    } else
                    {
                        item = new FieldItem();
                        item.key = annotation.name;
                        item.value = new FieldObject(annotation);
                    }
                } else
                {
                    item = new FieldItem();
                    item.key = annotation.name;
                    item.value = new FieldObject(annotation);
                }
                JSONObject obj = new JSONObject();
                obj["key"] = item.key;
                obj["value"] = item.value.Node;
                array.Add(obj);
                items.Add(item);
            }
            value = items.ToArray();
            node["value"] = array;
        }

        public int Length
        {
            get
            {
                switch (type)
                {
                    case FieldType.Object:
                        {
                            return (value as FieldItem[]).Length;
                        }
                    case FieldType.Array:
                        {
                            return (value as FieldObject[]).Length;
                        }
                    default: return 1;
                }
            }
        }

        public FieldItem GetItem(int index)
        {
            if (type == FieldType.Object)
            {
                return (value as FieldItem[])[index];
            }
            throw new Exception("This is not a Object");
        }

        public FieldObject this[int index]
        {
            get
            {
                switch (type)
                {
                    case FieldType.Object:
                        {
                            return (value as FieldItem[])[index].value;
                        }
                    case FieldType.Array:
                        {
                            return (value as FieldObject[])[index];
                        }
                    default: return null;
                }
            }
        }

        public override string ToString()
        {
            return node.ToString();
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