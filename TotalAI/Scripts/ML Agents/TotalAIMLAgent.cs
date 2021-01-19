using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.MLAgents;
using Unity.MLAgents.Sensors;
using Unity.MLAgents.Actuators;

namespace TotalAI.DeepRL
{
    public class TotalAIMLAgent : Unity.MLAgents.Agent
    {
        
        void Start()
        {

        }

        public override void CollectObservations(VectorSensor sensor)
        {
            base.CollectObservations(sensor);
        }

        public override void Heuristic(in ActionBuffers actionsOut)
        {
            base.Heuristic(actionsOut);
        }

        public override void Initialize()
        {
            base.Initialize();
        }

        public override void OnActionReceived(ActionBuffers actions)
        {
            base.OnActionReceived(actions);
        }

        public override void OnEpisodeBegin()
        {
            base.OnEpisodeBegin();
        }

        public override void WriteDiscreteActionMask(IDiscreteActionMask actionMask)
        {
            base.WriteDiscreteActionMask(actionMask);
        }

        
    }
}
