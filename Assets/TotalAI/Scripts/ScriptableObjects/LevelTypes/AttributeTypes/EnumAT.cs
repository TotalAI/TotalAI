using System;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;

namespace TotalAI
{
    public abstract class EnumAT : AttributeType
    {
        [Serializable]
        public class EnumData : Data
        {
            public int enumIndexValue;
        }

        public Dictionary<Entity, EnumData> entitiesData;

        public virtual void OnEnable()
        {
            entitiesData = new Dictionary<Entity, EnumData>();
        }

        public override void Initialize(Entity entity, Data data)
        {
            EnumData enumData = (EnumData)data;
            entitiesData.Add(entity, new EnumData() {
                enumIndexValue = enumData.enumIndexValue
            });
        }

        public override Data GetData(Entity entity)
        {
            return entitiesData[entity];
        }

        public override Data SetData(Entity entity, Data data)
        {
            EnumData enumData = entitiesData[entity];
            int newIndex = ((EnumData)data).enumIndexValue;
            enumData.enumIndexValue = newIndex;
            return enumData;
        }

        public abstract T OptionValue<T>(Entity entity, int index);
        public abstract T OptionValue<T>(Entity entity);
        public abstract int NumberOptions();
        public abstract string[] OptionNames();
    }
}