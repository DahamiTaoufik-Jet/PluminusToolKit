using UnityEngine;
using Pluminus.Core;

namespace Pluminus.Integration
{
    /// <summary>
    /// Composant No-Code pour distribuer des récompenses lors de collisions physiques 3D (Murs, Sols, Ennemis).
    /// Supporte le filtrage par Tag et les événements Enter/Exit.
    /// </summary>
    [AddComponentMenu("Pluminus/Integration/Pluminus Collision Reward 3D")]
    public class PluminusCollisionReward3D : MonoBehaviour
    {
        [Tooltip("Le cerveau de l'IA qui doit recevoir la récompense (si vide, cherche automatiquement sur lui-même ou ses parents)")]
        public PluminusBrain targetBrain;

        [Tooltip("Si rempli, la collision ne marchera qu'avec les objets portant ce Tag.")]
        public string filterTag = "";

        [Tooltip("Le Flag de récompense à appliquer dans la Q-Table (ex: 'BlockedBullet')")]
        public string rewardFlag;

        [Header("Événements à Écouter")]
        public bool fireOnEnter = true;
        public bool fireOnExit = false;

        private void Awake()
        {
            if (targetBrain == null) targetBrain = GetComponentInParent<PluminusBrain>();
        }

        private void OnCollisionEnter(Collision collision)
        {
            if (!fireOnEnter) return;
            ProcessCollision(collision.gameObject, "Enter");
        }

        private void OnCollisionExit(Collision collision)
        {
            if (!fireOnExit) return;
            ProcessCollision(collision.gameObject, "Exit");
        }

        private void ProcessCollision(GameObject other, string state)
        {
            if (targetBrain == null) return;
            if (!string.IsNullOrEmpty(filterTag) && !other.CompareTag(filterTag)) return;

            Debug.Log($"<color=orange>[Pluminus Physics 3D] Collision ({state}) sur '{gameObject.name}' avec '{other.name}' -> Envoi de la récompense '{rewardFlag}' !</color>");
            targetBrain.ApplyRewardFlag(rewardFlag);
        }
    }
}
