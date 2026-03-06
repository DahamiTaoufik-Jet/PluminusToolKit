using UnityEngine;
using System.Collections.Generic;

namespace Pluminus.Data
{
    /// <summary>
    /// Liste de toutes les récompenses possibles pour un type d'ennemi. (ScriptableObject)
    /// </summary>
    [CreateAssetMenu(fileName = "NewRewardProfile", menuName = "Pluminus/Reward Profile")]
    public class RewardProfile : ScriptableObject
    {
        [Tooltip("Liste des événements générant des récompenses ou des pénalités.")]
        public List<RewardEvent> rewardEvents = new List<RewardEvent>();

        /// <summary>
        /// Cherche si un flag spécifique (ex: "TookDamage") existe dans ce profil et renvoie sa valeur.
        /// </summary>
        public bool TryGetReward(string flag, out RewardEvent rewardEvent)
        {
            foreach (var evt in rewardEvents)
            {
                if (evt.flagId == flag)
                {
                    rewardEvent = evt;
                    return true;
                }
            }
            rewardEvent = default;
            return false;
        }
    }
}
