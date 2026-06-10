#if UNITY_EDITOR
using UnityEngine;
using UnityEditor;
using Pluminus.Integration;

namespace Pluminus.EditorTools
{
    [CustomEditor(typeof(PluminusActionRouter))]
    public class PluminusActionRouterEditor : Editor
    {
        public override void OnInspectorGUI()
        {
            DrawDefaultInspector();

            PluminusActionRouter router = (PluminusActionRouter)target;

            if (!Application.isPlaying) return;

            EditorGUILayout.Space(10);
            EditorGUILayout.LabelField("Debug Live", EditorStyles.boldLabel);

            // Derniere action executee
            int last = router.LastExecutedAction;
            if (last >= 0)
            {
                float elapsed = Time.time - router.LastExecutedTime;
                string actionName = last < router.actions.Count ? $"Action {last}" : "?";

                // Highlight si recent (< 0.5s)
                GUIStyle style = new GUIStyle(EditorStyles.helpBox);
                if (elapsed < 0.5f)
                    style.normal.textColor = Color.green;

                EditorGUILayout.BeginVertical(style);
                EditorGUILayout.LabelField($"Derniere Action : [{last}] {actionName}", EditorStyles.boldLabel);
                EditorGUILayout.LabelField($"Il y a {elapsed:F1}s");
                EditorGUILayout.EndVertical();
            }
            else
            {
                EditorGUILayout.HelpBox("Aucune action executee.", MessageType.Info);
            }

            // Masque d'actions
            EditorGUILayout.Space(5);
            EditorGUILayout.LabelField("Masque d'Actions", EditorStyles.boldLabel);

            for (int i = 0; i < router.actions.Count; i++)
            {
                bool valid = router.IsActionValid(i);
                bool isLast = (i == last);

                GUIStyle rowStyle = new GUIStyle(EditorStyles.label);
                if (!valid)
                    rowStyle.normal.textColor = Color.red;
                else if (isLast && Application.isPlaying && (Time.time - router.LastExecutedTime) < 0.5f)
                    rowStyle.normal.textColor = Color.green;

                string icon = valid ? "O" : "X";
                string highlight = isLast ? "  <<" : "";
                EditorGUILayout.LabelField($"  [{i}] {icon}  Action {i}{highlight}", rowStyle);
            }

            // Force repaint en play mode
            Repaint();
        }
    }
}
#endif
