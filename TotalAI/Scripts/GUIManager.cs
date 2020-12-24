using System;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace TotalAI
{
    public class GUIManager : MonoBehaviour
    {
        public bool for2D = false;
        public bool showAgentsMappingType = true;
        public float markerFontSize = 24f;
        public float markerHeight = 2.5f;

        public List<EntityType> entityTypes;
        public RectTransform agentMarkerPrefab;         // Marker that is above the agent's head
        public RectTransform selectEntityButtonPrefab;  // Button prefab for selecting an entity
        public RectTransform driveTypeSliderPrefab;
        public Material validPlacementMaterial;
        public Material invalidPlacementMaterial;
        public Agent startingAgent;
        public int prefabVariantIndex = 0;

        private EntityType selectedEntityType;
        private float currentBuildHeight;
        private enum BuildableStatus { Valid, TerraformingRequired, Invalid}
        private BuildableStatus buildableStatus;
        private GameObject entityGhost;
        private Vector3 previousGhostPosition;
        private List<RectTransform> selectEntityButtons;

        private List<Agent> agents;
        [HideInInspector]
        public Agent agent;

        private Text nameText;
        private Text driveText;
        private Text drivesRankedText;
        private TextMeshProUGUI timeText;

        private GameObject selectedEntityPanel;

        private List<Slider> driveSliders;

        private int agentIndex;

        private TimeManager timeManager;

        private List<TextMeshProUGUI> markerTexts;

        private TerrainActions terrainActions;

        void Start()
        {
            if (Terrain.activeTerrain != null)
                terrainActions = Terrain.activeTerrain.GetComponent<TerrainActions>();

            timeManager = GameObject.Find("TimeManager").GetComponent<TimeManager>();

            selectEntityButtons = new List<RectTransform>();

            nameText = transform.Find("Agent Name").GetComponent<Text>();
            driveText = transform.Find("Bottom Right/Drive Text").GetComponent<Text>();
            drivesRankedText = transform.Find("Bottom Right/Drives Ranked Text").GetComponent<Text>();
            timeText = transform.Find("Time/Time Text").GetComponent<TextMeshProUGUI>();

            selectedEntityPanel = transform.Find("Selected Entity Info").gameObject;

            markerTexts = new List<TextMeshProUGUI>();
            agents = new List<Agent>(FindObjectsOfType<Agent>());
            foreach (Agent agent in agents)
            {
                if (agent.gameObject.activeInHierarchy)
                    AddAgent(agent);
            }

            if (startingAgent == null)
                agent = agents[0];
            else
                agent = startingAgent;

            currentBuildHeight = -100f;
            previousGhostPosition = Vector3.zero;
            
            foreach (EntityType entityType in entityTypes)
            {
                // Create button
                RectTransform newButton = Instantiate(selectEntityButtonPrefab, transform.Find("EntityTypes"), false);
                selectEntityButtons.Add(newButton);

                // Change text 
                newButton.Find("ButtonName").GetComponent<Text>().text = entityType.name;

                // Set the UnityEvent
                newButton.GetComponent<Button>().onClick.AddListener(delegate { SelectEntityType(entityType, newButton.GetComponent<Button>()); });
            }
        }

        void Update()
        {
            if (Input.GetKeyDown("p"))
            {
                if (Time.timeScale == 1)
                    Time.timeScale = 0;
                else
                    Time.timeScale = 1;
            }

            if (!EventSystem.current.IsPointerOverGameObject())
            {
                if (selectedEntityType != null)
                {
                    Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
                    RaycastHit hit;
                    if (Physics.Raycast(ray, out hit, 300, LayerMask.GetMask("Ground", "WorldObject")))
                    {
                        Vector3 locationOnGrid = hit.point;
                        if (terrainActions != null)
                        {
                            locationOnGrid = terrainActions.FindGridLocation(hit.point, currentBuildHeight);
                        }
                        //Vector3 locationOnGrid = terrainActions.FindGridLocation(hit.point, currentBuildHeight);
                        
                        locationOnGrid.y = hit.point.y;

                        // Place selected entity outline in the world at hit location
                        if (entityGhost != null)
                        {
                            if (!entityGhost.activeSelf)
                                entityGhost.SetActive(true);
                            entityGhost.transform.position = locationOnGrid;

                            // TODO: What about moving agents? - Do a OnTriggerEnter for the ghosts which could just clear out previousGhostPosition
                            if (previousGhostPosition != entityGhost.transform.position)
                            {
                                buildableStatus = GetBuildableStatus(selectedEntityType, entityGhost);
                                if (selectedEntityType.rotateToTerrain)
                                {
                                    // Rotate and transform ghost so that it is just above the terrain
                                    entityGhost.transform.up = hit.normal;
                                }
                                else if (terrainActions != null)
                                {
                                    // Calculate distance to move entityGhost in its up direction (normal to terrain)
                                    terrainActions.MoveObjectAboveTerrain(entityGhost);
                                }

                                previousGhostPosition = entityGhost.transform.position;

                                //TODO: Makes a utility function for changing all of a nested objects materials
                                Renderer[] renderers = entityGhost.GetComponentsInChildren<Renderer>();
                                if (renderers.Length == 0)
                                    Debug.LogError("Entity Ghost has no renderer! " + entityGhost.name);

                                Material material;
                                if (buildableStatus == BuildableStatus.Invalid)
                                    material = invalidPlacementMaterial;
                                else
                                    material = validPlacementMaterial;

                                for (int i = 0; i < renderers.Length; i++)
                                {
                                    if (renderers[i].material != material)
                                    {
                                        Material[] materials = new Material[renderers[i].materials.Length];
                                        for (var j = 0; j < renderers[i].materials.Length; j++)
                                        {
                                            materials[j] = material;
                                        }
                                        renderers[i].materials = materials;
                                    }
                                }
                            }

                            if (Input.GetKeyDown("r"))
                            {
                                entityGhost.transform.Rotate(new Vector3(0, 90, 0));
                                previousGhostPosition = Vector3.zero;
                            }
                        }
                        if (Input.GetMouseButtonDown(0))
                        {
                            Quaternion rotation = Quaternion.identity;
                            Vector3 scale = Vector3.one;
                            if (entityGhost != null)
                            {
                                rotation = entityGhost.transform.rotation;
                                scale = entityGhost.transform.localScale;

                                // TODO: Fix this so its not so much of a hack - the .02 prevents the flashing from ghost over placed object
                                // Could make every ghost be .02 larger and then always subtract .02 - also only do scale is FI can be scaled
                                if (scale.x.ToString().IndexOf(".02") != -1)
                                {
                                    scale.x = Mathf.Round(scale.x);
                                    scale.y = Mathf.Round(scale.y);
                                    scale.z = Mathf.Round(scale.z);
                                }
                            }

                            // Create Entity with owner permissions set to selected faction and the selected agent if not in overhead view
                            List<Agent> owners = new List<Agent>();
                            //if (!overheadCamera.enabled)
                            //    owners.Add(agent);
                            GameObject newGameObject;
                            if (entityGhost != null)
                            {
                                newGameObject = selectedEntityType.CreateEntity(prefabVariantIndex, entityGhost.transform.position, rotation, scale, agent);
                            }
                            else
                            {
                                newGameObject = selectedEntityType.CreateEntity(prefabVariantIndex, locationOnGrid, rotation, scale, agent);
                            }

                            if (selectedEntityType is AgentType)
                            {
                                // Add agent to the list of agents
                                newGameObject.name = selectedEntityType.name;
                                Agent a = newGameObject.GetComponent<Agent>();
                                agents.Add(a);
                                AddAgent(a);
                            }
                        }
                    }
                    else if (entityGhost != null && entityGhost.activeSelf)
                    {
                        entityGhost.SetActive(false);
                    }
                }
                else
                {
                    // No Entity Selected - Allow player to select entities with mouse click
                    // TODO: Add a create box select (hold mouse button down) and multi-entity select
                    if (Input.GetMouseButtonDown(0))
                    {
                        Debug.Log("Mouse Button Clicked");
                        Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
                        
                        Entity entity = null;
                        if (for2D)
                        {
                            RaycastHit2D[] hits = Physics2D.RaycastAll(Camera.main.ScreenToWorldPoint(Input.mousePosition), Vector2.zero,
                                                                       LayerMask.GetMask("Agent", "WorldObject", "AgentEvent"));
                            if (hits.Length > 0)
                            {
                                float bestDistance = float.PositiveInfinity;
                                foreach (RaycastHit2D hit in hits)
                                {
                                    if (hit.distance < bestDistance)
                                    {
                                        Entity[] hitEntities = hit.collider.gameObject.GetComponentsInParent<Entity>();
                                        foreach (Entity hitEntity in hitEntities)
                                        {
                                            if (hitEntity.inEntityInventory == null && hitEntity.enabled)
                                            {
                                                bestDistance = hit.distance;
                                                entity = hitEntity;
                                            }
                                        }
                                    }
                                }
                            }
                        }
                        else
                        {
                            RaycastHit[] hits = Physics.RaycastAll(ray, 500, LayerMask.GetMask("Agent", "WorldObject", "AgentEvent"),
                                                                  QueryTriggerInteraction.Collide);
                            if (hits.Length > 0)
                            {
                                float bestDistance = float.PositiveInfinity;
                                foreach (RaycastHit hit in hits)
                                {
                                    if (hit.distance < bestDistance)
                                    {
                                        Entity[] hitEntities = hit.collider.gameObject.GetComponentsInParent<Entity>();
                                        foreach (Entity hitEntity in hitEntities)
                                        {
                                            if (hitEntity.inEntityInventory == null && hitEntity.enabled)
                                            {
                                                bestDistance = hit.distance;
                                                entity = hitEntity;
                                            }
                                        }
                                    }
                                }
                            }
                        }
                    }
                }
            }

            if (driveSliders == null)
            {
                Transform driveTypeParent = transform.Find("DriveType Levels");

                driveSliders = new List<Slider>();
                int i = 0;
                foreach (DriveType driveType in agent.ActiveDrives().Keys)
                {
                    if (driveType.showInUI)
                    {
                        RectTransform newDriveType = Instantiate(driveTypeSliderPrefab, driveTypeParent);
                        newDriveType.transform.position = new Vector3(newDriveType.transform.position.x, newDriveType.transform.position.y - 30 * i);
                        newDriveType.GetComponentInChildren<TextMeshProUGUI>().text = driveType.name;

                        driveSliders.Add(newDriveType.GetComponentInChildren<Slider>());
                        ++i;
                    }
                }
            }

            driveText.text = "";
            int sliderIndex = 0;
            foreach (var drive in agent.ActiveDrives())
            {
                if (drive.Key.name != "None" && drive.Key.showInUI)
                {
                    float driveLevel = drive.Value.GetLevel();
                    driveText.text += drive.Key.name + ": " + Mathf.Round(driveLevel) + "\n";
                    driveSliders[sliderIndex].value = driveLevel / 100f;
                    sliderIndex++;
                }
            }

            drivesRankedText.text = "";
            int driveNumber = 1;
            if (agent.decider.currentDriveTypesRanked != null)
            {
                foreach (KeyValuePair<DriveType, float> drive in agent.decider.currentDriveTypesRanked)
                {
                    drivesRankedText.text += driveNumber + ": " + drive.Key.name + " (" + System.Math.Round(drive.Value, 3) + ")\n";
                    ++driveNumber;
                }
            }

            if (agent.decider.CurrentDriveType != null) drivesRankedText.text += "Current: " + agent.decider.CurrentDriveType.name + "\n";
            else drivesRankedText.text += "Current: None\n";

            if (showAgentsMappingType)
            {
                for (int i = 0; i < markerTexts.Count; i++)
                {
                    if (!agents[i].gameObject.activeSelf)
                    {
                        if (agents[i] == agent)
                        {
                            HandleCameraSwitching(true);
                        }

                        agents.Remove(agents[i]);

                        GameObject item = markerTexts[i].gameObject;
                        markerTexts.Remove(markerTexts[i]);
                        Destroy(item);
                        continue;
                    }

                    SetMarkerAboveObject(agents[i].gameObject, markerTexts[i]);

                    markerTexts[i].color = Color.white;
                    if (agents[i].decider.CurrentMapping != null)
                        markerTexts[i].text = agents[i].decider.CurrentMapping.mappingType.name;
                    else
                        markerTexts[i].text = "None";

                    /*
                    // TODO: Get this to float up and not cover the mappingType name
                    HistoryType.OutputChangeLog outputChangeLog = agents[i].historyType.GetLastOutputChangeLog(agents[i]);
                    if (outputChangeLog != null && outputChangeLog.time > Time.time - 2f)
                    {
                        markerTexts[i].text = outputChangeLog.outputChange.ToString();
                        if (outputChangeLog.outputChange.floatValue != outputChangeLog.amount)
                            markerTexts[i].text += "\nActual Amount = " + outputChangeLog.amount;
                        if (outputChangeLog.succeeded)
                            markerTexts[i].color = Color.green;
                        else
                            markerTexts[i].color = Color.red;
                    }
                    */
                }
            }

            HandleCameraSwitching();

            // TODO: Only do this every X seconds
            nameText.text = agent.name + " - " + agent.CurrentAge();
            timeText.text = timeManager.PrettyPrintDateTime();
        }

        // Return the ablility to build this entity based on the location
        private BuildableStatus GetBuildableStatus(EntityType entityType, GameObject entityGhost)
        {
            // If this entity requires flat land make sure the land underneath it is flat
            if (terrainActions != null && entityType.requiresFlatLand)
            {
                Renderer[] renderers = entityGhost.GetComponentsInChildren<Renderer>();
                if (renderers.Length == 0)
                    Debug.LogError("Entity Ghost has no renderer! " + entityGhost.name);

                Bounds ghostBounds = renderers[0].bounds;
                for (int i = 1; i < renderers.Length; i++)
                {
                    ghostBounds.Encapsulate(renderers[i].bounds);
                }

                // Get the max slope detected under the bounds
                float maxSlope = terrainActions.MaxSlope(ghostBounds);

                // Get the min and max height difference between the terrain and the ghostbounds
                float[] heightDiffs = terrainActions.GetHeightDiffs(ghostBounds);
                float minDiff = heightDiffs[0];
                float maxDiff = heightDiffs[1];

                // Make sure all diffs are small - object is slightly on top of terrain
                if (maxDiff - minDiff > .2f || maxDiff > .2f || minDiff < -.2f)
                    return BuildableStatus.Invalid;

                // Check for any collider collisions
                Collider[] hitColliders = Physics.OverlapBox(entityGhost.transform.position, entityGhost.transform.localScale / 2, entityGhost.transform.rotation,
                                                             LayerMask.GetMask("WorldObject", "Agent"), QueryTriggerInteraction.Collide);
                foreach (Collider collider in hitColliders)
                {
                    //Output all of the collider names
                    Debug.Log("Hit : " + collider.name);
                    return BuildableStatus.Invalid;
                }
            }
            else if(entityType.rotateToTerrain && entityType.maxSlopePlacement < 90f && entityType.maxSlopePlacement >= 0f)
            {
                // Get the max slope detected under the bounds
                Renderer[] renderers = entityGhost.GetComponentsInChildren<Renderer>();
                if (renderers.Length == 0)
                    Debug.LogError("Entity Ghost has no renderer! " + entityGhost.name);

                Bounds ghostBounds = renderers[0].bounds;
                for (int i = 1; i < renderers.Length; i++)
                {
                    ghostBounds.Encapsulate(renderers[i].bounds);
                }

                if (terrainActions != null && terrainActions.MaxSlope(ghostBounds) > entityType.maxSlopePlacement)
                    return BuildableStatus.Invalid;
            }
            
            return BuildableStatus.Valid;
        }


        private float IncreaseBuildHeight(float currentHeight)
        {
            return currentHeight + 1f;
        }

        private float DecreaseBuildHeight(float currentHeight)
        {
            return currentHeight - 1f;
        }

        private void IncreaseBuildScale()
        {
            entityGhost.transform.localScale += new Vector3(1,0,1);
        }

        private void DecreaseBuildScale()
        {
            entityGhost.transform.localScale -= new Vector3(1, 0, 1);
        }

        // UnityEvent OnClick listener for placing entity type
        public void SelectEntityType(EntityType entityType, Button buttonPressed)
        {
            if (entityGhost != null)
                Destroy(entityGhost);

            if (selectedEntityType != entityType)
            {
                // Default build height to the height of the active agent

                currentBuildHeight = 20f; // FindGridHeight(agent.transform.position.y);

                selectedEntityType = entityType;
                if (entityType.ghostPrefabs != null && entityType.ghostPrefabs.Count > prefabVariantIndex &&
                    entityType.ghostPrefabs[prefabVariantIndex] != null)
                    entityGhost = Instantiate(entityType.ghostPrefabs[prefabVariantIndex]);

                ColorBlock colors;

                // TODO: Keep track of currently selected button
                foreach (RectTransform rectTransform in selectEntityButtons)
                {
                    var button = rectTransform.GetComponent<Button>();
                    colors = button.colors;
                    colors.normalColor = selectEntityButtonPrefab.GetComponent<Button>().colors.normalColor;
                    colors.highlightedColor = selectEntityButtonPrefab.GetComponent<Button>().colors.highlightedColor;
                    button.colors = colors;
                }

                colors = buttonPressed.colors;
                colors.normalColor = Color.yellow;
                colors.highlightedColor = Color.yellow;
                buttonPressed.colors = colors;
            }
            else
            {
                selectedEntityType = null;

                ColorBlock colors;

                // TODO: Keep track of currently selected button
                foreach (RectTransform rectTransform in selectEntityButtons)
                {
                    var button = rectTransform.GetComponent<Button>();
                    colors = button.colors;
                    colors.normalColor = selectEntityButtonPrefab.GetComponent<Button>().colors.normalColor;
                    colors.highlightedColor = selectEntityButtonPrefab.GetComponent<Button>().colors.highlightedColor;
                    button.colors = colors;
                }
            }
        }

        private void AddAgent(Agent agent)
        {
            //firstPersonCameras.Add(a.transform.Find("Main Camera").GetComponent<Camera>());
            //firstPersonCameras[firstPersonCameras.Count - 1].enabled = false;

            if (showAgentsMappingType)
            {
                TextMeshProUGUI t = Instantiate(agentMarkerPrefab, transform).GetComponent<TextMeshProUGUI>();
                t.text = agent.name;
                t.fontSize = markerFontSize;
                markerTexts.Add(t);
            }
        }

        public void EnableAgent(Agent agent)
        {
            agents.Add(agent);

            AddAgent(agent);
        }

        private void RemoveAgent(Agent agent, int index)
        {
            markerTexts.RemoveAt(index);
        }

        public void DisableAgent(Agent agent)
        {
            int index = agents.IndexOf(agent);
            agents.RemoveAt(index);

            RemoveAgent(agent, index);
        }

        private void HandleCameraSwitching(bool force = false)
        {
            if (Input.GetKeyDown("1") || force)
            {
                //ShowFirstPersonView(cameraIndex + 1);

                if (agents.Count == 0)
                {
                    // No agents left!
                    return;
                }

                if (agentIndex >= agents.Count - 1)
                    agentIndex = 0;
                else
                    ++agentIndex;
                SwitchToAgent(agentIndex);
            }
            else if (Input.GetKeyDown("2"))
            {
                //ShowOverheadView();
            }
        }

        public void SwitchToAgent(Agent agent)
        {
            int index = agents.IndexOf(agent);
            if (index != -1)
                SwitchToAgent(index);
        }

        private void SwitchToAgent(int agentIndex)
        {
            agent = agents[agentIndex];

            // Change virtual camera to follow newly selected agent
            //vCamera.Follow = agent.transform;

            driveSliders = new List<Slider>();
            Transform driveTypeParent = transform.Find("DriveType Levels");
            foreach (Transform child in driveTypeParent)
            {
                Destroy(child.gameObject);
            }

            int i = 0;
            foreach (DriveType driveType in agent.ActiveDrives().Keys)
            {
                if (driveType.showInUI)
                {
                    RectTransform newDriveType = Instantiate(driveTypeSliderPrefab, driveTypeParent);
                    newDriveType.transform.position = new Vector3(newDriveType.transform.position.x, newDriveType.transform.position.y - 30 * i);
                    newDriveType.GetComponentInChildren<TextMeshProUGUI>().text = driveType.name;

                    driveSliders.Add(newDriveType.GetComponentInChildren<Slider>());
                    ++i;
                }
            }
        }

        private void SetMarkerAboveObject(GameObject target, TextMeshProUGUI markerText)
        {
            float offsetPosY = target.transform.position.y + markerHeight;
            Vector3 offsetPos = new Vector3(target.transform.position.x, offsetPosY, target.transform.position.z);
            Vector2 canvasPos;
            Vector2 screenPoint = Camera.main.WorldToScreenPoint(offsetPos);
            RectTransformUtility.ScreenPointToLocalPointInRectangle(GetComponent<RectTransform>(), screenPoint, null, out canvasPos);
            markerText.GetComponent<RectTransform>().localPosition = canvasPos;
        }
    }
}