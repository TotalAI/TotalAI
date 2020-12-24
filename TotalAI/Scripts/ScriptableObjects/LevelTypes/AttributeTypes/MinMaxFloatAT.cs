using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace TotalAI
{
    [CreateAssetMenu(fileName = "MinMaxFloatAT", menuName = "Total AI/Level Types/Attribute Types/Min Max Float", order = 0)]
    public class MinMaxFloatAT : AttributeType
    {
        [Serializable]
        public class ChangeCurve
        {
            public MinMaxFloatAT minMaxFloatAT;
            public MinMaxCurve changeMinMaxCurve;
        }
        [Header("Used to change this Attribute based on another Attribute")]
        [Tooltip("For example, can use the curve to to change energy levels based on movement speed.")]
        public List<ChangeCurve> changeCurves;

        [Serializable]
        public class MinMaxFloatData : Data
        {
            public float floatValue;
            public float min;
            public float max;
        }

        public Dictionary<Entity, MinMaxFloatData> entitiesData;

        private void OnEnable()
        {
            entitiesData = new Dictionary<Entity, MinMaxFloatData>();
        }

        public override void Initialize(Entity entity, Data data)
        {
            // Needed to clear out Dicts after playing for AgentView Editor Window
            if (!Application.isPlaying && entitiesData != null && entitiesData.ContainsKey(entity))
            {
                OnEnable();
            }

            MinMaxFloatData minMaxFloatData = (MinMaxFloatData)data;
            entitiesData.Add(entity, new MinMaxFloatData() {
                floatValue = minMaxFloatData.floatValue,
                min = minMaxFloatData.min,
                max = minMaxFloatData.max
            });
        }

        public override Data GetData(Entity entity)
        {
            return entitiesData[entity];
        }
        
        public override Data SetData(Entity entity, Data data)
        {
            MinMaxFloatData minMaxFloatData = entitiesData[entity];
            float newLevel = ((MinMaxFloatData)data).floatValue;

            if (newLevel > minMaxFloatData.max)
                newLevel = minMaxFloatData.max;
            else if (newLevel < minMaxFloatData.min)
                newLevel = minMaxFloatData.min;

            Debug.Log(entity.name +": " + name + " Set to " + newLevel);

            minMaxFloatData.floatValue = newLevel;
            return minMaxFloatData;
        }

        public float GetNormalizedLevel(Entity entity)
        {
            MinMaxFloatData minMaxFloatData = entitiesData[entity];
            return Mathf.Clamp((minMaxFloatData.floatValue - minMaxFloatData.min) / (minMaxFloatData.max - minMaxFloatData.min), 0f, 1f);
        }

        public void SetValueFromNormalizedValue(Entity entity, float normalizedValue)
        {
            MinMaxFloatData minMaxFloatData = entitiesData[entity];
            minMaxFloatData.floatValue = normalizedValue * (minMaxFloatData.max - minMaxFloatData.min) + minMaxFloatData.min;
        }

        private ChangeCurve FindChangeCurve(MinMaxFloatAT otherAT)
        {
            return changeCurves.Find(x => x.minMaxFloatAT == otherAT);
        }

        public void ChangeLevelUsingChangeCurve(Entity entity, float curveInput, MinMaxFloatAT otherAT, float multiplier, bool increaseLevel)
        {
            ChangeCurve changeCurve = FindChangeCurve(otherAT);

            if (changeCurve == null)
            {
                Debug.LogError(entity.name + ": Trying to change an Attribute (" + name + ") Level with " + otherAT.name +
                               " but it does not exist in the Change Curves Mappings.  Please Fix.");
                return;
            }

            float rate = changeCurve.changeMinMaxCurve.NormAndEval(curveInput, otherAT.GetMin(entity), otherAT.GetMax(entity));
            float changeAmount = multiplier * rate;
            if (!increaseLevel)
                changeAmount = -changeAmount;

            SetData(entity, new MinMaxFloatData() { floatValue = changeAmount + entitiesData[entity].floatValue });

            //Debug.Log(entity.name + ": Burned " + changeAmount + " Energy from " + rate + " burn rate for " + multiplier);
        }

        public float GetMin(Entity entity)
        {
            return entitiesData[entity].min;
        }

        public float GetMax(Entity entity)
        {
            return entitiesData[entity].max;
        }

        public override void DrawFixedValueField(SerializedProperty sp)
        {
            EditorGUILayout.PropertyField(sp.FindPropertyRelative("fixedValue"));
        }

        public override void DrawFixedValueField(SerializedProperty sp, Rect rect)
        {
            EditorGUI.PropertyField(rect, sp.FindPropertyRelative("fixedValue"));
        }

        public override void DrawDefaultValueFields(Rect rect, SerializedProperty sp)
        {
            EditorGUI.PropertyField(new Rect(rect.x + 235, rect.y, 40, EditorGUIUtility.singleLineHeight),
                                        sp.FindPropertyRelative("minMaxFloatData.floatValue"), GUIContent.none);
            EditorGUI.PropertyField(new Rect(rect.x + 290, rect.y, 40, EditorGUIUtility.singleLineHeight),
                                sp.FindPropertyRelative("minMaxFloatData.min"), GUIContent.none);
            EditorGUI.LabelField(new Rect(rect.x + 330, rect.y, 20, EditorGUIUtility.singleLineHeight), " - ");
            EditorGUI.PropertyField(new Rect(rect.x + 345, rect.y, 40, EditorGUIUtility.singleLineHeight),
                                sp.FindPropertyRelative("minMaxFloatData.max"), GUIContent.none);
        }

        public override Type ForType()
        {
            return typeof(float);
        }
    }
}