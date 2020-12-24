using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace TotalAI
{
    public class Behavior
    {
        public class Context
        {
            public ActionType actionType;
            public Entity target;
            public List<Selector> attributeSelectors;

            public void Set(ActionType actionType, Entity target, List<Selector> attributeSelectors)
            {
                this.actionType = actionType;
                this.target = target;
                this.attributeSelectors = attributeSelectors;
            }

            public void Reset()
            {
                actionType = null;
                target = null;
                attributeSelectors = null;
            }
        }

        private Agent agent;
        
        private Context behaviorContext;
        private BehaviorType behaviorType;
        private Vector3 targetPosition;

        private bool isRunning;

        private bool isWaiting;
        private float waitTime;
        private float startedWaitingAt;

        private bool wasInterrupted;
        private float lastOCUpdateTime;

        public void ResetBehavior()
        {
            behaviorType = null;
            isRunning = false;
            isWaiting = false;
            waitTime = 0f;
            startedWaitingAt = 0f;
            wasInterrupted = false;
            lastOCUpdateTime = 0f;

            behaviorContext.Reset();
        }

        public void Setup(Agent agent)
        {
            this.agent = agent;
            behaviorContext = new Context();
        } 

        // Can already be interrupted due to BeforeStart OutputChanges failing
        public void StartBehavior(ActionType actionType, Entity target, List<Selector> attributeSelectors, bool wasInterrupted)
        {
            behaviorType = actionType.behaviorType;
            behaviorContext.Set(actionType, target, attributeSelectors);

            isRunning = true;
            this.wasInterrupted = wasInterrupted;
            lastOCUpdateTime = Time.time;

            behaviorType.SetContext(agent, behaviorContext);
            agent.historyType.RecordBehaviorLog(agent, HistoryType.BehaviorRunType.Started, behaviorType);
            behaviorType.StartBehavior(agent);
            agent.StartCoroutine(UpdateBehavior());
        }

        private IEnumerator UpdateBehavior()
        {
            yield return new WaitForSeconds(behaviorType.afterStartWaitTime);
            
            while (isRunning)
            {
                // Check to see if repeating OutputChanges need to run
                agent.decider.RunRepeatingOutputChanges();
                agent.decider.RunAfterGameMinutesOutputChanges();

                if (isWaiting && Time.time - startedWaitingAt > waitTime)
                    isWaiting = false;

                if (!isWaiting && behaviorType.IsFinished(agent))
                {
                    agent.historyType.RecordBehaviorLog(agent, HistoryType.BehaviorRunType.Finished, behaviorType);
                    isRunning = false;
                }
                else
                {
                    agent.historyType.RecordBehaviorLog(agent, HistoryType.BehaviorRunType.Updated, behaviorType);
                    behaviorType.UpdateBehavior(agent);

                    yield return new WaitForSeconds(behaviorType.afterStartWaitTime);
                }
            }

            yield return new WaitForSeconds(behaviorType.beforeFinishWaitTime);
            FinishBehavior();
        }

        // TODO: Add hook into BehaviorType - possible to want to do clean up 
        public void FinishBehavior()
        {
            bool wasInterrupted = this.wasInterrupted;

            Entity target = behaviorContext.target;
            ResetBehavior();

            agent.decider.MappingFinished(target, wasInterrupted);
        }

        // Can be called to stop running behavior if BehaviorType.IsFinished can't be used
        // For an example see the BehaviorDesigner Integration BehaviorType
        public void EndBehavior()
        {
            isRunning = false;
        }

        public void InterruptBehavior(bool cameFromBehavior)
        {
            agent.historyType.RecordBehaviorLog(agent, HistoryType.BehaviorRunType.Interrupted, behaviorType, cameFromBehavior);

            // Interrupts can come from the decider or from the behavior
            if (cameFromBehavior)
                agent.decider.InterruptMapping(true);
            else if (behaviorType != null)
                behaviorType.InterruptBehavior(agent);

            isRunning = false;
            wasInterrupted = true;
        }

        // Used by BehaviorTypes so that Gizmo will show correctly
        public void SetTargetPosition(Vector3 targetPosition)
        {
            this.targetPosition = targetPosition;
        }

        public void SetIsWaiting(float secondsToWait)
        {
            if (secondsToWait == -1f)
                secondsToWait = float.PositiveInfinity;

            if (secondsToWait <= 0.001f)
            {
                isWaiting = false;
                waitTime = 0f;
                startedWaitingAt = 0f;
            }
            else
            {
                isWaiting = true;
                waitTime = secondsToWait;
                startedWaitingAt = Time.time;
            }
        }

        private void OnDrawGizmosSelected()
        {
            Gizmos.color = Color.red;
            if (behaviorContext != null && behaviorContext.target != null)
                Gizmos.DrawSphere(behaviorContext.target.transform.position, 0.25f);

            Gizmos.color = Color.magenta;
            if (behaviorContext != null)
                Gizmos.DrawSphere(targetPosition, 0.25f);
        }
    }
}