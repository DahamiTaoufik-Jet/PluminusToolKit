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
    [AddComponentMenu("Pluminus/Integration/Pluminus Agent Manager")]
    public class PluminusAgentManager : MonoBehaviour
    {
        [Header("Cible")]
        public AdaptiveBrain brain;

        [Header("Rythme de Décision")]
        [Tooltip("Si coché, l'IA décide automatiquement à intervalle régulier.")]
        public bool autoTick = true;
        [Tooltip("Temps en secondes entre chaque décision de l'IA.")]
        public float decisionRate = 0.1f;
        private float timer;

        [Header("Accélérateur de Temps")]
        [Range(1f, 100f)]
        [Tooltip("Accélère le temps du jeu pour entraîner l'IA plus vite.")]
        public float trainingSpeed = 1f;

        [Header("Gestion d'Épisode (Reset)")]
        [Tooltip("Point de départ pour le Soft Reset (laisse vide pour utiliser la position au Start).")]
        public Transform startPoint;
        public UnityEvent OnReset;

        private Vector3 spawnPosition;
        private Quaternion spawnRotation;

        private void Awake()
        {
            if (brain == null) brain = GetComponent<AdaptiveBrain>();
            
            spawnPosition = (startPoint != null) ? startPoint.position : transform.position;
            spawnRotation = (startPoint != null) ? startPoint.rotation : transform.rotation;
        }

        private void Update()
        {
            // Applique l'accélérateur de temps
            if (Time.timeScale != trainingSpeed) Time.timeScale = trainingSpeed;

            if (autoTick && brain != null && !brain.useHeuristic)
            {
                timer += Time.deltaTime;
                if (timer >= decisionRate)
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
