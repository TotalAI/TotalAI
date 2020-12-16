using UnityEngine;

namespace TotalAI
{
    public abstract class DriveTypeEquation : ScriptableObject
    {
        public abstract float GetEquationMax(Agent agent);
        public abstract float GetEquationRawLevel(Agent agent);
        public abstract float ChangeInOutputChange(Agent agent, DriveType driveType, OutputChange outputChange, Mapping mapping);

        public virtual float GetEquationMin(Agent agent)
        {
            return 0f;
        }

        public virtual float GetEquationLevel(Agent agent)
        {
            float min = GetEquationMin(agent);
            float max = GetEquationMax(agent);
            float rawLevel = Mathf.Clamp(GetEquationRawLevel(agent), min, max);
            return 100 - (rawLevel - min) / (max - min) * 100;
        }

        public virtual float CalculateEquationDriveLevelChange(Agent agent, DriveType driveType, Mapping mapping, OutputChange outputChange)
        {
            float amount = 0f;

            if (outputChange != null)
                amount = ChangeInOutputChange(agent, driveType, outputChange, mapping);
            else
                amount = mapping.EquationDriveTypeChangeInTree(agent, driveType);

            float min = GetEquationMin(agent);
            float max = GetEquationMax(agent);
            float range = max - min;

            float rawAmount = Mathf.Clamp(GetEquationRawLevel(agent), min, max);
            float rawNewAmount = Mathf.Clamp(rawAmount + amount, min, max);

            float oldLevel = 100 - (rawAmount - min) / range * 100;
            float newLevel = 100 - (rawNewAmount - min) / range * 100;

            //Debug.Log(name + " CalcEquDriveLvl: mapping = " + mapping.mappingType.name + " outputChange = " + outputChange + " level = " + (newLevel - oldLevel));
            return newLevel - oldLevel;
        }
    }
}