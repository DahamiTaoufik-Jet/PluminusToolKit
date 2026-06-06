using UnityEngine;
using UnityEditor;
using Pluminus.Data;

namespace Pluminus.EditorTools
{
    [CustomEditor(typeof(QTableData))]
    public class QTableDataEditor : UnityEditor.Editor
    {
        private bool showTopStates = false;
        private Vector2 scrollPos;

        public override void OnInspectorGUI()
        {
            DrawDefaultInspector();

            QTableData data = (QTableData)target;

            int stateCount = data.stateIds.Count;

            EditorGUILayout.Space(10);
            EditorGUILayout.LabelField("Contenu de la Q-Table", EditorStyles.boldLabel);
            EditorGUILayout.BeginVertical(EditorStyles.helpBox);

            EditorGUILayout.LabelField("Etats explores", stateCount.ToString("N0"));
            EditorGUILayout.LabelField("Actions par etat", data.numActions.ToString());
            EditorGUILayout.LabelField("Entrees totales", (stateCount * data.numActions).ToString("N0"));

            if (stateCount > 0)
            {
                // Calcul des stats sur les Q-values
                float globalMin = float.MaxValue;
                float globalMax = float.MinValue;
                float sum = 0f;
                int totalValues = 0;
                int nonZeroStates = 0;

                for (int i = 0; i < stateCount; i++)
                {
                    float[] vals = data.stateValues[i].values;
                    if (vals == null) continue;

                    bool hasNonZero = false;
                    for (int j = 0; j < vals.Length; j++)
                    {
                        float v = vals[j];
                        if (v < globalMin) globalMin = v;
                        if (v > globalMax) globalMax = v;
                        sum += v;
                        totalValues++;
                        if (v != 0f) hasNonZero = true;
                    }
                    if (hasNonZero) nonZeroStates++;
                }

                float avg = totalValues > 0 ? sum / totalValues : 0f;

                EditorGUILayout.Space(5);
                EditorGUILayout.LabelField("Statistiques Q-Values", EditorStyles.boldLabel);
                EditorGUILayout.LabelField("Q min", globalMin.ToString("F4"));
                EditorGUILayout.LabelField("Q max", globalMax.ToString("F4"));
                EditorGUILayout.LabelField("Q moyenne", avg.ToString("F4"));
                EditorGUILayout.LabelField("Etats actifs (non-zero)", $"{nonZeroStates} / {stateCount}");

                // Barre visuelle min/max
                EditorGUILayout.Space(5);
                float range = Mathf.Max(Mathf.Abs(globalMin), Mathf.Abs(globalMax));
                if (range > 0f)
                {
                    Rect r = EditorGUILayout.GetControlRect(false, 16);
                    float half = r.width * 0.5f;
                    float negWidth = range > 0 ? (Mathf.Abs(globalMin) / range) * half : 0f;
                    float posWidth = range > 0 ? (Mathf.Abs(globalMax) / range) * half : 0f;

                    // Fond
                    EditorGUI.DrawRect(r, new Color(0.15f, 0.15f, 0.15f));
                    // Negatif (rouge)
                    EditorGUI.DrawRect(new Rect(r.x + half - negWidth, r.y, negWidth, r.height), new Color(0.8f, 0.2f, 0.2f, 0.7f));
                    // Positif (vert)
                    EditorGUI.DrawRect(new Rect(r.x + half, r.y, posWidth, r.height), new Color(0.2f, 0.8f, 0.2f, 0.7f));
                    // Centre
                    EditorGUI.DrawRect(new Rect(r.x + half - 1, r.y, 2, r.height), Color.white);
                }

                // Apercu des meilleurs etats
                EditorGUILayout.Space(10);
                showTopStates = EditorGUILayout.Foldout(showTopStates, $"Apercu des etats ({Mathf.Min(20, stateCount)} premiers)");
                if (showTopStates)
                {
                    scrollPos = EditorGUILayout.BeginScrollView(scrollPos, GUILayout.MaxHeight(250));

                    int shown = Mathf.Min(20, stateCount);
                    for (int i = 0; i < shown; i++)
                    {
                        float[] vals = data.stateValues[i].values;
                        if (vals == null) continue;

                        // Trouve la meilleure action
                        int bestAction = 0;
                        float bestVal = vals[0];
                        for (int j = 1; j < vals.Length; j++)
                        {
                            if (vals[j] > bestVal)
                            {
                                bestVal = vals[j];
                                bestAction = j;
                            }
                        }

                        // Construit la ligne de Q-values
                        System.Text.StringBuilder sb = new System.Text.StringBuilder();
                        for (int j = 0; j < vals.Length; j++)
                        {
                            if (j > 0) sb.Append("  ");
                            if (j == bestAction && bestVal != 0f)
                                sb.Append($"[{vals[j]:+0.00;-0.00}]");
                            else
                                sb.Append($" {vals[j]:+0.00;-0.00} ");
                        }

                        EditorGUILayout.BeginHorizontal();
                        EditorGUILayout.LabelField($"S{data.stateIds[i]}", GUILayout.Width(50));
                        EditorGUILayout.LabelField(sb.ToString(), EditorStyles.miniLabel);
                        EditorGUILayout.EndHorizontal();
                    }

                    EditorGUILayout.EndScrollView();
                }
            }
            else
            {
                EditorGUILayout.HelpBox("Q-Table vide. Lancez un entrainement puis exportez.", MessageType.Info);
            }

            EditorGUILayout.EndVertical();

            // Bouton de purge
            EditorGUILayout.Space(5);
            if (stateCount > 0)
            {
                GUI.color = new Color(1f, 0.6f, 0.6f);
                if (GUILayout.Button("Vider la Q-Table"))
                {
                    if (EditorUtility.DisplayDialog("Vider la Q-Table",
                        $"Supprimer les {stateCount} etats memorises ?\nCette action est irreversible.", "Vider", "Annuler"))
                    {
                        data.stateIds.Clear();
                        data.stateValues.Clear();
                        data.numActions = 0;
                        EditorUtility.SetDirty(data);
                        AssetDatabase.SaveAssets();
                    }
                }
                GUI.color = Color.white;
            }
        }
    }
}
