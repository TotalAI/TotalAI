using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace TotalAI
{
    // All plans for a drive - most plans will not be "complete" - which means the agent can't successfully do them.
    // TODO: Optimize this to not be for all, just for the X highest scoring complete plans
    [System.Serializable]
    public class Plans
    {

        // Status of the Plan (Tree starting at root mapping)
        public enum Status { NotComplete, Complete, Running, Finished, Interrupted }

        public DriveType driveType;
        public List<Mapping> rootMappings;
        public List<Status> statuses;

        public List<float> driveAmountEstimates;
        public List<float> timeEstimates;
        public List<float> sideEffectsUtility;
        public List<float> utility;

        public Plans(DriveType driveType, Mapping singleMapping)
        {
            this.driveType = driveType;
            rootMappings = new List<Mapping>() { singleMapping };
            driveAmountEstimates = new List<float>() { -1f };
            timeEstimates = new List<float>() { -1f };
            sideEffectsUtility = new List<float>() { -1f };
            utility = new List<float>() { -1f };
        }

        public Plans(DriveType driveType, List<Mapping> mappings)
        {
            // Does this need to be copied? (ref vs val)
            this.driveType = driveType;
            rootMappings = mappings;
            driveAmountEstimates = new List<float>();
            timeEstimates = new List<float>();
            sideEffectsUtility = new List<float>();
            utility = new List<float>();
            foreach (Mapping mapping in mappings)
            {
                driveAmountEstimates.Add(-1f);
                timeEstimates.Add(-1f);
                sideEffectsUtility.Add(-1f);
                utility.Add(-1f);
            }
        }

        public void Add(Mapping mapping)
        {
            rootMappings.Add(mapping);
            driveAmountEstimates.Add(-1f);
            timeEstimates.Add(-1f);
            sideEffectsUtility.Add(-1f);
            utility.Add(-1f);
        }

        public void Add(List<Mapping> mappings)
        {
            foreach (Mapping mapping in mappings)
            {
                rootMappings.Add(mapping);
                driveAmountEstimates.Add(-1f);
                timeEstimates.Add(-1f);
                sideEffectsUtility.Add(-1f);
                utility.Add(-1f);
            }
        }

        // Do the two plans the same MappingTypes?
        private bool EqualMappingTypes(Mapping rootMappingA, Mapping rootMappingB)
        {
            int numChildrenA = rootMappingA.children == null ? 0 : rootMappingA.children.Count;
            int numChildrenB = rootMappingB.children == null ? 0 : rootMappingB.children.Count;

            if (rootMappingA.mappingType != rootMappingB.mappingType || numChildrenA != numChildrenB)
            {
                return false;
            }
            else if (numChildrenA != 0)
            {
                for (int i = 0; i < numChildrenA; i++)
                {
                    bool isEqual = EqualMappingTypes(rootMappingA.children[i], rootMappingB.children[i]);
                    if (!isEqual)
                        return false;
                }
            }

            return true;
        }

        // Returns all rootMappings with all leaves that have all input conditions satisfied
        public List<Mapping> GetCompletePlans(List<Mapping> excludeRootMappings = null)
        {
            List<Mapping> completeMappings = new List<Mapping>();
            statuses = new List<Status>();

            foreach (Mapping mapping in rootMappings)
            {
                // TODO: Is this ever true?  Shouldn't this check MappingType?
                if (excludeRootMappings != null && excludeRootMappings.Contains(mapping))
                    continue;

                if (mapping.PlanIsComplete())
                {
                    completeMappings.Add(mapping);
                    statuses.Add(Status.Complete);
                }
                else
                {
                    statuses.Add(Status.NotComplete);
                }
            }

            return completeMappings;
        }

        public int GetRootMappingIndex(Mapping mapping)
        {
            return rootMappings.IndexOf(mapping);
        }

        public Mapping GetRootMapping(int index)
        {
            return rootMappings[index];
        }

        public void SetSelectedPlanStatus(Status status, int selectedPlan)
        {
            statuses[selectedPlan] = status;
        }

        // We want tostring to print out the each rootMapping and its tree
        public override string ToString()
        {
            string planAsString = "";
            if (driveType != null)
                planAsString = driveType.name + ": ";

            if (rootMappings != null)
            {
                foreach (Mapping rootMapping in rootMappings)
                {
                    planAsString += rootMapping + ": " + rootMapping.NumberLeaves() + " / " + rootMapping.NumberMappings() + " - " + rootMapping.PlanIsComplete() + " ";
                }
            }

            return planAsString;
        }
    }
}
