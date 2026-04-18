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
        private List<float> smoothRewards = new List<float>();

        [MenuItem("Window/Pluminus/AI Performance Dashboard")]
        public static void ShowWindow()
        {
            GetWindow<PluminusPerformanceDashboard>("Pluminus Dashboard");
        }

        private void OnGUI()
        {
            EditorGUILayout.BeginVertical(EditorStyles.helpBox);
            GUILayout.Label("📊 Pluminus AI Performance", EditorStyles.boldLabel);
            
            selectedBrain = (AdaptiveBrain)EditorGUILayout.ObjectField("Agent à suivre", selectedBrain, typeof(AdaptiveBrain), true);
            EditorGUILayout.EndVertical();

            if (selectedBrain == null)
            {
                EditorGUILayout.HelpBox("Sélectionnez un GameObject avec un AdaptiveBrain pour voir les stats.", MessageType.Info);
                return;
            }

            scrollPos = EditorGUILayout.BeginScrollView(scrollPos);

            DrawStats();
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
            GUILayout.Label("Courbe de Récompense (100 derniers épisodes)", EditorStyles.miniBoldLabel);

            Rect graphRect = GUILayoutUtility.GetRect(200, 200, GUILayout.ExpandWidth(true));
            EditorGUI.DrawRect(graphRect, new Color(0.15f, 0.15f, 0.15f));

            var history = selectedBrain.episodeRewards;
            if (history.Count < 2)
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
