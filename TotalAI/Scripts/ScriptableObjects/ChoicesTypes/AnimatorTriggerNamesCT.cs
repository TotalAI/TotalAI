using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace TotalAI
{
    [CreateAssetMenu(fileName = "AnimatorTriggerNamesCT", menuName = "Total AI/Choice Types/Animator Trigger Names", order = 0)]
    public class AnimatorTriggerNamesCT : ChoicesType
    {
        [Header("Required if selecting a Fixed Value in the Editor is Needed.")]
        public Agent prefabAgentForEditor;

        // How to access this in Editor?  Allow an EditorAnimatorController?  Prefab Agent - grab controller off of it?
        protected Agent UsePrefabAgentForEditor()
        {
            Agent agent = prefabAgentForEditor;
            agent.agentType = (AgentType)agent.entityType;
            agent.ResetAnimationType(true);
            return agent;
        }

        public override int NumberChoices(Agent agent)
        {
            bool forEditor = false;
            if (agent == null)
            {
                agent = UsePrefabAgentForEditor();
                forEditor = true;
            }
            return agent.animationType.GetTriggerParamNames(agent, forEditor).Length;
        }

        public override string[] OptionNames(Agent agent)
        {
            bool forEditor = false;
            if (agent == null)
            {
                agent = UsePrefabAgentForEditor();
                forEditor = true;
            }
            return agent.animationType.GetTriggerParamNames(agent, forEditor);
        }

        public override List<T> GetChoices<T>(Agent agent)
        {
            bool forEditor = false;
            if (agent == null)
            {
                agent = UsePrefabAgentForEditor();
                forEditor = true;
            }
            return agent.animationType.GetTriggerParamNames(agent, forEditor).Cast<T>().ToList();
        }

        public override Type ForType(Agent agent)
        {
            return typeof(string);
        }
    }
}

