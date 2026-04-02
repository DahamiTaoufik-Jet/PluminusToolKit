using UnityEngine;
using Pluminus.Core;

namespace Pluminus.Integration
{
    /// <summary>
    /// Composant No-Code pour distribuer des récompenses lors de collisions physiques lourdes (Murs, Sols, Ennemis).
    /// </summary>
    [RequireComponent(typeof(Collider2D))]
    public class PluminusCollisionReward : MonoBehaviour
    {
        [Tooltip("Le cerveau de l'IA qui doit recevoir la récompense (si vide, le script cherchera automatiquement sur lui-même ou ses parents)")]
        public AdaptiveBrain targetBrain;

        [Tooltip("Si rempli, la collision ne marchera qu'avec les objets portant ce Tag. Idéal pour repérer le sol 'Ground' !")]
        public string filterTag = "Ground";

        [Tooltip("Le Flag de récompense à appliquer dans la Q-Table (ex: 'TouchGround')")]
        public string rewardFlag;

        [Header("Événements à Écouter")]
        public bool fireOnEnter = true;
        public bool fireOnExit = false;

        private void Awake()
        {
            if (targetBrain == null) targetBrain = GetComponentInParent<AdaptiveBrain>();
        }

        private void OnCollisionEnter2D(Collision2D collision)
        {
            if (!fireOnEnter) return;
            ProcessCollision(collision, "Enter");
        }

        private void OnCollisionExit2D(Collision2D collision)
        {
            if (!fireOnExit) return;
            ProcessCollision(collision, "Exit");
        }

        private void ProcessCollision(Collision2D collision, string state)
        {
            if (targetBrain == null) return;
            if (!string.IsNullOrEmpty(filterTag) && !collision.gameObject.CompareTag(filterTag)) return;

            Debug.Log($"<color=orange>[Pluminus Physics] Collision ({state}) sur '{gameObject.name}' avec '{collision.gameObject.name}' -> Envoi de la récompense '{rewardFlag}' !</color>");
            targetBrain.ApplyRewardFlag(rewardFlag);
        }
    }
}
