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
        private bool showContinuous = false;

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
            
            // Si on n'a rien sélectionné, on essaie de trouver un cerveau par défaut
            if (selectedBrain == null)
            {
                selectedBrain = FindObjectOfType<AdaptiveBrain>();
            }

            EditorGUI.BeginChangeCheck();
            selectedBrain = (AdaptiveBrain)EditorGUILayout.ObjectField("Agent Inspecté", selectedBrain, typeof(AdaptiveBrain), true);
            if (EditorGUI.EndChangeCheck()) Repaint();

            EditorGUILayout.EndVertical();

            if (selectedBrain == null)
            {
                EditorGUILayout.HelpBox("Sélectionnez un agent (GameObject avec AdaptiveBrain) dans la Hiérarchie.", MessageType.Info);
                return;
            }

            scrollPos = EditorGUILayout.BeginScrollView(scrollPos);

            DrawStats();
            
            EditorGUILayout.Space();
            EditorGUILayout.BeginHorizontal();
            if (GUILayout.Toggle(!showContinuous, "Par Épisodes", "Button")) showContinuous = false;
            if (GUILayout.Toggle(showContinuous, "Temps Réel (Continu)", "Button")) showContinuous = true;
            EditorGUILayout.EndHorizontal();

            DrawGraph();

            EditorGUILayout.EndScrollView();

            // Auto-repaint pendant le mode Play
            if (Application.isPlaying) Repaint();
        }

        private void DrawStats()
        {
            EditorGUILayout.BeginVertical("box");
            GUILayout.Label("Statistiques Actuelles", EditorStyles.boldLabel);
            
            EditorGUILayout.LabelField("Épisodes totaux", selectedBrain.GetTotalEpisodes().ToString());
            EditorGUILayout.LabelField("Dernière récompense", selectedBrain.GetLastEpisodeReward().ToString("F2"));
            
            float epsilon = selectedBrain.GetCurrentEpsilon();
            GUI.color = epsilon > 0.1f ? Color.cyan : Color.green;
            EditorGUILayout.LabelField("Exploration (Epsilon)", (epsilon * 100f).ToString("F1") + "%");
            GUI.color = Color.white;

            EditorGUILayout.EndVertical();
        }

        private void DrawGraph()
        {
            EditorGUILayout.Space();
            string title = showContinuous ? "Récompense Totale Cumulée (Continu)" : "Scores par Épisodes (100 derniers)";
            GUILayout.Label(title, EditorStyles.miniBoldLabel);

            Rect graphRect = GUILayoutUtility.GetRect(200, 200, GUILayout.ExpandWidth(true));
            EditorGUI.DrawRect(graphRect, new Color(0.15f, 0.15f, 0.15f));

            List<float> history = null;
            
            // On essaie de lire depuis l'asset d'analytics pour la persistance
            if (selectedBrain != null && selectedBrain.analyticsData != null)
            {
                history = showContinuous ? selectedBrain.analyticsData.continuousHistory : selectedBrain.analyticsData.episodeRewards;
            }
            else if (selectedBrain != null)
            {
                history = showContinuous ? selectedBrain.continuousHistory : selectedBrain.episodeRewards;
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
