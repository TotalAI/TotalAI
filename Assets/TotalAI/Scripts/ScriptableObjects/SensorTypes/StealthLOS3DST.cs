using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;

namespace TotalAI
{
    [CreateAssetMenu(fileName = "StealthLOS3DST", menuName = "Total AI/Sensor Types/Stealth LOS 3D", order = 0)]
    public class StealthLOS3DST : SensorType
    {
        [Header("Alertness Attribute Type")]
        public MinMaxFloatAT alertnessAT;

        [Header("Stealth Attribute Type")]
        public MinMaxFloatAT stealthAT;

        [Header("Max distance to sense")]
        public OneFloatAT maxDistanceAT;

        [Header("Field of view in degrees")]
        public OneFloatAT fieldOfViewAT;

        [Header("Which layers should be able to block the agent's line of sight?")]
        public LayerMask layersForRaycast;

        [Header("How high up or down from transform of agent should raycast start at?")]
        public float heightToStartRaycast;

        [Header("Curves to control impact of factors - all should be 0-1 on x and y")]
        public AnimationCurve alertnessCurve;
        public AnimationCurve stealthCurve;
        public AnimationCurve distanceCurve;
        public AnimationCurve angleCurve;

        public override void Setup(Agent agent, int entityLayers)
        {
            base.Setup(agent, entityLayers);

            if (alertnessAT == null)
            {
                Debug.LogError("Stealth LOS Sensor needs an AttributeType for the alertnessAT.");
            }
            if (!agent.attributes.Keys.Contains(alertnessAT))
            {
                Debug.LogError("Stealth LOS Sensor is attached to an agent who is missing the alertnessAT: " + alertnessAT.name);
            }
            if (maxDistanceAT == null)
            {
                Debug.LogError("Stealth LOS Sensor needs an AttributeType for the max distance.");
            }
            if (!agent.attributes.Keys.Contains(maxDistanceAT))
            {
                Debug.LogError("Stealth LOS Sensor is attached to an agent who is missing the maxDistanctAttributeType: " + maxDistanceAT.name);
            }
            if (fieldOfViewAT == null)
            {
                Debug.LogError("Stealth LOS Sensor needs an Field of View (FOV).");
            }
            if (!agent.attributes.Keys.Contains(fieldOfViewAT))
            {
                Debug.LogError("Stealth LOS Sensor is attached to an agent who is missing the FOVAttributeType: " + fieldOfViewAT.name);
            }
        }

        public override int Run(Agent agent, Entity[] entities)
        {
            int numEntities = 0;
            float maxDistance = agent.attributes[maxDistanceAT].GetLevel();
            float fieldOfView = agent.attributes[fieldOfViewAT].GetLevel();
            float alertness = agent.attributes[alertnessAT].GetLevel();

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

                    if (angle < fieldOfView / 2f)
                    {
                        float distanceToEntity = Vector3.Distance(entity.transform.position, agent.transform.position);
                        RaycastHit hit;
                        Vector3 addedHeight = new Vector3(agent.transform.position.x, agent.transform.position.y + heightToStartRaycast,
                                                          agent.transform.position.z);
                        if (!Physics.Raycast(addedHeight, directionToEntity, out hit, distanceToEntity, layersForRaycast))
                        {
                            //Debug.Log(agent.name + ": StealthLOSSensor detected and has LOS to " + entity.name);

                            // Check to see if target was stealthy enough to avoid detection
                            if (!(entity is Agent target) || agent.memoryType.InShortTermMemory(agent, target) ||
                                !AvoidedDetection(agent, target, alertness / 100f, distanceToEntity / maxDistance, angle / (fieldOfView / 2f)))
                            {
                                //Debug.Log(agent.name + ": StealthLOSSensor " + entity.name + " did NOT avoid detection");

                                entities[numEntities] = entity;
                                numEntities++;
                            }
                        }
                    }
                }
            }
            return numEntities;
        }

        public bool AvoidedDetection(Agent agent, Agent target, float alertness, float distance, float angle)
        {
            if (!target.attributes.TryGetValue(stealthAT, out Attribute attribute))
            {
                Debug.Log(agent.name + ": StealthLOSSensor " + target.name + " does not have StealthAT - automatically detected");
                return false;
            }
            float stealth = attribute.GetLevel() / 100f;

            // Figure in alertness, stealth, angle, and distance to determine detection probablility
            // Lower alertness less chance of detection
            // Higher stealth less chance of detection
            // Farther distance less chance of detection
            // Greater angle less chance of detection

            // Each factor should give a 0-1 probablity for detection - then weight each one to come up with final probability
            // Feels like weights could also change - if target is close to agent they should be able to detect it
            float alertnessFactor = alertnessCurve.Evaluate(alertness);
            float stealthFactor = stealthCurve.Evaluate(stealth);
            float distanceFactor = distanceCurve.Evaluate(distance);
            float angleFactor = angleCurve.Evaluate(angle);

            // Higher factors means the target has a lower chance of being detected
            float weightedFactors = (alertnessFactor + stealthFactor + distanceFactor + angleFactor) / 4f;
            Debug.Log(agent.name + ": StealthLOSSensor " + alertnessFactor + ":" + stealthFactor + ":" + distanceFactor + ":" +
                      angleFactor + " = " + weightedFactors);

            bool avoidedDetection = Random.Range(0f, 1f) < weightedFactors;
            Debug.Log(agent.name + ": StealthLOSSensor " + target.name + " avoided detection = " + avoidedDetection);
            return avoidedDetection;
        }

        public override void DrawGizmos(Agent agent)
        {
            if (agent.attributes != null && agent.attributes.Count > 0)
            {
                Gizmos.color = Color.yellow;
                Gizmos.DrawWireSphere(agent.transform.position, agent.attributes[maxDistanceAT].GetLevel());

                Vector3 addedHeight = new Vector3(agent.transform.position.x, agent.transform.position.y + heightToStartRaycast,
                                                              agent.transform.position.z);
                Gizmos.DrawRay(addedHeight, Quaternion.AngleAxis(agent.attributes[fieldOfViewAT].GetLevel() / 2f, Vector3.up) *
                                agent.transform.forward * agent.attributes[maxDistanceAT].GetLevel());
                Gizmos.DrawRay(addedHeight, Quaternion.AngleAxis(-agent.attributes[fieldOfViewAT].GetLevel() / 2f, Vector3.up) *
                                agent.transform.forward * agent.attributes[maxDistanceAT].GetLevel());
            }
        }
    }
}