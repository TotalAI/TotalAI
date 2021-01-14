using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace TotalAI
{
    public class Agent : Entity
    {
        public float mainLoopInterval = 0.2f;
        public float maxMainLoopRandomWait = 0.2f;
        public float currentRandomWait;

        public enum Gender { Unknown, Female, Male }
        public Gender gender;

        // Age when game starts in years
        public int startAge;
        public float gLevel;

        public Faction faction;
        public List<RoleType> roleTypes;

        public WorldObjectType corpseWorldObjectType;
        public int corpsePrefabVariantIndex;
        public bool makeNewCorpseGameObject;
        public string dieAnimatorBoolParamName;
        public string dieAnimatorStateName;
        public float dieWaitTime;
        public bool moveInventoryToCorpse;

        public Dictionary<DriveType, Drive> drives;
        public Dictionary<ActionType, Action> actions;
        public Dictionary<RoleType, Role> roles;

        // Used to record where the agent has been
        public List<Vector3> pastLocations;

        public bool isAutonomous;
        public bool isAlive;

        [HideInInspector]
        public MovementType movementType;

        [HideInInspector]
        public AnimationType animationType;

        [HideInInspector]
        public UtilityFunctionType utilityFunction;

        [HideInInspector]
        public List<SensorType> sensorTypes;
        [HideInInspector]
        public Entity[] detectedEntities;

        public Dictionary<SensorType, Entity[]> sensorJustDetected;
        public Dictionary<SensorType, int> sensorJustDetectedNum;

        [HideInInspector]
        public MemoryType memoryType;

        private DeciderType deciderType;
        public Decider decider;

        [HideInInspector]
        public MappingType noPlansMappingType;

        [HideInInspector]
        public DriveType noneDriveType;

        [HideInInspector]
        public HistoryType historyType;

        [HideInInspector]
        public AgentType agentType;

        [HideInInspector]
        public TimeManager timeManager;

        [HideInInspector]
        public Behavior behavior;

        public AgentEvent inAgentEvent;

        [HideInInspector]
        public HashSet<MappingType> availableMappingTypes;

        private Coroutine mainCoroutine;

        private new void Awake()
        {
            base.Awake();
            agentType = (AgentType)entityType;
            GetAgentComponents();
            ResetAgent();
        }

        void Start()
        {
            inAgentEvent = null;
            isAutonomous = true;
            isAlive = true;

            pastLocations.Add(transform.position);

            mainCoroutine = StartCoroutine(MainAgentLoop());
        }

        public void Revive(Vector3 position, Quaternion rotation)
        {
            transform.position = position;
            transform.rotation = rotation;

            Revive();
        }

        public void Revive()
        {
            behavior.ResetBehavior();
            isAlive = true;
            gameObject.layer = LayerMask.NameToLayer("Agent");
            gameObject.SetActive(true);

            StopCoroutine(mainCoroutine);
            mainCoroutine = StartCoroutine(MainAgentLoop());
        }

        private void GetAgentComponents()
        {
            GameObject timeGameObject = GameObject.Find("TimeManager");
            if (timeGameObject == null)
            {
                Debug.LogError("Please add an Empty GameObject named TimeManager to the scene and add TimeManager component to it.");
                return;
            }
            timeManager = timeGameObject.GetComponent<TimeManager>();
            if (timeManager == null)
            {
                Debug.LogError("Please add TimeManager component to the TimeManager GameObject.");
                return;
            }
        }

        private IEnumerator MainAgentLoop()
        {
            while (true)
            {
                if (isAlive)
                {
                    currentRandomWait = UnityEngine.Random.value * maxMainLoopRandomWait;
                    yield return new WaitForSeconds(currentRandomWait);

                    if (inAgentEvent != null && inAgentEvent.TimeLimitExpired())
                        QuitEvent();

                    DetectEntities();
                    decider.Run();
                    UpdateDriveLevels();
                    RunEntityTriggers(null, EntityTrigger.TriggerType.MainLoop);
                    SetPastLocation();

                    if (agentType.useAnimatorOverrides)
                        animationType.UpdateAnimations(this);
                }
                yield return new WaitForSeconds(mainLoopInterval);
            }
        }

        private void Update()
        {
            RunEntityTriggers(null, EntityTrigger.TriggerType.UpdateLoop);
        }

        // Call to run the decider loop early - for example if immediate starting of a mapping is required
        public void RunEarly()
        {
            DetectEntities();
            decider.Run();
        }

        public void Freeze(float seconds)
        {
            if (!isAlive || seconds <= 0f)
                return;

            isAlive = false;
            behavior.InterruptBehavior(false);
            movementType.SetStopped(this, true);
            animationType.Disable(this);
            movementType.Disable(this);
            Rigidbody ridgidbody = GetComponent<Rigidbody>();
            if (ridgidbody != null)
            {
                ridgidbody.isKinematic = true;
            }
            StartCoroutine(FreezeCoroutine(seconds));
        }

        private IEnumerator FreezeCoroutine(float seconds)
        {
            yield return new WaitForSeconds(seconds);

            animationType.Enable(this);
            movementType.Enable(this);
            Rigidbody ridgidbody = GetComponent<Rigidbody>();
            if (ridgidbody != null)
            {
                ridgidbody.isKinematic = false;
            }
            isAlive = true;
        }

        // Killed by agent if not null - agent gets ownership on corpse
        public override void DestroySelf(Agent agent, float delay = 0f)
        {
            if (!isAlive)            
                return;

            isAlive = false;
            movementType.SetStopped(this, true);

            // TODO: Move into movementType?  Still not sure if I need a ridgidbody on Agents
            //       Also need some kind of death height - so the body isn't in the ground or use ridgidbody?
            Rigidbody ridgidbody = GetComponent<Rigidbody>();
            if (ridgidbody != null)
            {
                ridgidbody.isKinematic = true;
            }

            // Let Die Animation play
            if (dieAnimatorStateName != null && dieAnimatorStateName.Length > 0)
                animationType.PlayAnimationState(this, dieAnimatorStateName);
            else if (dieAnimatorBoolParamName != null && dieAnimatorBoolParamName.Length > 0)
                animationType.SetBool(this, dieAnimatorBoolParamName, true);

            // Change layer to prevent this WorldObject from getting added back to known entities
            SetLayerRecursively(gameObject, LayerMask.NameToLayer("Default"));

            OnDisable();

            // TODO: Handle other behavior systems
            //BehaviorTree bt = gameObject.GetComponent<BehaviorTree>();
            //if (bt != null)
            //    bt.DisableBehavior();

            // TODO: Does there need to be a separate KillBehavior?  I think this is okay - need time to die anyways
            // TODO: Should this call decider.InterruptMapping instead?
            behavior.InterruptBehavior(false);

            // TODO: Eventually do object pooling
            //Destroy(gameObject);
            //Debug.Log(name + " has been destroyed");

            // Spawn corpse if it is set
            StartCoroutine(SpawnCorpse(agent));
        }

        // Wait until die animation is done and then spawn the agent's corpse
        private IEnumerator SpawnCorpse(Agent agent)
        {
            yield return new WaitForSeconds(dieWaitTime);
            
            if (corpseWorldObjectType != null)
            {
                if (makeNewCorpseGameObject)
                {
                    Transform corpseTransform = corpseWorldObjectType.prefabVariants[corpsePrefabVariantIndex].transform;
                    GameObject corpse = corpseWorldObjectType.CreateEntity(corpsePrefabVariantIndex, transform.position, corpseTransform.rotation,
                                                                           corpseTransform.localScale, null);
                    if (moveInventoryToCorpse)
                    {
                        yield return null;
                        Entity corpseEntity = corpse.GetComponent<Entity>();
                        if (corpseEntity == null)
                        {
                            Debug.LogError(name + ": Died - its corpse " + corpse.name + " does not have a WorldObject attached to it.");
                        }
                        else
                        {
                            inventoryType.MoveAllInventoryTo(this, corpseEntity);
                        }
                    }
                    gameObject.SetActive(false);
                }
                else
                {
                    gameObject.SetActive(false);
                    enabled = false;
                    animationType.Disable(this);
                    movementType.Disable(this);
                    WorldObject worldObject = gameObject.AddComponent<WorldObject>() as WorldObject;
                    worldObject.entityType = corpseWorldObjectType;
                    gameObject.layer = LayerMask.NameToLayer("WorldObject");
                    List<Entity> entities = inventoryType.GetAllEntities(this);
                    foreach (Entity entity in entities)
                    {
                        entity.inEntityInventory = worldObject;
                    }
                    gameObject.name = corpseWorldObjectType.name;
                    gameObject.SetActive(true);
                }
            }
            else
            {
                gameObject.SetActive(false);
            }
        }

        // Can retrieve any LevelType's level - returns inf if Agent does not have LevelType
        // TODO: Shouldn't allow this to be called for TagTypes?
        public float GetLevel(LevelType levelType)
        {
            switch (levelType)
            {
                case DriveType driveType:
                    return drives[driveType].GetLevel();
                case ActionType actionType:
                    return actions[actionType].GetLevel();
                case AttributeType attributeType:
                    return attributes[attributeType].GetLevel();
                case RoleType roleType:
                    return roles[roleType].GetLevel();
                case TagType tagType:
                    return tags[tagType][0].GetLevel();
            }

            return float.PositiveInfinity;
        }

        //TODO: Create utility class and move this there
        private void SetLayerRecursively(GameObject obj, int newLayer)
        {
            if (null == obj)
            {
                return;
            }

            obj.layer = newLayer;

            foreach (Transform child in obj.transform)
            {
                if (null == child)
                {
                    continue;
                }
                SetLayerRecursively(child.gameObject, newLayer);
            }
        }

        // Agent is an attendee in an AgentEvent that has just started - change roles if needed
        public void EventStarting(AgentEvent agentEvent)
        {
            Debug.Log(name + ": Starting Event");
            inAgentEvent = agentEvent;

            // Figure out what roles need to be applied
            // TODO: Only CreatorAttendee Type is implemented
            List<RoleType> roleTypes = agentEvent.GetRoleTypes(this);

            // Handle applying roles
            foreach (RoleType roleType in roleTypes)
            {
                roleType.AddToAgent(this);
            }
        }

        public void QuitEvent()
        {
            Debug.Log(name + ": Quitting Event");

            // Run On Quit AgentEvent OutputChangesWaitEndOCT
            decider.RunOutputChangesFromAgent(this, null, OutputChange.Timing.OnQuitAgentEvent);

            // Disable any AgentEvent roles and figure out changes to Drives and Actions
            List<RoleType> roleTypes = inAgentEvent.GetRoleTypes(this);

            // Handle applying roles
            foreach (RoleType roleType in roleTypes)
            {
                roleType.RemoveFromAgent(this);
            }

            inAgentEvent.RemoveAttendee(this);

            inAgentEvent = null;
        }

        public void ChangeStatusOnAllActionsAndDrives(bool activate)
        {
            foreach (Drive drive in drives.Values)
            {
                drive.SetActive(this, activate);
            }
            foreach (Action action in actions.Values)
            {
                action.SetActive(this, activate);
            }
        }

        public void ChangeStatusOnActionsAndDrivesFromRoles(bool activate)
        {

        }

        // TODO: Better to just maintain an activeDrives Dictionary?
        public Dictionary<DriveType, Drive> ActiveDrives()
        {
            return drives.Where(x => x.Value.GetStatus()).ToDictionary(x => x.Key, x => x.Value);
        }

        public Dictionary<ActionType, Action> ActiveActions()
        {
            return actions.Where(x => x.Value.GetStatus()).ToDictionary(x => x.Key, x => x.Value);
        }

        public Dictionary<RoleType, Role> ActiveRoles()
        {
            return roles.Where(x => x.Value.GetStatus()).ToDictionary(x => x.Key, x => x.Value);
        }

        public Dictionary<DriveType, Drive> DisabledDrives()
        {
            return drives.Where(x => !x.Value.GetStatus()).ToDictionary(x => x.Key, x => x.Value);
        }

        public Dictionary<ActionType, Action> DisabledActions()
        {
            return actions.Where(x => !x.Value.GetStatus()).ToDictionary(x => x.Key, x => x.Value);
        }

        public Dictionary<RoleType, Role> DisabledRoles()
        {
            return roles.Where(x => !x.Value.GetStatus()).ToDictionary(x => x.Key, x => x.Value);
        }

        public bool DriveTypeInActiveRoleTypes(DriveType driveType)
        {
            // TODO: What is one role adds and a later one removes it?
            return ActiveRoles().Keys.Any(x => x.driveChanges.Any(y => y.driveType == driveType && y.changeType == RoleType.ChangeType.Add));
        }

        public bool ActionTypeInActiveRoleTypes(ActionType actionType)
        {
            // TODO: What is one role adds and a later one removes it?
            return ActiveRoles().Keys.Any(x => x.actionChanges.Any(y => y.actionType == actionType && y.changeType == RoleType.ChangeType.Add));
        }

        // Updates all the agents Drives based on their change over time amounts
        private void UpdateDriveLevels()
        {
            float numSecondsInGameHour = timeManager.realTimeInMinutesPerDay * 60 / 24;

            foreach (KeyValuePair<DriveType, Drive> drive in ActiveDrives())
            {
                drive.Value.ChangeLevel(drive.Value.CurrentDriveChangeRate(timeManager.MinutesIntoDay()) *
                                        (mainLoopInterval + currentRandomWait) / numSecondsInGameHour);
            }
        }

        private void SetPastLocation()
        {
            // TODO: Move this into its own method
            if (Vector3.Distance(transform.position, pastLocations[pastLocations.Count - 1]) > 5)
                pastLocations.Add(transform.position);
        }

        // Returns the agent's current age based on starting age and current time
        // Assume for now 120 days in a year (4 months - 30 days each)
        public float CurrentAgeFloat()
        {
            return startAge + timeManager.DaysSinceStart() / 120f;
        }

        public int CurrentAge()
        {
            return startAge + timeManager.Year();
        }

        // Sets Agent's info based on AgentType and AgentTypeOverrides
        // Also handles reset if this is in the Editor and not playing - for AgentView Editor
        public void ResetAgent(bool inEditor = false)
        {
            if (inEditor)
            {
                agentType = (AgentType)entityType;
                if (agentType == null)
                    return;
            }
            behavior = new Behavior();
            if (faction != null)
                faction.SetupAgent(this);
            behavior.Setup(this);
            ResetNoPlans();
            ResetMovementType();
            ResetAnimationType();
            ResetEntity(inEditor);
            ResetDrives();
            ResetActions();
            ResetRoles();
            ResetAvailableMappingTypes();
            ResetUtilityFunction();
            ResetSensorTypes();
            ResetMemoryType();
            ResetDeciderType();
            ResetHistoryType();

            CheckForRequiredTypes();
        }

        // Checks to make sure agent has the minimal required types to actually be a functioning agent
        // TODO:  Is DeciderType the only requirement?  What about None Drive, at least one Action?
        private void CheckForRequiredTypes()
        {
            string errorText = "";

            if (deciderType == null)
                errorText += " DeciderType ";

            if (errorText != "")
                Debug.LogError(name + "Missing types: " + errorText);
        }

        private void ResetNoPlans()
        {
            noPlansMappingType = agentType.defaultNoPlansMappingType;
            foreach (AgentTypeOverride agentTypeOverride in entityTypeOverrides)
            {
                if (agentTypeOverride != null && agentTypeOverride.defaultNoPlansMappingType != null)
                    noPlansMappingType = agentTypeOverride.defaultNoPlansMappingType;
            }
            noneDriveType = agentType.defaultNoPlansDriveType;
            foreach (AgentTypeOverride agentTypeOverride in entityTypeOverrides)
            {
                if (agentTypeOverride != null && agentTypeOverride.defaultNoPlansDriveType != null)
                    noneDriveType = agentTypeOverride.defaultNoPlansDriveType;
            }
        }

        public void ResetAnimationType(bool forEditor = false)
        {
            animationType = agentType.defaultAnimationType;
            if (animationType == null)
            {
                Debug.LogError(name + ": AgentType = " + agentType.name + " is missing a defaultAnimationType.  Please fix.");
                return;
            }
            foreach (AgentTypeOverride agentTypeOverride in entityTypeOverrides)
            {
                if (agentTypeOverride != null && agentTypeOverride.defaultAnimationType != null)
                    animationType = agentTypeOverride.defaultAnimationType;
            }

            animationType.SetupAgent(this, forEditor);
        }

        private void ResetMovementType()
        {
            movementType = agentType.defaultMovementType;
            if (movementType == null)
            {
                Debug.LogError(name + ": AgentType = " + agentType.name + " is missing a defaultMovementType.  Please fix.");
                return;
            }
            foreach (AgentTypeOverride agentTypeOverride in entityTypeOverrides)
            {
                if (agentTypeOverride != null && agentTypeOverride.defaultMovementType != null)
                    movementType = agentTypeOverride.defaultMovementType;
            }
            movementType.SetupAgent(this);
            if (Application.isPlaying)
                movementType.SetStopped(this, true);
        }

        private void ResetHistoryType()
        {
            historyType = agentType.defaultHistoryType;
            if (historyType == null)
            {
                Debug.LogError(name + ": AgentType = " + agentType.name + " is missing a defaultHistoryType.  Please fix.");
                return;
            }
            foreach (AgentTypeOverride agentTypeOverride in entityTypeOverrides)
            {
                if (agentTypeOverride.defaultHistoryType != null)
                    historyType = agentTypeOverride.defaultHistoryType;
            }
            historyType.SetupAgent(this);
        }

        private void ResetUtilityFunction()
        {
            utilityFunction = agentType.defaultUtilityFunction;

            foreach (AgentTypeOverride agentTypeOverride in entityTypeOverrides)
            {
                if (agentTypeOverride.defaultUtilityFunction != null)
                    utilityFunction = agentTypeOverride.defaultUtilityFunction;
            }
        }
        
        private void ResetDrives()
        {
            drives = new Dictionary<DriveType, Drive>();
            foreach (AgentType.DefaultDrive defaultDrive in agentType.defaultDrives)
            {
                DriveType driveType = defaultDrive.driveType;
                Drive newDrive;
                if (defaultDrive.overrideDriveType)
                    newDrive = new Drive(this, driveType, defaultDrive.startingLevel, driveType.utilityCurve, defaultDrive.changePerGameHour,
                                         defaultDrive.rateTimeCurve, defaultDrive.minTimeCurve, defaultDrive.maxTimeCurve);
                else
                    newDrive = new Drive(this, driveType, defaultDrive.startingLevel, driveType.utilityCurve, driveType.changePerGameHour,
                                         driveType.rateTimeCurve, driveType.minTimeCurve, driveType.maxTimeCurve);

                drives.Add(defaultDrive.driveType, newDrive);
            }

            // Handle Overrides - goes from first one to last one
            foreach (AgentTypeOverride agentTypeOverride in entityTypeOverrides)
            {
                if (agentTypeOverride != null && agentTypeOverride.overrideDrives == EntityOverrideType.AddOrReplace)
                {
                    for (int i = 0; i < agentTypeOverride.defaultDriveTypes.Count; i++)
                    {
                        // If drive already exists this will overwrite the attribute's value
                        drives[agentTypeOverride.defaultDriveTypes[i]] =
                            new Drive(this, agentTypeOverride.defaultDriveTypes[i], agentTypeOverride.defaultDriveLevels[i],
                                      agentTypeOverride.defaultDriveTypes[i].utilityCurve, agentTypeOverride.defaultDriveChangesPerHour[i]);
                    }
                }
                else if (agentTypeOverride != null && agentTypeOverride.overrideDrives == EntityOverrideType.Remove)
                {
                    for (int i = 0; i < agentTypeOverride.defaultDriveTypes.Count; i++)
                    {
                        drives.Remove(agentTypeOverride.defaultDriveTypes[i]);
                    }
                }
            }

            // Make sure DriveTypes have a set utility curve
            // TODO: This should not be here - maybe in AgentType inspector?
            foreach (DriveType driveType in drives.Keys)
            {
                if (driveType.utilityCurve == null || driveType.utilityCurve.length == 0)
                    Debug.LogError("Utility curve is not set for " + driveType.name + " - Please fix this.");
            }
        }

        // Set all actions and actionLevels to the agent types defaults
        private void ResetActions()
        {
            actions = new Dictionary<ActionType, Action>();
            foreach (AgentType.DefaultAction action in agentType.defaultActions)
            {
                actions.Add(action.actionType, new Action(action.actionType, action.level, action.changeProbability, action.changeAmount));
            }
            
            // Handle Overrides - goes from first one to last one
            foreach (AgentTypeOverride agentTypeOverride in entityTypeOverrides)
            {
                if (agentTypeOverride != null && agentTypeOverride.overrideActions == EntityOverrideType.AddOrReplace)
                {
                    for (int i = 0; i < agentTypeOverride.defaultActionTypes.Count; i++)
                    {
                        // If action already exists this will overwrite the attribute's value
                        actions[agentTypeOverride.defaultActionTypes[i]] =
                                       new Action(agentTypeOverride.defaultActionTypes[i], agentTypeOverride.defaultActionLevels[i],
                                                  agentTypeOverride.defaultActionChangeProbabilities[i], agentTypeOverride.defaultActionChangeAmounts[i]);
                    }
                }
                else if (agentTypeOverride != null && agentTypeOverride.overrideActions == EntityOverrideType.Remove)
                {
                    for (int i = 0; i < agentTypeOverride.defaultActionTypes.Count; i++)
                    {
                        actions.Remove(agentTypeOverride.defaultActionTypes[i]);
                    }
                }
            }
        }

        private void ResetRoles()
        {
            roles = new Dictionary<RoleType, Role>();
            foreach (RoleType roleType in roleTypes)
            {
                if (roleType != null)
                    roles.Add(roleType, new Role(roleType, 0));
            }

            // Adds all drives and actions from the roles in the correct order - should be called anytime a role is added
            Role.InitDrivesActionsFromRoles(this, roles.Keys.ToList());
        }

        public void ResetAvailableMappingTypes()
        {
            availableMappingTypes = new HashSet<MappingType>();
            foreach (ActionType actionType in ActiveActions().Keys)
            {
                availableMappingTypes.UnionWith(actionType.FilteredMappingTypes(agentType));
            }
        }

        private void ResetSensorTypes()
        {
            sensorTypes = new List<SensorType>(agentType.defaultSensorTypes);

            // Handle Overrides - goes from first one to last one
            foreach (AgentTypeOverride agentTypeOverride in entityTypeOverrides)
            {
                if (agentTypeOverride.overrideSensorTypes == EntityOverrideType.AddOrReplace)
                {
                    for (int i = 0; i < agentTypeOverride.defaultSensorTypes.Count; i++)
                    {
                        sensorTypes.Add(agentTypeOverride.defaultSensorTypes[i]);
                    }
                }
                else if (agentTypeOverride.overrideSensorTypes == EntityOverrideType.Remove)
                {
                    for (int i = 0; i < agentTypeOverride.defaultSensorTypes.Count; i++)
                    {
                        sensorTypes.Remove(agentTypeOverride.defaultSensorTypes[i]);
                    }
                }
            }

            detectedEntities = new Entity[agentType.maxNumDetectedEntities];
            sensorJustDetected = new Dictionary<SensorType, Entity[]>();
            sensorJustDetectedNum = new Dictionary<SensorType, int>();
            foreach (SensorType sensorType in sensorTypes)
            {
                if (sensorType == null)
                {
                    Debug.Log(name + ": Has a null SensorType in their AgentType or in their AgentTypeOverrides.");
                    continue;
                }
                sensorType.Setup(this, entityLayers);
                sensorJustDetected[sensorType] = new Entity[agentType.maxNumDetectedEntities];
                sensorJustDetectedNum[sensorType] = 0;
            }
        }

        private void ResetMemoryType()
        {
            memoryType = agentType.defaultMemoryType;
            foreach (AgentTypeOverride agentTypeOverride in entityTypeOverrides)
            {
                if (agentTypeOverride.defaultMemoryType != null)
                    memoryType = agentTypeOverride.defaultMemoryType;
            }
            if (memoryType == null)
            {
                Debug.LogError(name + ": AgentType = " + agentType.name + " is missing a defaultMemoryType.  Please fix.");
                return;
            }
            memoryType.Setup(this);
        }

        private void ResetDeciderType()
        {
            deciderType = agentType.defaultDeciderType;
            foreach (AgentTypeOverride agentTypeOverride in entityTypeOverrides)
            {
                if (agentTypeOverride.defaultDeciderType != null)
                    deciderType = agentTypeOverride.defaultDeciderType;
            }
            if (deciderType == null)
            {
                Debug.LogError(name + ": AgentType = " + agentType.name + " is missing a defaultDeciderType.  Please fix.");
                return;
            }
            decider = new Decider(this, deciderType);
        }

        public int NumTags()
        {
            int total = 0;
            foreach (KeyValuePair<TagType, List<Tag>> tags in tags)
            {
                total += tags.Value.Count;
            }
            return total;
        }

        private void DetectEntities()
        {
            memoryType.UpdateShortTermMemory(this);
            foreach (SensorType sensorType in sensorTypes)
            {
                if (sensorType.TimeToRun(this))
                {
                    int numEntities = sensorType.Run(this, detectedEntities);

                    // Save this for logging - probably want to turn this off for production
                    detectedEntities.CopyTo(sensorJustDetected[sensorType], 0);
                    sensorJustDetectedNum[sensorType] = numEntities;

                    memoryType.AddEntities(this, detectedEntities, numEntities);
                }
            }
        }

        // TODO: Move these to Attribute or AttributeType?
        public float AttributeLevel(AttributeType attributeType)
        {
            if (!attributes.TryGetValue(attributeType, out Attribute attribute))
            {
                Debug.LogError(name + ": Missing required AttributeType = " + attributeType.name);
                return 0;
            }

            return attribute.GetLevel();
        }

        public float NormalizedAttributeLevel(AttributeType attributeType)
        {
            float min = attributes[attributeType].GetMin();
            float max = attributes[attributeType].GetMax();

            return (AttributeLevel(attributeType) - min) / (max - min);
        }

        public float ActionSkillWithItemModifiers(ActionType actionType)
        {
            return actions[actionType].GetLevelWithItemActionSkillModifiers(this);
        }

        // This is the function to call from an AnimationEvent
        // Make sure it is exactly "AnimationOutputChange"
        // TODO: How does an Entity get passed in?  Its hard coded - EntityType could be possible?
        public void AnimationOutputChange(Entity entity)
        {
            decider.RunOutputChangesFromAgent(this, entity, OutputChange.Timing.OnAnimationEvent);
        }

        public override void OnCollisionEnter(Collision collision)
        {
            base.OnCollisionEnter(collision);

            Entity entity = collision.gameObject.GetComponent<Entity>();
            Debug.Log("Agent = On Collision Enter between " + name + " and " + entity);
            if (entity != null)
                decider.RunOutputChangesFromAgent(this, entity, OutputChange.Timing.OnCollisionEnter);
        }

        public override void OnTriggerEnter(Collider other)
        {
            base.OnTriggerEnter(other);

            Entity entity = other.gameObject.GetComponent<Entity>();
            if (entity != null)
                decider.RunOutputChangesFromAgent(this, entity, OutputChange.Timing.OnTriggerEnter);
        }

        private new void OnDrawGizmosSelected()
        {
            base.OnDrawGizmosSelected();

            if (pastLocations != null)
            {
                foreach (Vector3 location in pastLocations)
                {
                    Gizmos.color = Color.black;
                    Gizmos.DrawSphere(location, 0.3f);
                }
            }

            if (sensorTypes != null)
            {
                foreach (SensorType sensorType in sensorTypes)
                {
                    sensorType.DrawGizmos(this);
                }
            }
        }
    }
}
