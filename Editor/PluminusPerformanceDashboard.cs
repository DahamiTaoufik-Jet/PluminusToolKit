using UnityEngine;
using UnityEditor;
using System.Collections.Generic;
using Pluminus.Core;

namespace Pluminus.EditorTools
{
    public class PluminusPerformanceDashboard : EditorWindow
    {
        private AdaptiveBrain selectedBrain;
        private Vector2 scrollPos;
        private int graphMode = 0; // 0: Episodes, 1: Continuous, 2: Winrate

        [MenuItem("Window/Pluminus/AI Performance Dashboard")]
        public static void ShowWindow()
        {
            GetWindow<PluminusPerformanceDashboard>("Pluminus Dashboard");
        }

        private void OnSelectionChange()
        {
            if (Selection.activeGameObject != null)
            {
                var brain = Selection.activeGameObject.GetComponentInChildren<AdaptiveBrain>();
                if (brain != null)
                {
                    selectedBrain = brain;
                    Repaint();
                }
            }
        }

        private void OnGUI()
        {
            EditorGUILayout.BeginVertical(EditorStyles.helpBox);
            GUILayout.Label("📊 Pluminus AI Performance", EditorStyles.boldLabel);
            
            if (selectedBrain == null) selectedBrain = FindObjectOfType<AdaptiveBrain>();

            EditorGUI.BeginChangeCheck();
            selectedBrain = (AdaptiveBrain)EditorGUILayout.ObjectField("Agent Inspecté", selectedBrain, typeof(AdaptiveBrain), true);
            if (EditorGUI.EndChangeCheck()) Repaint();

            EditorGUILayout.EndVertical();

            if (selectedBrain == null)
            {
                EditorGUILayout.HelpBox("Sélectionnez un agent dans la Hiérarchie.", MessageType.Info);
                return;
            }

            scrollPos = EditorGUILayout.BeginScrollView(scrollPos);

            DrawStats();
            
            EditorGUILayout.Space();
            graphMode = GUILayout.Toolbar(graphMode, new string[] { "Épisodes", "Temps Réel", "Winrate (%)" });

            DrawGraph();

            EditorGUILayout.EndScrollView();

            // Auto-repaint pendant le mode Play
            if (Application.isPlaying) Repaint();
        }

        private void DrawStats()
        {
            EditorGUILayout.BeginVertical("box");
            GUILayout.Label("Statistiques de Performance", EditorStyles.boldLabel);
            
            EditorGUILayout.LabelField("Épisodes Totaux", selectedBrain.GetTotalEpisodes().ToString());

            // 1. WINRATE PAR CATEGORIES
            EditorGUILayout.Space(2);
            DrawMetricLabel("Winrate Global", (selectedBrain.analyticsData != null ? ((float)selectedBrain.analyticsData.totalSuccesses / Mathf.Max(1, selectedBrain.analyticsData.totalEpisodes) * 100f) : 0));
            
            // Winrate 100 derniers
            var epHistory = selectedBrain.episodeRewards;
            float recentWinrate = epHistory.Count > 0 ? (float)epHistory.FindAll(r => r > 0).Count / epHistory.Count * 100f : 0;
            DrawMetricLabel("Winrate (100 derniers éps)", recentWinrate);

            // Précision Coup par Coup (100 derniers)
            DrawMetricLabel("Précision Récente (100 coups)", selectedBrain.GetRecentAccuracy());

            EditorGUILayout.Space(5);

            // 2. EXPLORATION HAUTE PRECISION
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
            string[] titles = { "Scores par Épisodes (100 derniers)", "Récompense Totale Cumulée", "Taux de Succès (Winrate %)" };
            GUILayout.Label(titles[graphMode], EditorStyles.miniBoldLabel);

            Rect graphRect = GUILayoutUtility.GetRect(200, 200, GUILayout.ExpandWidth(true));
            EditorGUI.DrawRect(graphRect, new Color(0.15f, 0.15f, 0.15f));

            List<float> history = null;
            
            // Sélecteur de données selon le mode
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
                // Si on est en mode Winrate mais pas encore de morts, on montre le winrate "Live" de session
                if (graphMode == 2 && selectedBrain != null)
                {
                    int pos = selectedBrain.GetPositiveRewards();
                    int neg = selectedBrain.GetNegativeRewards();
                    float liveWinRate = (pos + neg) > 0 ? (float)pos / (pos + neg) * 100f : 0;
                    EditorGUI.LabelField(graphRect, $"Winrate de Session : {liveWinRate:F1}% (Calculé sur les points récoltés)", EditorStyles.centeredGreyMiniLabel);
                }
                else
                {
                    EditorGUI.LabelField(graphRect, "En attente de données...", EditorStyles.centeredGreyMiniLabel);
                }
                return;
            }

            // Calcul des min/max pour le cadrage
            float min = float.MaxValue;
            float max = float.MinValue;
            foreach (var r in history)
            {
                if (r < min) min = r;
                if (r > max) max = r;
            }
            if (max == min) { max += 10; min -= 10; } // On donne de l'air pour 1 seul point

            // Dessin des lignes de grille
            Handles.BeginGUI();
            Handles.color = new Color(0.3f, 0.3f, 0.3f, 0.5f);
            float zeroY = MapToGraph(graphMode == 2 ? 50 : 0, min, max, graphRect); // Ligne 50% pour winrate
            Handles.DrawLine(new Vector2(graphRect.x, zeroY), new Vector2(graphRect.xMax, zeroY));
            Handles.EndGUI();

            // Dessin de la courbe
            Handles.BeginGUI();
            Handles.color = graphMode == 2 ? Color.cyan : Color.green;
            
            if (history.Count == 1)
            {
                // Un seul point : on dessine une ligne horizontale
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
