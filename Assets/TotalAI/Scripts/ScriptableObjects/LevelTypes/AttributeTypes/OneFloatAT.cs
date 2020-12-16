using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace TotalAI
{
    [CreateAssetMenu(fileName = "OneFloatAT", menuName = "Total AI/Level Types/Attribute Types/One Float", order = 0)]
    public class OneFloatAT : AttributeType
    {
        [Serializable]
        public class OneFloatData : Data
        {
            public float floatValue;
        }

        public Dictionary<Entity, OneFloatData> entitiesData;

        private void OnEnable()
        {
            entitiesData = new Dictionary<Entity, OneFloatData>();
        }

        public override void Initialize(Entity entity, Data data)
        {
            OneFloatData oneFloatData = (OneFloatData)data;
            entitiesData.Add(entity, new OneFloatData() { floatValue = oneFloatData.floatValue });
        }

        public override Data GetData(Entity entity)
        {
            return entitiesData[entity];
        }

        public override Data SetData(Entity entity, Data data)
        {
            entitiesData[entity] = (OneFloatData)data;
            return data;
        }

        public override void DrawFixedValueField(SerializedProperty sp)
        {
            EditorGUILayout.PropertyField(sp.FindPropertyRelative("fixedFloatValue"));
        }

        public override void DrawFixedValueField(SerializedProperty sp, Rect rect)
        {
            EditorGUI.PropertyField(rect, sp.FindPropertyRelative("fixedFloatValue"));
        }

        public override void DrawDefaultValueFields(Rect rect, SerializedProperty sp)
        {
            EditorGUI.PropertyField(new Rect(rect.x + 235, rect.y, 40, EditorGUIUtility.singleLineHeight),
                                        sp.FindPropertyRelative("oneFloatData.floatValue"), GUIContent.none);
        }

        public override Type ForType()
        {
            return typeof(float);
        }
    }
}
