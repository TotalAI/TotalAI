using System.Collections.Generic;
using UnityEngine;

namespace TotalAI
{
    [CreateAssetMenu(fileName = "DriveLevelOCT", menuName = "Total AI/Output Change Types/Drive Level", order = 0)]
    public class DriveLevelOCT : OutputChangeType
    {
        public new void OnEnable()
        {
            typeInfo = new TypeInfo()
            {
                description = "<b>Drive Level</b>: Change the drive level of this agent or target agent.",
                possibleTargetTypes = new OutputChange.TargetType[] { OutputChange.TargetType.ToSelf, OutputChange.TargetType.ToEntityTarget },
                possibleValueTypes = new OutputChange.ValueType[] { OutputChange.ValueType.FloatValue, OutputChange.ValueType.ActionSkillCurve,
                                                                    OutputChange.ValueType.Selector },
                usesLevelType = true,
                mostRestrictiveLevelType = typeof(DriveType),
                levelTypeLabel = "Drive Type",
                floatLabel = "Drive Level Change",
                actionSkillCurveLabel = "Drive Level Change From Action Skill",
                selectorLabel = "Drive Level Change"
            };
        }

        public override bool MakeChange(Agent agent, Entity target, OutputChange outputChange, Mapping mapping, float actualAmount, out bool forceStop)
        {
            forceStop = false;

            float actualChange = ((Agent)target).drives[(DriveType)outputChange.levelType].ChangeLevel(actualAmount);

            if (target != null)
                Debug.Log(agent.name + ": target = " + target.name + " - Change drive (" + outputChange.levelType.name + ") level by : " + actualChange);
            else
                Debug.Log(agent.name + ": target = None - Change drive (" + outputChange.levelType.name + ") level by : " + actualChange);
            return true;
        }

        public override float CalculateAmount(Agent agent, Entity target, OutputChange outputChange, Mapping mapping)
        {
            if (outputChange.valueType == OutputChange.ValueType.ActionSkillCurve)
            {
                return outputChange.actionSkillCurve.Eval0to100(agent.ActionSkillWithItemModifiers(mapping.mappingType.actionType));
            }
            else if (outputChange.valueType == OutputChange.ValueType.Selector)
            {
                return outputChange.selector.GetFloatValue(agent, mapping);
            }

            // This looks at the ENTIRE Plan for an equation DriveType
            // will only do it for root mappings (mapping.parent == null) so there's no double counting the changes
            DriveType driveType = (DriveType)outputChange.levelType;
            if (mapping != null && mapping.parent == null && driveType.syncType == DriveType.SyncType.Equation)
            {
                return driveType.driveTypeEquation.CalculateEquationDriveLevelChange(agent, driveType, mapping, null);
            }

            return outputChange.floatValue;
        }

        public override float CalculateSEUtility(Agent agent, Entity target, OutputChange outputChange,
                                                 Mapping mapping, DriveType mainDriveType, float amount)
        {
            // Negative amount is good (so flip sign) unless the target is an Enemy
            if (outputChange.targetType == OutputChange.TargetType.ToEntityTarget &&
                agent.memoryType.KnownEntity(agent, target).rLevel < 50)
            {
                return amount;
            }
            else if (outputChange.targetType == OutputChange.TargetType.ToSelf &&
                     mainDriveType == (DriveType)outputChange.levelType)
            {
                return 0f;
            }
            return -amount;
        }

        public override bool Match(Agent agent, OutputChange outputChange, MappingType outputChangeMappingType,
                                   InputCondition inputCondition, MappingType inputConditionMappingType)
        {
            // TODO: Handle middle ranges
            return true;
        }

        public override bool PreMatch(OutputChange outputChange, MappingType outputChangeMappingType, InputCondition inputCondition,
                                      MappingType inputConditionMappingType, List<EntityType> allEntityTypes)
        {
            // This OTC can change the drive level positive or negative for the agent or a drive level for its target agent
            if (outputChange.targetType == OutputChange.TargetType.ToSelf)
            {
                // Match this to DriveLevelICTs that are checking this agent's Drive Levels (boolValue == false)
                // Need the change amount to push the level towards the ICTs min-max range

                // No access the game state since this is run before to cache the IC to MT matches
                // If DriveLevelICT fails do we want to push the level up or down to fix it?
                // With a middle range (40-60) we have no way to know unless we know the agent's level
                // if its end ranges though (0-10 or 90-100) we do know
                // Switch to have a floatValue and say <= or >=

                // Or have runtime checks - have it match both increase and decrease
                // then check agent's drive level and filter out one of them

                if (inputCondition.levelType == outputChange.levelType && !inputCondition.boolValue)
                {
                    if (inputCondition.max == 100)
                    {
                        // The ICT wants a high drive level - Make sure OCT is pushing the value up
                        if (outputChange.valueType == OutputChange.ValueType.FloatValue && outputChange.floatValue > 0 ||
                            outputChange.valueType == OutputChange.ValueType.ActionSkillCurve && outputChange.actionSkillCurve.max > 0)
                            return true;
                    }
                    else if (inputCondition.min == 0)
                    {
                        // The ICT wants a low drive level - Make sure OCT is pushing the value up
                        if (outputChange.valueType == OutputChange.ValueType.FloatValue && outputChange.floatValue < 0 ||
                            outputChange.valueType == OutputChange.ValueType.ActionSkillCurve && outputChange.actionSkillCurve.min < 0)
                            return true;
                    }
                    else
                    {
                        // Not sure - we need to know agent's level so we need to do a runtime check
                        return true;
                    }

                    return false;
                }

            }
            else
            {
                // TODO: What does a Drive Level change to target agent match with?

            }
            return false;
        }
    }
}
