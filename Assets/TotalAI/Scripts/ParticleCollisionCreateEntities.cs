using System;
using System.Collections.Generic;
using UnityEngine;

namespace TotalAI
{
    public class ParticleCollisionCreateEntities : MonoBehaviour
    {
        [Serializable]
        public class CreateEntityTypeMapping
        {
            public EntityType sourceEntityType;
            public EntityType entityTypeToCreate;
            public int prefabVariantIndex;
            public AudioClip audioClip;
            public int soundSpotIndex;
        }

        public List<CreateEntityTypeMapping> entityTypeMappings;

        private void OnParticleCollision(GameObject other)
        {
            List<ParticleCollisionEvent> collisionEvents = new List<ParticleCollisionEvent>();
            ParticlePhysicsExtensions.GetCollisionEvents(other.GetComponent<ParticleSystem>(), gameObject, collisionEvents);

            foreach (ParticleCollisionEvent collisionEvent in collisionEvents)
            {
                Debug.Log(collisionEvent.intersection);

                // Create Entity at hit location - Entity created depends on the other
                Entity target = other.gameObject.GetComponentInParent<Entity>();
                if (target == null)
                    return;
                Debug.Log(name + ": OnParticleCollision - " + target.name);

                CreateEntityTypeMapping mapping = entityTypeMappings.Find(x => x.sourceEntityType == target.entityType);
                if (mapping == null)
                    return;

                GameObject prefab = mapping.entityTypeToCreate.prefabVariants[mapping.prefabVariantIndex];
                GameObject gameObject = mapping.entityTypeToCreate.CreateEntity(mapping.prefabVariantIndex, collisionEvent.intersection,
                                                                                prefab.transform.rotation, prefab.transform.localScale, target);
                if (mapping.audioClip != null)
                {
                    AudioSource audioSource = gameObject.GetComponent<Entity>().GetSFXGameObject(mapping.soundSpotIndex).GetComponent<AudioSource>();
                    if (audioSource == null)
                    {
                        Debug.LogError("ParticleCollisionCreateEntities: Trying to play a sound on newly created Entity but missing audio source.");
                    }
                    else
                    {
                        audioSource.clip = mapping.audioClip;
                        audioSource.loop = false;
                        audioSource.Play();
                    }
                }
            }
        }
    }
}
