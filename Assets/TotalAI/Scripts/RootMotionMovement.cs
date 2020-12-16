using System.Collections;
using UnityEngine;

namespace TotalAI
{
	//[RequireComponent(typeof(Rigidbody))]
	public class RootMotionMovement : MonoBehaviour
	{
		[SerializeField]
        private float movingTurnSpeed = 360;

        [SerializeField]
        private float stationaryTurnSpeed = 180;

        //[SerializeField]
        //private float moveSpeedMultiplier = 1f;

        //[SerializeField]
        //private float animSpeedMultiplier = 1f;

        private Agent agent;
        //private Rigidbody agentRigidbody;
        private float turnAmount;
        private float forwardAmount;
        //Vector3 groundNormal;

        private Coroutine offMeshLinkCoroutine;

        public void Initialize(Agent agent)
		{
            this.agent = agent;

            //agentRigidbody = GetComponent<Rigidbody>();
            //agentRigidbody.constraints = RigidbodyConstraints.FreezeRotationX | RigidbodyConstraints.FreezeRotationY | RigidbodyConstraints.FreezeRotationZ;
        }

        private void Update()
        {
            if (!agent.movementType.IsOnOffMeshLink(agent) && offMeshLinkCoroutine != null)
                offMeshLinkCoroutine = null;

            if (agent.movementType.IsOnOffMeshLink(agent) && offMeshLinkCoroutine == null)
            {
                agent.movementType.AlignForOffMeshLinkTraversal(agent);
                offMeshLinkCoroutine = StartCoroutine(agent.movementType.TraverseOffMeshLink(agent));
            }

            if (offMeshLinkCoroutine == null)
            {
                if (agent.inEntityInventory == null && agent.movementType.RemainingDistance(agent) > agent.movementType.StoppingDistance(agent))
                    Move(agent.movementType.VelocityVector(agent));
                else
                    Move(Vector3.zero);
            }

            //Vector3 worldDeltaPosition = agent.movementType.NextPosition(agent) - transform.position;
            //if (worldDeltaPosition.magnitude > agent.movementType.Radius(agent))
            //    agent.movementType.SetNextPosition(agent, transform.position + 0.9f * worldDeltaPosition);
        }

        private void Move(Vector3 move)
		{
            //if (move.magnitude > 1f) move.Normalize();
			move = transform.InverseTransformDirection(move);

            // TODO: I think this needs to be added in?  Might be messed up if moving on hills?
            //move = Vector3.ProjectOnPlane(move, groundNormal);

            turnAmount = Mathf.Atan2(move.x, move.z);
			forwardAmount = move.z;

			ApplyExtraTurnRotation();

			UpdateAnimator(move);
		}

		void UpdateAnimator(Vector3 move)
		{
			// update the animator parameters
			agent.animationType.SetFloat(agent, "Forward", forwardAmount, 0.1f, Time.deltaTime);
            agent.animationType.SetFloat(agent, "Turn", turnAmount, 0.1f, Time.deltaTime);

            // the anim speed multiplier allows the overall speed of walking/running to be tweaked in the inspector,
            // which affects the movement speed because of the root motion.
            /*
            if (move.magnitude > 0)
			{
				m_Animator.speed = animSpeedMultiplier;
			}
			else
			{
				// don't use that while airborne
				m_Animator.speed = 1;
			}
            */
		}

		void ApplyExtraTurnRotation()
		{
			// help the character turn faster (this is in addition to root rotation in the animation)
			float turnSpeed = Mathf.Lerp(stationaryTurnSpeed, movingTurnSpeed, forwardAmount);
			transform.Rotate(0, turnAmount * turnSpeed * Time.deltaTime, 0);
		}
        
		public void OnAnimatorMove()
		{
			if (Time.deltaTime > 0 && !agent.movementType.IsOnOffMeshLink(agent))
			{
                transform.position = agent.movementType.NextPosition(agent);
            }
            else
            {
                transform.position += agent.animationType.DeltaPosition(agent);
            }
		}

        

    }
}
