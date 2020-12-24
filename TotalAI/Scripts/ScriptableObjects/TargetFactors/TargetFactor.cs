using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace TotalAI
{
    public abstract class TargetFactor : ScriptableObject
    {
        public MinMaxCurve minMaxCurve;

        public abstract float Evaluate(Agent agent, Entity entity, Mapping mapping, bool forInventory);
    }
}
