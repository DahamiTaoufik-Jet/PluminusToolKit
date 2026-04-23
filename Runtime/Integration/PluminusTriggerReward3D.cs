using UnityEngine;
using Pluminus.Core;

namespace Pluminus.Integration
{
    /// <summary>
    /// Composant No-Code pour distribuer des récompenses lors de contacts physiques 3D (Triggers / Zones fantômes).
    /// Supporte le filtrage par Tag, la destruction de l'objet en contact, et le StompCheck.
    /// </summary>
    [AddComponentMenu("Pluminus/Integration/Pluminus Trigger Reward 3D")]
    public class PluminusTriggerReward3D : MonoBehaviour
    {
        [Tooltip("Le cerveau de l'IA qui doit recevoir la récompense (si vide, cherche automatiquement sur lui-même ou ses parents)")]
        public PluminusBrain targetBrain;

        [Tooltip("Si rempli, le trigger ne réagira qu'avec les objets portant ce Tag (ex: 'Bullet')")]
        public string filterTag = "";

        [Tooltip("Le Flag de récompense à appliquer dans la Q-Table (ex: 'HitByBullet')")]
        public string rewardFlag;

        [Header("Options Avancées (No-Code)")]
        [Tooltip("Si vrai, détruit l'objet qui vient de rentrer dans la zone")]
        public bool destroyInflicter = false;

        [Tooltip("Si vrai, n'applique la récompense QUE si l'IA est plus HAUTE que l'objet touché")]
        public bool requireHitFromTop = false;

        private void Awake()
        {
            if (targetBrain == null) targetBrain = GetComponentInParent<PluminusBrain>();
        }

        private void OnTriggerEnter(Collider other)
        {
            if (targetBrain == null) return;
            if (!string.IsNullOrEmpty(filterTag) && !other.CompareTag(filterTag)) return;

            if (requireHitFromTop)
            {
                if (other.transform.position.y > transform.position.y) return;
            }

            Debug.Log($"<color=orange>[Pluminus Physics 3D] Trigger sur '{gameObject.name}' touché par '{other.name}' -> Envoi de la récompense '{rewardFlag}' !</color>");
            targetBrain.ApplyRewardFlag(rewardFlag);

            if (destroyInflicter)
            {
                Destroy(other.gameObject);
            }
        }
    }
}
