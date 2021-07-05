using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace qjs
{

    [Serializable]
    public class AttrValue
    {
        [SerializeField]
        public FieldType type = FieldType.Unkown;

        [SerializeField]
        [SerializeReference]
        private object value;

        [SerializeField]
        private UnityEngine.Object _object;

        [SerializeField]
        [SerializeReference]
        private List<object> array = new List<object>() ;

        [SerializeField]
        private string typeRef;
        private Type objectType;

        private AttrValue GetValue(int idx)
        {
            return array[idx] as AttrValue;
        }

        public object Value
        {
            get
            {
                switch (type)
                {
                    case FieldType.Object:
                        {
                            if (typeof(UnityEngine.Object).IsAssignableFrom(objectType))
                            {
                                return _object;
                            } else
                            {
                                return value;
                            }
                        }
                    case FieldType.Array:
                        {
                            Type listType = objectType.MakeArrayType();
                            Array list = Activator.CreateInstance(listType, array.Count) as Array;
                            for (int i = 0, t = array.Count; i < t; ++i)
                            {
                                list.SetValue(GetValue(i).Value, i);
                            }
                            return list;
                        }
                    default:
                        return value;
                }
            }

            set
            {
                switch (type)
                {
                    case FieldType.Object:
                        {
                            if (typeof(UnityEngine.Object).IsAssignableFrom(objectType))
                            {
                                _object = value as UnityEngine.Object;
                                this.value = null;
                                array.Clear();
                            } else
                            {
                                this.value = value;
                                _object = null;
                                array.Clear();
                            }
                            break;
                        }
                    case FieldType.Array:
                        {
                            array.Clear();
                            if (value is JSValue)
                            {
                                JSValue jsValue = value as JSValue;
                                int length = jsValue.GetLength();
                                array.Capacity = length;
                                for (int i = 0; i < length; ++i)
                                {
                                    JSValue val = jsValue.Get(i);
                                    var objVal = val.Value;
                                    AttrValue attrValue = new AttrValue();
                                    attrValue.Initialize(fromType(ObjectType), ObjectType, objVal);
                                    array.Add(attrValue);
                                }
                            } else if (value != null)
                            {
                                if (value.GetType() == ObjectType.MakeArrayType())
                                {
                                    Array arr = value as Array;
                                    for (int i = 0, t = arr.Length; i < t; ++i)
                                    {
                                        AttrValue attrValue = new AttrValue();
                                        attrValue.Initialize(fromType(ObjectType), ObjectType, arr.GetValue(i));
                                        array.Add(attrValue);
                                    }
                                }
                            }
                            break;
                        }
                    default:
                        this.value = value;
                        break;
                }
            }
        }

        private Type GetType(object obj)
        {
            if (obj == null)
            {
                return null;
            }
            else {
                return obj.GetType();
            }
        }

        public void Update(FieldType type, Type objectType) {
            if (this.type != type)
            {
                if (type != FieldType.Object)
                {
                    _object = null;
                } else if (type != FieldType.Array)
                {
                    array.Clear();
                }
                this.type = type;
                switch (type)
                {
                    case FieldType.Bool:
                        value = false;
                        break;
                    case FieldType.BoundsInt:
                        value = new BoundsInt();
                        break;
                    case FieldType.Bounds:
                        value = new Bounds();
                        break;
                    case FieldType.Color:
                        value = Color.white;
                        break;
                    case FieldType.Double:
                        value = 0.0f;
                        break;
                    case FieldType.Long:
                        value = 0L;
                        break;
                    case FieldType.Quaternion:
                        value = Quaternion.identity;
                        break;
                    case FieldType.RectInt:
                        value = new RectInt();
                        break;
                    case FieldType.Rect:
                        value = new Rect();
                        break;
                    case FieldType.String:
                        value = "";
                        break;
                    case FieldType.Vector2Int:
                        value = Vector2Int.zero;
                        break;
                    case FieldType.Vector2:
                        value = Vector2.zero;
                        break;
                    case FieldType.Vector3Int:
                        value = Vector3Int.zero;
                        break;
                    case FieldType.Vector3:
                        value = Vector3.zero;
                        break;
                    case FieldType.Vector4:
                        value = Vector4.zero;
                        break;
                    default:
                        value = null;
                        break;
                }
            }

            if (objectType != this.ObjectType)
            {
                this.ObjectType = objectType;
                _object = null;
                array.Clear();
            }
        }

        public void Initialize(FieldType type, Type objectType, object obj)
        {
            this.type = type;
            if (objectType == null && obj != null)
            {
                objectType = obj.GetType();
            }
            this.ObjectType = objectType;
            switch (type)
            {
                case FieldType.Bool:
                    if (obj is bool)
                        value = obj;
                    else
                        value = false;
                    break;
                case FieldType.BoundsInt:
                    if (obj is BoundsInt)
                        value = obj;
                    else
                        value = new BoundsInt();
                    break;
                case FieldType.Bounds:
                    if (obj is Bounds)
                        value = obj;
                    else
                        value = new Bounds();
                    break;
                case FieldType.Color:
                    if (obj is Color)
                        value = obj;
                    else
                        value = Color.white;
                    break;
                case FieldType.Double:
                    if (obj is float)
                        value = obj;
                    else
                        value = 0.0f;
                    break;
                case FieldType.Long:
                    if (obj is long)
                        value = obj;
                    else
                        value = 0L;
                    break;
                case FieldType.Quaternion:
                    if (obj is Quaternion)
                        value = obj;
                    else
                        value = Quaternion.identity;
                    break;
                case FieldType.RectInt:
                    if (obj is RectInt)
                        value = obj;
                    else
                        value = new RectInt();
                    break;
                case FieldType.Rect:
                    if (obj is Rect)
                        value = obj;
                    else
                        value = new Rect();
                    break;
                case FieldType.String:
                    if (obj is string)
                        value = obj;
                    else
                        value = "";
                    break;
                case FieldType.Vector2Int:
                    if (obj is Vector2Int)
                        value = obj;
                    else
                        value = Vector2Int.zero;
                    break;
                case FieldType.Vector2:
                    if (obj is Vector2)
                        value = obj;
                    else
                        value = Vector2.zero;
                    break;
                case FieldType.Vector3Int:
                    if (obj is Vector3Int)
                        value = obj;
                    else
                        value = Vector3Int.zero;
                    break;
                case FieldType.Vector3:
                    if (obj is Vector3)
                        value = obj;
                    else
                        value = Vector3.zero;
                    break;
                case FieldType.Vector4:
                    if (obj is Vector4)
                        value = obj;
                    else
                        value = Vector4.zero;
                    break;
                case FieldType.Object:
                    if (typeof(UnityEngine.Object).IsAssignableFrom(objectType))
                    {
                        _object = obj as UnityEngine.Object;
                    } else 
                    {
                        if (obj == null && objectType.IsSerializable)
                        {
                            try
                            {
                                obj = Activator.CreateInstance(objectType);
                            } finally
                            {

                            }
                        }
                        value = obj;
                    }
                    break;
                case FieldType.Array:
                    {
                        if (obj is JSValue)
                        {
                            JSValue jsValue = obj as JSValue;
                            int length = jsValue.GetLength();
                            array.Capacity = length;
                            for (int i = 0; i < length; ++i)
                            {
                                JSValue val = jsValue.Get(i);
                                var objVal = val.Value;
                                AttrValue attrValue = new AttrValue();
                                attrValue.Initialize(fromType(objectType), objectType, objVal);
                                array.Add(attrValue);
                            }
                        }
                        break;
                    }
                default:
                    value = null;
                    break;
            }
        }

        FieldType fromType(Type type)
        {
            FieldType fieldType;
            if (type == typeof(bool))
            {
                fieldType = FieldType.Bool;
            } else if (type == typeof(BoundsInt))
            {
                fieldType = FieldType.BoundsInt;
            } else if (type == typeof(Bounds))
            {
                fieldType = FieldType.Bounds;
            } else if (type == typeof(Color))
            {
                fieldType = FieldType.Color;
            } else if (type == typeof(Double))
            {
                fieldType = FieldType.Double;
            } else if (type == typeof(long))
            {
                fieldType = FieldType.Long;
            } else if (type == typeof(Quaternion))
            {
                fieldType = FieldType.Quaternion;
            } else if (type == typeof(RectInt))
            {
                fieldType = FieldType.RectInt;
            } else if (type == typeof(Rect))
            {
                fieldType = FieldType.Rect;
            } else if (type == typeof(string))
            {
                fieldType = FieldType.String; 
            } else if (type == typeof(Vector2Int))
            {
                fieldType = FieldType.Vector2Int;
            } else if (type == typeof(Vector2))
            {
                fieldType = FieldType.Vector2;
            } else if (type == typeof(Vector3Int))
            {
                fieldType = FieldType.Vector3Int;
            } else if (type == typeof(Vector3))
            {
                fieldType = FieldType.Vector3;
            } else if (type == typeof(Vector4))
            {
                fieldType = FieldType.Vector4;
            } else if (typeof(UnityEngine.Object).IsAssignableFrom(type))
            {
                fieldType = FieldType.Object;
            } else if (type.IsArray)
            {
                fieldType = FieldType.Array;
            } else
            {
                fieldType = FieldType.Unkown;
            }
            return fieldType;
        }

        public void InsertNew()
        {
            if (type == FieldType.Array)
            {
                AttrValue newValue = new AttrValue();
                newValue.Initialize(fromType(ObjectType), ObjectType, null);
                array.Add(newValue);
            } else
            {
                Debug.LogWarning("InsertNew only for Array value.");
            }
        }

        public void RemoveAt(int idx)
        {
            if (type == FieldType.Array)
            {
                array.RemoveAt(idx);
            }
            else
            {
                Debug.LogWarning("RemoveAt only for Array value.");
            }
        }

        static string GetClassRef(Type type)
        {
            return type != null
                ? type.FullName + ", " + type.Assembly.GetName().Name
                : "";
        }

        public Type ObjectType
        {
            get
            {
                if (objectType == null)
                {
                    if (typeRef != null && typeRef.Length > 0)
                    {
                        objectType = Type.GetType(typeRef);
                    }
                }
                return objectType;
            }

            set
            {
                typeRef = GetClassRef(value);
                objectType = value;
            }
        }
    }

    [Serializable]
    public class Attribute
    {
        public string key;

        public AttrValue value = new AttrValue();
    }

    [Serializable]
    public class Container
    {
        public object target;

        public Container(object target)
        {
            this.target = target;
        }

        private static Dictionary<TextAsset, JSValue> modules = new Dictionary<TextAsset, JSValue>();

        [HideInInspector]
        private JSValue value = JSValue.Zero;
        public JSValue Value {
            get
            {
                if (value.IsNull && script != null && target != null)
                {
                    JSValue module = Module;
                    if (!module.IsNull)
                    {
                        value = QuickJS.Instance.NewBind(module, this);
                    }
                }
                return value;
            }
        }

        public TextAsset script;

        [HideInInspector]
        private JSValue module = JSValue.Zero;
        private JSValue Module
        {
            get
            {
                if ((module == null || module.IsNull) && script != null)
                {
                    if (modules.ContainsKey(script))
                    {
                        module = modules[script];
                    }
                    else
                    {
                        string name;
                        if (Configure.Index != null)
                        {
                            name = Configure.Index.FindIndex(script);
                            if (name == null) name = script.name;
                        }
                        else
                            name = script.name;
                        module = QuickJS.Instance.Load(script.text, name);
                        if (module.IsNull) return JSValue.Zero;
                        modules[script] = module;
                    }
                }
                return module;
            }
        }

        private bool isInit = false;

        [SerializeField]
        private List<Attribute> attributes = new List<Attribute>();

        static JSAtom fieldsAtom;
        public List<Attribute> GetAttributes()
        {
            if (!isInit)
            {
                JSValue module = Module;
                if (!module.IsNull)
                {
                    if (fieldsAtom == null)
                    {
                        fieldsAtom = GetAtom("_$fields");
                    }
                    JSValue fields = module.Get(fieldsAtom);
                    int len = fields.GetLength();
                    Annotation[] annotations = new Annotation[len];
                    for (int i = 0; i < len; ++i)
                    {
                        annotations[i] = Annotation.From(fields.Get(i));
                    }

                    Dictionary<string, Attribute> tmp = new Dictionary<string, Attribute>();
                    foreach (var attr in attributes)
                    {
                        tmp[attr.key] = attr;
                    }
                    attributes = new List<Attribute>();
                    if (annotations != null)
                    {
                        foreach (var anno in annotations)
                        {
                            if (tmp.ContainsKey(anno.name))
                            {
                                Attribute attr = tmp[anno.name];
                                attr.value.Update(anno.type, anno.objectType);
                                attributes.Add(attr);
                            }
                            else
                            {
                                object value = anno.defaultValue;
                                Attribute attr = new Attribute()
                                {
                                    key = anno.name,
                                };
                                attr.value.Initialize(anno.type, anno.objectType, value);
                                attributes.Add(attr);
                            }
                        }
                    }
                } else
                {
                    attributes.Clear();
                }
                isInit = true;
            }

            return attributes;
        }

        public void reload()
        {
            module = JSValue.Zero;
            value = JSValue.Zero;
            if (script != null)
                modules.Remove(script);
            isInit = false;
        }

        static Dictionary<string, JSAtom> atomCache = new Dictionary<string, JSAtom>();

        public static JSAtom GetAtom(string name)
        {
            if (atomCache.ContainsKey(name))
            {
                return atomCache[name];
            } else
            {
                JSAtom atom = QuickJS.Instance.NewAtom(name);
                atomCache[name] = atom;
                return atom;
            }
        }
    }
}
