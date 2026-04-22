#if UNITY_EDITOR
using UnityEditor;
using UnityEngine;
using Pluminus.Sensors;

namespace Pluminus.Sensors.Editor {
    [InitializeOnLoad]
    public static class PluminusSceneTracker {
        private static int lastLoggedStateCount = 0;
        private static bool isWarningLogged = false;

        static PluminusSceneTracker() {
            SceneView.duringSceneGui += OnSceneGUI;
            Selection.selectionChanged += OnSelectionChanged;
        }

        private static void OnSelectionChanged() {
            isWarningLogged = false;
        }

        private static void OnSceneGUI(SceneView sceneView) {
            if (Selection.activeGameObject == null) return;
            
            PluminusEyes eyes = Selection.activeGameObject.GetComponentInChildren<PluminusEyes>();
            if (eyes == null) return;

            int states = eyes.GetTheoreticalMaxStates();

            if (states > 300000 && (!isWarningLogged || lastLoggedStateCount != states)) {
                Debug.LogWarning($"[Pluminus] Matrice lourde detectee : Le nombre total d'etats ({states}) depasse 300 000. L'apprentissage de cette entite sera tres lent.");
                isWarningLogged = true;
                lastLoggedStateCount = states;
            } else if (states <= 300000) {
                isWarningLogged = false;
            }

            Handles.BeginGUI();
            
            GUIStyle style = new GUIStyle(GUI.skin.box);
            style.fontSize = 12;
            style.alignment = TextAnchor.MiddleCenter;
            style.normal.textColor = Color.white;
            
            string text = $"Pluminus Total States: {states}";
            Vector2 size = style.CalcSize(new GUIContent(text));
            
            size.x += 16f;
            size.y += 10f;
            
            Rect rect = new Rect(sceneView.camera.pixelWidth - size.x - 20, sceneView.camera.pixelHeight - size.y - 40, size.x, size.y);
            
            GUI.Box(rect, text, style);
            
            Handles.EndGUI();
            sceneView.Repaint();
        }
    }
}
#endif
