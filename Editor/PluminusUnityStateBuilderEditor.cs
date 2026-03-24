#if UNITY_EDITOR
using UnityEngine;
using UnityEditor;
using Pluminus.Sensors;

namespace Pluminus.Sensors.Editor
{
    [CustomEditor(typeof(PluminusUnityStateBuilder))]
    public class PluminusUnityStateBuilderEditor : UnityEditor.Editor
    {
        public override void OnInspectorGUI()
        {
            PluminusUnityStateBuilder builder = (PluminusUnityStateBuilder)target;

            EditorGUILayout.Space(10);
            GUIStyle titleStyle = new GUIStyle(EditorStyles.boldLabel) { fontSize = 14 };
            EditorGUILayout.LabelField("🎮 Pluminus No-Code State Builder", titleStyle);
            EditorGUILayout.HelpBox("Ce composant remplace le script manuel IEnvironmentObserver. Il scanne tous les capteurs Pluminus sur cet objet et génère l'ID d'État à la volée.", MessageType.Info);

            EditorGUILayout.Space(10);
            EditorGUILayout.LabelField("Capteurs Actifs", EditorStyles.boldLabel);

            PluminusStateSensor[] sensors = builder.GetComponents<PluminusStateSensor>();
            
            if (sensors.Length == 0)
            {
                EditorGUILayout.HelpBox("Ajoutez des capteurs (ex: DistanceToTargetSensor) via 'Add Component' pour commencer.", MessageType.Warning);
            }
            else
            {
                int totalStates = builder.GetMaxStates();
                
                GUIStyle sensorStyle = new GUIStyle(EditorStyles.helpBox);
                foreach (var sensor in sensors)
                {
                    EditorGUILayout.BeginVertical(sensorStyle);
                    EditorGUILayout.LabelField($"🧠 {sensor.sensorName}", EditorStyles.boldLabel);
                    EditorGUILayout.LabelField($"Type : {sensor.GetType().Name}");
                    EditorGUILayout.LabelField($"États générés : {sensor.GetSubStateCount()}", EditorStyles.miniLabel);
                    EditorGUILayout.EndVertical();
                }

                EditorGUILayout.Space(10);

                if (totalStates > 5000)
                {
                    EditorGUILayout.HelpBox($"⚠️ MATRICE LOURDE : {totalStates} d'états ! L'IA va mettre du temps à tout explorer. Réduisez le nombre de capteurs.", MessageType.Warning);
                }
                else
                {
                    EditorGUILayout.HelpBox($"✅ Matrice Optimisée. Nombre total d'États d'exploration : {totalStates}", MessageType.Info);
                }
            }
            
            if (GUILayout.Button("Actualiser la Matrice"))
            {
                // Force le rafraîchissement
                Repaint();
            }

            EditorGUILayout.Space(10);
            DrawDefaultInspector();
        }
    }
}
#endif
