using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace TotalAI
{
    public class NonRootMotionMovement : MonoBehaviour
    {
        [SerializeField]
        private string isMovingBoolParamName = "isRunning";
        private int isMovingBoolParamID;

        [SerializeField]
        private float movingTurnSpeed = 360;

        [SerializeField]
        private float stationaryTurnSpeed = 180;

        private Agent agent;

        public void Initialize(Agent agent)
        {
            isMovingBoolParamID = Animator.StringToHash(isMovingBoolParamName);
            this.agent = agent;
        }

        private void Update()
        {
            Vector3 move = Vector3.zero;
            if (agent.movementType.RemainingDistance(agent) > agent.movementType.StoppingDistance(agent))
            {
                move = agent.movementType.DesiredVelocityVector(agent);
                Move(move);
            }

            UpdateAnimator(move, agent.movementType.LookAt(agent));
        }

        private void Move(Vector3 move)
        {
            //if (move.magnitude > 1f) move.Normalize();
            move = transform.InverseTransformDirection(move);         
            float turnAmount = Mathf.Atan2(move.x, move.z);
            float forwardAmount = move.z;
            float turnSpeed = Mathf.Lerp(stationaryTurnSpeed, movingTurnSpeed, forwardAmount);

            //transform.Rotate(0, turnAmount * turnSpeed * Time.deltaTime, 0);

        }

        private void UpdateAnimator(Vector3 move, Vector3 lookAt)
        {
            //agent.animationType.SetBool(agent, isMovingBoolParamID, move.magnitude > 0.01f);
            //agent.animationType.SetFloat(agent, "Forward", move.magnitude);
            agent.animationType.SetFloat(agent, "Speed", move.magnitude);
            //agent.animationType.SetFloat(agent, "Vertical", move.x);
            //agent.animationType.SetFloat(agent, "Horizontal", move.y);

            if (lookAt != Vector3.zero)
            {
                agent.animationType.SetFloat(agent, "LookX", lookAt.x);
                agent.animationType.SetFloat(agent, "LookY", lookAt.y);
            }
        }
    }
}
