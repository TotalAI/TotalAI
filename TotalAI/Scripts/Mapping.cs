using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace TotalAI
{
    public class Mapping
    {
        public MappingType mappingType;
        public Entity target;
        public List<Entity> inventoryTargets;
        public float previousOutputChangeAmount;

        public Mapping parent;
        public List<Mapping> children;
        public List<InputCondition> reasonForChildren;
        public int childIndex;

        public bool isComplete;

        // Saved for Editor UI
        public float sideEffectUtility; 

        public Mapping(MappingType mappingType)
        {
            this.mappingType = mappingType;
            target = null;
            inventoryTargets = null;
            parent = null;
            children = null;
            reasonForChildren = null;
            childIndex = 0;
            isComplete = false;
        }

        // Return either this ActionType or its parent's ActionType if this is a GoTo mapping
        public ActionType NonGoToActionType()
        {
            if (parent != null && mappingType.actionType == parent.mappingType.goToActionType)
                return parent.mappingType.actionType;
            return mappingType.actionType;
        }

        public bool ContainsChild(Mapping targetChild)
        {
            if (targetChild == this)
                return true;

            if (children != null && children.Count > 0)
            {
                foreach (Mapping child in children)
                {
                    if (child.ContainsChild(targetChild))
                        return true;
                }
            }
            return false;
        }

        public void AddChild(Mapping newChild, InputCondition reasonForChild)
        {
            if (children == null)
            {
                children = new List<Mapping>();
                reasonForChildren = new List<InputCondition>();
            }
            newChild.parent = this;
            newChild.childIndex = children.Count;
            reasonForChildren.Add(reasonForChild);
            children.Add(newChild);
        }

        public void InsertChild(int index, Mapping newChild, InputCondition reasonForChild)
        {
            if (children == null)
            {
                children = new List<Mapping>();
                reasonForChildren = new List<InputCondition>();
            }
            newChild.parent = this;
            newChild.childIndex = children.Count;

            // TODO: Not sure if this is needed - guessing Insert handles this case
            if (index >= children.Count)
            {
                reasonForChildren.Add(reasonForChild);
                children.Add(newChild);
            }
            else
            {
                reasonForChildren.Insert(index, reasonForChild);
                children.Insert(index, newChild);
            }
            
        }
        
        private void ResetChildIndexes()
        {
            // TODO: Not sure if children will always be in ascending order?  I think they will be.
            for (int i = 0; i < children.Count; i++)
            {
                children[i].childIndex = i;
            }
        }

        public bool HasGoToMapping()
        {
            if (children != null)
            {
                foreach (Mapping child in children)
                {
                    if (child.mappingType.actionType == mappingType.goToActionType)
                        return true;
                }
            }
            return false;
        }

        public List<Entity> GetAllTargets()
        {
            List<Entity> targets = new List<Entity>();
            if (target != null && !targets.Contains(target))
                targets.Add(target);

            if (children != null)
            {
                foreach (Mapping child in children)
                {
                    targets.AddRange(child.GetAllTargets());
                }
            }
            return targets;
        }

        // Copies this tree from left to right until it gets to mapping - will include mapping in the returned copy
        public Mapping CopyTreeUpToMapping(ref Mapping targetMapping)
        {
            if (parent != null)
            {
                Debug.LogError("Trying to CopyTreeUpToMapping starting at a non-root Mapping! - " + this);
                return null;
            }

            Mapping currentMapping = this;
            Mapping parentMapping = null;
            Dictionary<Mapping, int> currentChildIndex = new Dictionary<Mapping, int>();
            while (true)
            {
                int index = 0;
                if (!currentChildIndex.TryGetValue(currentMapping, out index))
                {
                    if (currentMapping == targetMapping)
                    {
                        currentMapping = currentMapping.Copy(true);
                        currentMapping.parent = parentMapping;
                        targetMapping = currentMapping;
                    }
                    else
                    {
                        currentMapping = currentMapping.Copy(true);
                        currentMapping.parent = parentMapping;
                    }
                    currentChildIndex.Add(currentMapping, 0);

                    if (currentMapping.children == null || currentMapping.children.Count == 0)
                    {
                        // Exit while loop when the targetMapping copy has no more children to copy
                        if (currentMapping == targetMapping)
                            break;

                        // No more children - go back up the tree looking for more children to copy
                        currentMapping = currentMapping.parent;
                        while (currentMapping != null && currentMapping.children != null && currentChildIndex[currentMapping] >= currentMapping.children.Count)
                        {
                            // Exit while loop when the targetMapping copy has no more children to copy
                            if (currentMapping == targetMapping)
                                break;
                            currentMapping = currentMapping.parent;
                        }

                        // The break above only breaks out of the small while loop
                        if (currentMapping == targetMapping)
                            break;

                        // This should never happen - means it never found the targetMapping
                        if (currentMapping == null)
                        {
                            Debug.LogError("CopyTreeUpToMapping ended up with a null currentMapping! - " + this);
                            return null;
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
                    // Exit while loop when the targetMapping copy has no more children to copy
                    if (currentMapping == targetMapping)
                        break;

                    // No more children - go back up the tree looking for more children to copy
                    currentMapping = currentMapping.parent;
                    while (currentMapping != null && currentMapping.children != null && currentChildIndex[currentMapping] >= currentMapping.children.Count)
                    {
                        // Exit while loop when the targetMapping copy has no more children to copy
                        if (currentMapping == targetMapping)
                            break;
                        currentMapping = currentMapping.parent;
                    }

                    // The break above only breaks out of the small while loop
                    if (currentMapping == targetMapping)
                        break;

                    // This should never happen - means it never found the targetMapping
                    if (currentMapping == null)
                    {
                        Debug.LogError("CopyTreeUpToMapping ended up with a null currentMapping! - " + this);
                        return null;
                    }
                }
            }

            // Go up to rootMapping
            while (currentMapping.parent != null)
            {
                currentMapping = currentMapping.parent;
            }
            return currentMapping;
        }

        // Simple copy without copying parent - copies children if copyChildren is true
        public Mapping Copy(bool copyChildren)
        {
            Mapping copy = new Mapping(mappingType)
            {
                target = target,
                parent = parent,
                childIndex = childIndex,
                isComplete = isComplete
            };

            if (copyChildren && children != null) {
                copy.children = new List<Mapping>(children);
                copy.reasonForChildren = new List<InputCondition>(reasonForChildren);
            }

            return copy;
        }

        // Copies tree branch (copies every node) back to the root that this Mapping is part of - this should always be a leaf node
        // it returns the new copy of the Mapping that is in the same spot in the tree as this
        // And also returns in the out variable the new copied root mapping
        public Mapping CopyTreeBranch(out Mapping newRootMapping)
        {
            Mapping newTree = Copy(false);
            
            int previousChildIndex;
            Mapping previousNode = null;
            Mapping currentNode = newTree;
            while (currentNode.parent != null)
            {
                previousChildIndex = currentNode.childIndex;
                previousNode = currentNode;
                currentNode = currentNode.parent;
                
                currentNode = currentNode.Copy(true);

                // Change the correct child node to the copy
                currentNode.children[previousChildIndex] = previousNode;
            }

            newRootMapping = currentNode;

            return newTree;
        }

        // Returns the next mapping the agent should do in the tree
        public Mapping NextMapping()
        {
            if (parent == null)
                return null;

            // Are there any siblings left to do?
            if (parent.children.Count - 1 > childIndex)
            {
                return parent.children[childIndex + 1].GetLeftmostLeaf();
            }

            return parent;
        }

        public Mapping GetLeftmostLeaf()
        {
            if (children == null || children.Count == 0)
                return this;
            
            return children[0].GetLeftmostLeaf();
        }

        public Mapping GetRootMapping()
        {
            if (parent == null)
                return this;

            return parent.GetRootMapping();
        }

        // Starting at this mapping how many nodes are in entire tree including this node?
        public int NumberMappings()
        {
            int numberMappings = 1;

            if (children != null)
            {
                foreach (Mapping child in children)
                {
                    if (child.children == null || child.children.Count == 0)
                    {
                        ++numberMappings;
                    }
                    else
                    {
                        numberMappings += child.NumberMappings();
                    }
                }
            }
            return numberMappings;
        }

        // Starting at this mapping how many leaves are there below it?
        // TODO: If this has no children it returns 0 - should it consider itself a leaf in that case?
        public int NumberLeaves()
        {
            int numberLeaves = 0;

            if (children != null)
            {
                foreach (Mapping child in children)
                {
                    if (child.children == null || child.children.Count == 0)
                    {
                        ++numberLeaves;
                    }
                    else
                    {
                        numberLeaves += child.NumberLeaves();
                    }
                }
            }

            return numberLeaves;
        }

        // Returns the level this mapping is on - 1 means its the root mapping
        public int GetLevel(int level = 1)
        {
            if (parent == null)
                return level;
            return parent.GetLevel(level + 1);
        }

        // Are all of the mappings starting at this as the root node completed?  (Have all inputs)
        // A parent could have InputConditions that fail without a leaf (no matching OutputChange to fix failed InputCondition)
        public bool PlanIsComplete()
        {
            // If this Mapping is not complete return false and don't bother to check any children
            if (!isComplete)
                return false;

            if (children == null || children.Count == 0)
            {
                return isComplete;
            }

            bool childLeavesComplete = true;
            foreach (Mapping child in children)
            {
                childLeavesComplete = child.PlanIsComplete();
                if (!childLeavesComplete)
                    break;
            }

            return childLeavesComplete;
        }

        // Starts at this rootMapping and finds the Equation DriveType change for the rest of the tree
        public float EquationDriveTypeChangeInTree(Agent agent, DriveType driveType)
        {
            float amount = EquationDriveTypeChangeInMapping(agent, driveType);
            
            if (children != null)
            {
                foreach (Mapping child in children)
                {
                    if (child.children == null || child.children.Count == 0)
                    {
                        amount += child.EquationDriveTypeChangeInMapping(agent, driveType);
                    }
                    else
                    {
                        amount += child.EquationDriveTypeChangeInTree(agent, driveType);
                    }
                }
            }

            return amount;
        }
        
        public float EquationDriveTypeChangeInMapping(Agent agent, DriveType driveType)
        {
            float amount = 0f;

            foreach (OutputChange outputChange in mappingType.outputChanges)
            {
                amount += driveType.driveTypeEquation.ChangeInOutputChange(agent, driveType, outputChange, this);
            }

            return amount;
        }

        // Starts at this rootMapping and returns [driveAmount, timeEst, sideEffectsUtility] in the float array (only go through tree once)
        // If the drive uses an equation the entire drive level change is calculated above (see DriveType.CalculateEquationDriveLevelChange)
        public void CalcUtilityInfoForTree(Agent agent, DriveType driveType, float[] results, bool firstCall = true)
        {
            if (firstCall || driveType.syncType != DriveType.SyncType.Equation)
                results[0] += CalcDriveReduction(agent, driveType);
            results[1] += CalcTimeToComplete(agent);
            results[2] += CalcSideEffectsUtility(agent, driveType);

            if (children != null)
            {
                foreach (Mapping child in children)
                {
                    if (child.children == null || child.children.Count == 0)
                    {
                        if (driveType.syncType != DriveType.SyncType.Equation)
                            results[0] += child.CalcDriveReduction(agent, driveType);
                        results[1] += child.CalcTimeToComplete(agent);
                        results[2] += child.CalcSideEffectsUtility(agent, driveType);
                    }
                    else
                    {
                        child.CalcUtilityInfoForTree(agent, driveType, results, false);
                    }
                }
            }
        }

        private float CalcTimeToComplete(Agent agent)
        {
            // Add in GoTo Mapping if needed
            float goToTime = 0f;
            DeciderType deciderType = agent.decider.DeciderType();
            if (deciderType.RequiresGoToMapping(agent, this))
            {
                Mapping goToMapping = deciderType.CreateGoToMapping(this);
                goToTime = goToMapping.mappingType.actionType.behaviorType.EstimatedTimeToComplete(agent, goToMapping);
                goToMapping = null;
            }
            return goToTime + mappingType.actionType.behaviorType.EstimatedTimeToComplete(agent, this);
        }

        // Starts at this rootMapping and finds the DriveType change for the rest of the tree
        public float CalcDriveChangeForTree(Agent agent, DriveType driveType)
        {
            float driveChange = CalcDriveReduction(agent, driveType);

            // If the drive uses an equation the entire drive level change is calculated above - see DriveLevelOCT.CalculateAmount
            if (children != null && driveType.syncType != DriveType.SyncType.Equation)
            {
                foreach (Mapping child in children)
                {
                    if (child.children == null || child.children.Count == 0)
                    {
                        driveChange += child.CalcDriveReduction(agent, driveType);
                    }
                    else
                    {
                        driveChange += child.CalcDriveChangeForTree(agent, driveType);
                    }
                }
            }

            return driveChange;
        }

        public float CalcDriveReduction(Agent agent, DriveType driveType)
        {
            float driveAmount = 0f;

            foreach (OutputChange outputChange in mappingType.outputChanges)
            {
                // Find output change that changes the main drive
                if (driveType == outputChange.levelType && outputChange.targetType == OutputChange.TargetType.ToSelf)
                {
                    if (outputChange.timing == OutputChange.Timing.Repeating)
                    {
                        driveAmount += outputChange.changeEstimateForPlanner;
                    }
                    else
                    {
                        driveAmount += outputChange.outputChangeType.CalculateAmount(agent, agent, outputChange, this);
                    }
                }
            }

            return driveAmount;
        }

        public float CalcTimeToCompleteForTree(Agent agent)
        {
            float timeToComplete = mappingType.actionType.behaviorType.EstimatedTimeToComplete(agent, this);

            if (children != null)
            {
                foreach (Mapping child in children)
                {
                    if (child.children == null || child.children.Count == 0)
                    {
                        timeToComplete += child.mappingType.actionType.behaviorType.EstimatedTimeToComplete(agent, child);
                    }
                    else
                    {
                        timeToComplete += child.CalcTimeToCompleteForTree(agent);
                    }
                }
            }

            return timeToComplete;
        }

        // This is called on root mappings and will traverse the plan tree figuring out the utility of all the side effects 
        public float CalcSideEffectsUtilityForTree(Agent agent, DriveType driveType)
        {
            float utility = CalcSideEffectsUtility(agent, driveType);

            if (children != null)
            {
                foreach (Mapping child in children)
                {
                    if (child.children == null || child.children.Count == 0)
                    {
                        utility += child.CalcSideEffectsUtility(agent, driveType);
                    }
                    else
                    {
                        utility += child.CalcSideEffectsUtilityForTree(agent, driveType);
                    }
                }
            }

            return utility;
        }

        private float CalcSideEffectsUtility(Agent agent, DriveType driveType)
        {
            float utility = 0f;

            foreach (OutputChange outputChange in mappingType.outputChanges)
            {
                utility += outputChange.CalculateSEUtility(driveType, agent, this);
            }

            sideEffectUtility = utility;
            return utility;
        }

        /*
        // Removes any mapping types already used from the mappingTypes list
        public void RemoveUsedMappingTypes(List<ActionTypeMappingType> actionTypeMappingTypes, bool setToRoot)
        {
            // First time in this finds the rootMapping and then recurses with the rootMapping
            if (setToRoot)
            {
                Mapping rootMapping = this;
                while (rootMapping.parent != null)
                {
                    rootMapping = rootMapping.parent;
                }

                rootMapping.RemoveUsedMappingTypes(actionTypeMappingTypes, false);
                return;
            }

            int i = ActionTypeMappingType.IndexOfMappingType(actionTypeMappingTypes, mappingType);
            if (i != -1)
                actionTypeMappingTypes.RemoveAt(i);

            if (children != null)
            {
                foreach (Mapping child in children)
                {
                    if (child.children == null || child.children.Count == 0)
                    {
                        i = ActionTypeMappingType.IndexOfMappingType(actionTypeMappingTypes, mappingType);
                        if (i != -1)
                            actionTypeMappingTypes.RemoveAt(i);
                    }
                    else
                    {
                        child.RemoveUsedMappingTypes(actionTypeMappingTypes, false);
                    }
                }
            }
        }
        */

        // TODO: Only compile this if editor
        // This is originally called on a rootMapping and then recursively goes through children
        public void Draw(float x, float y, float width, float height, float widthGap, float heightGap, Mapping currentMapping, Agent agent, int recLevel = 1, bool drawSE = false)
        {
            widthGap = widthGap / recLevel;

            // Set drawSE when looking at rootMapping
            if (recLevel == 1 && PlanIsComplete())
                drawSE = true;
            
            Color currentBGColor = GUI.backgroundColor;
            if (currentMapping == this)
                GUI.backgroundColor = Color.yellow;

            // Draw this Mapping
            GUI.Box(new Rect(x, y, width, height), "", "Button");
            GUI.Label(new Rect(x + 5, y + height / 2 - 11, width, 20), mappingType.name, new GUIStyle("label") { fontSize = 10 });

            GUI.backgroundColor = Color.clear;

            if (GUI.Button(new Rect(x, y, width, height), ""))
            {
                UnityEditor.Selection.objects = new Object[] { mappingType };
            }
            GUI.backgroundColor = currentBGColor;

            // If Mapping is complete put check mark
            Color currentColor = GUI.color;
            if (isComplete)
            {
                GUI.color = Color.green;
                GUI.Box(new Rect(x + 3, y + 3, 7, 7), "");
            }
            else
            {
                GUI.color = Color.red;
                GUI.Box(new Rect(x + 3, y + 3, 7, 7), "");
            }
            GUI.color = currentColor;

            if (drawSE)
                GUI.Label(new Rect(x + width - 30, y + 5, 25, 20), sideEffectUtility.ToString(),
                          new GUIStyle { fontSize = 10, alignment = TextAnchor.UpperRight });

            // Draw Target
            if (target != null)
            {
                if (GUI.Button(new Rect(x + width + 5, y + height / 2 - 35, 150, 20), target.name,
                               new GUIStyle("label") { fontSize = 10, alignment = TextAnchor.MiddleLeft }))
                {
                    UnityEditor.Selection.activeGameObject = target.gameObject;
                }
            }

            // Draw Inventory Targets
            if (inventoryTargets != null && inventoryTargets.Count > 0)
            {
                for (int i = 0; i < inventoryTargets.Count; i++)
                {
                    Entity inventoryTarget = inventoryTargets[i];
                    if (inventoryTarget != null)
                    {
                        if (GUI.Button(new Rect(x + width + 5, y + height / 2 - 20 + (15 * i), 150, 20), (i + 1) + ". " + inventoryTarget.name,
                                       new GUIStyle("label") { fontSize = 10, alignment = TextAnchor.MiddleLeft }))
                        {
                            UnityEditor.Selection.activeGameObject = inventoryTarget.gameObject;
                        }
                    }
                }
            }

            // Draw InputCondition bubbles
            if (mappingType.inputConditions != null)
            {

                for (int i = 0; i < mappingType.inputConditions.Count; i++)
                {
                    if (agent != null && target != null && target.gameObject != null &&
                        mappingType.inputConditions[i].inputConditionType.Check(mappingType.inputConditions[i], agent, this, target, false))
                        GUI.backgroundColor = Color.green;
                    else
                        GUI.backgroundColor = Color.red;

                    float newX = x + width / 2 - 10 - (mappingType.inputConditions.Count - 1) * (20 + 20) / 2;
                    GUI.Box(new Rect(newX + i * (20 + 20), y + height / 2 + 13, 20, 20), new GUIContent("", mappingType.inputConditions[i].ToString()));
                    GUI.backgroundColor = currentBGColor;

                    // Draw line from bubble to the child it maps to
                    if (reasonForChildren != null && reasonForChildren.Count > 0)
                    {
                        for (int j = 0; j < reasonForChildren.Count; j++)
                        {
                            if (reasonForChildren[j] == mappingType.inputConditions[i])
                            {
                                // Draw line from middle of bubble to the middle of the child
                                float bubbleX = newX + i * (20 + 20) + 10;
                                float bubbleY = y + height / 2 + 20 + 13;

                                // Actually the line needs to go to the top middle of the output change bubble
                                // Figure out which output change this input condition matches
                                int matchIndex = 0;
                                if (children[j].mappingType.outputChanges != null)
                                {
                                    foreach (OutputChange outputChange in children[j].mappingType.outputChanges)
                                    {
                                        if (agent == null || outputChange.outputChangeType.Match(agent, outputChange, children[j].mappingType,
                                                                                                 mappingType.inputConditions[i], mappingType))
                                        {
                                            // TODO: This needs to be fixed for agent == null - not sure what to do
                                            // TODO: Why is this not using the fixInputConditions Dict?
                                            break;
                                        }
                                        ++matchIndex;
                                    }

                                    if (matchIndex >= children[j].mappingType.outputChanges.Count)
                                    {
                                        matchIndex = 0;
                                    }
                                }

                                float childX = x - (children.Count - 1) * (width + widthGap) / 2 + j * (width + widthGap) + width / 2;
                                float childY = y + heightGap;

                                // Move childX based on the index and total number of output changes in child
                                if (children[j].mappingType.outputChanges != null)
                                {
                                    childX += -1 * (children[j].mappingType.outputChanges.Count - 1) * (20 + 20) / 2 + matchIndex * (20 + 20);
                                }
                                else
                                {
                                    childX += matchIndex * (20 + 20);
                                }


                                //UnityEditor.Handles.color = new Color(.9f, .9f, .9f);
                                UnityEditor.Handles.DrawBezier(new Vector3(bubbleX, bubbleY, 0), new Vector3(childX, childY, 0),
                                                               new Vector3(bubbleX, bubbleY, 0), new Vector3(childX, childY, 0),
                                                               new Color(.8f, .8f, .8f), null, 2f);
                            }
                        }
                    }
                }
            }
            // Draw OutputChanges bubbles
            if (mappingType.outputChanges != null)
            {
                GUI.backgroundColor = Color.gray;
                for (int i = 0; i < mappingType.outputChanges.Count; i++)
                {
                    float newX = x + width / 2 - 10 - (mappingType.outputChanges.Count - 1) * (20 + 5) / 2;
                    GUI.Box(new Rect(newX + i * (20 + 5), y + 5, 20, 20),
                            new GUIContent(mappingType.outputChanges[i].TimingInitial(), mappingType.outputChanges[i].ToString()));
                }
                GUI.backgroundColor = currentBGColor;
            }
            // Draw any children
            if (children != null && children.Count > 0)
            {
                float newX = x - (children.Count - 1) * (width + widthGap) / 2;
                for (int i = 0; i < children.Count; i++)
                {
                    children[i].Draw(newX + i * (width + widthGap), y + heightGap, width, height, widthGap, heightGap, currentMapping, agent, recLevel + 1, drawSE);
                }
            }
        }

        public override string ToString()
        {
            return mappingType != null ? mappingType.name : "No MT";
        }
    }
}
