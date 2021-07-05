using System.Collections;
using System.Collections.Generic;
using System.Text;
using com.spacepuppyeditor;
using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;

namespace qjs
{
    [CustomPropertyDrawer(typeof(AttrValue))]
    public class AttrValueDrawer : PropertyDrawer
    {
        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            EditorGUI.BeginProperty(position, label, property);
            AttrValue attrValue = EditorHelper.GetTargetObjectOfProperty(property) as AttrValue;

            FieldType type = attrValue.type;
            switch (type)
            {
                case FieldType.Object:
                    {
                        System.Type ObjectType = attrValue.ObjectType;
                        if (typeof(UnityEngine.Object).IsAssignableFrom(ObjectType))
                        {
                            SerializedProperty objectPro = property.FindPropertyRelative("_object");
                            UnityEngine.Object newObj = EditorGUI.ObjectField(position, label, objectPro.objectReferenceValue, attrValue.ObjectType, true);
                            if (newObj != objectPro.objectReferenceValue)
                            {
                                objectPro.objectReferenceValue = newObj;
                                property.serializedObject.ApplyModifiedProperties();
                            }
                        } else
                        {
                            SerializedProperty valuePro = property.FindPropertyRelative("value");
                            EditorGUI.PropertyField(position, valuePro, label, true);
                        }
                        break;
                    }
                case FieldType.Array:
                    {
                        SerializedProperty arrayPro = property.FindPropertyRelative("array");
                        EditorGUI.LabelField(new Rect(
                            position.x, position.y, position.width, EditorGUIUtility.singleLineHeight), label);
                        if (arrayPro.isExpanded)
                        {
                            if (GUI.Button(new Rect(
                                position.x - 20, position.y, 20, 20),
                                new GUIContent("▼")))
                            {
                                arrayPro.isExpanded = false;
                            }
                            float offsetY = position.y + EditorGUIUtility.singleLineHeight + EditorGUIUtility.standardVerticalSpacing;
                            int length = arrayPro.arraySize;
                            EditorGUI.indentLevel += 1;
                            for (int i = 0; i < length; ++i)
                            {
                                var subPro = arrayPro.GetArrayElementAtIndex(i);
                                float height = EditorGUI.GetPropertyHeight(subPro);
                                EditorGUI.PropertyField(new Rect(
                                    position.x,
                                    offsetY, position.width - 20,
                                    height), subPro, new GUIContent(i.ToString()));
                                if (GUI.Button(new Rect(
                                    position.x + position.width - 20,
                                    offsetY, 20, EditorGUIUtility.singleLineHeight), "-"))
                                {
                                    attrValue.RemoveAt(i);
                                    property.serializedObject.UpdateIfRequiredOrScript();
                                    break;
                                }

                                offsetY += height + EditorGUIUtility.standardVerticalSpacing;
                            }
                            const float insetWidth = 46;
                            if (GUI.Button(new Rect(
                                position.x + insetWidth,
                                offsetY, position.width - insetWidth,
                                EditorGUIUtility.singleLineHeight), new GUIContent("+")))
                            {
                                attrValue.InsertNew();
                                property.serializedObject.UpdateIfRequiredOrScript();
                            }
                            EditorGUI.indentLevel -= 1;
                        } else
                        {
                            if (GUI.Button(new Rect(
                                position.x - 20, position.y, 20, 20),
                                new GUIContent("▶")))
                            {
                                arrayPro.isExpanded = true;
                            }
                        }
                        break;
                    }
                case FieldType.Unkown:
                    {
                        EditorGUI.LabelField(position, label);
                        break;
                    }
                default:
                    {
                        SerializedProperty valuePro = property.FindPropertyRelative("value");
                        EditorGUI.PropertyField(position, valuePro, label, true);
                        break;
                    }
            }
            EditorGUI.EndProperty();
        }

        public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
        {
            AttrValue attrValue = EditorHelper.GetTargetObjectOfProperty(property) as AttrValue;

            FieldType type = attrValue.type;
            switch (type)
            {
                case FieldType.Object:
                    {
                        System.Type ObjectType = attrValue.ObjectType;
                        if (typeof(UnityEngine.Object).IsAssignableFrom(ObjectType))
                        {
                            SerializedProperty objectPro = property.FindPropertyRelative("_object");
                            return EditorGUI.GetPropertyHeight(objectPro);
                        } else
                        {
                            SerializedProperty valuePro = property.FindPropertyRelative("value");
                            return EditorGUI.GetPropertyHeight(valuePro);
                        }
                    }
                case FieldType.Array:
                    {
                        SerializedProperty arrayPro = property.FindPropertyRelative("array");
                        //float height = EditorGUIUtility.singleLineHeight;
                        //for (int i = 0, t = arrayPro.arraySize; i < t; ++i)
                        //{
                        //    height += heightOfValue(arrayPro.GetArrayElementAtIndex(i));
                        //}
                        //return height;
                        return EditorGUI.GetPropertyHeight(arrayPro);
                    }
                case FieldType.Unkown:
                    {
                        return EditorGUIUtility.singleLineHeight;
                    }
                default:
                    {
                        SerializedProperty valuePro = property.FindPropertyRelative("value");
                        return EditorGUI.GetPropertyHeight(valuePro);
                    }
            }
        }
    }

    [CustomPropertyDrawer(typeof(Container))]
    public class ContainerDrawer : PropertyDrawer
    {
        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            EditorGUI.BeginProperty(position, label, property);

            var container = EditorHelper.GetTargetObjectOfProperty(property) as Container;
            SerializedProperty scriptProperty = property.FindPropertyRelative("script");
            TextAsset script = scriptProperty.objectReferenceValue as TextAsset;
            TextAsset newScript = EditorGUI.ObjectField(new Rect(position.x, position.y,
                position.width - 20, EditorGUIUtility.singleLineHeight),
                label, script, typeof(TextAsset), true) as TextAsset;

            if (newScript != script)
            {
                scriptProperty.objectReferenceValue = newScript;
                container.script = newScript;
                container.reload();
                scriptProperty.serializedObject.UpdateIfRequiredOrScript();
                EditorUtility.SetDirty(scriptProperty.serializedObject.targetObject);
            }

            if (GUI.Button(new Rect(position.x + position.width - 20, position.y, 20, EditorGUIUtility.singleLineHeight), "r"))
            {
                container.reload();
            }

            container.GetAttributes();
            property.serializedObject.UpdateIfRequiredOrScript();
            var attributesProperty = property.FindPropertyRelative("attributes");
            List<Attribute> attributes = EditorHelper.GetTargetObjectOfProperty(attributesProperty) as List<Attribute>;
            float offset = EditorGUIUtility.singleLineHeight;
            if (attributesProperty.arraySize == attributes.Count)
            {
                for (int i = 0, t = attributesProperty.arraySize; i < t; ++i)
                {
                    try
                    {
                        Attribute attribute = attributes[i];
                        SerializedProperty attr = attributesProperty.GetArrayElementAtIndex(i);
                        string key = attr.FindPropertyRelative("key").stringValue;
                        string lower = key.ToLower();
                        StringBuilder sb = new StringBuilder();
                        bool lastIsLower = false;
                        for (int n = 0; n < key.Length; ++n)
                        {
                            char ch = key[n];
                            if (n == 0)
                            {
                                sb.Append(char.ToUpper(ch));
                            } else
                            {
                                char lch = lower[n];
                                if (ch == lch)
                                {
                                    sb.Append(ch);
                                    lastIsLower = true;
                                } else
                                {
                                    if (lastIsLower)
                                    {
                                        sb.Append(' ');
                                    }
                                    sb.Append(ch);
                                }
                            }
                        }
                        SerializedProperty valuePro = attr.FindPropertyRelative("value");
                        float rowHeight = EditorGUI.GetPropertyHeight(valuePro);
                        EditorGUI.PropertyField(
                            new Rect(position.x, position.y + offset,
                                            position.width, rowHeight),
                            valuePro, new GUIContent(sb.ToString()));

                        offset += rowHeight;
                    }
                    catch (System.Exception e)
                    {
                        Debug.Log(e);
                        Debug.Log("Error postition  " + i);
                    }
                }
            } else
            {
                Debug.Log("Diff");
            }

            EditorGUI.EndProperty();
        }

        public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
        {
            var container = EditorHelper.GetTargetObjectOfProperty(property) as Container;
            if (container.target == null)
                container.target = property.serializedObject.targetObject;
            container.GetAttributes();

            var attributesProperty = property.FindPropertyRelative("attributes");
            float height = EditorGUIUtility.singleLineHeight;
            for (int i = 0, t = attributesProperty.arraySize; i < t; ++i)
            {
                SerializedProperty valueProperty = attributesProperty.GetArrayElementAtIndex(i).FindPropertyRelative("value");
                height += EditorGUI.GetPropertyHeight(valueProperty, true);
            }

            return height;
        }

        private void PropertyField(Rect postion, SerializedProperty property)
        {
            var value = property.objectReferenceValue;
            var newValue = EditorGUI.ObjectField(postion, value, value.GetType(), true);
            if (value != newValue)
            {
                property.objectReferenceValue = newValue;
                EditorUtility.SetDirty(property.serializedObject.targetObject);
            }
        }
    }
}
