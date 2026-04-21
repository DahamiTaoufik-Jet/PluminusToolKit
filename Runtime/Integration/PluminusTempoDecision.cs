using UnityEngine;
using UnityEngine.Events;
using Pluminus.Core;

namespace Pluminus.Integration
{
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

        [Header("Gestion d'Épisode (Reset)")]
        [Tooltip("Point de départ pour le Soft Reset (laisse vide pour utiliser la position au Start).")]
        public Transform startPoint;
        public UnityEvent OnReset;

        private Vector3 spawnPosition;
        private Quaternion spawnRotation;

        private void Awake()
        {
            if (brain == null) brain = GetComponent<PluminusBrain>();
            
            spawnPosition = (startPoint != null) ? startPoint.position : transform.position;
            spawnRotation = (startPoint != null) ? startPoint.rotation : transform.rotation;
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
        /// Téléporte l'agent au début et réinitialise son état interne.
        /// À appeler depuis vos événements de collision/mort via l'inspecteur.
        /// </summary>
        public void PerformSoftReset()
        {
            if (brain != null) brain.EndEpisode();

            transform.position = spawnPosition;
            transform.rotation = spawnRotation;
            
            // Si l'objet a un Rigidbody, on le fige
            Rigidbody rb = GetComponent<Rigidbody>();
            if (rb != null)
            {
                rb.velocity = Vector3.zero;
                rb.angularVelocity = Vector3.zero;
            }

            timer = 0;
            OnReset?.Invoke();
            
            Debug.Log($"<color=green>[Pluminus] Soft Reset effectué sur {gameObject.name}</color>");
        }
    }
}
