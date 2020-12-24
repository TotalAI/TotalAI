using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace TotalAI
{
    [CreateAssetMenu(fileName = "Sphere3DST", menuName = "Total AI/Sensor Types/Sphere 3D", order = 0)]
    public class Sphere3DST : SensorType
    {
        [Header("Radius of the detection sphere")]
        public AttributeType radiusAttributeType;

        [Header("Should Entities that are in inventory also be detected?")]
        public bool detectInventoryEntities = true;

        [Header("Do a full recursive nested inventory detection?")]
        public bool detectNestedInventoryEntities = false;

        public override void Setup(Agent agent, int entityLayers)
        {
            base.Setup(agent, entityLayers);

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
            int numHits = Physics.OverlapSphereNonAlloc(agent.transform.position, agent.attributes[radiusAttributeType].GetLevel(),
                                                        colliders, entityLayers, QueryTriggerInteraction.Collide);
            for (int i = 0; i < numHits; ++i)
            {
                if (!colliders[i].gameObject.activeInHierarchy)
                {
                    Debug.LogError("Inactive game object sensed: " + colliders[i].gameObject.name);
                }
                
                // There should only be one active Entity but there could be a disabled one (Agent -> CorpseWorldObjectType)
                // TODO: Should it be required that the Collider and Entity are on the root?
                Entity[] enabledEntities = colliders[i].gameObject.GetComponentsInParent<Entity>(false);
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
                            if (detectInventoryEntities && entity.inventoryType != null)
                            {
                                foreach (Entity inventoryEntity in entity.inventoryType.GetAllEntities(entity, detectNestedInventoryEntities))
                                {
                                    entities[numEntities] = inventoryEntity;
                                    numEntities++;
                                }
                            }
                        }
                    }
                }
            }

            return numEntities;
        }

        public override void DrawGizmos(Agent agent)
        {
            Gizmos.color = Color.yellow;
            if (Application.isPlaying)
            {
                Gizmos.DrawWireSphere(agent.transform.position, agent.attributes[radiusAttributeType].GetLevel());
            }
            else if (agent.agentType != null)
            {
                EntityType.DefaultAttribute defaultAttribute = agent.agentType.defaultAttributes.Find(x => x.type == radiusAttributeType);
                if (defaultAttribute != null && defaultAttribute.minMaxFloatData != null)
                    Gizmos.DrawWireSphere(agent.transform.position, defaultAttribute.minMaxFloatData.floatValue);
            }
        }
    }
}