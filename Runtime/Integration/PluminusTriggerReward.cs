using UnityEngine;
using Pluminus.Core;

namespace Pluminus.Integration
{
    /// <summary>
    /// Composant No-Code pour distribuer des récompenses lors de contacts physiques (Triggers / Zones fantômes).
    /// </summary>
    [RequireComponent(typeof(Collider2D))]
    public class PluminusTriggerReward : MonoBehaviour
    {
        [Tooltip("Le cerveau de l'IA qui doit recevoir la récompense (si vide, le script cherchera automatiquement sur lui-même ou ses parents)")]
        public PluminusBrain targetBrain;

        [Tooltip("Si rempli, la collision ne marchera qu'avec les objets portant ce Tag (ex: 'Enemy')")]
        public string filterTag = "Enemy";

        [Tooltip("Le Flag de récompense à appliquer dans la Q-Table (ex: 'HitAir')")]
        public string rewardFlag;

        [Header("Options Avancées (No-Code)")]
        [Tooltip("Si vrai, détruit l'objet qui vient de rentrer dans la zone (ex: la boule de feu disparaît lors de l'impact)")]
        public bool destroyInflicter = false;

        [Tooltip("Idéal pour Mario : Si vrai, n'applique la récompense QUE si l'IA est située physiquement PLUS HAUT que l'objet touché (Stomp parfait).")]
        public bool requireHitFromTop = false;

        private void Awake()
        {
            if (targetBrain == null) targetBrain = GetComponentInParent<PluminusBrain>();
        }

        private void OnTriggerEnter2D(Collider2D collision)
        {
            if (targetBrain == null) return;
            if (!string.IsNullOrEmpty(filterTag) && !collision.CompareTag(filterTag)) return;

            // Vérification de l'angle d'impact (No-Code Stomp)
            if (requireHitFromTop)
            {
                // Si l'objet qui nous touche n'est PAS strictement en dessous de nous, on annule
                if (collision.transform.position.y > transform.position.y) return;
            }

            Debug.Log($"<color=orange>[Pluminus Physics] Trigger sur '{gameObject.name}' touché par '{collision.name}' -> Envoi de la récompense '{rewardFlag}' !</color>");
            targetBrain.ApplyRewardFlag(rewardFlag);

            if (destroyInflicter)
            {
                Destroy(collision.gameObject);
            }
        }
    }
}
