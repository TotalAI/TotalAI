using System;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;

namespace TotalAI.GOAP
{
    public class GOAPPlannerManager : MonoBehaviour
    {
        private TotalAIManager totalAIManager;
        public GOAPPT plannerTypeForGOAP;

        // Helper class for generating DecisionPaths
        public class ChoiceInfo
        {
            public Mapping mapping;
            public InputCondition inputCondition;
            public List<Mapping> mappingDependencies;
            public List<InputCondition> inputConditionDependencies;
            public List<Mapping> possibleMappingDependencies;

            public static List<ChoiceInfo> FindEndChoices(List<ChoiceInfo> choiceInfos)
            {
                List<ChoiceInfo> endChoices = new List<ChoiceInfo>();

                foreach (ChoiceInfo choiceInfo in choiceInfos)
                {
                    if (choiceInfos.Where(x => x.mappingDependencies.Contains(choiceInfo.mapping) &&
                                          x.inputConditionDependencies.Contains(choiceInfo.inputCondition)).Count() == 0)
                        endChoices.Add(choiceInfo);
                }

                return endChoices;
            }
            
            public static List<ChoiceInfo> FindNoDependencyChoices(List<ChoiceInfo> choiceInfos)
            {
                List<ChoiceInfo> choices = new List<ChoiceInfo>();

                foreach (ChoiceInfo choiceInfo in choiceInfos)
                {
                    // Does choices removed from choiceInfos allows this choice to now be dependency free
                    if (!choiceInfos.Any(x => choiceInfo.mappingDependencies.Contains(x.mapping) &&
                                              choiceInfo.inputConditionDependencies.Contains(x.inputCondition)))
                            choices.Add(choiceInfo);
                }

                return choices;
            }
        }
        
        void Start()
        {
            if (plannerTypeForGOAP == null)
            {
                Debug.LogError("GOAPPlannerManager is missing plannerTypeForGOAP.  Please add.");
                return;
            }

            totalAIManager = FindObjectOfType<TotalAIManager>();
            if (totalAIManager == null)
                Debug.LogError("Missing TotalAIManager in Scene.  Please add.");

            // For DeepRL we need a list of all possible values for each type so we can encode the types as one-hot vectors
            // TODO: Global param turning DeepRL on/off
            //CreateTypeLists();
        }

        /*
        // For Deep Learning
        public void CreateTypeLists()
        {
            Agent[] agents = FindObjectsOfType<Agent>();

            foreach (Agent agent in agents)
            {
                // Check each ActionType an agent can perform, grab all MTs attached to these ActionTypes
                foreach (ActionType actionType in ((AgentType)agent.entityType).defaultActionTypes)
                {
                    foreach (MappingType mappingType in actionType.mappingTypes)
                    {
                        // Add to MT list if it is not already in the list

                    }
                }
                // Grab all of the Drives

            }
            // Sort list so it never changes?  What happens if new MTs show up?  Create date?
        }
        */

        // Called while a plan is being run - check to see if the plan should change targets
        // Mapping will be the next Mapping to run - so ignore any Mappings to left of it (already run Mappings)
        public void ReevaluateTargets(Agent agent, Mapping mapping)
        {
            
        }

        // Takes in plans with only rootMappings for a drive - when finished plans will have all possible plans for this Drive Type
        public Plans CreatePlansForDriveType(Agent agent, AgentType agentType, DriveType driveType)
        {
            // Editor only code - since Start has not run
            if (totalAIManager == null)
            {
                totalAIManager = FindObjectOfType<TotalAIManager>();
                if (totalAIManager == null)
                {
                    Debug.LogError("Missing TotalAIManager in Scene.  Please add.");
                    return null;
                }
                else if (totalAIManager.fixesInputCondition == null || totalAIManager.fixesInputCondition.Count == 0)
                {
                    totalAIManager.CreateICToMTDictionary();
                }
            }

            if (driveType == null) return null;

            // Find all mappings that reduce the currentDrive
            // This is pretending that a DriveLevelICT for currentDriveType failed and then finding all OutputChanges that fix that
            // TODO: agent will only be null in Editor - remove code path for builds
            List<ActionType> actionTypes;
            if (agent != null)
                actionTypes = agent.ActiveActions().Keys.ToList();
            else
                actionTypes = AgentType.DefaultAction.ActionTypes(agentType.defaultActions);

            InputCondition inputCondition = totalAIManager.fakeDriveLevelMappingTypes[driveType].inputConditions[0];
            List<Mapping> possibleMappings = FindMappingsForOutputChange(agent, agentType, inputCondition,
                                                                         totalAIManager.fakeDriveLevelMappingTypes[driveType]);
            
            // Find all plans that result in reducing the currentDriveType
            Plans plans = new Plans(driveType, possibleMappings);

            // Root mappings can be added while creating plans - so we need to get the count now and loop with the original count
            int numberRootMappings = plans.rootMappings.Count;

            if (numberRootMappings > 0)
            {
                // Cache these for InventoryTarget Selection
                List<EntityType> allAgentInventoryEntityTypes;
                List<Entity> allAgentInventoryEntities;
                if (agent == null)
                {
                    allAgentInventoryEntityTypes = totalAIManager.allEntityTypes;
                    allAgentInventoryEntities = new List<Entity>();
                    GameObject[] gos = UnityEngine.SceneManagement.SceneManager.GetActiveScene().GetRootGameObjects();

                    foreach (var go in gos)
                    {
                        allAgentInventoryEntities.AddRange(go.GetComponentsInChildren<Entity>());
                    }
                }
                else
                {
                    allAgentInventoryEntityTypes = agent.inventoryType.GetAllEntityTypes(agent, true);
                    allAgentInventoryEntities = agent.inventoryType.GetAllEntities(agent, true);
                }

                for (int r = 0; r < numberRootMappings; r++)
                {
                    Mapping rootMapping = plans.rootMappings[r];
                    List<ChoiceInfo> choiceInfos = CreatePlanTree(agent, agentType, rootMapping, out Dictionary<Mapping, List<Mapping>> entitySelections);

                    if ((rootMapping.isComplete || !Application.isPlaying) && choiceInfos.Count > 0)
                    {
                        int numNewRootMappings = 0;
                        // TODO: Can this can alter choiceInfos?  If not it can be moved inside if
                        BuildDependencies(choiceInfos, rootMapping);
                        if (choiceInfos.Count > 0)
                        {
                            List<Mapping[]> paths = FindAllDecisionPaths(agent, plans, choiceInfos);
                            numNewRootMappings = paths.Count - 1;
                            ConvertPathsToRootMappings(agent, rootMapping, plans, choiceInfos, paths, entitySelections);
                        }

                        // Select Entity Target on any new root mappings
                        for (int i = plans.rootMappings.Count - 1; i >= plans.rootMappings.Count - numNewRootMappings; i--)
                        {
                            Mapping newRootMapping = plans.rootMappings[i];
                            if (newRootMapping.isComplete)
                            {
                                if (!PlanEntitySelection(agent, newRootMapping, entitySelections))
                                {
                                    newRootMapping.isComplete = false;
                                }
                                else
                                {
                                    if (!SetInventoryTargets(agent, newRootMapping.GetLeftmostLeaf(), allAgentInventoryEntityTypes,
                                                             allAgentInventoryEntities, new List<Entity>()))
                                    {
                                        Debug.Log("Setting rootMapping (" + newRootMapping.mappingType.name + ") to false due to failed SetInventoryTargets");
                                        newRootMapping.isComplete = false;
                                    }
                                }
                            }
                        }
                    }

                    // Select Entity Targets on rootMapping
                    if (rootMapping.isComplete)
                    {
                        if (!PlanEntitySelection(agent, rootMapping, entitySelections))
                        {
                            rootMapping.isComplete = false;
                        }
                        else
                        {
                            if (!SetInventoryTargets(agent, rootMapping.GetLeftmostLeaf(), allAgentInventoryEntityTypes,
                                                     allAgentInventoryEntities, new List<Entity>()))
                            {
                                Debug.Log("Setting rootMapping (" + rootMapping.mappingType.name + ") to false due to failed SetInventoryTargets");
                                rootMapping.isComplete = false;
                            }
                        }
                    }
                }
            }
            return plans;
        }

        // Recursively goes through a completed plan starting at the left most leaf and sets any Inventory Targets
        // that are needed (has an InventoryGrouping IC and isn't already set)
        private bool SetInventoryTargets(Agent agent, Mapping mapping, List<EntityType> allAgentInventoryEntityTypes,
                                         List<Entity> allAgentInventoryEntities, List<Entity> pickedUpEntities)
        {
            List<KeyValuePair<Entity, float>> targetsRanked = null;

            for (int i = 0; i < mapping.mappingType.inputConditions.Count; i++)
            {
                InputCondition inputCondition = mapping.mappingType.inputConditions[i];
                List<TypeGroup> groupings = inputCondition.InventoryTypeGroups(mapping.mappingType, out List<int> indexesToSet);

                // Need to see if there are additional InventoryTypeGroups from parent mapping
                List<int> parentsIndexesToSet = null;
                if (groupings != null && mapping.parent != null)
                {
                    // See if this Entity Selection came from a failed InventoryGroup ICT
                    // If it did include this grouping in the EntityType Selection
                    InputCondition parentInputCondition = mapping.parent.reasonForChildren[mapping.childIndex];

                    // TODO: Easier way to figure out which OC is fixing the IC?  Save this info on the Mapping?  reasonForParent?
                    int outputChangeFixIndex = mapping.mappingType.OutputChangeTypeMatch(parentInputCondition, mapping.parent.mappingType,
                                                                                         totalAIManager.allEntityTypes);

                    // TODO: This logic is still complicated - need to test it more
                    if (parentInputCondition.inputConditionType.typeInfo.usesInventoryTypeGroup &&
                        mapping.mappingType.outputChanges[outputChangeFixIndex].outputChangeType.typeInfo.usesInventoryTypeGroupMatchIndex)
                    {
                        groupings.AddRange(parentInputCondition.InventoryTypeGroups(mapping.parent.mappingType, out parentsIndexesToSet));
                    }
                }

                if (groupings != null && (mapping.inventoryTargets == null || i >= mapping.inventoryTargets.Count || mapping.inventoryTargets[i] == null))
                {
                    // This handles making sure there are enough inventoryTargets
                    // Many MTs will have no InventoryGroupings so its never allocated in these cases
                    if (mapping.inventoryTargets == null)
                        mapping.inventoryTargets = new List<Entity>();

                    if (mapping.inventoryTargets.Count < mapping.mappingType.inputConditions.Count)
                    {
                        for (int j = mapping.inventoryTargets.Count; j < mapping.mappingType.inputConditions.Count; j++)
                        {
                            mapping.inventoryTargets.Add(null);
                        }
                    }

                    // Target is from the agent's inventory if the input condition does not require an Entity Target
                    bool fromAgentsInventory = !inputCondition.RequiresEntityTarget();

                    List<Entity> possibleTargets = null;
                    // Add in any entities that were picked up from previous mappings
                    List<EntityType> pickedUpEntityTypes = pickedUpEntities.Select(x => x.entityType).Distinct().ToList();

                    if (fromAgentsInventory)
                    {
                        List<EntityType> entityTypes = allAgentInventoryEntityTypes.Union(pickedUpEntityTypes).ToList();
                        entityTypes = TypeGroup.InAllTypeGroups(groupings, entityTypes);
                        possibleTargets = allAgentInventoryEntities.Union(pickedUpEntities).ToList().Where(x => entityTypes.Contains(x.entityType)).ToList();
                    }
                    else
                    {
                        // Grab Entity Target and use it's inventory - also need any inventory that was put into the target earlier in the plan tree
                        List<EntityType> entityTypes = mapping.target.inventoryType.GetAllEntityTypes(mapping.target).Union(pickedUpEntityTypes).ToList();
                        entityTypes = TypeGroup.InAllTypeGroups(groupings, entityTypes);
                        possibleTargets = mapping.target.inventoryType.GetAllEntities(mapping.target, entityTypes, false);
                        List<Entity> possiblePickedUp = pickedUpEntities.Where(x => entityTypes.Contains(x.entityType)).ToList();
                        possibleTargets = possibleTargets.Union(possiblePickedUp).ToList();
                    }

                    if (agent == null)
                    {
                        // TODO: Is it possible to run any of the TFs while in Editor?
                        // TODO: In GOAP Plan Tree Editor - Someway to select which combo to show?
                        if (possibleTargets.Count > 0)
                        {
                            targetsRanked = new List<KeyValuePair<Entity, float>>() { new KeyValuePair<Entity, float>(possibleTargets[0], 1f) };
                            Entity target = targetsRanked[0].Key;
                            foreach (int index in indexesToSet)
                            {
                                mapping.inventoryTargets[index] = target;
                            }
                            if (parentsIndexesToSet != null)
                            {
                                if (mapping.parent.inventoryTargets.Count < mapping.parent.mappingType.inputConditions.Count)
                                {
                                    for (int j = mapping.parent.inventoryTargets.Count; j < mapping.parent.mappingType.inputConditions.Count; j++)
                                    {
                                        mapping.parent.inventoryTargets.Add(null);
                                    }
                                }
                                foreach (int index in parentsIndexesToSet)
                                {
                                    Debug.Log("Setting parent's inventory target - " + target.name + " index == " + index);
                                    
                                    mapping.parent.inventoryTargets[index] = target;
                                }
                            }
                        }
                    }
                    else
                    {
                        targetsRanked = plannerTypeForGOAP.SelectTarget(agent, mapping, possibleTargets, true);
                        if (targetsRanked != null)
                        {
                            // TODO: Handle more selection algorithms - this is just best
                            Entity target = targetsRanked[0].Key;
                            foreach (int index in indexesToSet)
                            {
                                mapping.inventoryTargets[index] = target;
                            }
                            if (parentsIndexesToSet != null)
                            {
                                if (mapping.parent.inventoryTargets == null)
                                    mapping.parent.inventoryTargets = new List<Entity>();

                                if (mapping.parent.inventoryTargets.Count < mapping.parent.mappingType.inputConditions.Count)
                                {
                                    for (int j = mapping.parent.inventoryTargets.Count; j < mapping.parent.mappingType.inputConditions.Count; j++)
                                    {
                                        mapping.parent.inventoryTargets.Add(null);
                                    }
                                }
                                foreach (int index in parentsIndexesToSet)
                                {
                                    Debug.Log("Setting parent's inventory target - " + target.name + " index == " + index);
                                    mapping.parent.inventoryTargets[index] = target;
                                }
                            }
                        }
                        else
                        {
                            Debug.Log(agent.name + ": GOAPPlannerManager.SetInventoryTargets unable to find target - MT = " + mapping.mappingType +
                                      " of rootMapping MT = " + mapping.GetRootMapping().mappingType);
                            return false;
                        }
                    }
                }
                else if (mapping.inventoryTargets != null && i < mapping.inventoryTargets.Count && mapping.inventoryTargets[i] != null)
                {
                    // Already set due to chaining - add this to picked up Entities
                    Debug.Log(mapping.mappingType.name + ": PickedUp = " + mapping.inventoryTargets[i].name);
                    pickedUpEntities.Add(mapping.inventoryTargets[i]);
                }
            }

            // Starts at leftmost leaf and then works its way back in the order that Mappings would be carried out
            // See if there's anymore children
            if (mapping.parent != null && mapping.childIndex < mapping.parent.children.Count - 1)
            {
                // Go to next child and takes its left most leaf
                mapping = mapping.parent.children[mapping.childIndex + 1].GetLeftmostLeaf();

                if (!SetInventoryTargets(agent, mapping, allAgentInventoryEntityTypes, allAgentInventoryEntities, pickedUpEntities))
                    return false;
            }
            else if (mapping.parent != null)
            {
                // No more children - do the parent mapping
                if (!SetInventoryTargets(agent, mapping.parent, allAgentInventoryEntityTypes, allAgentInventoryEntities, pickedUpEntities))
                    return false;
            }
            return true;
        }
        
        private List<Mapping[]> FindAllDecisionPaths(Agent agent, Plans plans, List<ChoiceInfo> originalChoiceInfos)
        {
            int numChoices = originalChoiceInfos.Count;
            List<ChoiceInfo> choiceInfos = new List<ChoiceInfo>(originalChoiceInfos);

            // End up with a list of all possible ways to combine choices, -1 means choice is not reachable
            List<Mapping[]> paths = new List<Mapping[]>();

            int loopCount = 0;
            bool firstTime = true;
            while (choiceInfos.Count > 0)
            {
                // Find choices that have no dependencies
                List<ChoiceInfo> choices = ChoiceInfo.FindNoDependencyChoices(choiceInfos);

                for (int j = 0; j < choices.Count; j++)
                {
                    // Go through all combinations of these choices
                    List<Mapping> possibleMappings = choices[j].mapping.children.Where(x => choices[j].mapping.reasonForChildren[x.childIndex] ==
                                                                                            choices[j].inputCondition).ToList();
                    int numInitialPaths = paths.Count;
                    for (int i = 0; i < possibleMappings.Count; i++)
                    {
                        if (firstTime)
                        {
                            paths.Add(new Mapping[numChoices]);
                            paths[paths.Count - 1][originalChoiceInfos.IndexOf(choices[j])] = possibleMappings[i];
                        }
                        else
                        {
                            // Need to add each possible combo to every existing paths - so end up with numPaths = paths.Count * numPossibleMappings
                            for (int k = 0; k < numInitialPaths; k++) 
                            {
                                Mapping choiceValue = possibleMappings[i];

                                // Are all of the choice's possible mapping dependencies in the path?
                                if (!choices[j].possibleMappingDependencies.All(x => paths[k].Contains(x)))
                                {
                                    choiceValue = null;
                                }

                                if (i == 0) {
                                    paths[k][originalChoiceInfos.IndexOf(choices[j])] = choiceValue;
                                }
                                else if (choiceValue != null)
                                {
                                    Mapping[] newArray = new Mapping[numChoices];
                                    Array.Copy(paths[k], newArray, numChoices);
                                    paths.Add(newArray);
                                    paths[paths.Count - 1][originalChoiceInfos.IndexOf(choices[j])] = choiceValue;
                                }
                            }
                        }
                    }
                    firstTime = false;

                    choiceInfos.RemoveAt(choiceInfos.IndexOf(choices[j]));
                }

                ++loopCount;
                if (loopCount > 50)
                {
                    Debug.LogError(name + ":" + " FindAllDecisionPaths went over 50 Loops! ");
                    return null;
                }
            }

            return paths;
        }

        // Takes in all possible paths and converts them to extra rootMappings
        private void ConvertPathsToRootMappings(Agent agent, Mapping rootMapping, Plans plans, List<ChoiceInfo> choiceInfos,
                                                List<Mapping[]> paths, Dictionary<Mapping, List<Mapping>> entitySelections)
        {
            for (int i = 0; i < paths.Count - 1; i++)
            {
                Mapping newRootMapping = MakePlanFromPath(rootMapping, paths[i], choiceInfos, true, entitySelections);
                plans.Add(newRootMapping);
            }

            // Use original rootMapping that is already in plans - sets copyTree to false
            MakePlanFromPath(rootMapping, paths[paths.Count - 1], choiceInfos, false, entitySelections);
        }

        public Mapping MakePlanFromPath(Mapping rootMapping, Mapping[] path, List<ChoiceInfo> choiceInfos, bool copyTree,
                                        Dictionary<Mapping, List<Mapping>> entitySelections)
        {
            if (rootMapping.parent != null)
                Debug.LogError("MakePlanFromPath called by a non-root Mapping.");

            Mapping newRootMapping = rootMapping;
            bool atRoot = true;
            Mapping currentMapping = rootMapping;
            Mapping parentMapping = null;
            Mapping oldMapping = null;
            Dictionary<Mapping, int> currentChildIndex = new Dictionary<Mapping, int>();
            int loopCount = 0;
            while (true)
            {
                if (!currentChildIndex.TryGetValue(currentMapping, out int index))
                {
                    // See if currentMapping has any choices
                    List<ChoiceInfo> choiceMatches = choiceInfos.FindAll(x => x.mapping == currentMapping);

                    // Copy it if copyTree and set parent to new copy of parent
                    // The currentMapping is now a different mapping with a different child List
                    // but the new child List still points to the original children
                    oldMapping = currentMapping;
                    if (copyTree)
                    {
                        currentMapping = currentMapping.Copy(true);
                        currentMapping.parent = parentMapping;

                        // If this mapping is in entitySelections need to change to new copy
                        if (entitySelections.TryGetValue(oldMapping, out List<Mapping> mappings))
                        {
                            entitySelections[currentMapping] = new List<Mapping>(entitySelections[oldMapping])
                            {
                                [0] = currentMapping
                            };
                        }
                        else
                        {
                            // Need to change it to new value in entitySelections
                            // See if has an ancestor that is in entitySelections
                            Mapping ancestorMapping = currentMapping.parent;
                            while (ancestorMapping != null)
                            {
                                if (entitySelections.TryGetValue(ancestorMapping, out List<Mapping> mappings2))
                                {
                                    int i = mappings2.IndexOf(oldMapping);
                                    if (i != -1)
                                        mappings2[i] = currentMapping;
                                    break;
                                }
                                ancestorMapping = ancestorMapping.parent;
                            }
                        }

                        if (atRoot)
                        {
                            newRootMapping = currentMapping;
                            atRoot = false;
                        }
                    }

                    if (parentMapping != null)
                    {
                        // Need to make sure childIndex is correct - could be wrong due to removing children
                        int childIndex = parentMapping.children.IndexOf(oldMapping);
                        parentMapping.children[childIndex] = currentMapping;
                        parentMapping.children[childIndex].childIndex = childIndex;
                    }

                    currentChildIndex.Add(currentMapping, 0);

                    // Need to cache this since it will change when a Mapping has multiple choices
                    List<InputCondition> tempReasonForChildren = null;
                    if (currentMapping.reasonForChildren != null)
                        tempReasonForChildren = new List<InputCondition>(currentMapping.reasonForChildren);

                    foreach (ChoiceInfo choiceMatch in choiceMatches)
                    {
                        int choiceIndex = choiceInfos.IndexOf(choiceMatch);
                        Mapping choosenMapping = path[choiceIndex];

                        // If its null it means the choice will end up not being in the tree
                        // since an earlier choice was made that removed this branch
                        if (choosenMapping == null)
                            continue;

                        List<Mapping> possibleMappings = currentMapping.children.Where(x => tempReasonForChildren[x.childIndex] ==
                                                                                            choiceMatch.inputCondition).ToList();
                        foreach (Mapping possibleMapping in possibleMappings)
                        {
                            if (possibleMapping != choosenMapping)
                            {
                                int childIndex = currentMapping.children.IndexOf(possibleMapping);
                                Mapping childMapping = currentMapping.children[childIndex];

                                // Should this be removing children below it?  Going to cause memory issues?
                                currentMapping.children.RemoveAt(childIndex);
                                currentMapping.reasonForChildren.RemoveAt(childIndex);

                                // Remove from entitySelections if its in there
                                Mapping ancestorMapping = currentMapping;
                                while (ancestorMapping != null)
                                {
                                    if (entitySelections.TryGetValue(ancestorMapping, out List<Mapping> mappings))
                                    {
                                        mappings.Remove(childMapping);
                                        break;
                                    }
                                    ancestorMapping = ancestorMapping.parent;
                                }
                            }
                        }
                    }

                    if (currentMapping.children == null || currentMapping.children.Count == 0)
                    {
                        // No more children - go back up the tree looking for more children to copy
                        currentMapping = currentMapping.parent;
                        while (currentMapping != null && currentMapping.children != null && currentChildIndex[currentMapping] >= currentMapping.children.Count)
                        {
                            currentMapping = currentMapping.parent;
                        }

                        // Done - only root mapping has no parent
                        if (currentMapping == null)
                        {
                            break;
                        }
                    }
                    else
                    {
                        // currentMapping has children - goto first child
                        currentChildIndex[currentMapping] = 1;
                        parentMapping = currentMapping;
                        currentMapping = currentMapping.children[0];
                    }
                }
                else if (index < currentMapping.children.Count)
                {
                    // Move currentMapping to next child
                    currentChildIndex[currentMapping]++;
                    parentMapping = currentMapping;
                    currentMapping = currentMapping.children[currentChildIndex[currentMapping] - 1];
                }
                else
                {
                    // No more children - go back up the tree looking for more children to copy
                    currentMapping = currentMapping.parent;
                    while (currentMapping != null && currentMapping.children != null && currentChildIndex[currentMapping] >= currentMapping.children.Count)
                    {
                        currentMapping = currentMapping.parent;
                    }

                    // Done - only root mapping has no parent
                    if (currentMapping == null)
                    {
                        break;
                    }
                }

                ++loopCount;
                if (loopCount > 25)
                {
                    Debug.LogError("MakePlanFromPath went over 25 Loops! " + rootMapping);
                    return null;
                }
            }

            return newRootMapping;
        }

        // Goes from choiceInfo mapping to root collecting dependencies along the way
        private void BuildDependencies(List<ChoiceInfo> choiceInfos, Mapping rootMapping)
        {
            int numChoices = choiceInfos.Count;
            for (int i = 0; i < numChoices; i++)
            {
                // Make sure this choice is in the tree still - not sure if this is the most performant way to do this (do this while making tree?)
                // TODO: Is this ever true?
                if (!rootMapping.ContainsChild(choiceInfos[i].mapping))
                {
                    Debug.LogError("BuildDependencies Removed a ChoiceInfo");
                    choiceInfos.RemoveAt(i);
                    numChoices--;
                    i--;
                }
                else
                {
                    Mapping childMapping = choiceInfos[i].mapping;
                    Mapping currentMapping = choiceInfos[i].mapping.parent;
                    InputCondition inputCondition;
                    while (currentMapping != null)
                    {
                        inputCondition = currentMapping.reasonForChildren[childMapping.childIndex];

                        if (choiceInfos.Where(x => x.mapping == currentMapping && x.inputCondition == inputCondition).Count() > 0)
                        {
                            // These dependencies end up in reverse order - bottom of tree up to root
                            choiceInfos[i].mappingDependencies.Add(currentMapping);
                            choiceInfos[i].inputConditionDependencies.Add(inputCondition);
                            choiceInfos[i].possibleMappingDependencies.Add(childMapping);
                        }

                        childMapping = currentMapping;
                        currentMapping = currentMapping.parent;
                    }
                }
            }
        }

        private bool PlanEntitySelection(Agent agent, Mapping rootMapping, Dictionary<Mapping, List<Mapping>> entitySelections)
        {
            
            foreach (List<Mapping> mappings in entitySelections.Values)
            {
                // First Mapping in list is always the rootEntityMapping
                Mapping rootEntityMapping = mappings[0];

                // Make sure the rootEntityMapping is still in this rootMapping - it may have been removed due to Choices
                if (!rootMapping.ContainsChild(rootEntityMapping))
                    continue;

                List<List<InputCondition>> possibleCombos = AllEntityCombos(mappings, rootEntityMapping, out TypeGroup groupingToAdd);

                Entity bestTarget = BestEntityTarget(agent, rootEntityMapping, mappings, possibleCombos, groupingToAdd, out List <InputCondition> bestCombo);

                if (bestTarget == null)
                    return false;

                // Fix up tree by removing Mappings not in bestCombo and setting the Mappings' target
                TargetSelectedFixPlanTree(mappings, bestCombo, bestTarget, groupingToAdd != null);
            }
            return true;
        }

        private List<List<InputCondition>> AllEntityCombos(List<Mapping> mappings, Mapping rootEntityMapping,
                                                           out TypeGroup inventoryGroupingToAdd)
        {
            List<List<InputCondition>> possibleCombos = new List<List<InputCondition>>();
            inventoryGroupingToAdd = null;

            // First combo is always all the ICs from rootEntityMapping
            List<InputCondition> rootEntityICs = rootEntityMapping.mappingType.EntityTypeInputConditions();
            possibleCombos.Add(rootEntityICs);

            InputCondition inputCondition;
            if (rootEntityMapping.parent != null)
            {
                // See if this Entity Selection came from a failed InventoryGroup ICT
                // If it did include this grouping in the EntityType Selection
                inputCondition = rootEntityMapping.parent.reasonForChildren[rootEntityMapping.childIndex];

                // TODO: Easier way to figure out which OC is fixing the IC?  Save this info on the Mapping?  reasonForParent?
                int outputChangeFixIndex = rootEntityMapping.mappingType.OutputChangeTypeMatch(inputCondition, rootEntityMapping.parent.mappingType,
                                                                                               totalAIManager.allEntityTypes);

                // ICT with both InventoryGroup and Group are confusing - possible to restrict to one or the other?
                // Example: OtherInventoryAmountICT - break into two ICTs?
                // NearEntityICT (G) and OtherInventoryAmountICT (IG) - need to add in useGroupingFromIndex?
                // TODO: This logic is still complicated - need to test it more
                if (inputCondition.inputConditionType.typeInfo.usesInventoryTypeGroup &&
                    !rootEntityMapping.mappingType.outputChanges[outputChangeFixIndex].outputChangeType.typeInfo.usesInventoryTypeGroupMatchIndex)
                {
                    inventoryGroupingToAdd = inputCondition.GetInventoryTypeGroup();
                }
            }
            
            Mapping currentMapping;
            List<InputCondition> newCombo;
            for (int i = 1; i < mappings.Count; i++)
            {
                currentMapping = mappings[i];

                // Figure out which parent IC this Mapping is for
                newCombo = currentMapping.mappingType.EntityTypeInputConditions();

                // Traverse back to the rootEntityMapping - collecting ICs to remove along the way
                while (currentMapping != rootEntityMapping)
                {
                    inputCondition = currentMapping.parent.reasonForChildren[currentMapping.childIndex];
                    currentMapping = currentMapping.parent;

                    newCombo.AddRange(currentMapping.mappingType.EntityTypeInputConditions());
                    newCombo.Remove(inputCondition);
                }

                possibleCombos.Add(newCombo);
            }

            // Also need to add in the combos of having multiple ICs in same mapping have possible mappings
            // Need to know which Mappings have the same parent but were caused by different InputConditions
            // Travel both branches at the same time?  Gonna need to draw this out...
            // TODO: Handle multiple branches


            return possibleCombos;
        }

        private Entity BestEntityTarget(Agent agent, Mapping rootEntityMapping, List<Mapping> mappings, List<List<InputCondition>> possibleCombos,
                                        TypeGroup groupingToAdd, out List<InputCondition> bestCombo)
        {
            // Get possible Entity targets from agent's memory
            List<MemoryType.EntityInfo> entityInfos = null;
            
            // Find best target
            float bestScore = -1f;
            Entity bestTarget = null;
            bestCombo = null;
            foreach (List<InputCondition> combo in possibleCombos)
            {
                List<Entity> possibleTargets;
                if (agent != null)
                {
                    // Grab list of EntityTypes that matches all of the Mappings ICs
                    // TODO: Using all allows choosing 
                    //List<EntityType> entityTypes = Grouping.PossibleEntityTypes(combo, agent.memoryType.GetKnownEntityTypes(agent));
                    List<EntityType> entityTypes = TypeGroup.PossibleEntityTypes(combo, totalAIManager.allEntityTypes);

                    if (groupingToAdd != null)
                        groupingToAdd.FilterInPlace(entityTypes);

                    float searchRadius = rootEntityMapping.mappingType.GetEntityTargetSearchRadius();
                    if (searchRadius > 0)
                        entityInfos = agent.memoryType.GetKnownEntities(agent, entityTypes, searchRadius, true);
                    else
                        entityInfos = agent.memoryType.GetKnownEntities(agent, entityTypes, -1, true);

                    possibleTargets = CheckPossibleTargets(agent, mappings, combo, entityInfos);
                }
                else
                {
                    // In Editor - Just grab all possible entities of the required EntityType
                    GameObject[] gos = UnityEngine.SceneManagement.SceneManager.GetActiveScene().GetRootGameObjects();
                    //UnityEngine.Object[] targets = GameObject.f  FindObjectsOfType(combo[0].inputOutputType.GetType());

                    List<Entity> entities = new List<Entity>();
                    foreach (var go in gos)
                    {
                        entities.AddRange(go.GetComponentsInChildren<Entity>());
                    }

                    possibleTargets = new List<Entity>();

                    List<EntityType> entityTypes = TypeGroup.PossibleEntityTypes(combo, totalAIManager.allEntityTypes);
                    foreach (var entity in entities)
                    {
                        if (entityTypes.Contains(entity.entityType))
                            possibleTargets.Add(entity);
                    }
                    //Debug.Log("Number of " + combo[0].inputOutputType.name + " in Scene = " + possibleTargets.Count);
                }

                List<KeyValuePair<Entity, float>> targetsRanked = null;
                if (agent == null)
                {
                    // TODO: Is it possible to run any of the TFs while in Editor?
                    // TODO: In GOAP Plan Tree Editor - Someway to select which combo to show?
                    if (possibleTargets.Count > 0)
                        targetsRanked = new List<KeyValuePair<Entity, float>>() { new KeyValuePair<Entity, float>(possibleTargets[0], 1f) };
                }
                else
                {
                    // TODO: Have this return the best reachable target - Need a way to choose best, best weighted, etc...
                    // If groupingToAdd exists it means this came out of an InventoryGrouping ICT
                    // So SelectTarget will use the rootEntity's parent mapping's InventoryTFs
                    if (groupingToAdd != null)
                        targetsRanked = plannerTypeForGOAP.SelectTarget(agent, rootEntityMapping.parent, possibleTargets, true);
                    else
                        targetsRanked = plannerTypeForGOAP.SelectTarget(agent, rootEntityMapping, possibleTargets, false);
                }

                if (targetsRanked != null && targetsRanked.Count > 0 && targetsRanked[0].Value >= bestScore)
                {
                    bestCombo = combo;
                    bestTarget = targetsRanked[0].Key;
                    bestScore = targetsRanked[0].Value;
                }
            }
            //Debug.Log("Best Target = " + bestTarget + " - Best Score = " + bestScore);
            return bestTarget;
        }

        private void TargetSelectedFixPlanTree(List<Mapping> mappings, List<InputCondition> bestCombo, Entity bestTarget, bool setParentInventoryTarget)
        {
            // Modify Plan Tree based on chosen target - skip rootEntityMapping since it will always be in tree
            mappings[0].target = bestTarget;

            if (setParentInventoryTarget)
            {
                InputCondition inputCondition = mappings[0].parent.reasonForChildren[mappings[0].childIndex];
                SetInventoryTargetsForChaining(mappings[0].parent, inputCondition, bestTarget);
            }

            // TODO: Performance common case - if all ICs are in combo then just remove all children of these EntityICs
            Mapping currentMapping;
            for (int i = 1; i < mappings.Count; i++)
            {
                currentMapping = mappings[i];
                currentMapping.target = bestTarget;

                // If the mapping has no ICs in combo and no children with ICs in combo - remove Mapping from Plan Tree
                while (currentMapping != null && !currentMapping.mappingType.HasAnyInputConditions(bestCombo))
                {
                    // None found - only stays in plan tree if its EntityICs have descendants in it
                    InputCondition firstEntityIC = currentMapping.mappingType.EntityTypeInputConditions()[0];
                    int childIndex = -1;
                    if (currentMapping.reasonForChildren != null && currentMapping.reasonForChildren.Count > 0)
                    {
                        childIndex = currentMapping.reasonForChildren.IndexOf(firstEntityIC);
                    }

                    if (childIndex != -1)
                    {
                        currentMapping = currentMapping.children[childIndex];
                    }
                    else
                    {
                        // EntityIC has no child - remove this mapping
                        // TODO: Branch will be orphaned - is this an issue with memory?
                        // TODO: Remove mappings in this branch from mappings?
                        currentMapping = mappings[i].parent;
                        currentMapping.children.RemoveAt(mappings[i].childIndex);
                        currentMapping.reasonForChildren.RemoveAt(mappings[i].childIndex);

                        // Fix parents children's childIndexes
                        for (int j = mappings[i].childIndex; j < currentMapping.children.Count; j++)
                        {
                            currentMapping.children[j].childIndex = j;
                        }

                        currentMapping = null;
                    }
                }
            }
        }

        // TODO: This shared info can all be cached on a MappingType in the OnEnable
        private void SetInventoryTargetsForChaining(Mapping mapping, InputCondition targetInputCondition, Entity target)
        {
            if (mapping.inventoryTargets == null)
                mapping.inventoryTargets = new List<Entity>();

            // See if inputCondition shares its InventoryGrouping with any other ICs
            int thisIndex = -1;
            int[] sharesWith = new int[mapping.mappingType.inputConditions.Count];

            for (int i = 0; i < mapping.mappingType.inputConditions.Count; i++)
            {
                // If needed pad out inventoryTarget with nulls for each IC
                if (mapping.inventoryTargets.Count == i)
                    mapping.inventoryTargets.Add(null);

                InputCondition inputCondition = mapping.mappingType.inputConditions[i];
                if (inputCondition.inputConditionType.typeInfo.usesInventoryTypeGroupShareWith)
                    sharesWith[i] = inputCondition.sharesInventoryTypeGroupWith;
                else
                    sharesWith[i] = -1;

                if (inputCondition == targetInputCondition)
                    thisIndex = i;
            }

            mapping.inventoryTargets[thisIndex] = target;

            // Need to know if this sharesWith or if any other ICs share with this one
            for (int i = 0; i < mapping.mappingType.inputConditions.Count; i++)
            {
                if (sharesWith[i] == thisIndex)
                    mapping.inventoryTargets[i] = target;
                else if (i == thisIndex && sharesWith[i] != -1)
                    mapping.inventoryTargets[sharesWith[i]] = target;
            }
        }

        // Goes through all ICs and finds an Entity that matches
        public List<Entity> CheckPossibleTargets(Agent agent, List<Mapping> mappings, List<InputCondition> entityICs,
                                                 List<MemoryType.EntityInfo> entityInfos)
        {
            List<Entity> possibleEntities = new List<Entity>();
            foreach (MemoryType.EntityInfo entityInfo in entityInfos)
            {
                bool passedAllChecks = true;
                foreach (InputCondition inputCondition in entityICs)
                {
                    // TODO: This could be wrong if MT is in mappings more than once
                    Mapping mapping = FindMapping(inputCondition, mappings);
                    if (!inputCondition.inputConditionType.Check(inputCondition, agent, mapping, entityInfo.entity, false))
                    {
                        passedAllChecks = false;
                        break;
                    }
                }
                if (passedAllChecks)
                    possibleEntities.Add(entityInfo.entity);
            }

            return possibleEntities;
        }

        private Mapping FindMapping(InputCondition inputCondition, List<Mapping> mappings)
        {
            foreach (Mapping mapping in mappings)
            {
                if (mapping.mappingType.inputConditions.Contains(inputCondition))
                    return mapping;
            }
            return null;
        }

        // Generates the Mapping Tree for one rootMapping with all possible decisions mapped out
        // Also returns a list of ChoiceInfos which represent every location in the tree where a choice needs to be made
        private List<ChoiceInfo> CreatePlanTree(Agent agent, AgentType agentType, Mapping rootMapping, out Dictionary<Mapping, List<Mapping>> entitySelections)
        {
            int maxLevels = plannerTypeForGOAP.maxLevels;
            
            // Maintains the next IC to explore for a specific Mapping
            Dictionary<Mapping, int> currentICForMapping = new Dictionary<Mapping, int>();

            // Maintains the next possible Mapping to explore for a specific IC
            Dictionary<(Mapping, InputCondition), int> currentPossibleMappingForIC = new Dictionary<(Mapping, InputCondition), int>();
            Dictionary<(Mapping, InputCondition), List<Mapping>> possibleMappingsForIC = new Dictionary<(Mapping, InputCondition), List<Mapping>>();

            // Hold the info on all choices available in the plan tree including any dependencies of each choice
            List<ChoiceInfo> choiceInfos = new List<ChoiceInfo>();

            // Holds the info on all entity selections in this plan tree
            entitySelections = new Dictionary<Mapping, List<Mapping>>();

            Mapping currentMapping = rootMapping;
            int loopCount = 0;
            while (currentMapping != null)
            {
                // Get the current IC
                if (!currentICForMapping.TryGetValue(currentMapping, out int currentICIndex))
                {
                    // First time to see this Mapping
                    currentICForMapping.Add(currentMapping, 0);
                }

                List<InputCondition> inputConditions = currentMapping.mappingType.inputConditions;
                InputCondition inputCondition = inputConditions[currentICIndex];

                bool forEntityType = inputCondition.RequiresEntityTarget();
                if (forEntityType && !entitySelections.ContainsKey(currentMapping))
                {
                    // Walk up the tree seeing if this came from EntityICs
                    Mapping childMapping = currentMapping;
                    Mapping parentMapping = currentMapping.parent;
                    while (parentMapping != null && parentMapping.reasonForChildren[childMapping.childIndex].RequiresEntityTarget())
                    {
                        if (entitySelections.ContainsKey(parentMapping))                       
                            break;
                        childMapping = parentMapping;
                        parentMapping = parentMapping.parent;
                    }

                    if (parentMapping == null || !parentMapping.reasonForChildren[childMapping.childIndex].RequiresEntityTarget())
                        entitySelections.Add(currentMapping, new List<Mapping>() { currentMapping });
                    else if (!entitySelections[parentMapping].Contains(currentMapping))
                        entitySelections[parentMapping].Add(currentMapping);
                }

                // If IC is for an EntityType we assume it failed so it will look for fixes - Entity selection is figured out later
                if (forEntityType || !inputCondition.inputConditionType.Check(inputCondition, agent, currentMapping, null, false))
                {
                    if (currentPossibleMappingForIC.TryGetValue((currentMapping, inputCondition), out int currentMappingIndex))
                    {
                        // Already figured out the possible mappings - try next one
                        currentMapping.AddChild(possibleMappingsForIC[(currentMapping, inputCondition)][currentMappingIndex], inputCondition);
                        currentPossibleMappingForIC[(currentMapping, inputCondition)]++;
                        currentMapping = possibleMappingsForIC[(currentMapping, inputCondition)][currentMappingIndex];
                    }
                    else
                    {
                        currentPossibleMappingForIC.Add((currentMapping, inputCondition), 0);

                        // First time to see this input condition
                        List<Mapping> possibleMappings = FindMappingsForOutputChange(agent, agentType, inputCondition, currentMapping.mappingType);

                        if (possibleMappings.Count > 0 && currentMapping.GetLevel() <= maxLevels)
                        {
                            possibleMappingsForIC.Add((currentMapping, inputCondition), possibleMappings);

                            // Move to the first possible mapping
                            currentMapping.AddChild(possibleMappings[0], inputCondition);
                            currentPossibleMappingForIC[(currentMapping, inputCondition)]++;
                            currentMapping = possibleMappings[0];
                        }
                        else if (forEntityType)
                        {
                            // Special logic for Entity Type ICs - they always fail without a check but should be considered completed
                            while (currentMapping != null)
                            {
                                currentICForMapping[currentMapping]++;
                                if (currentICForMapping[currentMapping] == currentMapping.mappingType.inputConditions.Count)
                                {
                                    // This was the last IC in this MT
                                    currentMapping.isComplete = true;
                                }
                                else
                                {
                                    // There are more ICs to explore - go to next IC
                                    break;
                                }

                                currentMapping = currentMapping.parent;

                                if (currentMapping != null)
                                {
                                    inputCondition = currentMapping.mappingType.inputConditions[currentICForMapping[currentMapping]];

                                    // If there are two completed PMs and it is not already in choiceInfos add it
                                    int numCompletedPMs = possibleMappingsForIC[(currentMapping, inputCondition)].Where(x => x.isComplete).Count();
                                    if (numCompletedPMs == 2 &&
                                        choiceInfos.Where(x => x.mapping == currentMapping && x.inputCondition == inputCondition).Count() == 0)
                                    {
                                        // Do not know the full dependency list yet - just create empty lists
                                        ChoiceInfo choiceInfo = new ChoiceInfo()
                                        {
                                            mapping = currentMapping,
                                            inputCondition = inputCondition,
                                            mappingDependencies = new List<Mapping>(),
                                            inputConditionDependencies = new List<InputCondition>(),
                                            possibleMappingDependencies = new List<Mapping>()
                                        };
                                        choiceInfos.Add(choiceInfo);
                                    }

                                    // See if there are any more possible mappings to explore
                                    if (currentPossibleMappingForIC[(currentMapping, inputCondition)] <
                                        possibleMappingsForIC[(currentMapping, inputCondition)].Count)
                                    {
                                        // More possible mappings - Explore next possible mapping
                                        break;
                                    }
                                }
                            }
                        }
                        else
                        {
                            // Dead end - no matches for this failed IC - go back up to next possible mapping
                            while (currentMapping != null)
                            {
                                // Remove this dead-end and keep moving up tree deleting along the way
                                int childIndex = currentMapping.childIndex;
                                currentMapping = currentMapping.parent;
                                bool deleteMappings = true;
                                if (currentMapping != null)
                                {
                                    if (deleteMappings)
                                    {
                                        // Also remove from choiceInfos
                                        int choiceIndex = choiceInfos.FindIndex(x => x.mapping == currentMapping.children[childIndex]);
                                        if (choiceIndex != -1)
                                            choiceInfos.RemoveAt(choiceIndex);

                                        currentMapping.children.RemoveAt(childIndex);
                                        currentMapping.reasonForChildren.RemoveAt(childIndex);
                                    }

                                    // See if there are any more possible mappings to explore
                                    inputCondition = currentMapping.mappingType.inputConditions[currentICForMapping[currentMapping]];
                                    if (currentPossibleMappingForIC[(currentMapping, inputCondition)] <
                                        possibleMappingsForIC[(currentMapping, inputCondition)].Count)
                                    {
                                        // More possible mappings - Explore next possible mapping
                                        break;
                                    }
                                    else
                                    {
                                        // See if this mapping should be marked as complete
                                        if (possibleMappingsForIC[(currentMapping, inputCondition)].Where(x => x.isComplete).Count() > 0)
                                        {
                                            // This IC was completed by a previous mapping - don't delete anymore
                                            deleteMappings = false;

                                            // Need to see if there are any more ICs to explore or if we should keep going up the tree (but don't delete)
                                            currentICForMapping[currentMapping]++;
                                            if (currentICForMapping[currentMapping] == currentMapping.mappingType.inputConditions.Count)
                                            {
                                                // This was the last IC in this MT
                                                currentMapping.isComplete = true;
                                            }
                                            else
                                            {
                                                // There are more ICs to explore - go to next IC
                                                break;
                                            }
                                        }
                                    }
                                }
                            }
                        }
                    }
                }
                else
                {
                    // InputCondition is met - move to next IC
                    currentICForMapping[currentMapping]++;
                    
                    // Are there any more ICs left to check?
                    if (currentICForMapping[currentMapping] == currentMapping.mappingType.inputConditions.Count)
                    {
                        // This was the last IC in this MT
                        currentMapping.isComplete = true;

                        // Need to move currentMapping up the tree to next IC or next PM to check
                        while (currentMapping != null)
                        {
                            currentMapping = currentMapping.parent;
                            if (currentMapping != null)
                            {
                                inputCondition = currentMapping.mappingType.inputConditions[currentICForMapping[currentMapping]];

                                // If there are two completed PMs and it is not already in choiceInfos add it
                                int numCompletedPMs = possibleMappingsForIC[(currentMapping, inputCondition)].Where(x => x.isComplete).Count();
                                if (numCompletedPMs == 2 &&
                                    choiceInfos.Where(x => x.mapping == currentMapping && x.inputCondition == inputCondition).Count() == 0)
                                {
                                    // Do not know the full dependency list yet - just create empty lists
                                    ChoiceInfo choiceInfo = new ChoiceInfo()
                                    {
                                        mapping = currentMapping,
                                        inputCondition = inputCondition,
                                        mappingDependencies = new List<Mapping>(),
                                        inputConditionDependencies = new List<InputCondition>(),
                                        possibleMappingDependencies = new List<Mapping>()
                                    };
                                    choiceInfos.Add(choiceInfo);
                                }

                                // See if the current IC has any more PMs to check
                                if (currentPossibleMappingForIC[(currentMapping, inputCondition)] <
                                    possibleMappingsForIC[(currentMapping, inputCondition)].Count)
                                {
                                    break;
                                }

                                // See if there are any more ICs to check
                                currentICForMapping[currentMapping]++;
                                if (currentICForMapping[currentMapping] < currentMapping.mappingType.inputConditions.Count)
                                {
                                    // Still ICs to explore in this mapping
                                    break;
                                }
                                else
                                {
                                    // No more ICs - This mapping is complete
                                    currentMapping.isComplete = true;
                                }
                            }
                        }
                    }
                }

                ++loopCount;
                if (loopCount > 100)
                {
                    Debug.LogError(name + ":" + " CreatePlanTree went over 100 Loops! " + rootMapping);
                    return null;
                }
            }

            return choiceInfos;
        }

        private List<Mapping> FindMappingsForOutputChange(Agent agent, AgentType agentType, InputCondition inputCondition,
                                                          MappingType inputConditionMappingType)
        {
            List<Mapping> possibleMappings = new List<Mapping>();
            List<TotalAIManager.FixInputCondition> fixInputConditions = totalAIManager.FindMappingTypeMatches(inputCondition);

            HashSet<MappingType> availableMappingTypes;

            if (agent != null)
                availableMappingTypes = agent.availableMappingTypes;
            else
                availableMappingTypes = agentType.AvailableMappingTypes();

            if (fixInputConditions != null)
            {
                // Create a Mapping for each MappingType
                foreach (TotalAIManager.FixInputCondition fixInputCondition in fixInputConditions)
                {
                    // Make sure this agent (if runtime) or agentType (if editor) can perform this MappingType
                    if (availableMappingTypes.Contains(fixInputCondition.mappingType))
                    {
                        OutputChange outputChange = fixInputCondition.mappingType.outputChanges[fixInputCondition.outputChangeIndex];

                        if (agent != null && !outputChange.outputChangeType.Match(agent, outputChange, fixInputCondition.mappingType,
                                                                                    inputCondition, inputConditionMappingType))
                        {
                            continue;
                        }

                        // Create Mappings that are missing the actually Entities - these will be figured out later along with if agent has input conditions
                        possibleMappings.Add(new Mapping(fixInputCondition.mappingType));
                    }
                }
            }
            return possibleMappings;
        }
    }
}