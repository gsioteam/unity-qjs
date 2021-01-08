using System.Collections;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using UnityEngine;

namespace qjs
{
    public class QuickBehaviour : MonoBehaviour
    {
        public interface ResourceIndex
        {
            string FindIndex(TextAsset text);
        }

        public static ResourceIndex Index;

        private static Dictionary<TextAsset, JSValue> modules = new Dictionary<TextAsset, JSValue>();

        [SerializeField]
        private TextAsset script;

        [SerializeField]
        private string m_Fields;

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
                        if (Index != null)
                        {
                            name = Index.FindIndex(script);
                            if (name == null) name = script.name;
                        } else
                            name = script.name;
                        module = QuickJS.Instance.Load(script.text, name);
                        if (module.IsNull) return JSValue.Zero;
                        modules[script] = module;
                    }
                }
                return module;
            }
        }

        private JSValue value = JSValue.Zero;
        protected JSValue Value
        {
            get
            {
                if (value.IsNull && script != null)
                {
                    JSValue module = Module;
                    if (!module.IsNull)
                    {
                        value = QuickJS.Instance.NewBind(module, this);
                        if (!value.IsNull)
                        {
                            value.behaviour = this;
                        }
                    }
                }
                return value;
            }
        }

        private FieldObject fieldData;
        public FieldObject FieldData
        {
            get
            {
                if (fieldData == null)
                {
                    if (m_Fields != null)
                    {
                        JSONNode node = JSONNode.Parse(m_Fields);
                        if (node.IsObject)
                        {
                            fieldData = new FieldObject(node.AsObject);
                            var annotations = Value.GetAnnotations();
                            if (annotations == null)
                                annotations = new Annotation[0];
                            fieldData.Update(annotations);
                        } else
                        {
                            fieldData = FieldObject.NewObject(Value.GetAnnotations());
                        }
                    } else
                    {

                        fieldData = FieldObject.NewObject(Value.GetAnnotations());
                    }
                }
                return fieldData;
            }
        }

        static Dictionary<string, JSAtom> atomDB = new Dictionary<string, JSAtom>();
        [MethodImpl(MethodImplOptions.Synchronized)]
        protected static JSAtom GetAtom(string name)
        {
            if (atomDB.ContainsKey(name))
            {
                return atomDB[name];
            } else
            {
                JSAtom atom = QuickJS.Instance.NewAtom(name);
                atomDB[name] = atom;
                return atom;
            }
        }

        public void OnScriptChanged()
        {
            module = JSValue.Zero;
            value = JSValue.Zero;
            fieldData = null;
        }

        public void OnReloadScript()
        {
            modules.Remove(script);
            OnScriptChanged();
        }

    }
}
