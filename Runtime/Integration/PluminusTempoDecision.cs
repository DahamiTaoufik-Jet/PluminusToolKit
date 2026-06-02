using UnityEngine;
using UnityEngine.Events;
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
    /// Le Manager d'Agent No-Code.
    /// Ce script sert de 'colle' entre votre personnage et Pluminus sans que vous ayez à coder.
    /// Il gère le rythme de décision (Tick) et la réinitialisation (Soft Reset).
    /// </summary>
    [AddComponentMenu("Pluminus/Integration/Pluminus Tempo Decision")]
    public class PluminusTempoDecision : MonoBehaviour
    {
        [Header("Cible")]
        public PluminusBrain brain;

        [Header("Mode d'Entrainement")]
        [Tooltip("Infinite = l'agent ne se reset jamais (HP infini, adaptation temps reel). Episode = cycle reset automatique entre chaque episode.")]
        public TrainingMode trainingMode = TrainingMode.Episode;

        [Header("Rythme de Décision")]
        [Tooltip("Si coché, l'IA décide automatiquement à intervalle régulier.")]
        public bool autoTick = true;
        [Tooltip("Temps de base entre chaque décision (à x1).")]
        public float decisionRate = 0.1f;
        [Tooltip("Si coché, l'IA décidera plus souvent quand le jeu est accéléré pour éviter de rater des obstacles.")]
        public bool dynamicDecisionRate = true;
        private float timer;

        [Header("Accélérateur de Temps")]
        [Range(1f, 100f)]
        [Tooltip("Accélère le temps du jeu pour entraîner l'IA plus vite.")]
        public float trainingSpeed = 1f;
        [Tooltip("Si coché, limite les FPS pour éviter que ton PC freeze pendant l'entraînement intensif.")]
        public bool limitFrameRate = true;
        public int targetFrameRate = 60;

        [Header("Gestion d'Épisode (Mode Episode uniquement)")]
        [Tooltip("Declenche quand un episode se termine. Glissez vos PluminusResetable.ResetToInitial() ici !")]
        public UnityEvent OnReset;

        private void Awake()
        {
            if (brain == null) brain = GetComponent<PluminusBrain>();
        }

        private void Update()
        {
            // Applique l'accélérateur de temps
            if (Time.timeScale != trainingSpeed) Time.timeScale = trainingSpeed;

            // Protection Anti-Freeze PC : Limite le CPU/GPU pendant l'entraînement
            if (limitFrameRate && trainingSpeed > 1f)
            {
                if (Application.targetFrameRate != targetFrameRate)
                {
                    Application.targetFrameRate = targetFrameRate;
                    QualitySettings.vSyncCount = 0; // Obligatoire pour laisser la main au targetFrameRate
                }
            }
            else if (Application.targetFrameRate != -1 && trainingSpeed <= 1.1f)
            {
                Application.targetFrameRate = -1; // Rend la main à Unity en mode normal
            }

            if (autoTick && brain != null && !brain.useHeuristic)
            {
                // On calcule le seuil : si dynamique, on divise par la vitesse pour garder la même précision temporelle
                float currentDecisionRate = dynamicDecisionRate ? (decisionRate / Mathf.Max(1f, trainingSpeed)) : decisionRate;
                
                // On utilise unscaledDeltaTime pour un contrôle précis du rythme par rapport à la vitesse réelle
                timer += Time.unscaledDeltaTime;

                if (timer >= currentDecisionRate)
                {
                    brain.TickDecision();
                    timer = 0;
                }
            }
        }

        /// <summary>
        /// Termine l'episode et declenche le reset de tous les objets branches sur OnReset.
        /// En mode Infinite, seul EndEpisode est appele (comptabilite des stats) sans reset.
        /// </summary>
        public void PerformSoftReset()
        {
            if (brain != null) brain.EndEpisode();

            if (trainingMode == TrainingMode.Episode)
            {
                timer = 0;
                OnReset?.Invoke();
                Debug.Log($"<color=green>[Pluminus] Episode Reset sur {gameObject.name}</color>");
            }
        }
    }
}
