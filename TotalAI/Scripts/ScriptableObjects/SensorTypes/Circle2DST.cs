using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace TotalAI
{
    [CreateAssetMenu(fileName = "Circle2DST", menuName = "Total AI/Sensor Types/Circle 2D", order = 0)]
    public class Circle2DST : SensorType
    {
        [Header("Radius of the detection sphere")]
        public AttributeType radiusAttributeType;

        // TODO: Maybe have colliders in each sensor class - not in base class?
        private Collider2D[] colliders2D;
        
        public override void Setup(Agent agent, int entityLayers)
        {
            // Needed to clear out Dicts after playing for AgentView Editor Window
            if (!Application.isPlaying && runCounter != null && runCounter.ContainsKey(agent))
            {
                OnEnable();
            }
            runCounter[agent] = 1;
            this.entityLayers = entityLayers;
            colliders2D = new Collider2D[maxColliders];

            if (radiusAttributeType == null)
            {
                Debug.LogError("Sphere Sensor needs an AttributeType for the radius of the sphere.");
            }
            if (!agent.attributes.Keys.Contains(radiusAttributeType))
            {
                Debug.LogError("Sphere Sensor is attached to an agent who is missing the radiusAttributeType: " + radiusAttributeType.name);
            }
        }

        public override int Run(Agent agent, Entity[] entities)
        {
            int numEntities = 0;
            int numHits = Physics2D.OverlapCircleNonAlloc(agent.transform.position, agent.attributes[radiusAttributeType].GetLevel(),
                                                          colliders2D, entityLayers);
            for (int i = 0; i < numHits; ++i)
            {
                if (!colliders2D[i].gameObject.activeInHierarchy)
                {
                    Debug.LogError("Inactive game object sensed: " + colliders2D[i].gameObject.name);
                }
                
                // There should only be one active Entity but there could be a disabled one (Agent -> CorpseWorldObjectType)
                // TODO: Should it be required that the Collider and Entity are on the root?
                Entity[] enabledEntities = colliders2D[i].gameObject.GetComponentsInParent<Entity>(false);
                if (enabledEntities == null || enabledEntities.Length == 0)
                {
                    //Debug.LogError("Missing Entity on a GameObject - " + colliders[i].gameObject.name);
                }
                else
                {
                    foreach (Entity entity in enabledEntities)
                    {
                        if (entity != agent && entity.enabled && Array.IndexOf(entities, entity, 0, numEntities) == -1)
                        {
                            entities[numEntities] = entity;
                            numEntities++;
                        }
                    }
                }
            }

            return numEntities;
        }

        public override void DrawGizmos(Agent agent)
        {
            if (!Application.isPlaying)
            {
                if (agent.agentType != null && agent.agentType.defaultAttributes != null)
                {
                    EntityType.DefaultAttribute defaultAttribute = agent.agentType.defaultAttributes.Find(x => x.type == radiusAttributeType);
                    if (defaultAttribute != null)
                    {
                        Gizmos.color = Color.yellow;
                        if (defaultAttribute.minMaxFloatData != null)
                            Gizmos.DrawWireSphere(agent.transform.position, defaultAttribute.minMaxFloatData.floatValue);
                        else
                            Gizmos.DrawWireSphere(agent.transform.position, defaultAttribute.oneFloatData.floatValue);
                    }
                }
            }
            else
            {
                if (agent.attributes != null && agent.attributes.ContainsKey(radiusAttributeType))
                {
                    Gizmos.color = Color.yellow;
                    Gizmos.DrawWireSphere(agent.transform.position, agent.attributes[radiusAttributeType].GetLevel());
                }
            }
        }
    }
}