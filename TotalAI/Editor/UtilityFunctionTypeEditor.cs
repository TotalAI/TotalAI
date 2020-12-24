using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace TotalAI.Editor
{
    [CustomEditor(typeof(UtilityFunctionType), true)]
    public class UtilityFunctionTypeEditor : UnityEditor.Editor
    {
        private UtilityFunctionType utilityFunctionType;

        private void OnEnable()
        {
            utilityFunctionType = (UtilityFunctionType)target;
        }

        public override void OnInspectorGUI()
        {
            GUILayout.Space(20);
            GUILayout.Label(utilityFunctionType.editorDescription,
                new GUIStyle("helpBox")
                {
                    padding = new RectOffset(10, 10, 10, 10),
                    margin = new RectOffset(0, 15, 0, 0),
                    richText = true,
                    wordWrap = true,
                    fontSize = 13
                });
            GUILayout.Space(20);
            DrawDefaultInspector();
        }
    }
}
