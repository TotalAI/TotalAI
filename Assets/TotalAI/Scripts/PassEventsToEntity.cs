using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace TotalAI
{
    // Use this if you want the Colliders to be on children of Entity GameObject
    public class PassEventsToEntity : MonoBehaviour
    {
        
        public bool onCollisionEnter = true;
        public bool onCollisionExit = true;
        public bool onCollisionStay = true;
        public bool onTriggerEnter = true;
        public bool onTriggerExit = true;
        public bool onTriggerStay = true;
        public bool onParticleCollision = true;

        private Entity entity;

        void Start()
        {
            entity = GetComponentInParent<Entity>();
            if (entity == null)
                Debug.LogError(name + ": PassEventsToEntity has no parent with an Entity component.  Please Fix.");
        }

        private void OnCollisionEnter(Collision collision)
        {
            if (onCollisionEnter)
                entity.OnCollisionEnter(collision);
        }

        private void OnCollisionExit(Collision collision)
        {
            if (onCollisionExit)
                entity.OnCollisionExit(collision);
        }

        private void OnCollisionStay(Collision collision)
        {
            if (onCollisionStay)
                entity.OnCollisionStay(collision);
        }

        private void OnTriggerEnter(Collider other)
        {
            if (onTriggerEnter)
                entity.OnTriggerEnter(other);
        }

        private void OnTriggerExit(Collider other)
        {
            if (onTriggerExit)
                entity.OnTriggerExit(other);
        }

        private void OnTriggerStay(Collider other)
        {
            if (onTriggerStay)
                entity.OnTriggerStay(other);
        }

        private void OnParticleCollision(GameObject other)
        {
            if (onParticleCollision)
                entity.OnParticleCollision(other);
        }
    }
}
