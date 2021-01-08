using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

namespace qjs
{
    [CustomEditor(typeof(AsarAsset))]
    public class AsarAssetEditor : Editor
    {
        Vector2 scrollPosition = Vector2.zero;

        private void OnEnable()
        {
            
        }

        public override void OnInspectorGUI()
        {
            foreach (var target in serializedObject.targetObjects)
            {
                AsarAsset asar = target as AsarAsset;
                var keys = asar.Files.Keys;

                foreach (var key in keys)
                {
                    EditorGUILayout.LabelField(key);
                }
            }
        }
    }
}