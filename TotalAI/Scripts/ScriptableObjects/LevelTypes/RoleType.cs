using System;
using System.Collections.Generic;
using UnityEngine;

namespace TotalAI
{
    [CreateAssetMenu(fileName = "RoleType", menuName = "Total AI/Level Types/Role Type", order = 0)]
    public class RoleType : LevelType
    {
        [Header("What needs to be true for role to apply to Agent?")]
        public List<InputCondition> inputConditions;

        [Header("Role will override these types.  None will do nothing.")]
        public PlannerType overridePlannerType;
        public MappingType overrideDefaultMappingType;
        public MappingType overrideDefaultDriveType;

        public enum OverrideType { None, All, OtherRoles }
        [Header("Will this role disable none, all, or just other roles DriveTypes and ActionTypes?")]
        public OverrideType overrideType;

        public enum ChangeType { Add, Remove }

        // Whenever a role is added or removed from an agent
        // All DriveChanges and ActionChanges are run through with largest priority going last
        // If priorities are the same it will go in physicial list order
        [Serializable]
        public class DriveChange {
            public ChangeType changeType;
            public DriveType driveType;  
            public float level;
            public float changePerGameHour;
            public AnimationCurve rateTimeCurve;
            public float maxTimeCurve;
            public float minTimeCurve;
            public int priority;
        }
        public List<DriveChange> driveChanges;

        [Serializable]
        public class ActionChange
        {
            public ChangeType changeType;
            public ActionType actionType;
            public float level;
            public float changeProbability;
            public float changeAmount;
            public int priority;
        }
        public List<ActionChange> actionChanges;

        public virtual void AddToAgent(Agent agent)
        {
            switch (overrideType)
            {
                case OverrideType.All:
                    // Disable all of the agent's Actions and Drives
                    agent.ChangeStatusOnAllActionsAndDrives(false);
                    break;
                case OverrideType.OtherRoles:
                    // Disable all Drives and Actions that are due to other roles
                    agent.ChangeStatusOnActionsAndDrivesFromRoles(false);
                    break;
            }

            Role role = ActivateRole(agent);
            if (role != null)
                role.AddRole(agent);
        }

        // Returns null if the role is already active on the agent
        private Role ActivateRole(Agent agent)
        {
            if (agent.roles.TryGetValue(this, out Role role))
            {
                if (role.GetStatus())
                    return null;
                role.SetActive(agent, true);
            }
            else
            {
                role = new Role(this, 0);
                agent.roles[this] = role;
            }
            return role;
        }

        public virtual void RemoveFromAgent(Agent agent)
        {
            switch (overrideType)
            {
                case OverrideType.All:
                    // Enable all of the agent's Actions and Drives
                    agent.ChangeStatusOnAllActionsAndDrives(true);
                    break;
                case OverrideType.OtherRoles:
                    // Enable all Drives and Actions that are due to other roles
                    agent.ChangeStatusOnActionsAndDrivesFromRoles(true);
                    break;
            }

            Role role = DisableRole(agent);
            if (role != null)
                role.RemoveRole(agent);
        }

        // Returns null if the role is already disabled on the agent
        private Role DisableRole(Agent agent)
        {
            if (agent.roles.TryGetValue(this, out Role role))
            {
                if (!role.GetStatus())
                    return null;
                role.SetActive(agent, false);
            }
            else
            {
                // This probably shold never happen - trying to disable a role that the agent doesn't have
                Debug.LogError(agent.name + ": RoleType.DisableRole trying to disable a RoleType " + name + " that agent does not have.");
                return null;
            }
            return role;
        }
    }
}
