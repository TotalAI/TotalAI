using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;

namespace TotalAI
{
    public abstract class SensorType : ScriptableObject
    {        
        public int maxColliders = 1000;
        public int runEveryXMainLoops = 1;

        protected Collider[] colliders;
        protected int entityLayers;
        protected Dictionary<Agent, int> runCounter;

        public void OnEnable()
        {
            runCounter = new Dictionary<Agent, int>();
        }

        public virtual void Setup(Agent agent, int entityLayers)
        {
            // Needed to clear out Dicts after playing for AgentView Editor Window
            if (!Application.isPlaying && runCounter != null && runCounter.ContainsKey(agent))
            {
                OnEnable();
            }
            runCounter[agent] = 1;
            this.entityLayers = entityLayers;
            colliders = new Collider[maxColliders];
        }

        public virtual bool TimeToRun(Agent agent)
        {
            if (runCounter[agent] == runEveryXMainLoops)
            {
                runCounter[agent] = 1;
                return true;
            }
            runCounter[agent]++;
            return false;
        }

        public abstract int Run(Agent agent, Entity[] entities);
        public abstract void DrawGizmos(Agent agent);        
    }
}
