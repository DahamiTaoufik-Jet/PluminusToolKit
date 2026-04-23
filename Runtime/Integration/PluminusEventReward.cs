using UnityEngine;
using Pluminus.Core;

namespace Pluminus.Integration
{
    /// <summary>
    /// Répartiteur d'Événements (No-Code).
    /// Permet de donner ou retirer des points à l'IA depuis n'importe quel UnityEvent de l'inspecteur.
    /// Idéal pour les scripts existants qui ont déjà des événements (OnHit, OnDeath, OnScore...).
    /// </summary>
    [AddComponentMenu("Pluminus/Integration/Pluminus Event Reward")]
    public class PluminusEventReward : MonoBehaviour
    {
        [Tooltip("Le cerveau de l'IA qui doit recevoir la récompense (si vide, cherche automatiquement sur lui-même ou ses parents)")]
        public PluminusBrain targetBrain;

        [Tooltip("Le Flag de récompense par défaut utilisé par les fonctions simples")]
        public string defaultRewardFlag;

        private void Awake()
        {
            if (targetBrain == null) targetBrain = GetComponentInParent<PluminusBrain>();
        }

        /// <summary>
        /// Déclenche la récompense par défaut. Glissez cette fonction dans un UnityEvent de l'inspecteur !
        /// </summary>
        public void TriggerReward()
        {
            if (targetBrain == null || string.IsNullOrEmpty(defaultRewardFlag)) return;
            Debug.Log($"<color=yellow>[Pluminus Event]</color> '{defaultRewardFlag}' déclenché sur '{gameObject.name}'");
            targetBrain.ApplyRewardFlag(defaultRewardFlag);
        }

        /// <summary>
        /// Déclenche une récompense par nom de flag. Utile si plusieurs événements différents doivent utiliser le même composant.
        /// </summary>
        public void TriggerRewardByFlag(string flag)
        {
            if (targetBrain == null || string.IsNullOrEmpty(flag)) return;
            Debug.Log($"<color=yellow>[Pluminus Event]</color> '{flag}' déclenché sur '{gameObject.name}'");
            targetBrain.ApplyRewardFlag(flag);
        }

        /// <summary>
        /// Injecte directement une valeur de récompense (positif = bonus, négatif = pénalité).
        /// </summary>
        public void AddDirectReward(float amount)
        {
            if (targetBrain == null) return;
            Debug.Log($"<color=yellow>[Pluminus Event]</color> Récompense directe de {amount} sur '{gameObject.name}'");
            targetBrain.AddReward(amount);
        }

        /// <summary>
        /// Termine l'épisode. Idéal pour un événement de "Mort" ou "Game Over".
        /// </summary>
        public void EndEpisode()
        {
            if (targetBrain == null) return;
            Debug.Log($"<color=yellow>[Pluminus Event]</color> Fin d'épisode déclenchée sur '{gameObject.name}'");
            targetBrain.EndEpisode();
        }

        /// <summary>
        /// Donne une pénalité et termine immédiatement l'épisode (Game Over instantané).
        /// </summary>
        public void TriggerGameOver(string flag)
        {
            if (targetBrain == null) return;
            targetBrain.ApplyRewardFlag(flag);
            targetBrain.EndEpisode();
        }
    }
}
