using UnityEngine;
using UnityEditor;
using Pluminus.Core;

namespace Pluminus.EditorTools
{
    /// <summary>
    /// Éditeur visuel personnalisé (Custom Editor) pour l'inspecteur d'AdaptiveBrain.
    /// Il ajoute des graphiques et des visualisations en temps réel pour analyser le comportement de l'IA pendant le Play Mode.
    /// </summary>
    [CustomEditor(typeof(AdaptiveBrain))]
    public class AdaptiveBrainEditor : UnityEditor.Editor
    {
        public override void OnInspectorGUI()
        {
            // 1. Dessine l'interface par défaut habituelle (pour paramétrer le BrainConfig et RewardProfile)
            DrawDefaultInspector();

            // Récupère le script AdaptiveBrain attaché à l'objet surveillé
            AdaptiveBrain brain = (AdaptiveBrain)target;

            EditorGUILayout.Space(15);
            EditorGUILayout.LabelField("Monitor Temps Réel (Pluminus Debug)", EditorStyles.boldLabel);

            // Si le jeu ne tourne pas (Mode Édition seulement)
            if (!Application.isPlaying)
            {
                EditorGUILayout.HelpBox("Lancez le jeu (Play Mode) pour observer le comportement de l'IA en temps réel.", MessageType.Info);
                return;
            }

            // Dessine une jolie boîte semi-grisée pour grouper les infos "Live"
            EditorGUILayout.BeginVertical(EditorStyles.helpBox);
            
            // Affichage de l'Epsilon (Taux d'Exploration)
            float epsilon = brain.GetCurrentEpsilon();
            EditorGUILayout.LabelField($"Taux d'Exploration: {(epsilon*100f):F1} %");
            
            // Dessine une vraie barre de chargement colorée (verte/grise) selon l'exploration
            Rect r = EditorGUILayout.GetControlRect(false, 20);
            string progressText = epsilon > 0.051f ? "Phase d'Apprentissage Actif" : "Phase d'Exploitation Optimisée";
            EditorGUI.ProgressBar(r, epsilon, progressText);

            EditorGUILayout.Space(10);

            // Bouton de Debug rapide pour reset l'exploration manuellement en plein jeu
            if (GUILayout.Button("Forcer l'Exploration (Reset Epsilon)"))
            {
                if (brain.brainConfig != null)
                {
                    brain.SetCurrentEpsilon(brain.brainConfig.explorationRate);
                }
            }

            EditorGUILayout.EndVertical();

            EditorGUILayout.Space(15);

            // --- SECTION PERSISTANCE (Save/Load) ---
            EditorGUILayout.LabelField("Persistance de la Mémoire", EditorStyles.boldLabel);
            EditorGUILayout.BeginVertical(EditorStyles.helpBox);
            
            EditorGUILayout.HelpBox("Pour sauvegarder l'IA entraînée ou la pré-charger, glissez un 'Pluminus > Trained Brain' dans le champ 'Memory Asset' plus haut.", MessageType.None);

            GUI.enabled = brain.memoryAsset != null && Application.isPlaying; // Only allow Export/Import when playing and asset is assigned
            
            EditorGUILayout.BeginHorizontal();
            
            if (GUILayout.Button("📥 Charger (Import)"))
            {
                brain.ImportBrain(brain.memoryAsset);
            }

            if (GUILayout.Button("💾 Sauvegarder (Export)"))
            {
                brain.ExportBrain(brain.memoryAsset);
                
                // Indique à Unity que l'asset a été modifié et force la sauvegarde physique du fichier
                EditorUtility.SetDirty(brain.memoryAsset);
                AssetDatabase.SaveAssets();
                
                Debug.Log("[Pluminus] Mémoire sauvegardée sur le disque.");
            }

            EditorGUILayout.EndHorizontal();
            GUI.enabled = true; // Remet l'état normal
            
            EditorGUILayout.EndVertical();

            // Demande à Unity de redessiner l'inspecteur en boucle pour voir la barre bouger en direct (quand l'objet est sélectionné)
            Repaint();
        }
    }
}
