using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace TotalAI
{
    public class WorldObject : Entity
    {
        public WorldObjectType.State currentState;
        public WorldObjectType.State startState;
        public bool runOutputChangesOnStart;
        public float startCompletePoints;
        public int currentSkinPrefabIndex;
        public float completePoints;
        public bool isComplete;
        public float damage;

        [HideInInspector]
        // Is used to cache how much of something should change for each complete point changed - i.e. terrain height
        public float amountPerCompletePoint;  

        [HideInInspector]
        public WorldObjectType worldObjectType;

        public float CompletionPercentage { get { return completePoints / worldObjectType.pointsToComplete * 100f; } }
        public float DamagePercentage { get { return damage / worldObjectType.damageToDestroy * 100f; } }
        public bool IsDamaged { get { return damage > 0f; } }
        public bool IsDamagable { get { return worldObjectType.damageToDestroy > 0f && damage < worldObjectType.damageToDestroy; } }

        private TimeManager timeManager;
        private Animator animator;

        // TODO: I think this should be in Start
        private new void Awake()
        {
            base.Awake();
            worldObjectType = (WorldObjectType)entityType;

            timeManager = GameObject.Find("TimeManager").GetComponent<TimeManager>();
            animator = GetComponent<Animator>();
            completePoints = startCompletePoints;
            currentState = null;
            currentSkinPrefabIndex = -1;

            ResetEntity();
        }

        private void Start()
        {
            if (worldObjectType.states.Count > 0)
            {
                if (startState != null)
                {
                    // Have to refresh the start state from WOT since it might be stale (WO has a serialized copy saved)
                    startState = worldObjectType.states.Find(x => x.name == startState.name);
                    if (startState == null)
                    {
                        Debug.Log(name + ": Start State '" + startState.name + "' can't be found.  Please fix in inspector.  Setting to first state.");
                        startState = worldObjectType.states[0];
                    }
                    ChangeState(null, startState);
                }
                else
                {
                    ChangeState(null, worldObjectType.states[0]);
                }
            }

            if (worldObjectType.completeType == WorldObjectType.CompleteType.None || completePoints >= worldObjectType.pointsToComplete)
                isComplete = true;
            else
                isComplete = false;

            // If WorldObject grows and there are no states - start up the growth 
            if (worldObjectType.completeType == WorldObjectType.CompleteType.Grows && worldObjectType.states.Count == 0)
                StartCoroutine(GrowCoroutine());

            ChangePrefabIfNeeded();
        }

        private IEnumerator GrowCoroutine()
        {
            // TODO: Make this a variable in the WorldObjectType - some items grow fast and some grow slow
            float waitSeconds = Random.Range(1f, 2f);

            // Growth rate is complete points per game hour
            // points / game hour * realtime seconds / (realtime seconds / game hour) = points
            // Example: 1 second * (10 points / game hour) / (1 realtime second / game minute * 60 minutes / hour) = 10 / 60 = 1/6 
            float completePointsPerTick = waitSeconds * worldObjectType.growthRate / (timeManager.RealTimeSecondsPerGameMinute() * 60);
            while (true)
            {
                yield return new WaitForSeconds(waitSeconds);

                //Debug.Log(name + ": Growing - " + completePointsPerTick);
                IncreaseCompletion(completePointsPerTick);

                if (completePoints >= worldObjectType.pointsToComplete)
                {
                    isComplete = true;
                    break;
                }
            }
        }

        // Changes state if allowed - return true if a state change occurred
        public bool ChangeState(Agent agent, WorldObjectType.State newState)
        {
            if (newState != null && newState != currentState)
            {
                // currentState will be null when starting up so see if OutputChanges should be run on startup
                if ((currentState != null || runOutputChangesOnStart) &&
                    (newState.hasEnterOutputChanges || (currentState != null && currentState.hasExitOutputChanges)))
                    RunOutputChanges(agent, newState, currentState);

                Debug.Log(name + ": Changed State from " + (currentState == null ? "Null" : currentState.name) + " to " + newState.name);
                currentState = newState;
                ChangePrefabIfNeeded();

                // Start timer if this state is timed
                if (newState.isTimed)
                {
                    float time = newState.secondsUntilNextState;
                    if (newState.timeInGameMinutes)
                        time = newState.gameMinutesUntilNextState * timeManager.RealTimeSecondsPerGameMinute();

                    StartCoroutine(StateTimedOut(time));
                }

                if (worldObjectType.completeType == WorldObjectType.CompleteType.Grows && worldObjectType.states.Count > 0 && currentState.allowsCompletion)
                    StartCoroutine(GrowCoroutine());

                return true;
            }
            return false;
        }

        private void RunOutputChanges(Agent agent, WorldObjectType.State newState, WorldObjectType.State currentState)
        {
            for (int i = 0; i < worldObjectType.statesOutputChanges.Count; i++)
            {
                OutputChange outputChange = worldObjectType.statesOutputChanges[i];
                // currentState can be null on Start
                if (newState.enterOutputChangesIndexes.Contains(i) || (currentState != null && currentState.exitOutputChangesIndexes.Contains(i)))
                {
                    float amount = outputChange.outputChangeType.CalculateAmount(agent, this, outputChange, null);
                    if (!outputChange.CheckConditions(this, agent, null, amount))
                    {
                        Debug.Log(agent + ": WorldObject.RunOutputChanges - output conditions failed - " + this);
                        if (outputChange.stopType == OutputChange.StopType.OnOCCFailed)
                            return;
                    }
                    bool succeeded = outputChange.outputChangeType.MakeChange(agent, this, outputChange, null, amount, out bool forceStop);
                    Debug.Log(agent + ": WorldObject.RunOutputChanges - output condition " + outputChange.outputChangeType.name + " - " + succeeded);
                    if ((forceStop && !outputChange.blockMakeChangeForcedStop) ||
                        (outputChange.stopType == OutputChange.StopType.OnChangeFailed && !succeeded))
                        return;
                }
            }
        }

        public void IncreaseCompletion(float amount)
        {
            completePoints += amount;

            if (completePoints >= worldObjectType.pointsToComplete)
            {
                isComplete = true;
                completePoints = worldObjectType.pointsToComplete;
                //if (worldObjectType.addItemsTiming == WorldObjectType.AddItemsTiming.Completed)
                //    CreateInventory();

                Debug.Log(this + " Completed!");

                // Check for inventory to add on completion
                if (worldObjectType.addDefaultInventoryTiming == WorldObjectType.AddInventoryTiming.Completed &&
                    worldObjectType.defaultInventory != null && worldObjectType.defaultInventory.Count > 0)
                {
                    foreach (EntityType.DefaultInventory defaultInventory in entityType.defaultInventory)
                    {
                        if (Random.Range(0f, 100f) <= defaultInventory.probability)
                        {
                            int numToAdd = Mathf.RoundToInt(defaultInventory.amountCurve.Eval(Random.Range(0f, 1f)));
                            int numAdded = inventoryType.Add(this, defaultInventory.entityType, defaultInventory.prefabVariantIndex,
                                                             defaultInventory.inventorySlot, numToAdd);

                            if (numAdded != numToAdd)
                                Debug.LogError(name + ": Failed to add default inventory - added " + numAdded + "/" +
                                               numToAdd + " of " + defaultInventory.entityType.name);

                            // TODO: Add in EntityModifiers that an Entity might add

                        }
                    }
                }
            }

            ChangePrefabIfNeeded();
            ChangeStateIfNeeded(true);
        }

        public void ChangeDamage(float amount)
        {
            damage += amount;
            Debug.Log("Changing " + this + " damage by " + amount);

            if (damage >= worldObjectType.damageToDestroy)
            {
                damage = worldObjectType.damageToDestroy;
                if (worldObjectType.removeOnFullDamage)
                    DestroySelf(null);

            }
            else if (damage < 0)
            {
                damage = 0;
            }

            ChangePrefabIfNeeded();
            ChangeStateIfNeeded(false);
        }
        
        private IEnumerator StateTimedOut(float seconds)
        {
            yield return new WaitForSeconds(seconds);

            WorldObjectType.State newState = worldObjectType.states.Find(x => x.name == currentState.nextStateName);

            if (newState == null)
            {
                Debug.LogError(name + ": '" + currentState.name + "' StateTimedOut - Trying to switch to state '" + currentState.nextStateName +
                               "' that doesn't exist.  Please check spelling.");
            }
            else
            {
                ChangeState(null, newState);
            }
        }

        // This is called after a completion change or damage change to see if a new state should be entered
        private void ChangeStateIfNeeded(bool dueToCompletionChange)
        {
            if (currentState == null)
                return;

            WorldObjectType.State newState = null;
            if (dueToCompletionChange)
            {
                if (completePoints >= currentState.minComplete && completePoints <= currentState.maxComplete)
                    return;
                foreach (WorldObjectType.State state in worldObjectType.states)
                {
                    if (completePoints >= state.minComplete && completePoints <= state.maxComplete)
                    {
                        newState = state;
                        break;
                    }
                }
            }
            else
            {
                if (damage >= currentState.minDamage && damage <= currentState.maxDamage)
                    return;
                foreach (WorldObjectType.State state in worldObjectType.states)
                {
                    if (damage >= state.minDamage && damage <= state.maxDamage)
                    {
                        newState = state;
                        break;
                    }
                }
            }

            if (newState != null)
                ChangeState(null, newState);
        }

        private void ChangePrefabIfNeeded()
        {
            if (worldObjectType.autoScale)
            {
                // Scale transform based on % done
                if (worldObjectType.pointsToComplete < 1)
                {
                    Debug.LogError(name + " is set to autoScale but its pointsToComplete is less than 1.  Please fix.");
                    return;
                }
                float completeLevel = completePoints / worldObjectType.pointsToComplete;
                transform.localScale = new Vector3(completeLevel, completeLevel, completeLevel);

                totalAIManager.UpdateAllNavMeshes();
            }
            else if (inventoryType != null)
            {
                inventoryType.ChangeSkinIfNeeded(this);
            }
        }

        // TODO: Should this be moved into WOTInventoryRecipe?
        public void CheckRecipes(EntityType addedEntityType)
        {
            // Search for this entityType as an input to a recipe
            List<WOTInventoryRecipe.Recipe> possibleRecipes = new List<WOTInventoryRecipe.Recipe>();
            foreach (WOTInventoryRecipe recipe in worldObjectType.recipes)
            {
                possibleRecipes.AddRange(recipe.FindRecipesWithInput(addedEntityType));
            }

            // See if any of these are complete matches
            foreach (WOTInventoryRecipe.Recipe recipe in possibleRecipes)
            {
                if (currentState.name == recipe.state || recipe.state == "")
                {
                    bool hasInputs = true;
                    for (int i = 0; i < recipe.inputEntityTypes.Count; i++)
                    {
                        if (inventoryType.GetEntityTypeAmount(this, recipe.inputEntityTypes[i]) < recipe.inputAmounts[i]) {
                            hasInputs = false;
                            break;
                        }
                    }

                    if (hasInputs)
                        StartCoroutine(RunRecipe(recipe));
                }
            }
        }

        private IEnumerator RunRecipe(WOTInventoryRecipe.Recipe recipe)
        {
            // TODO: Play a sound/effect?  Should this come from the WOT or the recipe?
            yield return new WaitForSeconds(recipe.timeInGameMinutes * timeManager.RealTimeSecondsPerGameMinute());
            // TODO: Stop sound/effect

            bool success = inventoryType.Convert(this, recipe.inputEntityTypes, recipe.inputAmounts, recipe.outputEntityTypes, recipe.outputAmounts);

            if (!success)
                Debug.LogError(name + ": Convert Inventory failed for recipe " + recipe.inputEntityTypes[0] + " -> " + recipe.outputEntityTypes[0]);
        }

        public override void DestroySelf(Agent agent, float delay = 0f)
        {
            // Change layer to prevent this WorldObject from getting added back to known entities
            //SetLayerRecursively(gameObject, LayerMask.NameToLayer("Default"));

            Destroy(gameObject, delay);
            Debug.Log(this + " has been destroyed");            
        }

        public override void OnDestroy()
        {
            base.OnDestroy();

            totalAIManager.UpdateAllNavMeshes();
        }

        private string TimeUntilCompletePretty()
        {
            // growthRate is complete points per game hour
            float hoursUntilFinished = (worldObjectType.pointsToComplete - completePoints) / worldObjectType.growthRate;
            return hoursUntilFinished.ToString();
        }
    }
}