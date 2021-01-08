using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace qjs
{

    public class SupportTypesWindow : EditorWindow
    {
        private Vector2 scrollPostion = Vector2.zero;
        float timeCounter = 0;
        private string message;

        private void OnEnable()
        {
            titleContent = new GUIContent("Registered types");
        }

        private void OnGUI()
        {
            var style = new GUIStyle(GUI.skin.label)
            {
                alignment = TextAnchor.MiddleCenter,
                fontSize = 12
            };
            var style2 = new GUIStyle(GUI.skin.label)
            {
                alignment = TextAnchor.MiddleCenter,
                fontSize = 10,
                normal = new GUIStyleState()
                {
                    textColor = new Color(0, 0.9f, 0)
                }
            };

            EditorGUILayout.LabelField("/*Import a c# type in js.*/", style2);
            EditorGUILayout.LabelField("const MonoBehaviour = unity('UnityEngine.MonoBehaviour');", style2);
            EditorGUILayout.LabelField("You can register more type in Configure Object.", style);
            EditorGUILayout.LabelField("If the class name is not registered, it would try to load ", style);
            EditorGUILayout.LabelField("class dynamically, which would be very slow.", style);

            Rect posBegin = EditorGUILayout.GetControlRect(false, 1);
            float height = Mathf.Max(position.height - posBegin.yMax - 180, 60);
            scrollPostion = EditorGUILayout.BeginScrollView(scrollPostion, GUILayout.MaxHeight(height), GUILayout.MinHeight(height));

            Rect rect;

            foreach (var type in QuickJS.Instance.GetSupportTypes())
            {
                rect = EditorGUILayout.GetControlRect(true, 20);
                const float ButtonWidth = 48;
                EditorGUI.SelectableLabel(new Rect(rect.x + 10, rect.y, rect.width - 20 - ButtonWidth, rect.height), type.FullName);
                if (GUI.Button(new Rect(rect.x + rect.width - 10 - ButtonWidth, rect.y, ButtonWidth, rect.height), "Copy"))
                {
                    EditorGUIUtility.systemCopyBuffer = type.FullName;
                    message = string.Format("\"{0}\" is copied.", type.FullName);
                }
            }

            EditorGUILayout.EndScrollView();
            Rect posEnd = EditorGUILayout.GetControlRect(false, 1);

            EditorGUI.DrawRect(posBegin, Color.gray);
            EditorGUI.DrawRect(posEnd, Color.gray);

            rect = EditorGUILayout.GetControlRect(true, 24);
            if (GUI.Button(new Rect(rect.x + rect.width - 120, rect.y, 120, rect.height), "Close"))
                Close();

            EditorGUILayout.LabelField(message, style);
        }

        private void Update()
        {
            if (!Application.isPlaying)
            {
                timeCounter += Time.deltaTime;
                if (timeCounter > 1)
                {
                    timeCounter = 0;
                    Repaint();
                }
            }
        }
    }

    [CustomEditor(typeof(QuickBehaviour), true)]
    [CanEditMultipleObjects]
    public class BehaviourEditor : Editor
    {
        SerializedProperty scriptProperty;
        SerializedProperty fieldProperty;

        private void OnEnable()
        {
            scriptProperty = serializedObject.FindProperty("script");
            fieldProperty = serializedObject.FindProperty("m_Fields");
        }

        private static readonly JSAtom propertiesAtom = QuickJS.Instance.NewAtom("properties");
        public override void OnInspectorGUI()
        {
            serializedObject.Update();
            if (scriptProperty != null)
            {
                TextAsset text = scriptProperty.objectReferenceValue as TextAsset;
                TextAsset nText = (TextAsset)EditorGUILayout.ObjectField("Script", text, typeof(TextAsset), true);
                if (nText != text)
                {
                    if (nText == null)
                    {
                        scriptProperty.objectReferenceValue = null;
                        CallChanged(serializedObject.targetObjects);
                    }
                    else
                    {
                        scriptProperty.objectReferenceValue = nText;
                        CallChanged(serializedObject.targetObjects);
                    }
                    serializedObject.ApplyModifiedProperties();
                }
                if (text == null)
                {
                    var rect = EditorGUILayout.GetControlRect(false, 24);
                    if (GUI.Button(rect, "Registered types"))
                    {
                        ShowSuportTypes();
                    }
                } else {
                    var rect = EditorGUILayout.GetControlRect(false, 24);
                    if (GUI.Button(new Rect(rect.x, rect.y, rect.width / 2, rect.height), "Reload"))
                    {
                        foreach (Object obj in serializedObject.targetObjects)
                        {
                            (obj as QuickBehaviour).OnReloadScript();
                        }
                    }
                    if (GUI.Button(new Rect(rect.x + rect.width / 2, rect.y, rect.width / 2, rect.height), "Registered types"))
                    {
                        ShowSuportTypes();
                    }
                }
            } else
            {
                Debug.LogWarning("WTF ? " + serializedObject.targetObject);
            }

            if (serializedObject.targetObjects.Length == 1)
            {
                QuickBehaviour behaviour = serializedObject.targetObject as QuickBehaviour;
                var fieldObject = behaviour.FieldData;
                if (fieldObject.Type == FieldType.Object)
                {
                    bool changed = false;
                    for (int i = 0, t = fieldObject.Length; i < t; ++i)
                    {
                        if (DrawField(fieldObject.GetItem(i)))
                        {
                            changed = true;
                        }
                    }
                    if (changed)
                    {
                        fieldProperty.stringValue = fieldObject.ToString();
                    }
                }
            }

            DrawPropertiesExcluding(serializedObject, "script", "m_Script", "m_Fields");
            serializedObject.ApplyModifiedProperties();
        }

        private void ShowSuportTypes()
        {
            SupportTypesWindow window = SupportTypesWindow.CreateInstance<SupportTypesWindow>();
            window.ShowUtility();
        }

        private void CallChanged(Object[] objects)
        {
            foreach (Object obj in objects)
            {
                (obj as QuickBehaviour).OnScriptChanged();
            }
        }

        private bool DrawField(FieldItem item)
        {
            string key = item.key;
            FieldObject value = item.value;
            bool changed = false;
            switch (value.Type)
            {
                case FieldType.Bool:
                    {
                        bool v = EditorGUILayout.Toggle(key, value.Booloon);
                        if (value.Booloon != v)
                        {
                            changed = true;
                            value.Booloon = v;
                        }
                        break;
                    }
                case FieldType.BoundsInt:
                    {
                        BoundsInt v = EditorGUILayout.BoundsIntField(key, value.BoundsInt);
                        if (value.BoundsInt != v)
                        {
                            changed = true;
                            value.BoundsInt = v;
                        }
                        break;
                    }
                case FieldType.Bounds:
                    {
                        Bounds v = EditorGUILayout.BoundsField(key, value.Bounds);
                        if (value.Bounds != v)
                        {
                            changed = true;
                            value.Bounds = v;
                        }
                        break;
                    }
                case FieldType.Color:
                    {
                        Color v = EditorGUILayout.ColorField(key, value.Color);
                        if (value.Color != v)
                        {
                            changed = true;
                            value.Color = v;
                        }
                        break;
                    }
                case FieldType.Double:
                    {
                        double v = EditorGUILayout.DoubleField(key, value.Double);
                        if (value.Double != v)
                        {
                            changed = true;
                            value.Double = v;
                        }
                        break;
                    }
                case FieldType.Long:
                    {
                        long v = EditorGUILayout.LongField(key, value.Long);
                        if (value.Long != v)
                        {
                            changed = true;
                            value.Long = v;
                        }
                        break;
                    }
                case FieldType.Quaternion:
                    {
                        var qua = value.Quaternion;
                        var vec4 = new Vector4(qua.x, qua.y, qua.z, qua.w);
                        Vector4 v = EditorGUILayout.Vector4Field(key, vec4);
                        if (vec4 != v)
                        {
                            changed = true;
                            value.Quaternion = new Quaternion(v.x, v.y, v.z, v.w);
                        }
                        break;
                    }
                case FieldType.Rect:
                    {
                        var v = EditorGUILayout.RectField(key, value.Rect);
                        if (value.Rect != v)
                        {
                            changed = true;
                            value.Rect = v;
                        }
                        break;
                    }
                case FieldType.RectInt:
                    {
                        var v = EditorGUILayout.RectIntField(key, value.RectInt);
                        if (value.RectInt.min != v.min || value.RectInt.max != v.max)
                        {
                            changed = true;
                            value.RectInt = v;
                        }
                        break;
                    }
                case FieldType.String:
                    {
                        var v = EditorGUILayout.TextField(key, value.String);
                        if (value.String != v)
                        {
                            changed = true;
                            value.String = v;
                        }
                        break;
                    }
                case FieldType.Vector2:
                    {
                        var v = EditorGUILayout.Vector2Field(key, value.Vector2);
                        if (value.Vector2 != v)
                        {
                            changed = true;
                            value.Vector2 = v;
                        }
                        break;
                    }
                case FieldType.Vector2Int:
                    {
                        var v = EditorGUILayout.Vector2IntField(key, value.Vector2Int);
                        if (value.Vector2Int != v)
                        {
                            changed = true;
                            value.Vector2Int = v;
                        }
                        break;
                    }
                case FieldType.Vector3:
                    {
                        var v = EditorGUILayout.Vector3Field(key, value.Vector3);
                        if (value.Vector3 != v)
                        {
                            changed = true;
                            value.Vector3 = v;
                        }
                        break;
                    }
                case FieldType.Vector3Int:
                    {
                        var v = EditorGUILayout.Vector3IntField(key, value.Vector3Int);
                        if (value.Vector3Int != v)
                        {
                            changed = true;
                            value.Vector3Int = v;
                        }
                        break;
                    }
                case FieldType.Vector4:
                    {
                        var v = EditorGUILayout.Vector4Field(key, value.Vector4);
                        if (value.Vector4 != v)
                        {
                            changed = true;
                            value.Vector4 = v;
                        }
                        break;
                    }
            }

            return changed;
        }
    }
}
