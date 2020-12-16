using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;

namespace TotalAI
{
    [CreateAssetMenu(fileName = "LOS3DST", menuName = "Total AI/Sensor Types/LOS 3D", order = 0)]
    public class LOS3DST : SensorType
    {
        [Header("Max distance to sense")]
        public OneFloatAT maxDistanceAttributeType;

        [Header("Field of view (FOV) in degrees")]
        public AttributeType FOVAttributeType;

        [Header("Which layers should be able to block the agent's line of sight?")]
        public LayerMask layersForRaycast;

        [Header("How high up or down from transform of agent should raycast start at?")]
        public float heightToStartRaycast;

        public override void Setup(Agent agent, int entityLayers)
        {
            base.Setup(agent, entityLayers);

            if (maxDistanceAttributeType == null)
            {
                Debug.LogError("LOS Sensor needs an AttributeType for the max distance.");
            }
            if (!agent.attributes.Keys.Contains(maxDistanceAttributeType))
            {
                Debug.LogError("LOS Sensor is attached to an agent who is missing the maxDistanctAttributeType: " + maxDistanceAttributeType.name);
            }
            if (FOVAttributeType == null)
            {
                Debug.LogError("LOS Sensor needs an Field of View (FOV).");
            }
            if (!agent.attributes.Keys.Contains(FOVAttributeType))
            {
                Debug.LogError("LOS Sensor is attached to an agent who is missing the FOVAttributeType: " + FOVAttributeType.name);
            }
        }

        public override int Run(Agent agent, Entity[] entities)
        {
            int numEntities = 0;
            float maxDistance = agent.attributes[maxDistanceAttributeType].GetLevel();
            int numHits = Physics.OverlapSphereNonAlloc(agent.transform.position, maxDistance,
                                                        colliders, entityLayers, QueryTriggerInteraction.Collide);
            for (int i = 0; i < numHits; ++i)
            {
                if (!colliders[i].gameObject.activeInHierarchy)
                {
                    Debug.LogError("Inactive game object sensed: " + colliders[i].gameObject.name);
                }

                Entity entity = colliders[i].gameObject.GetComponentInParent<Entity>();
                if (entity == null)
                {
                    Debug.LogError("Missing Entity on a GameObject - " + colliders[i].gameObject.name);
                }
                else if (entity != agent)  // Should dup entities be filtered out here?
                {
                    // See if entity if within the agent's FOV
                    Vector3 directionToEntity = entity.transform.position - agent.transform.position;
                    float angle = Vector3.Angle(directionToEntity, agent.transform.forward);

                    if (angle < agent.attributes[FOVAttributeType].GetLevel() / 2f)
                    {
                        float distanceToEntity = Vector3.Distance(entity.transform.position, agent.transform.position);
                        RaycastHit hit;
                        Vector3 addedHeight = new Vector3(agent.transform.position.x, agent.transform.position.y + heightToStartRaycast,
                                                          agent.transform.position.z);
                        if (!Physics.Raycast(addedHeight, directionToEntity, out hit, distanceToEntity, layersForRaycast))
                        {
                            // It hit nothing - the entity should be in the layerMask so it won't hit the entity
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
            if (agent.attributes != null && agent.attributes.Count > 0)
            {
                Gizmos.color = Color.yellow;
                //Gizmos.DrawWireSphere(agent.transform.position, agent.attributes[maxDistanceAttributeType].GetLevel());

                Vector3 addedHeight = new Vector3(agent.transform.position.x, agent.transform.position.y + heightToStartRaycast,
                                                              agent.transform.position.z);
                Gizmos.DrawRay(addedHeight, Quaternion.AngleAxis(agent.attributes[FOVAttributeType].GetLevel() / 2f, Vector3.up) *
                                agent.transform.forward * agent.attributes[maxDistanceAttributeType].GetLevel());
                Gizmos.DrawRay(addedHeight, Quaternion.AngleAxis(-agent.attributes[FOVAttributeType].GetLevel() / 2f, Vector3.up) *
                                agent.transform.forward * agent.attributes[maxDistanceAttributeType].GetLevel());
            }
        }
    }
}