using System;
using UnityEditor;
using UnityEngine;

namespace TotalAI
{
    public abstract class AttributeType : LevelType
    {
        public abstract class Data { }

        public abstract void Initialize(Entity entity, Data data);
        public abstract Data GetData(Entity entity);
        public abstract Data SetData(Entity entity, Data data);

        public abstract Type ForType();

        // Editor Methods
        public abstract void DrawFixedValueField(SerializedProperty sp);
        public abstract void DrawFixedValueField(SerializedProperty sp, Rect rect);
        public abstract void DrawDefaultValueFields(Rect rect, SerializedProperty sp);
    }
}
