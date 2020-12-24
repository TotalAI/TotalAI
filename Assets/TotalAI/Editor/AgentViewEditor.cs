using UnityEngine;
using UnityEditor;
using System.Collections.Generic;
using System;
using UnityEditor.SceneManagement;
using UnityEngine.SceneManagement;
using System.Linq;

namespace TotalAI.Editor
{
    public class AgentViewEditor : EditorWindow
    {
        private Vector2 scrollPosCenter;
        private Color defaultColor;
        private GUIStyle guiStyle;
        private GUIStyle greenStyle;
        private GUIStyle redStyle;
        private GUIStyle guiStyleBox;
        private GUIStyle subHeaderStyle;
        private GUIStyle toolBarStyle;
        private GUIStyle columnHeaderStyle;
        private int currentTab;

        private int numAgents;
        private List<Agent> agents;
        private int agentIndex;
        private Agent agent;

        private int selectedTab;
        private List<bool> entityTypeExpanded;

        private Texture logoImage;
        private Texture reloadIcon;
        private Texture downArrowIcon;
        private Texture rightArrowIcon;

        private Texture[] tabIcons;

        private bool[] toggles;

        private Vector2 scrollViewVector = Vector2.zero;
        private Vector2 scrollViewVector1 = Vector2.zero;
        private Vector2 scrollViewVector2 = Vector2.zero;
        private Vector2 scrollViewVector3 = Vector2.zero;
        private Vector2 scrollViewVector4 = Vector2.zero;
        private Vector2 scrollViewVector5 = Vector2.zero;
        private Vector2 scrollViewVector6 = Vector2.zero;
        private Vector2 scrollViewVector7 = Vector2.zero;
        private Vector2 scrollViewVector8 = Vector2.zero;
        private Vector2 scrollViewVectorMemory = Vector2.zero;
        private Vector2 scrollViewVectorInfo = Vector2.zero;
        private Vector2 mainScrollPosition = Vector2.zero;

        private Color l0 = new Color(18f / 255f, 18f / 255f, 18f / 255f);
        private Color l1 = new Color(33f / 255f, 33f / 255f, 33f / 255f);
        private Color l15 = new Color(45f / 255f, 45f / 255f, 45f / 255f);
        private Color l2 = new Color(66f / 255f, 66f / 255f, 66f / 255f);
        private Color l3 = new Color(97f / 255f, 97f / 255f, 97f / 255f);

        // New styles
        private GUIStyle textureStyle;
        private GUIStyle sectionHeaderStyle;
        private GUIStyle leftColumnHeaderStyle;
        private GUIStyle leftColumnButtonStyle;
        private GUIStyle textLinkHoverStyle;
        private GUIStyle disabledTextLinkStyle;
        private GUIStyle disabledTextLinkHoverStyle;
        private GUIStyle topTabStyle;

        private Sprite agentSprite;

        private GUIManager guiManager;
        private PlanTree planTree;

        private bool doubleClick;

        [MenuItem("Tools/Total AI/Agent View Window", false, 0)]
        static void Init()
        {
            // Get existing open window or if none, make a new one:
            AgentViewEditor window = (AgentViewEditor)GetWindow(typeof(AgentViewEditor), false, "TAI Agent View");
            window.Show();
        }

        private void OnEnable()
        {
            entityTypeExpanded = new List<bool>();
            selectedTab = 0;
            doubleClick = false;

            reloadIcon = (Texture)EditorGUIUtility.Load("Assets/TotalAI/Editor/Images/reload.png");
            logoImage = (Texture)EditorGUIUtility.Load("Assets/TotalAI/Editor/Images/TotalAILogo.png");
            downArrowIcon = (Texture)EditorGUIUtility.Load("Assets/TotalAI/Editor/Images/downarrow.png");
            rightArrowIcon = (Texture)EditorGUIUtility.Load("Assets/TotalAI/Editor/Images/rightarrow.png");

            tabIcons = new Texture[5];
            tabIcons[0] = (Texture)EditorGUIUtility.Load("Assets/TotalAI/Editor/Images/main.png");
            tabIcons[1] = (Texture)EditorGUIUtility.Load("Assets/TotalAI/Editor/Images/sensor.png");
            tabIcons[2] = (Texture)EditorGUIUtility.Load("Assets/TotalAI/Editor/Images/memory.png");
            tabIcons[3] = (Texture)EditorGUIUtility.Load("Assets/TotalAI/Editor/Images/plan.png");
            tabIcons[4] = (Texture)EditorGUIUtility.Load("Assets/TotalAI/Editor/Images/stack.png");

            agent = null;
            agents = null;
            agentIndex = 0;

            toggles = new bool[1000];
            defaultColor = GUI.color;

            Texture2D backgroundTexture = Texture2D.whiteTexture;
            textureStyle = new GUIStyle { normal = new GUIStyleState { background = backgroundTexture } };

            planTree = new PlanTree();
            planTree.Setup();
        }

        private void LoadAgentsInScene()
        {
            guiManager = FindObjectOfType<GUIManager>();

            agents = FindObjectsOfType<Agent>().ToList();
            numAgents = agents.Count;
            if (agents.Count > 0)
            {
                SortAgents();
                SetAgent(0);
            }
        }

        private void EditorLoadAgentsInScene()
        {
            agents = new List<Agent>();
            Scene scene = SceneManager.GetActiveScene();
            GameObject[] gameObjects = scene.GetRootGameObjects();
            foreach (GameObject gameObject in gameObjects)
            {
                Agent agentToAdd = gameObject.GetComponent<Agent>();
                if (agentToAdd != null)
                    agents.Add(agentToAdd);
            }
            numAgents = agents.Count;
            if (agents.Count > 0)
            {
                SortAgents();
                SetAgent(0);
            }
        }

        private void SortAgents()
        {
            agents.Sort(delegate (Agent x, Agent y)
            {
                if (x.entityType == null || y.entityType == null) return 0;
                int agentTypeOrder = x.entityType.name.CompareTo(y.entityType.name);
                if (agentTypeOrder == 0)
                    return x.name.CompareTo(y.name);
                return agentTypeOrder;
            });
        }

        private void SetAgent(int index)
        {
            if (agents[index].entityType == null)
                return;

            agent = agents[index];
            agentIndex = index;
            SpriteRenderer agentSpriteRenderer = agent.GetComponent<SpriteRenderer>();
            if (agentSpriteRenderer == null)
                agentSprite = null;
            else
                agentSprite = agentSpriteRenderer.sprite;
            if (Application.isPlaying && guiManager != null)
                guiManager.SwitchToAgent(agent);
        }

        private void DrawRect(Rect position, Color color, GUIContent content = null)
        {
            Color backgroundColor = GUI.backgroundColor;
            GUI.backgroundColor = color;
            GUI.Box(position, content ?? GUIContent.none, textureStyle);
            GUI.backgroundColor = backgroundColor;
        }

        private Vector2 DrawSection(Rect rect, Vector2 scrollViewVector, string title, string[,] data,
                                    List<UnityEngine.Object> objects, List<bool> actives,
                                    float afterFirstWidth = 35f, float firstwidth = 140f, float lineHeight = 20f,
                                    int linkedColumn = 0, int columns = 1, bool allLinked = false)
        {
            float horizOffset = rect.x;
            float vertOffset = rect.y;
            float sectionWidth = rect.width;
            float sectionHeight = rect.height;
            Vector2 mousePosition = Event.current.mousePosition;

            DrawRect(new Rect(horizOffset, vertOffset, sectionWidth, sectionHeight), l1);
            GUI.Label(new Rect(25 + horizOffset, vertOffset + 20, 200, 25), title, sectionHeaderStyle);
            
            // Figure out how big internal rect needs to be - so we can avoid scroll bars if possible
            int numRows = data.GetLength(0);
            int numCols = data.GetLength(1);
            Rect rectInteral = new Rect(0, 0, sectionWidth - 55, (numRows / columns) * lineHeight);
            scrollViewVector = GUI.BeginScrollView(new Rect(25 + horizOffset, vertOffset + 60, sectionWidth - 25, sectionHeight - 65),
                                                   scrollViewVector, rectInteral, false, false);
            float rowWidth = sectionWidth - 50;
            for (int i = 0; i < numRows; i++)
            {
                float columnX = (i % columns) * (firstwidth + afterFirstWidth * (numCols - 1) + 50);
                float y = (i / columns) * lineHeight;
                bool disabled = i < actives.Count && !actives[i];
                
                // Assuming its a header if single entry in multi-column row
                bool isHeader = numCols > 1 && data[i, 0] != null && data[i, 1] == null;
                if (!isHeader && numCols > 1)
                {
                    int col = i % columns;
                    float width = columns == 2 ? rowWidth / 2f : rowWidth;
                    Rect containsRect = new Rect(25 + horizOffset + width * col, y + vertOffset + 60, width, lineHeight);
                    if (containsRect.Contains(mousePosition))
                        DrawRect(new Rect(width * col + (col == 0 ? -5 : 10), y, width, lineHeight), l2);
                }

                for (int j = 0; j < numCols; j++)
                {
                    float width = j == 0 ? firstwidth : afterFirstWidth;
                    float horizSpacing = j == 0 ? 0 : firstwidth - afterFirstWidth + j * afterFirstWidth;
                    float x = horizSpacing + columnX;
                    if (((j == linkedColumn && i < objects.Count && objects[i] != null) ||
                        (allLinked && i * numCols + j < objects.Count && objects[i * numCols + j] != null)) &&
                        data[i, j] != "Null")
                    {
                        int linkIndex = i;
                        if (allLinked)
                            linkIndex = i * numCols + j;
                        if (!disabled)
                        {
                            if (j > 0)
                            {
                                if (GUI.Button(new Rect(x, y, width, lineHeight), data[i, j],
                                               new GUIStyle(textLinkHoverStyle) { alignment = TextAnchor.MiddleRight }))
                                    Selection.SetActiveObjectWithContext(objects[linkIndex], null);
                            }
                            else
                            {
                                if (GUI.Button(new Rect(x, y, width, lineHeight), data[i, j], textLinkHoverStyle))
                                    Selection.SetActiveObjectWithContext(objects[linkIndex], null);
                            }
                        }
                        else
                        {
                            if (j > 0)
                            {
                                if (GUI.Button(new Rect(x, y, width, lineHeight), data[i, j],
                                               new GUIStyle(disabledTextLinkHoverStyle) { alignment = TextAnchor.MiddleRight }))
                                    Selection.SetActiveObjectWithContext(objects[linkIndex], null);
                            }
                            else
                            {
                                if (GUI.Button(new Rect(x, y, width, lineHeight), data[i, j], disabledTextLinkHoverStyle))
                                    Selection.SetActiveObjectWithContext(objects[linkIndex], null);
                            }
                        }
                    }
                    else
                    {
                        if (!disabled)
                            if (j > 0)
                                GUI.Label(new Rect(x, y, width, lineHeight), data[i, j],
                                          new GUIStyle(EditorStyles.label) { alignment = TextAnchor.MiddleRight });
                            else if (isHeader)
                                GUI.Label(new Rect(x, y, width, lineHeight), data[i, j], EditorStyles.whiteLargeLabel);
                            else
                                GUI.Label(new Rect(x, y, width, lineHeight), data[i, j]);
                        else
                            if (j > 0)
                                GUI.Label(new Rect(x, y, width, lineHeight), data[i, j],
                                          new GUIStyle(disabledTextLinkStyle) { alignment = TextAnchor.MiddleRight });
                            else
                                GUI.Label(new Rect(x, y, width, lineHeight), data[i, j], disabledTextLinkStyle);
                    }
                }
                
            }
            if (numRows == 0)
                GUI.Label(new Rect(0, 0, 140, 20), "None");

            GUI.EndScrollView();
            if (columns == 2 && numRows > 1)
                DrawRect(new Rect(horizOffset + sectionWidth / 2f - 1, vertOffset + 65, 2, sectionHeight - 95), l2);
            return scrollViewVector;
        }
        
        private void DrawTopMenuBar()
        {
            Texture2D assetPreview = null;
            if (agentSprite != null)
                assetPreview = AssetPreview.GetAssetPreview(agentSprite);
            else if (agent != null)
                assetPreview = AssetPreview.GetAssetPreview(agent.gameObject);
            if (assetPreview != null && GUI.Button(new Rect(225, 10, 55, 55), assetPreview, EditorStyles.label))
            {
                Selection.SetActiveObjectWithContext(agent.gameObject, null);
                SceneView.FrameLastActiveSceneView();
            }

            //GUI.DrawTextureWithTexCoords(new Rect(215, 20, 150, 40), agentSprite.texture, agentSprite.textureRect);
            if (agent != null && GUI.Button(new Rect(290, 25, 160, 40), agent.name, leftColumnHeaderStyle))
                Selection.SetActiveObjectWithContext(agent.gameObject, null);

            Vector2 mousePosition = Event.current.mousePosition;
            string[] tabNames = new string[] { "Main", "Sensors", "Memory", "Plans", "History" };
            float[] spacing = new float[] { 0, 150, 300, 430, 570 };
            float[] lineSpacing = new float[] { -2, 120, 268, 425, 547 };
            float[] width = new float[] { 60, 85, 85, 60, 70 };
            float[] lineWidth = new float[] { 60, 85, 85, 60, 75 };
            float[] containsWidth = new float[] { 90, 90, 90, 90, 90 };
            float[] iconSpacing = new float[] { 60, 85, 85, 60, 73 };

            for (int i = 0; i < tabNames.Length; i++)
            {
                Rect containsRect = new Rect(450 + spacing[i] + 10, 20, containsWidth[i], 40);
                if (containsRect.Contains(mousePosition) && selectedTab == i)
                    DrawRect(new Rect(500 + lineSpacing[i], 60, lineWidth[i], 3), new Color(253f / 255f, 126f / 255f, 20f / 255f, .7f));
                else if (containsRect.Contains(mousePosition))
                    DrawRect(new Rect(500 + lineSpacing[i], 60, lineWidth[i], 3), new Color(253f / 255f, 126f / 255f, 20f / 255f, .6f));
                else if (selectedTab == i)
                    DrawRect(new Rect(500 + lineSpacing[i], 60, lineWidth[i], 3), new Color(253f / 255f, 126f / 255f, 20f / 255f, .6f));

                GUI.Button(new Rect(450 + spacing[i] + 10 - iconSpacing[i], 28, containsWidth[i], 22), tabIcons[i], topTabStyle);

                if (GUI.Button(containsRect, tabNames[i], topTabStyle))
                {
                    // Change tabs
                    selectedTab = i;
                }
            }
        }

        private void DrawPlayerMainTab()
        {
            string[,] data;
            List<bool> actives;
            List<UnityEngine.Object> objects;
            float xPos;
            float yPos;


            Rect rectInteral = new Rect(215, 75, 1000, 850);
            mainScrollPosition = GUI.BeginScrollView(new Rect(215, 75, Screen.width - 215, Screen.height - 100),
                                                     mainScrollPosition, rectInteral, false, false);

            xPos = 215;
            yPos = 75;
            data = GetAgentsPlayerOverviewData(agent, out objects, out actives);
            if (data != null)
                scrollViewVector1 = DrawSection(new Rect(xPos, yPos, 775, 165), scrollViewVector1, "Overview", data, objects, actives,
                                                140f, 200f, 20f, 1, 2);

            xPos = 215;
            yPos += 180;
            data = GetAgentsActionsData(agent, out objects, out actives);
            if (data != null)
                scrollViewVector1 = DrawSection(new Rect(xPos, yPos, 305, 315), scrollViewVector1, "Actions", data, objects, actives);

            xPos += 320;
            data = GetAgentsDrivesData(agent, out objects, out actives);
            if (data != null)
                scrollViewVector2 = DrawSection(new Rect(xPos, yPos, 220, 315), scrollViewVector2, "Drives", data, objects, actives, 30f, 120f);

            xPos += 235;
            data = GetAgentsRolesData(agent, out objects, out actives);
            if (data != null)
                scrollViewVector3 = DrawSection(new Rect(xPos, yPos, 220, 150), scrollViewVector3, "Roles", data, objects, actives);

            yPos += 165;
            data = GetAgentsTagsData(agent, out objects, out actives);
            if (data != null)
                scrollViewVector8 = DrawSection(new Rect(xPos, yPos, 220, 150), scrollViewVector8, "Tags", data, objects, actives, 90f, 80f);

            xPos = 215;
            yPos += 165;
            data = GetAgentsAttributesData(agent, out objects, out actives);
            if (data != null)
                scrollViewVector4 = DrawSection(new Rect(xPos, yPos, 305, 315), scrollViewVector4, "Attributes", data, objects, actives);

            xPos += 320;
            data = GetAgentsInventoryData(agent, out objects, out actives);
            if (data != null)
                scrollViewVector5 = DrawSection(new Rect(xPos, yPos, 455, 315), scrollViewVector5,
                                                "Inventory", data, objects, actives, 90f, 125f);

            xPos = 215 + 320 + 240 + 230;
            yPos = 75;
            data = GetAgentsMappingTypesData(agent, out objects, out actives);
            if (data != null)
                scrollViewVector6 = DrawSection(new Rect(xPos, yPos, 200, 830), scrollViewVector6, "Mapping Types", data, objects, actives, 200f);

            GUI.EndScrollView();
        }

        private void DrawEditorMainTab()
        {
            string[,] data;
            List<bool> actives;
            List <UnityEngine.Object> objects;
            float xPos;
            float yPos;

            Rect rectInteral = new Rect(215, 75, 1000, 850);
            mainScrollPosition = GUI.BeginScrollView(new Rect(215, 75, Screen.width - 215, Screen.height - 100),
                                                     mainScrollPosition, rectInteral, false, false);

            xPos = 215;
            yPos = 75;
            data = GetAgentsInfoData(agent, out objects, out actives);
            if (data != null)
                scrollViewVectorInfo = DrawSection(new Rect(xPos, yPos, 220, 315), scrollViewVectorInfo, "Info", data, objects, actives);

            xPos += 235;
            data = GetAgentsDrivesData(agent, out objects, out actives);
            if (data != null)
                scrollViewVector2 = DrawSection(new Rect(xPos, yPos, 220, 315), scrollViewVector2, "Drives", data, objects, actives, 30f, 110f);

            xPos += 235;
            data = GetAgentsActionsData(agent, out objects, out actives);
            if (data != null)
                scrollViewVector1 = DrawSection(new Rect(xPos, yPos, 305, 315), scrollViewVector1, "Actions", data, objects, actives);

            xPos += 320;
            data = GetAgentsRolesData(agent, out objects, out actives);
            if (data != null)
                scrollViewVector3 = DrawSection(new Rect(xPos, yPos, 220, 150), scrollViewVector3, "Roles", data, objects, actives);

            yPos += 165;
            data = GetAgentsTagsData(agent, out objects, out actives);
            if (data != null)
                scrollViewVector8 = DrawSection(new Rect(xPos, yPos, 220, 150), scrollViewVector8, "Tags", data, objects, actives, 90f, 80f);

            xPos = 215;
            yPos = 75 + 330;
            data = GetAgentsAttributesData(agent, out objects, out actives);
            if (data != null)
                scrollViewVector4 = DrawSection(new Rect(xPos, yPos, 305, 315), scrollViewVector4, "Attributes", data, objects, actives);

            xPos += 320;
            data = GetAgentsInventoryData(agent, out objects, out actives);
            if (data != null)
                scrollViewVector5 = DrawSection(new Rect(xPos, yPos, 455, 315), scrollViewVector5,
                                                "Starting Inventory", data, objects, actives, 90f, 125f, 20f, 0, 1, true);

            xPos = 215 + 320 + 240 + 230;
            yPos = 75 + 330;
            data = GetAgentsMappingTypesData(agent, out objects, out actives);
            if (data != null)
                scrollViewVector6 = DrawSection(new Rect(xPos, yPos, 220, 500), scrollViewVector6, "Mapping Types", data, objects, actives, 200f);

            xPos = 215;
            yPos = 75 + 330 + 330;
            data = GetAgentsCoreTypesData(agent, out objects, out actives);
            if (data != null)
                scrollViewVector7 = DrawSection(new Rect(xPos, yPos, 775, 170), scrollViewVector7, "Core Types", data, objects, actives,
                                                140f, 200f, 20f, 1, 2);

            GUI.EndScrollView();
        }

        private void DrawPlayerSensorTab()
        {
            string[,] data;
            List<bool> actives;
            List<UnityEngine.Object> objects;
            float xPos;
            float yPos;

            xPos = 215;
            yPos = 75;

            // Figure out how to lay out Sections based on number of Sensors
            int numSensors = agent.sensorTypes.Count;
            for (int i = 0; i < numSensors; i++)
            {

                data = GetAgentsOneSensorData(agent, agent.sensorTypes[i],  out objects, out actives);
                if (data != null)
                    scrollViewVector1 = DrawSection(new Rect(xPos, yPos, 385, 385), scrollViewVector1, agent.sensorTypes[i].name, data, objects, actives,
                                                    120f, 80f);
                if (i % 2 != 0)
                {
                    yPos += 400;
                    xPos = 215;
                }
                else
                {
                    xPos += 400;
                }
            }
        }

        private void DrawEditorSensorTab()
        {
            string[,] data;
            List<bool> actives;
            List<UnityEngine.Object> objects;
            float xPos;
            float yPos;

            xPos = 215;
            yPos = 75;
            data = GetAgentsSensorsData(agent, out objects, out actives);
            if (data != null)
                scrollViewVector1 = DrawSection(new Rect(xPos, yPos, 300, 315), scrollViewVector1, "Sensors", data, objects, actives,
                                                44f, 200f);
        }

        private void DrawMemoryTab()
        {
            string[,] data;
            List<bool> actives;
            List<UnityEngine.Object> objects;
            float xPos;
            float yPos;

            xPos = 215;
            yPos = 75;

            if (!Application.isPlaying)
            {
                data = GetAgentsMemoryData(agent, out objects, out actives, true);
                if (data != null)
                    scrollViewVectorMemory = DrawSection(new Rect(xPos, yPos, 300, 315), scrollViewVectorMemory, "Memory Settings", data, objects, actives,
                                                         44f, 200f);
            }
            else
            {
                data = GetAgentsMemoryData(agent, out objects, out actives, false);
                if (data != null)
                    scrollViewVectorMemory = DrawSection(new Rect(xPos, yPos, 800, 800), scrollViewVectorMemory, "Memory", data, objects, actives,
                                                         90f, 200f);
            }
        }

        private void DrawPlansTab()
        {
            if (!Application.isPlaying || agent == null || agent.historyType == null)
            {
                GUI.Label(new Rect(215, 75, 300, 40), "Press Play to start creating Plan Trees.", EditorStyles.whiteLargeLabel);
                return;
            }

            GUI.BeginGroup(new Rect(215, 75, position.width - 230, position.height - 95), new GUIStyle("helpbox"));
            planTree.DrawOnGUI(new Rect(0, 0, position.width - 230, position.height - 95), agent, agent.totalAIManager);
            GUI.EndGroup();
        }

        private void DrawLeftColumn()
        {
            // #243B53
            //Color selectedColor = new Color(16f / 255f, 42f / 255f, 67f / 255f);
            Color selectedColor = new Color(253f / 255f, 126f / 255f, 20f / 255f, .4f);
            // #102A43
            //Color selectedHoverColor = new Color(20f / 255f, 52f / 255f, 80f / 255f);
            Color selectedHoverColor = new Color(253f / 255f, 126f / 255f, 20f / 255f, .45f);
            // #334E68
            //Color hoverColor = new Color(51f / 255f, 79f / 255f, 104f / 255f);
            Color hoverColor = new Color(253f / 255f, 126f / 255f, 20f / 255f, .1f);
            Color repeatButtonColor = new Color(253f / 255f, 126f / 255f, 20f / 255f, .05f);
            Vector2 mousePosition = Event.current.mousePosition;

            int clickCount = 0;
            if (Event.current.type == EventType.MouseDown)
            {
                clickCount = Event.current.clickCount;
                if (clickCount > 1)
                    doubleClick = true;
                else
                    doubleClick = false;
            }

            // Full Window tint to dark grey
            DrawRect(new Rect(0, 0, Screen.width, Screen.height), l0);

            // Left Column Agent Select
            DrawRect(new Rect(0, 0, 200, Screen.height), l1);
            //GUI.Label(new Rect(20, 20, 150, 40), "Agents", leftColumnHeaderStyle);

            GUI.Label(new Rect(35, 10, 150, 60), logoImage);

            if (!Application.isPlaying && agent != null && agent.actions == null)
            {
                agent.ResetAgent(true);
            }
            float lineHeight = 40f;
            Rect rectInteral = new Rect(0, 0, 200, Screen.height - 200);
            scrollViewVector = GUI.BeginScrollView(new Rect(0, 75, 200, Screen.height - 30), scrollViewVector, rectInteral, false, false);
            EntityType currentEntityType = null;
            int numEntityTypes = 0;
            if (numEntityTypes >= entityTypeExpanded.Count)
                entityTypeExpanded.Add(true);
            int row = 0;
            for (int i = 0; i < agents.Count; i++)
            {
                if (agents[i] == null)
                {
                    // Agent was deleted in scene
                    agents.RemoveAt(i);
                    break;
                }

                if (currentEntityType != agents[i].entityType)
                {
                    if (GUI.Button(new Rect(20, (row + numEntityTypes) * lineHeight, 150, 40),
                                   agents[i].entityType == null ? "None" : agents[i].entityType.name, sectionHeaderStyle))
                        Selection.SetActiveObjectWithContext(agents[i].entityType, null);

                    Texture arrow;
                    if (entityTypeExpanded[numEntityTypes])
                        arrow = downArrowIcon;
                    else
                        arrow = rightArrowIcon;
                    if (GUI.Button(new Rect(170, (row + numEntityTypes) * lineHeight + 13, 20, 16), arrow, new GUIStyle(EditorStyles.label)))
                    {
                        entityTypeExpanded[numEntityTypes] = !entityTypeExpanded[numEntityTypes];
                    }
                    currentEntityType = agents[i].entityType;
                    ++numEntityTypes;
                }

                if (numEntityTypes >= entityTypeExpanded.Count)
                    entityTypeExpanded.Add(true);

                if (numEntityTypes > 0 && !entityTypeExpanded[numEntityTypes - 1])
                {
                    // Skipping it
                    continue;
                }

                Rect rect = new Rect(0, (row + numEntityTypes) * lineHeight, 200, 40);
                Rect containsRect = new Rect(0, 75 + (row + numEntityTypes) * lineHeight, 200, 40);
                bool containsMouse = containsRect.Contains(mousePosition);
                if (agentIndex == i && containsMouse)
                    DrawRect(rect, selectedHoverColor);
                else if (agentIndex == i)
                    DrawRect(rect, selectedColor);
                else if (containsMouse)
                    DrawRect(rect, hoverColor);

                if (GUI.RepeatButton(rect, "", leftColumnButtonStyle))
                {
                    DrawRect(rect, repeatButtonColor);
                    if (agentIndex != i)
                    {
                        SetAgent(i);
                    }
                    if (!Application.isPlaying && agent != null)
                        agent.ResetAgent(true);
                    Selection.SetActiveObjectWithContext(agent.gameObject, null);
                    if (doubleClick)
                    {
                        SceneView.FrameLastActiveSceneView();
                        doubleClick = false;
                    }
                }

                GUI.Label(rect, agents[i].name, leftColumnButtonStyle);

                if (!Application.isPlaying && agentIndex == i)
                    GUI.Button(new Rect(143, (row + numEntityTypes) * lineHeight + 12, 200, 16), reloadIcon, leftColumnButtonStyle);

                ++row;
            }
            GUI.EndScrollView();
        }

        private void OnGUI()
        {
            
            // TODO: Option to toggle sprite update - also handle 3D models
            if (Application.isPlaying && agent != null && agent.totalAIManager.settings.for2D)
                agentSprite = agent.GetComponentInChildren<SpriteRenderer>().sprite;

            // TODO: Need a way to detect adding and removing agents
            if (agents == null || agents.Count == 0 || agents[agentIndex] == null)
            {
                // Agent was destroyed - reload them
                if (Application.isPlaying)
                {
                    LoadAgentsInScene();
                }
                else
                {
                    EditorLoadAgentsInScene();
                }
            }
            
            sectionHeaderStyle = new GUIStyle(EditorStyles.boldLabel)
            {
                fontSize = 20,
                normal = new GUIStyleState()
                {
                    textColor = new Color(1f, 1f, 1f, .6f)
                }
            };
            leftColumnHeaderStyle = new GUIStyle(EditorStyles.boldLabel)
            {
                fontSize = 22,
                normal = new GUIStyleState()
                {
                    textColor = new Color(1f, 1f, 1f, .7f)
                }
            };
            leftColumnButtonStyle = new GUIStyle(GUI.skin.label)
            {
                fontSize = 18,
                alignment = TextAnchor.MiddleLeft,
                padding = new RectOffset(30, 0, 0, 0),
                normal = new GUIStyleState()
                {
                    textColor = new Color(1f, 1f, 1f, .7f)
                }
            };
            textLinkHoverStyle = new GUIStyle(EditorStyles.label)
            {
                hover = new GUIStyleState()
                {
                    textColor = new Color(253f / 255f, 126f / 255f, 20f / 255f)
                }
            };
            disabledTextLinkStyle = new GUIStyle(EditorStyles.label)
            {
                normal = new GUIStyleState()
                {
                    textColor = new Color(1f, 1f, 1f, .25f)
                }
            };
            disabledTextLinkHoverStyle = new GUIStyle(EditorStyles.label)
            {
                normal = new GUIStyleState()
                {
                    textColor = new Color(1f, 1f, 1f, .25f)
                },
                hover = new GUIStyleState()
                {
                    textColor = new Color(253f / 255f, 126f / 255f, 20f / 255f, .25f)
                }
            };
            topTabStyle = new GUIStyle(EditorStyles.label)
            {
                fontSize = 20,
                alignment = TextAnchor.MiddleRight,
                normal = new GUIStyleState()
                {
                    textColor = new Color(1f, 1f, 1f, .7f)
                },
            };

            DrawLeftColumn();
            DrawTopMenuBar();

            if (agent != null && Application.isPlaying)
            {
                switch (selectedTab)
                {
                    case 0:
                        DrawPlayerMainTab();
                        break;
                    case 1:
                        DrawPlayerSensorTab();
                        break;
                    case 2:
                        DrawMemoryTab();
                        break;
                    case 3:
                        DrawPlansTab();
                        break;
                    case 4:
                        DrawHistoryTab();
                        break;
                    default:
                        break;
                }
            }
            else if (agent != null)
            {
                switch (selectedTab)
                {
                    case 0:
                        DrawEditorMainTab();
                        break;
                    case 1:
                        DrawEditorSensorTab();
                        break;
                    case 2:
                        DrawMemoryTab();
                        break;
                    case 3:
                        DrawPlansTab();
                        break;
                    case 4:
                        DrawHistoryTab();
                        break;
                    default:
                        break;
                }
            }

            Repaint();
        }

        private void DrawHistoryTab()
        {
            if (!Application.isPlaying || agent == null || agent.historyType == null)
            {
                GUI.Label(new Rect(215, 75, 300, 40), "Press Play to start creating Agent Histories.", EditorStyles.whiteLargeLabel);
                return;
            }

            Rect top = new Rect(215, 75, position.width - 215, position.height - 300);
            Rect bottom = new Rect(215, position.height - 300 + 75, position.width - 215, 225);

            columnHeaderStyle = new GUIStyle(EditorStyles.whiteLargeLabel)
            {
                fontSize = 13,
                margin = new RectOffset(0, 0, 20, 10)
            };
            toolBarStyle = new GUIStyle("button")
            {
                fontSize = 18,
                padding = new RectOffset(0, 0, 5, 5)
            };
            guiStyle = new GUIStyle("label")
            {
                richText = true,
                wordWrap = true
            };
            greenStyle = new GUIStyle("label")
            {
                normal = new GUIStyleState()
                {
                    textColor = new Color(0f, .5f, 0f)
                }
            };
            redStyle = new GUIStyle("label")
            {
                normal = new GUIStyleState()
                {
                    textColor = new Color(.5f, 0f, 0f)
                }
            };

            int numWidth = 45;
            int lineHeight = 35;
            int columnSpacing = 120;
            int selectedLine = -1;

            currentTab = GUI.Toolbar(new Rect(top.x, top.y + 10, top.width - 125, 30), currentTab,
                                     new string[] { "Decider", "Behavior", "Plans", "OutputChanges" }, toolBarStyle);

            switch (currentTab)
            {
                case 0:
                    GUI.Label(new Rect(top.x + numWidth, top.y + 60, top.width, lineHeight), "Time", columnHeaderStyle);
                    GUI.Label(new Rect(top.x + numWidth + columnSpacing * 1, top.y + 60, top.width, lineHeight), "Type", columnHeaderStyle);
                    GUI.Label(new Rect(top.x + numWidth + columnSpacing * 2, top.y + 60, top.width, lineHeight), "MappingType", columnHeaderStyle);
                    GUI.Label(new Rect(top.x + numWidth + columnSpacing * 3, top.y + 60, top.width, lineHeight), "Interrupt From Decider", columnHeaderStyle);
                    break;
                case 1:
                    GUI.Label(new Rect(top.x + numWidth, top.y + 60, top.width, lineHeight), "Time", columnHeaderStyle);
                    GUI.Label(new Rect(top.x + numWidth + columnSpacing * 1, top.y + 60, top.width, lineHeight), "Type", columnHeaderStyle);
                    GUI.Label(new Rect(top.x + numWidth + columnSpacing * 2, top.y + 60, top.width, lineHeight), "BehaviorType", columnHeaderStyle);
                    GUI.Label(new Rect(top.x + numWidth + columnSpacing * 3, top.y + 60, top.width, lineHeight), "Interrupt From Behavior", columnHeaderStyle);
                    break;
                case 2:
                    GUI.Label(new Rect(top.x + numWidth, top.y + 60, top.width, lineHeight), "Time", columnHeaderStyle);
                    GUI.Label(new Rect(top.x + numWidth + columnSpacing * 1, top.y + 60, top.width, lineHeight), "DriveType", columnHeaderStyle);
                    GUI.Label(new Rect(top.x + numWidth + columnSpacing * 2, top.y + 60, top.width, lineHeight), "Amt", columnHeaderStyle);
                    GUI.Label(new Rect(top.x + numWidth + columnSpacing * 2.5f, top.y + 60, top.width, lineHeight), "Mapping", columnHeaderStyle);
                    GUI.Label(new Rect(top.x + numWidth + columnSpacing * 3.5f, top.y + 60, top.width, lineHeight), "# Map", columnHeaderStyle);
                    GUI.Label(new Rect(top.x + numWidth + columnSpacing * 4, top.y + 60, top.width, lineHeight), "# Lvs", columnHeaderStyle);
                    GUI.Label(new Rect(top.x + numWidth + columnSpacing * 4.5f, top.y + 60, top.width, lineHeight), "Status", columnHeaderStyle);
                    GUI.Label(new Rect(top.x + numWidth + columnSpacing * 5.5f, top.y + 60, top.width, lineHeight), "Utility", columnHeaderStyle);
                    GUI.Label(new Rect(top.x + numWidth + columnSpacing * 6f, top.y + 60, top.width, lineHeight), "SE", columnHeaderStyle);
                    GUI.Label(new Rect(top.x + numWidth + columnSpacing * 6.5f, top.y + 60, top.width, lineHeight), "Est(s)", columnHeaderStyle);
                    GUI.Label(new Rect(top.x + numWidth + columnSpacing * 7, top.y + 60, top.width, lineHeight), "Act(s)", columnHeaderStyle);
                    GUI.Label(new Rect(top.x + numWidth + columnSpacing * 7.5f, top.y + 60, top.width, lineHeight), "Plan(s)", columnHeaderStyle);
                    break;
                case 3:
                    GUI.Label(new Rect(top.x + numWidth, top.y + 60, top.width, lineHeight), "Time", columnHeaderStyle);
                    GUI.Label(new Rect(top.x + numWidth + columnSpacing * 1, top.y + 60, top.width, lineHeight), "OCT", columnHeaderStyle);
                    GUI.Label(new Rect(top.x + numWidth + columnSpacing * 3, top.y + 60, top.width, lineHeight), "To", columnHeaderStyle);
                    GUI.Label(new Rect(top.x + numWidth + columnSpacing * 4, top.y + 60, top.width, lineHeight), "Timing", columnHeaderStyle);
                    GUI.Label(new Rect(top.x + numWidth + columnSpacing * 5, top.y + 60, top.width, lineHeight), "IOT Type", columnHeaderStyle);
                    GUI.Label(new Rect(top.x + numWidth + columnSpacing * 6, top.y + 60, top.width, lineHeight), "IOT", columnHeaderStyle);
                    GUI.Label(new Rect(top.x + numWidth + columnSpacing * 7, top.y + 60, top.width, lineHeight), "Value", columnHeaderStyle);
                    break;
            }

            GUILayout.BeginArea(new Rect(top.x, top.y + 80, top.width, top.height - 80));
            scrollPosCenter = EditorGUILayout.BeginScrollView(scrollPosCenter);
            GUILayout.Space(20);

            lineHeight = 20;
            if (currentTab == 0)
            {
                List<HistoryType.DeciderLog> deciderLogs = agent.historyType.agentsDeciderLogs[agent];

                for (int i = deciderLogs.Count - 1; i >= 0; i--)
                {
                    int row = deciderLogs.Count - i - 1;

                    GUILayout.Space(lineHeight);
                    EditorGUI.BeginChangeCheck();
                    toggles[i] = GUI.Toggle(new Rect(0, lineHeight * row, top.width, lineHeight), toggles[i], GUIContent.none);
                    if (EditorGUI.EndChangeCheck())
                    {
                        for (int j = deciderLogs.Count - 1; j >= 0; j--)
                        {
                            if (j != i)
                                toggles[j] = false;
                        }
                    }

                    if (toggles[i])
                        selectedLine = i;

                    GUI.Label(new Rect(numWidth - 25, lineHeight * row, top.width, lineHeight), (i + 1) + ".");
                    GUI.Label(new Rect(numWidth, lineHeight * row, top.width, lineHeight),
                              Math.Round(deciderLogs[i].time, 2).ToString());
                    GUI.Label(new Rect(numWidth + columnSpacing * 1, lineHeight * row, top.width, lineHeight),
                              deciderLogs[i].runType.ToString());
                    GUI.Label(new Rect(numWidth + columnSpacing * 2, lineHeight * row, top.width, lineHeight),
                              deciderLogs[i].currentMapping.mappingType.name);
                    GUI.Label(new Rect(numWidth + columnSpacing * 3, lineHeight * row, top.width, lineHeight),
                              deciderLogs[i].interruptFromDecider.ToString());
                }
            }
            else if (currentTab == 1)
            {
                List<HistoryType.BehaviorLog> behaviorLogs = agent.historyType.agentsBehaviorLogs[agent];

                for (int i = behaviorLogs.Count - 1; i >= 0; i--)
                {
                    HistoryType.BehaviorLog behaviorLog = behaviorLogs[i];
                    int row = behaviorLogs.Count - i - 1;

                    GUILayout.Space(lineHeight);
                    EditorGUI.BeginChangeCheck();
                    toggles[i] = GUI.Toggle(new Rect(0, lineHeight * row, top.width, lineHeight), toggles[i], GUIContent.none);
                    if (EditorGUI.EndChangeCheck())
                    {
                        for (int j = behaviorLogs.Count - 1; j >= 0; j--)
                        {
                            if (j != i)
                                toggles[j] = false;
                        }
                    }

                    if (toggles[i])
                        selectedLine = i;

                    GUI.Label(new Rect(numWidth - 25, lineHeight * row, top.width, lineHeight), (i + 1) + ".");
                    GUI.Label(new Rect(numWidth, lineHeight * row, top.width, lineHeight),
                              Math.Round(behaviorLog.time, 2).ToString());
                    GUI.Label(new Rect(numWidth + columnSpacing * 1, lineHeight * row, top.width, lineHeight),
                              behaviorLog.runType.ToString());
                    GUI.Label(new Rect(numWidth + columnSpacing * 2, lineHeight * row, top.width, lineHeight),
                              behaviorLog.behaviorType != null ? behaviorLog.behaviorType.name : "None");
                    GUI.Label(new Rect(numWidth + columnSpacing * 3, lineHeight * row, top.width, lineHeight),
                              behaviorLog.interruptFromBehavior.ToString());
                }
            }
            else if (currentTab == 2)
            {
                List<HistoryType.PlansLog> plansLogs = agent.historyType.agentsPlansLogs[agent];

                for (int i = plansLogs.Count - 1; i >= 0; i--)
                {
                    HistoryType.PlansLog plansLog = plansLogs[i];
                    int row = plansLogs.Count - i - 1;

                    GUILayout.Space(lineHeight);
                    EditorGUI.BeginChangeCheck();
                    toggles[i] = GUI.Toggle(new Rect(0, lineHeight * row, top.width, lineHeight), toggles[i], GUIContent.none);
                    if (EditorGUI.EndChangeCheck())
                    {
                        for (int j = plansLogs.Count - 1; j >= 0; j--)
                        {
                            if (j != i)
                                toggles[j] = false;
                        }
                    }

                    if (toggles[i])
                        selectedLine = i;

                    GUI.Label(new Rect(numWidth - 25, lineHeight * row, top.width, lineHeight), (i + 1) + ".");
                    GUI.Label(new Rect(numWidth, lineHeight * row, top.width, lineHeight),
                              Math.Round(plansLog.time, 2).ToString());
                    GUI.Label(new Rect(numWidth + columnSpacing * 1, lineHeight * row, top.width, lineHeight),
                              plansLog.chosenDriveType.name);
                    GUI.Label(new Rect(numWidth + columnSpacing * 2, lineHeight * row, top.width, lineHeight),
                              Math.Round(plansLog.allPlans[plansLog.chosenDriveType].driveAmountEstimates[plansLog.chosenPlanIndex], 2).ToString());
                    GUI.Label(new Rect(numWidth + columnSpacing * 2.5f, lineHeight * row, top.width, lineHeight),
                              plansLog.allPlans[plansLog.chosenDriveType].rootMappings[plansLog.chosenPlanIndex].ToString());
                    GUI.Label(new Rect(numWidth + columnSpacing * 3.5f, lineHeight * row, top.width, lineHeight),
                              plansLog.allPlans[plansLog.chosenDriveType].rootMappings[plansLog.chosenPlanIndex].NumberMappings().ToString());
                    GUI.Label(new Rect(numWidth + columnSpacing * 4f, lineHeight * row, top.width, lineHeight),
                              plansLog.allPlans[plansLog.chosenDriveType].rootMappings[plansLog.chosenPlanIndex].NumberLeaves().ToString());
                    GUI.Label(new Rect(numWidth + columnSpacing * 4.5f, lineHeight * row, top.width, lineHeight),
                              plansLog.allPlans[plansLog.chosenDriveType].statuses[plansLog.chosenPlanIndex].ToString());

                    GUI.Label(new Rect(numWidth + columnSpacing * 5.5f, lineHeight * row, top.width, lineHeight),
                              Math.Round(plansLog.allPlans[plansLog.chosenDriveType].utility[plansLog.chosenPlanIndex], 2).ToString());
                    GUI.Label(new Rect(numWidth + columnSpacing * 6f, lineHeight * row, top.width, lineHeight),
                              plansLog.allPlans[plansLog.chosenDriveType].sideEffectsUtility[plansLog.chosenPlanIndex].ToString());
                    GUI.Label(new Rect(numWidth + columnSpacing * 6.5f, lineHeight * row, top.width, lineHeight),
                              plansLog.allPlans[plansLog.chosenDriveType].timeEstimates[plansLog.chosenPlanIndex].ToString());

                    float actualTime = agent.historyType.ActualPlanRunningTime(agent, plansLog);
                    GUI.Label(new Rect(numWidth + columnSpacing * 7, lineHeight * row, top.width, lineHeight),
                              Math.Round(actualTime, 2).ToString());
                    GUI.Label(new Rect(numWidth + columnSpacing * 7.5f, lineHeight * row, top.width, lineHeight),
                              plansLog.timeToPlan.ToString());
                }
            }
            else if (currentTab == 3)
            {
                List<HistoryType.OutputChangeLog> outputChangeLogs = agent.historyType.agentsOutputChangeLogs[agent];

                for (int i = outputChangeLogs.Count - 1; i >= 0; i--)
                {
                    HistoryType.OutputChangeLog outputChangeLog = outputChangeLogs[i];
                    int row = outputChangeLogs.Count - i - 1;

                    GUILayout.Space(lineHeight);
                    EditorGUI.BeginChangeCheck();
                    toggles[i] = GUI.Toggle(new Rect(0, lineHeight * row, top.width, lineHeight), toggles[i], GUIContent.none);
                    if (EditorGUI.EndChangeCheck())
                    {
                        for (int j = outputChangeLogs.Count - 1; j >= 0; j--)
                        {
                            if (j != i)
                                toggles[j] = false;
                        }
                    }

                    if (toggles[i])
                        selectedLine = i;
                    GUIStyle style = guiStyle;
                    if (!outputChangeLog.succeeded)
                        style = redStyle;

                    GUI.Label(new Rect(numWidth - 25, lineHeight * row, top.width, lineHeight), (i + 1) + ".");
                    GUI.Label(new Rect(numWidth, lineHeight * row, top.width, lineHeight),
                              Math.Round(outputChangeLog.time, 2).ToString(), style);
                    GUI.Label(new Rect(numWidth + columnSpacing * 1, lineHeight * row, top.width, lineHeight),
                              outputChangeLog.outputChange.outputChangeType.name, style);
                    GUI.Label(new Rect(numWidth + columnSpacing * 3, lineHeight * row, top.width, lineHeight),
                               outputChangeLog.outputChange.targetType.ToString(), style);
                    GUI.Label(new Rect(numWidth + columnSpacing * 4, lineHeight * row, top.width, lineHeight),
                               outputChangeLog.outputChange.timing.ToString(), style);
                    string typeName = "None";
                    string iotName = "None";
                    if (outputChangeLog.outputChange.levelType != null)
                    {
                        typeName = outputChangeLog.outputChange.levelType.GetType().Name;
                        iotName = outputChangeLog.outputChange.levelType.name;
                    }
                    GUI.Label(new Rect(numWidth + columnSpacing * 5, lineHeight * row, top.width, lineHeight), typeName, style);
                    GUI.Label(new Rect(numWidth + columnSpacing * 6, lineHeight * row, top.width, lineHeight), iotName, style);
                    GUI.Label(new Rect(numWidth + columnSpacing * 7, lineHeight * row, top.width, lineHeight),
                               outputChangeLog.outputChange.GetValueAsString(), style);

                }
            }
            
            EditorGUILayout.EndScrollView();
            GUILayout.EndArea();
            if (selectedLine == -1)
            {
                GUI.Label(new Rect(bottom.x, bottom.y + 10, bottom.width, 25), "Please check a box above to see more detailed info.", EditorStyles.whiteLargeLabel);
            }
            else if (currentTab == 0)
            {
                HistoryType.DeciderLog deciderLog = agent.historyType.agentsDeciderLogs[agent][selectedLine];
                float time = deciderLog.time;
                Mapping mapping = deciderLog.currentMapping;
                
                // Grab the PlanLog with same mapping as this deciderLog
                HistoryType.PlansLog plansLog = agent.historyType.FindPlansLogFromMapping(agent, mapping);
                if (plansLog == null)
                    return;

                Mapping rootMapping = plansLog.allPlans[plansLog.chosenDriveType].rootMappings[plansLog.chosenPlanIndex];
                float actualTime = agent.historyType.ActualPlanRunningTime(agent, plansLog);

                string heading;
                if (actualTime == -1f)
                    heading = (selectedLine + 1) + ". " + rootMapping.mappingType.name + ": " + Math.Round(plansLog.time, 2) + "s -> Currently Running";
                else
                    heading = (selectedLine + 1) + ". " + rootMapping.mappingType.name + ": " + Math.Round(plansLog.time, 2) +
                              "s -> " + Math.Round(plansLog.time + actualTime, 2) + "s (" + Math.Round(actualTime, 2) + "s)";
                GUI.Label(new Rect(bottom.x, bottom.y + 10, bottom.width, 20), heading, columnHeaderStyle);

                GUI.Label(new Rect(bottom.x, bottom.y + 40, bottom.width, 20), "Other Plans - Utility (Drive Amount, Time Est, Side-Effects)", columnHeaderStyle);
                GUI.Label(new Rect(bottom.x, bottom.y + 60, bottom.width - 300, 60), plansLog.DisplayAllPlansStats(agent),
                          new GUIStyle("label") { richText = true, wordWrap = true, alignment = TextAnchor.UpperLeft });

                GUI.Label(new Rect(bottom.x, bottom.y + 130, bottom.width, 20), "Drives Ranked - Utility", columnHeaderStyle);
                GUI.Label(new Rect(bottom.x, bottom.y + 150, bottom.width, 40), plansLog.DisplayDriveTypesRanked(),
                          new GUIStyle("label") { richText = true, wordWrap = true, alignment = TextAnchor.UpperLeft });
            }
            else if (currentTab == 1)
            {

            }
            else if (currentTab == 2)
            {

            }
            else if (currentTab == 3)
            {

            }
        }

        private string[,] GetAgentsInfoData(Agent agent, out List<UnityEngine.Object> objects, out List<bool> actives)
        {
            objects = new List<UnityEngine.Object>();
            actives = new List<bool>();
            int numRows = 4 + agent.entityTypeOverrides.Count;
            string[,] results = new string[numRows, 1];
            int row = 0;

            objects.Add(agent.agentType);
            results[row, 0] = agent.agentType != null ? agent.agentType.name : "Null - Please Fix";
            ++row;

            objects.Add(null);
            results[row, 0] = "Gender: " + agent.gender;
            ++row;

            objects.Add(null);
            results[row, 0] = "Age: " + agent.startAge;
            ++row;

            if (agent.faction != null)
            {
                objects.Add(agent.faction);
                results[row, 0] = agent.faction.name + " (" + agent.gLevel + ")";
            }
            else
            {
                objects.Add(null);
                results[row, 0] = "No Faction";
            }
            ++row;

            foreach (EntityTypeOverride entityTypeOverride in agent.entityTypeOverrides)
            {
                if (entityTypeOverride != null)
                {
                    objects.Add(entityTypeOverride);
                    results[row, 0] = entityTypeOverride.name;
                    ++row;
                }
            }
            
            return results;
        }

        private string[,] GetAgentsCoreTypesData(Agent agent, out List<UnityEngine.Object> objects, out List<bool> actives)
        {
            objects = new List<UnityEngine.Object>();
            actives = new List<bool>();
            string[,] results = new string[8, 2];

            MovementType movementType = agent.movementType;
            objects.Add(movementType);
            results[0, 0] = "MovementType";
            results[0, 1] = movementType == null ? "Null" : movementType.name;

            AnimationType animationType = agent.animationType;
            objects.Add(animationType);
            results[1, 0] = "AnimationType";
            results[1, 1] = animationType == null ? "Null" : animationType.name;

            DeciderType deciderType = agent.decider.DeciderType();
            objects.Add(deciderType);
            results[2, 0] = "DeciderType";
            results[2, 1] = deciderType == null ? "Null" : deciderType.name;

            MemoryType memoryType = agent.memoryType;
            objects.Add(memoryType);
            results[3, 0] = "MemoryType";
            results[3, 1] = memoryType == null ? "Null" : memoryType.name;
            
            HistoryType historyType = agent.historyType;
            objects.Add(historyType);
            results[4, 0] = "HistoryType";
            results[4, 1] = historyType == null ? "Null" : historyType.name;

            UtilityFunctionType utilityFunction = agent.utilityFunction;
            objects.Add(utilityFunction);
            results[5, 0] = "UtilityFunction";
            results[5, 1] = utilityFunction == null ? "Null" : utilityFunction.name;

            MappingType mappingType = agent.noPlansMappingType;
            objects.Add(mappingType);
            results[6, 0] = "No Plans MappingType";
            results[6, 1] = mappingType == null ? "Null" : mappingType.name;

            DriveType driveType = agent.noneDriveType;
            objects.Add(driveType);
            results[7, 0] = "No Plans DriveType";
            results[7, 1] = driveType == null ? "Null" : driveType.name;
            return results;
        }
        
        private string[,] GetAgentsPlayerOverviewData(Agent agent, out List<UnityEngine.Object> objects, out List<bool> actives)
        {
            objects = new List<UnityEngine.Object>() { null, null, null, null, null, null, null, null };
            actives = new List<bool>();
            string[,] results = new string[8, 2];

            if (agent.decider != null && agent.decider.CurrentDriveType != null &&
                agent.decider.AllCurrentPlans != null && agent.decider.AllCurrentPlans.Count > 0 && agent.decider.CurrentMapping != null)
            {
                DriveType driveType = agent.decider.CurrentDriveType;
                Mapping rootMapping = agent.decider.AllCurrentPlans[driveType].rootMappings[agent.decider.CurrentPlanIndex];
                objects[0] = agent.decider.CurrentDriveType;
                results[0, 0] = "Current Drive";
                results[0, 1] = agent.decider.CurrentDriveType.name;
                objects[2] = rootMapping.mappingType;
                results[2, 0] = "Current Plan";
                results[2, 1] = rootMapping.mappingType.name;
                objects[4] = agent.decider.CurrentMapping.mappingType;
                results[4, 0] = "Current Mapping";
                results[4, 1] = agent.decider.CurrentMapping.mappingType.name;
                Mapping previousMapping = agent.decider.PreviousMapping;
                results[6, 0] = "Previous Mapping";
                if (previousMapping != null)
                {
                    objects[6] = agent.decider.PreviousMapping.mappingType;
                    results[6, 1] = agent.decider.PreviousMapping.mappingType.name;
                }
                else
                {
                    objects[6] = null;
                    results[6, 1] = "None";
                }
            }
            else
            {
                objects[0] = null;
                results[0, 0] = "Current Drive";
                results[0, 1] = "None";
                Mapping previousMapping = agent.decider.PreviousMapping;
                results[2, 0] = "Previous Mapping";
                if (previousMapping != null)
                {
                    objects[2] = previousMapping.mappingType;
                    results[2, 1] = previousMapping.mappingType.name;
                }
                else
                {
                    objects[2] = null;
                    results[2, 1] = "None";
                }
                objects[4] = null;
                results[4, 0] = "";
                results[4, 1] = "";
                objects[6] = null;
                results[6, 0] = "";
                results[6, 1] = "";
            }

            if (agent.inAgentEvent != null)
            {
                objects[1] = agent.inAgentEvent;
                results[1, 0] = "In AgentEvent";
                results[1, 1] = agent.inAgentEvent.name;
                objects[3] = null;
                results[3, 0] = "State";
                results[3, 1] = agent.inAgentEvent.state.ToString();
                objects[5] = null;
                results[5, 0] =  "# Attendees";
                results[5, 1] = agent.inAgentEvent.attendees.Count.ToString();
                List<RoleType> roleTypes = agent.inAgentEvent.GetRoleTypes(agent);
                if (roleTypes.Count > 0)
                {
                    objects[7] = roleTypes[0];
                    results[7, 0] = roleTypes.Count > 1 ? "Roles" : "Role";
                    results[7, 1] = roleTypes[0].name + (roleTypes.Count > 1 ? ", ..." : "");
                }
                else
                {
                    objects[7] = null;
                    results[7, 0] = "Role";
                    results[7, 1] = "None";
                }
            }
            else
            {
                objects[1] = null;
                results[1, 0] = "In Agent Event";
                results[1, 1] = "None";
                objects[3] = null;
                results[3, 0] = "";
                results[3, 1] = "";
                objects[5] = null;
                results[5, 0] = "";
                results[5, 1] = "";
                objects[7] = null;
                results[7, 0] = "";
                results[7, 1] = "";
            }

            return results;
        }

        private string[,] GetAgentsMemoryData(Agent agent, out List<UnityEngine.Object> objects, out List<bool> actives, bool isEditor)
        {
            objects = new List<UnityEngine.Object>();
            actives = new List<bool>();
            if (agent.memoryType != null)
            {
                if (isEditor)
                    return agent.memoryType.EditorAgentViewData(agent, out objects);

                return agent.memoryType.PlayerAgentViewData(agent, out objects);
            }
            return null;
        }
        
        private string[,] GetAgentsPlansData(Agent agent, Plans plans, out List<UnityEngine.Object> objects, out List<bool> actives)
        {
            objects = new List<UnityEngine.Object>();
            actives = new List<bool>();
            string[,] results = null;
            if (plans != null && plans.rootMappings != null && plans.rootMappings.Count > 0)
            {
                results = new string[plans.rootMappings.Count + 2, 6];

                HistoryType.PlansLog plansLog = agent.historyType.FindPlansLogFromPlans(agent, plans);

                objects.Add(plans.driveType);
                results[0, 0] = plans.driveType.name;
                results[0, 1] = Mathf.Round(plansLog.time).ToString() + "s/" + Mathf.Round(Time.time) + "s";
                results[0, 2] = Mathf.Round(plansLog.timeToPlan).ToString() + " ms";
                results[0, 3] = plansLog.allPlans.Count.ToString() + " drvs";

                objects.Add(null);
                results[1, 1] = "Status";
                results[1, 2] = "Utility";
                results[1, 3] = "Drive Change";
                results[1, 4] = "Side Effects";
                results[1, 5] = "Lvs/Maps";

                int row = 2;
                for (int i = 0; i < plans.rootMappings.Count; i++)
                {
                    Mapping rootMapping = plans.rootMappings[i];
                    objects.Add(rootMapping.mappingType);
                    results[row, 0] = rootMapping.mappingType.name;
                    results[row, 1] = plans.statuses[i].ToString();
                    results[row, 2] = plans.utility[i].ToString();
                    results[row, 3] = plans.driveAmountEstimates[i].ToString();
                    results[row, 4] = plans.sideEffectsUtility[i].ToString();
                    results[row, 5] = rootMapping.NumberLeaves() + "/" + rootMapping.NumberMappings();
                    ++row;
                }
            }
            else
            {
                results = new string[1, 1];

                objects.Add(null);
                results[0, 0] = "None";
            }
            return results;
        }

        private string[,] GetAgentsSensorsData(Agent agent, out List<UnityEngine.Object> objects, out List<bool> actives)
        {
            objects = new List<UnityEngine.Object>();
            actives = new List<bool>();
            string[,] results = null;
            if (agent.sensorTypes != null)
            {
                results = new string[agent.sensorTypes.Count, 2];
                int i = 0;
                foreach (SensorType sensorType in agent.sensorTypes)
                {
                    objects.Add(sensorType);
                    results[i, 0] = sensorType.name;
                    results[i, 1] = sensorType.maxColliders.ToString();
                    ++i;
                }
            }
            return results;
        }

        private string[,] GetAgentsOneSensorData(Agent agent, SensorType sensorType, out List<UnityEngine.Object> objects, out List<bool> actives)
        {
            objects = new List<UnityEngine.Object>();
            actives = new List<bool>();
            string[,] results = null;
            if (sensorType != null)
            {
                Entity[] entities = agent.sensorJustDetected[sensorType];
                int numDetected = agent.sensorJustDetectedNum[sensorType];
                results = new string[numDetected, 3];

                int nonNullEntitities = 0;
                for (int i = 0; i < numDetected; i++)
                {
                    Entity entity = entities[i];
                    if (entity == null)
                        continue;

                    objects.Add(entity);
                    results[nonNullEntitities, 0] = entity.name;
                    results[nonNullEntitities, 1] = entity.entityType.name;
                    results[nonNullEntitities, 2] = Mathf.Round(Vector3.Distance(entity.transform.position, agent.transform.position)).ToString();
                    ++nonNullEntitities;
                }
            }
            return results;
        }

        private string[,] GetAgentsMappingTypesData(Agent agent, out List<UnityEngine.Object> objects, out List<bool> actives)
        {
            objects = new List<UnityEngine.Object>();
            actives = new List<bool>();
            string[,] results = null;
            if (agent.actions != null && agent.availableMappingTypes != null)
            {
                results = new string[agent.availableMappingTypes.Count, 1];
                int i = 0;

                List<MappingType> sorted = agent.availableMappingTypes.ToList();
                sorted.Sort(delegate(MappingType x, MappingType y)
                {
                    //if (x.ReducesDrive() &&  name.CompareTo(y.name))
                    return x.name.CompareTo(y.name);
                });

                foreach (MappingType mappingType in sorted)
                {
                    objects.Add(mappingType);
                    results[i, 0] = mappingType.name;
                    ++i;
                }
            }
            return results;
        }

        private string[,] GetAgentsActionsData(Agent agent, out List<UnityEngine.Object> objects, out List<bool> actives)
        {
            objects = new List<UnityEngine.Object>();
            actives = new List<bool>();
            string[,] results = null;
            if (agent.actions != null)
            {
                results = new string[agent.actions.Count, 4];
                int i = 0;
                foreach (KeyValuePair<ActionType, Action> action in agent.ActiveActions())
                {
                    objects.Add(action.Key);
                    actives.Add(true);
                    results[i, 0] = action.Key.name;
                    results[i, 1] = action.Value.GetLevel().ToString();
                    results[i, 2] = action.Value.GetChangeProbability().ToString();
                    results[i, 3] = action.Value.GetChangeAmount().ToString();
                    ++i;
                }
                foreach (KeyValuePair<ActionType, Action> action in agent.DisabledActions())
                {
                    objects.Add(action.Key);
                    actives.Add(false);
                    results[i, 0] = action.Key.name;
                    results[i, 1] = action.Value.GetLevel().ToString();
                    results[i, 2] = action.Value.GetChangeProbability().ToString();
                    results[i, 3] = action.Value.GetChangeAmount().ToString();
                    ++i;
                }
            }
            return results;
        }

        private string[,] GetAgentsDrivesData(Agent agent, out List<UnityEngine.Object> objects, out List<bool> actives)
        {
            objects = new List<UnityEngine.Object>();
            actives = new List<bool>();
            string[,] results = null;
            if (agent.drives != null)
            {
                results = new string[agent.drives.Count, 3];
                int i = 0;
                foreach (KeyValuePair<DriveType, Drive> drive in agent.ActiveDrives())
                {
                    objects.Add(drive.Key);
                    actives.Add(true);
                    results[i, 0] = drive.Key.name + drive.Key.Abbreviations();
                    results[i, 1] = Mathf.Round(drive.Value.GetLevel()).ToString();

                    // Default to Noon for Editor
                    float minutesIntoDay = 60 * 12;
                    if (Application.isPlaying)
                        minutesIntoDay = agent.timeManager.MinutesIntoDay();
                    results[i, 2] = Math.Round(drive.Value.CurrentDriveChangeRate(minutesIntoDay), 1).ToString();
                    ++i;
                }
                foreach (KeyValuePair<DriveType, Drive> drive in agent.DisabledDrives())
                {
                    objects.Add(drive.Key);
                    actives.Add(false);
                    results[i, 0] = drive.Key.name + drive.Key.Abbreviations();
                    results[i, 1] = Mathf.Round(drive.Value.GetLevel()).ToString();
                    ++i;
                }
            }
            return results;
        }

        private string[,] GetAgentsRolesData(Agent agent, out List<UnityEngine.Object> objects, out List<bool> actives)
        {
            objects = new List<UnityEngine.Object>();
            actives = new List<bool>();
            string[,] results = null;
            if (agent.roles != null)
            {
                results = new string[agent.roles.Count, 2];
                int i = 0;
                foreach (KeyValuePair<RoleType, Role> role in agent.ActiveRoles())
                {
                    objects.Add(role.Key);
                    actives.Add(true);
                    results[i, 0] = role.Key.name;
                    results[i, 1] = role.Value.GetLevel().ToString();
                    ++i;
                }
                foreach (KeyValuePair<RoleType, Role> role in agent.DisabledRoles())
                {
                    objects.Add(role.Key);
                    actives.Add(false);
                    results[i, 0] = role.Key.name;
                    results[i, 1] = role.Value.GetLevel().ToString();
                    ++i;
                }
            }
            return results;
        }

        private string[,] GetAgentsTagsData(Agent agent, out List<UnityEngine.Object> objects, out List<bool> actives)
        {
            objects = new List<UnityEngine.Object>();
            actives = new List<bool>();
            string[,] results = null;
            if (agent.tags != null)
            {
                results = new string[agent.NumTags(), 2];
                int i = 0;
                foreach (KeyValuePair<TagType, List<Tag>> tags in agent.tags)
                {
                    foreach (Tag tag in tags.Value)
                    {
                        objects.Add(tags.Key);
                        results[i, 0] = tags.Key.name;
                        results[i, 1] = tag.relatedEntity != null ? tag.relatedEntity.name : "None";
                        ++i;
                    }
                }
            }
            return results;
        }

        private string[,] GetAgentsAttributesData(Agent agent, out List<UnityEngine.Object> objects, out List<bool> actives)
        {
            objects = new List<UnityEngine.Object>();
            actives = new List<bool>();
            string[,] results = null;
            if (agent.attributes != null)
            {
                results = new string[agent.attributes.Count, 4];
                int i = 0;
                foreach (KeyValuePair<AttributeType, Attribute> attribute in agent.attributes)
                {
                    objects.Add(attribute.Key);
                    results[i, 0] = attribute.Key.name;
                    results[i, 1] = Math.Round(attribute.Value.GetLevel(), 1).ToString();
                    if (attribute.Key is MinMaxFloatAT)
                    {
                        results[i, 2] = attribute.Value.GetMin().ToString();
                        results[i, 3] = attribute.Value.GetMax().ToString();
                    }
                    else
                    {
                        results[i, 2] = "";
                        results[i, 3] = "";
                    }
                    ++i;
                }
            }
            return results;
        }

        private string[,] GetAgentsInventoryData(Agent agent, out List<UnityEngine.Object> objects, out List<bool> actives)
        {
            objects = new List<UnityEngine.Object>();
            actives = new List<bool>();
            string[,] results = null;
            if (Application.isPlaying)
            {
                List<EntityType> entityTypes = agent.inventoryType.GetAllEntityTypes(agent);

                results = new string[entityTypes.Count, 2];
                int i = 0;
                foreach (EntityType entityType in entityTypes)
                {
                    objects.Add(entityType);
                    results[i, 0] = entityType.name;
                    results[i, 1] = agent.inventoryType.GetEntityTypeAmount(agent, entityType).ToString();
                    ++i;
                }
            }
            else
            {
                List<EntityType.DefaultInventory> inventory = agent.agentType.defaultInventory;

                results = new string[inventory.Count, 4];
                int i = 0;
                foreach (EntityType.DefaultInventory defaultInventory in inventory)
                {
                    
                    objects.Add(defaultInventory.entityType);
                    objects.Add(defaultInventory.inventorySlot);
                    objects.Add(defaultInventory.amountCurve);
                    objects.Add(null);

                    results[i, 0] = defaultInventory.entityType != null ? defaultInventory.entityType.name : "Null";
                    results[i, 1] = defaultInventory.inventorySlot != null ? defaultInventory.inventorySlot.name : "Null";
                    results[i, 2] = defaultInventory.amountCurve != null ? defaultInventory.amountCurve.name : "Null";
                    results[i, 3] = defaultInventory.probability.ToString();
                    ++i;
                }
            }
            return results;
        }
    }
}
