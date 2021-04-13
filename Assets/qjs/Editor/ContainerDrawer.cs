using System.Collections;
using System.Collections.Generic;
using com.spacepuppyeditor;
using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;

namespace qjs
{
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
                container.GetAttributes();
            }

            var attributesProperty = property.FindPropertyRelative("attributes");

            float offset = EditorGUIUtility.singleLineHeight;
            for (int i = 0, t = attributesProperty.arraySize; i < t; ++i)
            {
                SerializedProperty attr = attributesProperty.GetArrayElementAtIndex(i);
                float rowHeight = EditorGUI.GetPropertyHeight(attr.FindPropertyRelative("value"));
                string key = attr.FindPropertyRelative("key").stringValue;
                //Attribute a = EditorHelper.GetTargetObjectOfProperty(attr) as Attribute;
                EditorGUI.PropertyField(
                    new Rect(position.x, position.y + offset,
                    position.width, rowHeight),
                    attr.FindPropertyRelative("value"),
                    new GUIContent(key), true);
                offset += rowHeight;
            }

            EditorGUI.EndProperty();
        }

        public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
        {
            var container = EditorHelper.GetTargetObjectOfProperty(property) as Container;
            if (container.target == null)
                container.target = property.serializedObject.targetObject;
            container.GetAttributes();
            property.serializedObject.UpdateIfRequiredOrScript();
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
