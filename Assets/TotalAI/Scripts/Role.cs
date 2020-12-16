using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace TotalAI
{
    public class Role : Level
    {
        private RoleType roleType;

        public Role(RoleType roleType, float level)
        {
            levelType = roleType;
            this.roleType = roleType;
            this.level = level;
        }

        // TODO: Handles adding a role to an agent during play
        //       Complicated since it needs to reset agent and then add back drives/actions but that would remove agent's progress??
        public void AddRole(Agent agent)
        {
            UpdateDrivesAndActionsFromAdd(agent, roleType.driveChanges, roleType.actionChanges);
            agent.ResetAvailableMappingTypes();
        }

        // TODO: Handles removing a role to an agent during play
        public void RemoveRole(Agent agent)
        {
            UpdateDrivesAndActionsFromRemove(agent, roleType.driveChanges, roleType.actionChanges);
            agent.ResetAvailableMappingTypes();
        }

        // Runs through all roles and ends up with the correct changes
        // to the agent's Drives and Actions for inititalizing the Agent
        public static void InitDrivesActionsFromRoles(Agent agent, List<RoleType> roleTypes)
        {
            // TODO: Might be able to change to LevelType and remove dup code?
            List<RoleType.DriveChange> driveChanges = new List<RoleType.DriveChange>();
            List<RoleType.ActionChange> actionChanges = new List<RoleType.ActionChange>();
            foreach (RoleType roleType in roleTypes)
            {
                if (roleType.inputConditions == null || roleType.inputConditions.Count == 0 ||
                    roleType.inputConditions.All(x => x.inputConditionType.Check(x, agent, null, null, false)))
                {
                    driveChanges.AddRange(roleType.driveChanges);
                    actionChanges.AddRange(roleType.actionChanges);
                }
            }

            // Sort each list by priority in-place
            driveChanges.Sort((x, y) => x.priority.CompareTo(y.priority));
            actionChanges.Sort((x, y) => x.priority.CompareTo(y.priority));

            UpdateDrivesAndActionsFromAdd(agent, driveChanges, actionChanges);
        }

        private static void UpdateDrivesAndActionsFromAdd(Agent agent, List<RoleType.DriveChange> driveChanges,
                                                          List<RoleType.ActionChange> actionChanges)
        {
            foreach (RoleType.DriveChange driveChange in driveChanges)
            {
                if (driveChange.driveType != null)
                {
                    switch (driveChange.changeType)
                    {
                        case RoleType.ChangeType.Add:
                            agent.drives[driveChange.driveType] = new Drive(agent, driveChange.driveType, driveChange.level,
                                                                            driveChange.driveType.utilityCurve,
                                                                            driveChange.changePerGameHour, driveChange.rateTimeCurve,
                                                                            driveChange.minTimeCurve, driveChange.maxTimeCurve);
                            break;
                        case RoleType.ChangeType.Remove:
                            if (agent.drives.TryGetValue(driveChange.driveType, out Drive drive))
                                drive.SetActive(agent, false);
                            break;
                    }
                }
                else
                {
                    Debug.Log("Null driveType found in an DriveChange in a RoleType.");
                }
            }
            foreach (RoleType.ActionChange actionChange in actionChanges)
            {
                if (actionChange.actionType != null)
                {
                    switch (actionChange.changeType)
                    {
                        case RoleType.ChangeType.Add:
                            agent.actions[actionChange.actionType] = new Action(actionChange.actionType, actionChange.level, actionChange.changeProbability,
                                                                          actionChange.changeAmount);
                            break;
                        case RoleType.ChangeType.Remove:
                            if (agent.actions.TryGetValue(actionChange.actionType, out Action action))
                                action.SetActive(agent, false);
                            break;
                    }
                }
                else
                {
                    Debug.Log("Null actionType found in an ActionChange in a RoleType.");
                }
            }
        }

        private static void UpdateDrivesAndActionsFromRemove(Agent agent, List<RoleType.DriveChange> driveChanges,
                                                             List<RoleType.ActionChange> actionChanges)
        {
            Drive drive;
            foreach (RoleType.DriveChange driveChange in driveChanges)
            {
                switch (driveChange.changeType)
                {
                    case RoleType.ChangeType.Remove:
                        if (agent.drives.TryGetValue(driveChange.driveType, out drive))
                            drive.SetActive(agent, true);
                        break;
                    case RoleType.ChangeType.Add:
                        if (agent.drives.TryGetValue(driveChange.driveType, out drive))
                        {
                            // See if this drive exists in AgentType or any other active roles
                            if (!agent.agentType.HasDriveType(driveChange.driveType) && !agent.DriveTypeInActiveRoleTypes(driveChange.driveType))
                                drive.SetActive(agent, false);
                        }
                        break;
                }
            }
            Action action;
            foreach (RoleType.ActionChange actionChange in actionChanges)
            {
                switch (actionChange.changeType)
                {
                    case RoleType.ChangeType.Remove:
                        if (agent.actions.TryGetValue(actionChange.actionType, out action))
                            action.SetActive(agent, true);
                        break;
                    case RoleType.ChangeType.Add:
                        if (agent.actions.TryGetValue(actionChange.actionType, out action))
                        {
                            if (!agent.agentType.HasActionType(actionChange.actionType) && !agent.ActionTypeInActiveRoleTypes(actionChange.actionType))
                                action.SetActive(agent, false);
                        }
                        break;
                }
            }
        }

        public override float GetLevel()
        {
            return level;
        }

        public override float ChangeLevel(float amount)
        {
            return amount;
        }
    }
}