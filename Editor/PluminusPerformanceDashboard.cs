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
            GUILayout.Label("Statistiques de Session", EditorStyles.boldLabel);
            
            EditorGUILayout.LabelField("Épisodes", selectedBrain.GetTotalEpisodes().ToString());
            
            // Calcul de la Précision Globale (Positif vs Négatif)
            if (selectedBrain.analyticsData != null)
            {
                int total = selectedBrain.analyticsData.totalPositiveRewards + selectedBrain.analyticsData.totalNegativeRewards;
                float accuracy = total > 0 ? (float)selectedBrain.analyticsData.totalPositiveRewards / total * 100f : 0;
                string color = accuracy > 75 ? "green" : (accuracy > 40 ? "yellow" : "red");
                EditorGUILayout.LabelField("Précision Globale (Efficacité)", $"<color={color}>{accuracy:F1}%</color> ({selectedBrain.analyticsData.totalPositiveRewards}/{total})", new GUIStyle(EditorStyles.label) { richText = true });
            }

            float epsilon = selectedBrain.GetCurrentEpsilon();
            GUI.color = epsilon > 0.1f ? Color.cyan : Color.green;
            EditorGUILayout.LabelField("Exploration (Epsilon)", (epsilon * 100f).ToString("F1") + "%");
            GUI.color = Color.white;

            EditorGUILayout.EndVertical();
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
            
            if (history == null || history.Count < 2)
            {
                EditorGUI.LabelField(graphRect, "En attente de données...", EditorStyles.centeredGreyMiniLabel);
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
            if (max == min) { max += 1; min -= 1; }

            // Dessin des lignes de grille
            Handles.BeginGUI();
            Handles.color = new Color(0.3f, 0.3f, 0.3f, 0.5f);
            float zeroY = MapToGraph(0, min, max, graphRect);
            Handles.DrawLine(new Vector2(graphRect.x, zeroY), new Vector2(graphRect.xMax, zeroY));
            Handles.EndGUI();

            // Dessin de la courbe
            Handles.BeginGUI();
            Handles.color = Color.green;
            
            Vector3[] points = new Vector3[history.Count];
            float xStep = graphRect.width / (history.Count - 1);
            
            for (int i = 0; i < history.Count; i++)
            {
                float x = graphRect.x + (i * xStep);
                float y = MapToGraph(history[i], min, max, graphRect);
                points[i] = new Vector3(x, y, 0);
            }

            Handles.DrawPolyLine(points);
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
