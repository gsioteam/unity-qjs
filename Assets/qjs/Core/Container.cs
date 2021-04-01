using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace qjs
{
    [Serializable]
    public class Attribute
    {
        public string key;

        [SerializeReference]
        public object value;

        public Type type;
    }

    [Serializable]
    public class Container
    {
        public UnityEngine.Object target;

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
                if (module.IsNull && script != null)
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
                                attributes.Add(tmp[anno.name]);
                            }
                            else
                            {
                                Attribute attr = new Attribute()
                                {
                                    key = anno.name,
                                    value = anno.defaultValue
                                };
                                if (attr.value != null)
                                {
                                    switch (anno.type)
                                    {
                                        case FieldType.Bool:
                                            if (attr.value.GetType() != typeof(bool))
                                                attr.value = null;
                                            break;
                                        case FieldType.BoundsInt:
                                            if (attr.value.GetType() != typeof(BoundsInt))
                                                attr.value = null;
                                            break;
                                        case FieldType.Bounds:
                                            if (attr.value.GetType() != typeof(Bounds))
                                                attr.value = null;
                                            break;
                                        case FieldType.Color:
                                            if (attr.value.GetType() != typeof(Color))
                                                attr.value = null;
                                            break;
                                        case FieldType.Double:
                                            if (attr.value.GetType() != typeof(float))
                                                attr.value = null;
                                            break;
                                        case FieldType.Long:
                                            if (attr.value.GetType() != typeof(long))
                                                attr.value = null;
                                            break;
                                        case FieldType.Quaternion:
                                            if (attr.value.GetType() != typeof(Quaternion))
                                                attr.value = null;
                                            break;
                                        case FieldType.RectInt:
                                            if (attr.value.GetType() != typeof(RectInt))
                                                attr.value = null;
                                            break;
                                        case FieldType.Rect:
                                            if (attr.value.GetType() != typeof(Rect))
                                                attr.value = null;
                                            break;
                                        case FieldType.String:
                                            if (attr.value.GetType() != typeof(string))
                                                attr.value = null;
                                            break;
                                        case FieldType.Vector2Int:
                                            if (attr.value.GetType() != typeof(Vector2Int))
                                                attr.value = null;
                                            break;
                                        case FieldType.Vector2:
                                            if (attr.value.GetType() != typeof(Vector2))
                                                attr.value = null;
                                            break;
                                        case FieldType.Vector3Int:
                                            if (attr.value.GetType() != typeof(Vector3Int))
                                                attr.value = null;
                                            break;
                                        case FieldType.Vector3:
                                            if (attr.value.GetType() != typeof(Vector3))
                                                attr.value = null;
                                            break;
                                        case FieldType.Vector4:
                                            if (attr.value.GetType() != typeof(Vector4))
                                                attr.value = null;
                                            break;
                                        case FieldType.Object:
                                            break;
                                    }
                                }
                                if (attr.value == null)
                                {
                                    switch (anno.type)
                                    {
                                        case FieldType.Bool:
                                            attr.value = false;
                                            break;
                                        case FieldType.BoundsInt:
                                            attr.value = new BoundsInt();
                                            break;
                                        case FieldType.Bounds:
                                            attr.value = new Bounds();
                                            break;
                                        case FieldType.Color:
                                            attr.value = Color.clear;
                                            break;
                                        case FieldType.Double:
                                            attr.value = 0.0f;
                                            break;
                                        case FieldType.Long:
                                            attr.value = 0L;
                                            break;
                                        case FieldType.Quaternion:
                                            attr.value = Quaternion.identity;
                                            break;
                                        case FieldType.RectInt:
                                            attr.value = new RectInt();
                                            break;
                                        case FieldType.Rect:
                                            attr.value = new Rect();
                                            break;
                                        case FieldType.String:
                                            attr.value = "";
                                            break;
                                        case FieldType.Vector2Int:
                                            attr.value = new Vector2Int();
                                            break;
                                        case FieldType.Vector2:
                                            attr.value = new Vector2();
                                            break;
                                        case FieldType.Vector3Int:
                                            attr.value = new Vector3Int();
                                            break;
                                        case FieldType.Vector3:
                                            attr.value = new Vector3();
                                            break;
                                        case FieldType.Vector4:
                                            attr.value = new Vector4();
                                            break;
                                        case FieldType.Object:
                                            break;
                                    }
                                }
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
