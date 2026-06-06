using UnityEngine;
using UnityEngine.Events;
using System.Collections.Generic;
using Pluminus.Core;

namespace Pluminus.Integration
{
    public enum TrainingMode
    {
        [Tooltip("L'agent tourne en continu sans jamais se reinitialiser. Ideal pour un ennemi qui s'adapte en temps reel au joueur.")]
        Infinite,

        [Tooltip("L'agent vit des cycles courts : debut -> actions -> condition terminale -> reset -> recommence. Ideal pour l'entrainement pur.")]
        Episode
    }

    /// <summary>
    /// Orchestrateur de Scene (1 seul par scene).
    /// Gere l'acceleration du temps, le mode d'entrainement (Infinite/Episode), et le reset global.
    /// Les cerveaux se tickent eux-memes a leur propre rythme.
    /// </summary>
    [AddComponentMenu("Pluminus/Integration/Pluminus Training Manager")]
    public class PluminusTrainingManager : MonoBehaviour
    {
        [Header("Mode d'Entrainement")]
        [Tooltip("Infinite = pas de reset, les agents s'adaptent en continu. Episode = cycle reset automatique entre chaque episode.")]
        public TrainingMode trainingMode = TrainingMode.Episode;

        [Header("Accelerateur de Temps")]
        [Range(1f, 100f)]
        [Tooltip("Accelere le temps du jeu pour entrainer l'IA plus vite.")]
        public float trainingSpeed = 1f;
        [Tooltip("Si coche, limite les FPS pour eviter que le PC freeze pendant l'entrainement intensif.")]
        public bool limitFrameRate = true;
        public int targetFrameRate = 60;

        [Header("Cerveaux de la Scene")]
        [Tooltip("Tous les cerveaux a gerer. Si vide, les detecte automatiquement au Start.")]
        public List<PluminusBrain> brains = new List<PluminusBrain>();

        [Header("Auto-Stop (Editor Only)")]
        [Tooltip("Si > 0, arrete automatiquement le Play Mode apres N minutes (temps reel). Sauvegarde tous les cerveaux avant l'arret.")]
        public float autoStopAfterMinutes = 0f;

        private float autoStopTimer = 0f;

        [Header("Gestion d'Episode (Mode Episode uniquement)")]
        [Tooltip("Declenche quand un episode se termine. Glissez vos PluminusResetable.ResetToInitial() et Health.ResetHealth() ici !")]
        public UnityEvent OnReset;

        private void Start()
        {
            if (brains.Count == 0)
            {
                PluminusBrain[] found = FindObjectsByType<PluminusBrain>(FindObjectsSortMode.None);
                foreach (var b in found)
                {
                    brains.Add(b);
                }
            }

            if (brains.Count == 0)
            {
                Debug.LogWarning("[Pluminus] TrainingManager: Aucun PluminusBrain trouve dans la scene !");
            }
        }

        private void Update()
        {
            // Applique l'accelerateur de temps
            if (Time.timeScale != trainingSpeed) Time.timeScale = trainingSpeed;

            // Protection Anti-Freeze PC : Limite le CPU/GPU pendant l'entrainement
            if (limitFrameRate && trainingSpeed > 1f)
            {
                if (Application.targetFrameRate != targetFrameRate)
                {
                    Application.targetFrameRate = targetFrameRate;
                    QualitySettings.vSyncCount = 0;
                }
            }
            else if (Application.targetFrameRate != -1 && trainingSpeed <= 1.1f)
            {
                Application.targetFrameRate = -1;
            }

            // Auto-stop : arrete le Play Mode apres N minutes reelles
#if UNITY_EDITOR
            if (autoStopAfterMinutes > 0f)
            {
                autoStopTimer += Time.unscaledDeltaTime;
                if (autoStopTimer >= autoStopAfterMinutes * 60f)
                {
                    // Dernier save de tous les cerveaux
                    foreach (var brain in brains)
                    {
                        if (brain != null && brain.memoryAsset != null)
                        {
                            brain.ExportBrain(brain.memoryAsset);
                            UnityEditor.EditorUtility.SetDirty(brain.memoryAsset);
                        }
                    }
                    UnityEditor.AssetDatabase.SaveAssets();
                    Debug.Log($"<color=yellow>[Pluminus AutoStop]</color> {autoStopAfterMinutes} min ecoulees. Arret du Play Mode. ({brains.Count} cerveaux sauvegardes)");
                    UnityEditor.EditorApplication.isPlaying = false;
                    return;
                }
            }
#endif
        }

        /// <summary>
        /// Termine l'episode pour TOUS les cerveaux et declenche le reset global.
        /// En mode Infinite, seul EndEpisode est appele (comptabilite des stats) sans reset.
        /// </summary>
        public void PerformSoftReset()
        {
            foreach (var brain in brains)
            {
                if (brain != null) brain.EndEpisode();
            }

            if (trainingMode == TrainingMode.Episode)
            {
                OnReset?.Invoke();
                Debug.Log($"<color=green>[Pluminus] Episode Reset global ({brains.Count} cerveaux)</color>");
            }
        }
    }
}
