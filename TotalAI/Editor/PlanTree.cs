using UnityEngine;
using UnityEditor;
using System.Collections.Generic;
using System.Linq;

namespace TotalAI.Editor {
    public class PlanTree
    {
        private GUIStyle guiStyle;
        private GUIStyle guiStyleBox;
        private int selectedPlan;
        private DriveType selectedDrive;
        private bool onlyComplete;
        
        private Dictionary<DriveType, Plans> allCurrentPlans;

        public void Setup()
        {
            guiStyle = new GUIStyle
            {
                richText = true
            };

            guiStyleBox = new GUIStyle
            {
                richText = true,
                border = new RectOffset(1, 1, 1, 1)
            };

            selectedPlan = -1;
            onlyComplete = false;
            selectedDrive = null;
            allCurrentPlans = null;
        }

        public void DrawOnGUI(Rect position, Agent selectedAgent, TotalAIManager totalAIManager)
        {
            GUI.backgroundColor = new Color(.4f, .4f, .4f, 2f);
            GUI.Box(new Rect(0, 0, 4000, 4000), "");
            GUI.backgroundColor = new Color(.8f, .8f, .8f, 1f);
            DrawGrid(position, 10, 0.2f, new Color(.2f, .2f, .2f, 2f));
            DrawGrid(position, 50, 0.4f, new Color(.2f, .2f, .2f, 2f));

            if (selectedAgent != null && selectedAgent.decider.AllCurrentPlans != null)
            {
                Dictionary<DriveType, float> drivesRanked = selectedAgent.decider.currentDriveTypesRanked;

                List<DriveType> driveTypes = drivesRanked.Keys.ToList();
                List<float> driveUtilities = drivesRanked.Values.ToList();

                string driveUtilityRound = "0.";
                for (int i = 0; i < selectedAgent.totalAIManager.settings.roundDriveUtility; i++)
                {
                    driveUtilityRound += "0";
                }

                DrawDriveButtons(driveTypes, driveUtilities, selectedAgent.decider.CurrentDriveType, 1, driveUtilityRound);
                if (selectedDrive != null)
                    DrawAgent(position, selectedAgent, null, false, totalAIManager);
            }
        }

        private void DrawDriveButtons(List<DriveType> driveTypes, List<float> driveUtilities, DriveType currentDriveType,
                                      int numColumns, string driveUtilityRound)
        {
            float buttonWidth = 150;
            float buttonHeight = 25;
            float overallHeight = buttonHeight * ((driveTypes.Count - 1) / numColumns + 1) + 30;
            float overallWidth = buttonWidth * numColumns + 10;

            GUI.BeginGroup(new Rect(20, 10, overallWidth, overallHeight), new GUIStyle("helpbox"));
            GUI.Box(new Rect(0, 0, overallWidth, overallHeight), "Drives Ranked");

            int driveNum = 0;
            for (int i = 0; i < driveTypes.Count; i++)
            {
                DriveType driveType = driveTypes[i];
                string name = driveType.name;
                if (currentDriveType != null && currentDriveType == driveType)
                    name = "*" + name + "*";
                int row = driveNum / numColumns;
                if (GUI.Button(new Rect(5 + buttonWidth * (driveNum % numColumns), 25 + buttonHeight * row, buttonWidth, buttonHeight),
                               driveUtilities[i].ToString(driveUtilityRound) + "\t" + name, 
                               new GUIStyle("button") { alignment = TextAnchor.MiddleLeft }))
                {
                    selectedDrive = driveType;
                }
                ++driveNum;
            }

            GUI.EndGroup();
        }

        private void DrawDriveButtonsForEditor(List<AgentType.DefaultDrive> defaultDrives, DriveType currentDriveType,
                                               float x, float y, int numColumns)
        {
            float buttonWidth = 125;
            float buttonHeight = 25;
            int driveNum = 0;
            foreach (AgentType.DefaultDrive defaultDrive in defaultDrives)
            {
                if (defaultDrive.driveType != null)
                {
                    string name = defaultDrive.driveType.name;
                    if (currentDriveType != null && currentDriveType == defaultDrive.driveType)
                        name = "*" + name + "*";
                    int row = driveNum / numColumns;
                    if (GUI.Button(new Rect(x + buttonWidth * (driveNum % numColumns), y + buttonHeight * row, buttonWidth, buttonHeight), name))
                    {
                        selectedDrive = defaultDrive.driveType;
                    }
                    ++driveNum;
                }
            }
        }

        private void DrawAgent(Rect position, Agent agent, AgentType agentType, bool resetPlans, TotalAIManager totalAIManager)
        {
            if (agent == null)
                return;

            allCurrentPlans = agent.decider.AllCurrentPlans;

            Plans selectedPlans = null;
            if (allCurrentPlans != null && allCurrentPlans.TryGetValue(selectedDrive, out selectedPlans) &&
                selectedPlans.rootMappings != null && selectedPlans.rootMappings.Count > 0)
            {
                if (selectedPlan == -1 && agent != null)
                    selectedPlan = agent.decider.CurrentPlanIndex;
                else if (selectedPlan == -1)
                    selectedPlan = 0;

                List<Mapping> rootMappings = selectedPlans.rootMappings;
                if (onlyComplete)
                    rootMappings = selectedPlans.GetCompletePlans();

                string title = (agent != null ? agent.name : agentType.name) + " : " + (selectedPlan + 1) + "/" +
                               rootMappings.Count + "\n" + selectedPlans.driveType.name;

                GUI.Box(new Rect(position.width / 2 - 175 / 2, 10, 175, 35), title, "Button");
                if (agent != null)
                    onlyComplete = GUI.Toggle(new Rect(position.width / 2 + 175 / 2 + 10, 10, 175, 25), onlyComplete, "Complete Plans");

                if (GUI.Button(new Rect(position.width / 2 - 175 / 2, 45, 50, 25), "Down"))
                {
                    --selectedPlan;
                }
                if (agent != null && GUI.Button(new Rect(position.width / 2 - 175 / 2 + 50, 45, 75, 25), "Current"))
                {
                    selectedPlan = agent.decider.CurrentPlanIndex;
                    selectedDrive = agent.decider.CurrentDriveType;
                }
                if (GUI.Button(new Rect(position.width / 2 - 175 / 2 + 125, 45, 50, 25), "Up"))
                {
                    ++selectedPlan;
                }

                //DrawICTCheckBoxes(agentType, totalAIManager);

                if (selectedPlan >= rootMappings.Count)
                    selectedPlan = 0;
                else if (selectedPlan < 0)
                    selectedPlan = rootMappings.Count - 1;

                if (selectedPlan < rootMappings.Count)
                {
                    if (agent != null)
                    {
                        rootMappings[selectedPlan].Draw(position.width / 2 - 175 / 2, 80, 175, 75, 120, 100, agent.decider.CurrentMapping, agent);
                        DrawPlanStats(position, selectedPlans);
                    }
                    else
                    {
                        rootMappings[selectedPlan].Draw(position.width / 2 - 175 / 2, 80, 175, 75, 120, 100, null, null);
                    }
                }
            }
            else if (selectedPlans != null)
            {
                selectedPlan = -1;
                GUI.Box(new Rect(position.width / 2 - 200 / 2, 10, 200, 35), "Planned - No Plans Found",
                        new GUIStyle("largeLabel") { alignment = TextAnchor.MiddleCenter });
            }
            else
            {
                selectedPlan = -1;
                GUI.Box(new Rect(position.width / 2 - 175 / 2, 10, 175, 35), "Did Not Plan",
                        new GUIStyle("largeLabel") { alignment = TextAnchor.MiddleCenter });
            }
        }
        
        private void DrawPlanStats(Rect position, Plans plans)
        {
            GUI.BeginGroup(new Rect(position.width - 210, 10, 200, 150), new GUIStyle("helpbox"));

            GUI.Box(new Rect(0, 0, 200, 150), "Plan Stats");

            if (plans.statuses[selectedPlan] != Plans.Status.NotComplete)
            {
                GUI.Label(new Rect(10, 25, 200, 20), "Utility: " + plans.utility[selectedPlan]);
                GUI.Label(new Rect(10, 45, 200, 20), "DriveType: " + plans.driveAmountEstimates[selectedPlan]);
                GUI.Label(new Rect(10, 65, 200, 20), "Time: " + plans.timeEstimates[selectedPlan]);
                GUI.Label(new Rect(10, 85, 200, 20), "SiEff: " + plans.sideEffectsUtility[selectedPlan]);
            }
            else
            {
                GUI.Label(new Rect(10, 25, 200, 20), "NOT COMPLETE");
            }
            
            GUI.EndGroup();
        }

        private void DrawGrid(Rect position, float gridSpacing, float gridOpacity, Color gridColor)
        {
            Vector2 offset = Vector2.zero;
            Vector2 drag = Vector2.zero;

            int widthDivs = Mathf.CeilToInt(position.width / gridSpacing);
            int heightDivs = Mathf.CeilToInt(position.height / gridSpacing);

            Handles.BeginGUI();
            Handles.color = new Color(gridColor.r, gridColor.g, gridColor.b, gridOpacity);

            offset += drag * 0.5f;
            Vector3 newOffset = new Vector3(offset.x % gridSpacing, offset.y % gridSpacing, 0);

            for (int i = 0; i < widthDivs; i++)
            {
                Handles.DrawLine(new Vector3(gridSpacing * i, -gridSpacing, 0) + newOffset, new Vector3(gridSpacing * i, position.height, 0f) + newOffset);
            }

            for (int j = 0; j < heightDivs; j++)
            {
                Handles.DrawLine(new Vector3(-gridSpacing, gridSpacing * j, 0) + newOffset, new Vector3(position.width, gridSpacing * j, 0f) + newOffset);
            }

            Handles.color = Color.white;
            Handles.EndGUI();
        }
    }
}
