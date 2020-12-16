using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace TotalAI
{
    public class Projectile : MonoBehaviour
    {
        public float speed = 20;        
        public GameObject target;
        public float timeAfterHitBeforeDestroy = 1f;

        private float hitTime = 0f;

        void Update()
        {
            if (target != null)
            {
                Vector3 targetPosition = target.transform.position + new Vector3(0f, 1.0f, 0f);
                transform.position = Vector3.MoveTowards(transform.position, targetPosition, Time.deltaTime * speed);

                // if it isn't moving anymore remove it after timeAfterHitBeforeDestroy seconds
                if (hitTime == 0f && Vector3.Distance(transform.position, targetPosition) < 0.1f)
                    hitTime = Time.time;
                else if (hitTime != 0f && Time.time - hitTime > timeAfterHitBeforeDestroy)
                    gameObject.GetComponent<Entity>().DestroySelf(null);
            }
        }
    }
}
