using UnityEngine;
using UnityEditor;
using System.Collections.Generic;
using Pluminus.Core;

namespace Pluminus.EditorTools
{
    public class PluminusPerformanceDashboard : EditorWindow
    {
        private PluminusBrain selectedBrain;
        private Vector2 scrollPos;
        private int graphMode = 0; // 0: Episodes, 1: Continuous, 2: Winrate

        // Multi-brain
        private PluminusBrain[] sceneBrains;
        private string[] brainNames;
        private int selectedBrainIndex;
        private double lastBrainScanTime;

        [MenuItem("Window/Pluminus/AI Performance Dashboard")]
        public static void ShowWindow()
        {
            GetWindow<PluminusPerformanceDashboard>("Pluminus Dashboard");
        }

        private void OnSelectionChange()
        {
            if (Selection.activeGameObject != null)
            {
                var brain = Selection.activeGameObject.GetComponentInChildren<PluminusBrain>();
                if (brain != null)
                {
                    selectedBrain = brain;
                    SyncIndexToBrain();
                    Repaint();
                }
            }
        }

        private void RefreshBrainList()
        {
            sceneBrains = FindObjectsByType<PluminusBrain>(FindObjectsSortMode.None);
            brainNames = new string[sceneBrains.Length];
            for (int i = 0; i < sceneBrains.Length; i++)
            {
                brainNames[i] = sceneBrains[i].gameObject.name;
            }
            SyncIndexToBrain();
        }

        private void SyncIndexToBrain()
        {
            if (sceneBrains == null) return;
            for (int i = 0; i < sceneBrains.Length; i++)
            {
                if (sceneBrains[i] == selectedBrain)
                {
                    selectedBrainIndex = i;
                    return;
                }
            }
            selectedBrainIndex = 0;
        }

        private double lastRepaintTime;

        private void OnGUI()
        {
            // Scan les cerveaux periodiquement (pas chaque frame)
            if (Application.isPlaying && EditorApplication.timeSinceStartup > lastBrainScanTime + 2.0)
            {
                lastBrainScanTime = EditorApplication.timeSinceStartup;
                RefreshBrainList();
            }

            EditorGUILayout.BeginVertical(EditorStyles.helpBox);
            GUILayout.Label("Pluminus AI Performance", EditorStyles.boldLabel);

            // Fallback si aucun cerveau
            if (selectedBrain == null)
            {
                RefreshBrainList();
                if (sceneBrains != null && sceneBrains.Length > 0)
                    selectedBrain = sceneBrains[0];
            }

            // Dropdown multi-cerveaux
            if (sceneBrains != null && sceneBrains.Length > 1)
            {
                EditorGUI.BeginChangeCheck();
                selectedBrainIndex = EditorGUILayout.Popup("Cerveau", selectedBrainIndex, brainNames);
                if (EditorGUI.EndChangeCheck())
                {
                    if (selectedBrainIndex >= 0 && selectedBrainIndex < sceneBrains.Length)
                        selectedBrain = sceneBrains[selectedBrainIndex];
                    Repaint();
                }
            }
            else
            {
                // Un seul cerveau ou aucun : champ classique
                EditorGUI.BeginChangeCheck();
                selectedBrain = (PluminusBrain)EditorGUILayout.ObjectField("Agent", selectedBrain, typeof(PluminusBrain), true);
                if (EditorGUI.EndChangeCheck()) Repaint();
            }

            EditorGUILayout.EndVertical();

            if (selectedBrain == null)
            {
                EditorGUILayout.HelpBox("Aucun PluminusBrain dans la scene. Lancez le Play Mode.", MessageType.Info);
                return;
            }

            scrollPos = EditorGUILayout.BeginScrollView(scrollPos);

            DrawStats();

            EditorGUILayout.Space();
            graphMode = GUILayout.Toolbar(graphMode, new string[] { "Episodes", "Temps Reel", "Winrate (%)" });

            DrawGraph();

            EditorGUILayout.EndScrollView();

            // Auto-repaint limite a 10 FPS
            if (Application.isPlaying && EditorApplication.timeSinceStartup > lastRepaintTime + 0.1)
            {
                lastRepaintTime = EditorApplication.timeSinceStartup;
                Repaint();
            }
        }

        private void DrawStats()
        {
            EditorGUILayout.BeginVertical("box");
            GUILayout.Label($"Stats : {selectedBrain.gameObject.name}", EditorStyles.boldLabel);

            EditorGUILayout.LabelField("Episodes Totaux", selectedBrain.GetTotalEpisodes().ToString());

            // Winrate global
            EditorGUILayout.Space(2);
            DrawMetricLabel("Winrate Global", (selectedBrain.analyticsData != null ? ((float)selectedBrain.analyticsData.totalSuccesses / Mathf.Max(1, selectedBrain.analyticsData.totalEpisodes) * 100f) : 0));

            // Winrate 100 derniers
            var epHistory = selectedBrain.episodeRewards;
            float recentWinrate = epHistory.Count > 0 ? (float)epHistory.FindAll(r => r > 0).Count / epHistory.Count * 100f : 0;
            DrawMetricLabel("Winrate (100 derniers eps)", recentWinrate);

            // Precision recente
            DrawMetricLabel("Precision Recente (100 coups)", selectedBrain.GetRecentAccuracy());

            EditorGUILayout.Space(5);

            // Exploration
            float epsilon = selectedBrain.GetCurrentEpsilon();
            GUI.color = epsilon > 0.1f ? Color.cyan : Color.green;
            EditorGUILayout.LabelField("Exploration (Epsilon)", (epsilon * 100f).ToString("F5") + "%", EditorStyles.boldLabel);
            GUI.color = Color.white;

            EditorGUILayout.EndVertical();
        }

        private void DrawMetricLabel(string label, float value)
        {
            string color = value > 75 ? "green" : (value > 40 ? "yellow" : "red");
            EditorGUILayout.LabelField(label, $"<color={color}>{value:F1}%</color>", new GUIStyle(EditorStyles.label) { richText = true });
        }

        private void DrawGraph()
        {
            EditorGUILayout.Space();
            string[] titles = { "Scores par Episodes (100 derniers)", "Recompense Totale Cumulee", "Taux de Succes (Winrate %)" };
            GUILayout.Label(titles[graphMode], EditorStyles.miniBoldLabel);

            Rect graphRect = GUILayoutUtility.GetRect(200, 200, GUILayout.ExpandWidth(true));
            EditorGUI.DrawRect(graphRect, new Color(0.15f, 0.15f, 0.15f));

            List<float> history = null;

            if (selectedBrain.analyticsData != null)
            {
                if (graphMode == 2) history = selectedBrain.analyticsData.winRateHistory;
                else if (graphMode == 1) history = selectedBrain.analyticsData.continuousHistory;
                else history = selectedBrain.analyticsData.episodeRewards;
            }
            else
            {
                if (graphMode == 1) history = selectedBrain.continuousHistory;
                else history = selectedBrain.episodeRewards;
            }

            if (history == null || history.Count == 0)
            {
                if (graphMode == 2 && selectedBrain != null)
                {
                    int pos = selectedBrain.GetPositiveRewards();
                    int neg = selectedBrain.GetNegativeRewards();
                    float liveWinRate = (pos + neg) > 0 ? (float)pos / (pos + neg) * 100f : 0;
                    EditorGUI.LabelField(graphRect, $"Winrate de Session : {liveWinRate:F1}%", EditorStyles.centeredGreyMiniLabel);
                }
                else
                {
                    EditorGUI.LabelField(graphRect, "En attente de donnees...", EditorStyles.centeredGreyMiniLabel);
                }
                return;
            }

            // Min/Max
            float min = float.MaxValue;
            float max = float.MinValue;
            foreach (var r in history)
            {
                if (r < min) min = r;
                if (r > max) max = r;
            }
            if (max == min) { max += 10; min -= 10; }

            // Ligne de grille (zero ou 50%)
            Handles.BeginGUI();
            Handles.color = new Color(0.3f, 0.3f, 0.3f, 0.5f);
            float zeroY = MapToGraph(graphMode == 2 ? 50 : 0, min, max, graphRect);
            Handles.DrawLine(new Vector2(graphRect.x, zeroY), new Vector2(graphRect.xMax, zeroY));
            Handles.EndGUI();

            // Courbe
            Handles.BeginGUI();
            Handles.color = graphMode == 2 ? Color.cyan : Color.green;

            if (history.Count == 1)
            {
                float y = MapToGraph(history[0], min, max, graphRect);
                Handles.DrawLine(new Vector2(graphRect.x, y), new Vector2(graphRect.xMax, y));
            }
            else
            {
                Vector3[] points = new Vector3[history.Count];
                float xStep = graphRect.width / (history.Count - 1);

                for (int i = 0; i < history.Count; i++)
                {
                    float x = graphRect.x + (i * xStep);
                    float y = MapToGraph(history[i], min, max, graphRect);
                    points[i] = new Vector3(x, y, 0);
                }
                Handles.DrawPolyLine(points);
            }
            Handles.EndGUI();

            // Labels Min/Max
            GUI.Label(new Rect(graphRect.x + 5, graphRect.y + 5, 100, 20), $"Max: {max:F1}", EditorStyles.miniLabel);
            GUI.Label(new Rect(graphRect.x + 5, graphRect.yMax - 20, 100, 20), $"Min: {min:F1}", EditorStyles.miniLabel);
        }

        private float MapToGraph(float value, float min, float max, Rect rect)
        {
            float range = max - min;
            float normalized = (value - min) / range;
            return rect.yMax - (normalized * rect.height);
        }
    }
}
