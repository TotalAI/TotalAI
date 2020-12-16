using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace TotalAI
{
    [CreateAssetMenu(fileName = "FactionAgesMMT", menuName = "Total AI/Min Max Types/Faction Ages", order = 0)]
    public class FactionAgesMMT : MinMaxType
    {

        public override float GetMin(Agent agent)
        {
            return agent.faction.GetAllAgents().Select(x => x.CurrentAge()).Min();
        }

        public override float GetMax(Agent agent)
        {
            return agent.faction.GetAllAgents().Select(x => x.CurrentAge()).Max();
        }

        public override float GetDefaultValue(Agent agent)
        {
            return (float)agent.faction.GetAllAgents().Select(x => x.CurrentAge()).Average();
        }
    }
}

