using System;
using UnityEngine;

namespace Pluminus.Data
{
    /// <summary>
    /// Représente une récompense spécifique liée à une action en jeu.
    /// </summary>
    [Serializable]
    public struct RewardEvent
    {
        [Tooltip("L'identifiant textuel de l'événement (ex: 'PlayerHit', 'TookDamage').")]
        public string flagId;
        
        [Tooltip("La valeur de la récompense (Positive = bonne action, Négative = mauvaise action).")]
        public float rewardValue;
        
        [Tooltip("Si coché, cet événement marque la fin d'une 'séquence' d'apprentissage (ex: Mort de l'IA ou Mort du joueur). L'action suivante sera considérée comme un nouveau départ.")]
        public bool isTerminalState;
    }
}
