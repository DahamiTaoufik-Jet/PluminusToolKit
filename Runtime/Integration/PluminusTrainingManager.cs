using UnityEngine;
using UnityEngine.Events;
using System.Collections.Generic;
using Pluminus.Core;

namespace Pluminus.Integration
{
    /// <summary>
    /// Une phase de curriculum : apres N episodes, debloque certaines actions sur des ActionRouters cibles.
    /// </summary>
    [System.Serializable]
    public class CurriculumPhase
    {
        [Tooltip("Nom de la phase pour s'y retrouver dans l'inspecteur.")]
        public string phaseName = "Nouvelle Phase";

        [Tooltip("Nombre d'episodes avant de passer a cette phase. Phase 0 = au demarrage.")]
        public int startAtEpisode = 0;

        [Tooltip("Si coche, reset l'epsilon de tous les cerveaux au debut de cette phase pour explorer les nouvelles actions.")]
        public bool resetEpsilonOnEnter = false;

        [Tooltip("Valeur d'epsilon a appliquer au debut de cette phase. 0 = reprend la valeur initiale du BrainConfig.")]
        [Range(0f, 1f)]
        public float epsilonOnEnter = 0f;

        [Tooltip("Masques d'actions a appliquer. Chaque entree cible un ActionRouter different.")]
        public List<CurriculumActionMask> actionMasks = new List<CurriculumActionMask>();
    }

    /// <summary>
    /// Masque d'actions pour un ActionRouter specifique dans une phase de curriculum.
    /// </summary>
    [System.Serializable]
    public class CurriculumActionMask
    {
        [Tooltip("L'ActionRouter sur lequel appliquer le masque.")]
        public PluminusActionRouter targetRouter;

        [Tooltip("Les index des actions autorisees pendant cette phase.")]
        public int[] allowedActions;
    }

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

        [Header("Curriculum (Optionnel)")]
        [Tooltip("Phases progressives d'entrainement. Chaque phase debloque de nouvelles actions apres N episodes. Laissez vide pour desactiver.")]
        public List<CurriculumPhase> curriculum = new List<CurriculumPhase>();

        private int currentPhaseIndex = -1;
        private int totalEpisodesGlobal = 0;

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

            // Applique la phase initiale du curriculum
            if (curriculum.Count > 0)
            {
                ApplyCurriculumPhase(0);
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

            totalEpisodesGlobal++;

            // Verifie si on doit passer a la phase suivante du curriculum
            if (curriculum.Count > 0)
            {
                CheckCurriculumProgress();
            }

            if (trainingMode == TrainingMode.Episode)
            {
                OnReset?.Invoke();
                Debug.Log($"<color=green>[Pluminus] Episode Reset global ({brains.Count} cerveaux, ep. {totalEpisodesGlobal})</color>");
            }
        }

        private void CheckCurriculumProgress()
        {
            // Cherche la phase la plus avancee a laquelle on a droit
            int bestPhase = currentPhaseIndex;
            for (int i = 0; i < curriculum.Count; i++)
            {
                if (totalEpisodesGlobal >= curriculum[i].startAtEpisode)
                    bestPhase = i;
            }

            if (bestPhase != currentPhaseIndex)
            {
                ApplyCurriculumPhase(bestPhase);
            }
        }

        private void ApplyCurriculumPhase(int phaseIndex)
        {
            if (phaseIndex < 0 || phaseIndex >= curriculum.Count) return;

            CurriculumPhase phase = curriculum[phaseIndex];
            currentPhaseIndex = phaseIndex;

            // Applique les masques d'actions sur chaque router cible
            int totalActions = 0;
            for (int i = 0; i < phase.actionMasks.Count; i++)
            {
                CurriculumActionMask mask = phase.actionMasks[i];
                if (mask.targetRouter == null) continue;

                if (mask.allowedActions != null && mask.allowedActions.Length > 0)
                {
                    mask.targetRouter.EnableOnlyActions(mask.allowedActions);
                    totalActions += mask.allowedActions.Length;
                }
                else
                {
                    mask.targetRouter.EnableAllActions();
                    totalActions += mask.targetRouter.GetMaxActions();
                }
            }

            // Reset epsilon sur tous les cerveaux si demande
            if (phase.resetEpsilonOnEnter)
            {
                for (int i = 0; i < brains.Count; i++)
                {
                    if (brains[i] == null) continue;

                    if (phase.epsilonOnEnter > 0f)
                    {
                        brains[i].SetCurrentEpsilon(phase.epsilonOnEnter);
                    }
                    else
                    {
                        // Reprend l'epsilon initial du brain config
                        brains[i].ResetEpsilonToInitial();
                    }
                }
                Debug.Log($"<color=cyan>[Pluminus Curriculum]</color> Epsilon reset sur {brains.Count} cerveaux (target: {(phase.epsilonOnEnter > 0f ? phase.epsilonOnEnter.ToString("F2") : "initial")})");
            }

            Debug.Log($"<color=cyan>[Pluminus Curriculum]</color> Phase {phaseIndex + 1}/{curriculum.Count} : '<b>{phase.phaseName}</b>' (episode {totalEpisodesGlobal}, {phase.actionMasks.Count} routers, {totalActions} actions actives)");
        }
    }
}
