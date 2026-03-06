using UnityEngine;

namespace Pluminus.Data
{
    /// <summary>
    /// Configuration principale de l'IA. 
    /// Utilisé comme un "cerveau de base" partagé par plusieurs ennemis (ScriptableObject).
    /// </summary>
    [CreateAssetMenu(fileName = "NewBrainConfig", menuName = "Pluminus/Brain Config")]
    public class BrainConfig : ScriptableObject
    {
        [Header("Hyperparamètres du Q-Learning")]
        [Tooltip("Learning Rate (Alpha) - Vitesse d'apprentissage. 1 = Ne retient que la dernière info, 0 = N'apprend rien.")]
        [Range(0.01f, 1f)]
        public float learningRate = 0.1f;

        [Tooltip("Discount Factor (Gamma) - Vision à long terme. 1 = Pense aux récompenses futures, 0 = Ne pense qu'à la récompense immédiate.")]
        [Range(0f, 1f)]
        public float discountFactor = 0.9f;

        [Header("Exploration vs Exploitation")]
        [Tooltip("Exploration Rate initial (Epsilon) - Pourcentage de chance que l'IA fasse une action au hasard pour tester de nouvelles choses (1 = 100%).")]
        [Range(0f, 1f)]
        public float explorationRate = 0.2f;

        [Tooltip("La valeur minimale à laquelle l'exploration peut descendre (on garde toujours un peu d'aléatoire pour éviter que l'IA soit prévisible).")]
        [Range(0f, 1f)]
        public float minExplorationRate = 0.05f;

        [Tooltip("A quelle vitesse l'exploration diminue à chaque apprentissage. (ex: 0.99 réduit l'aléatoire de 1% à chaque action)")]
        public float explorationDecayRate = 0.99f;

        [Header("Contrôles")]
        [Tooltip("Si désactivé, l'IA utilisera ce qu'elle a déjà appris mais n'apprendra plus rien de nouveau.")]
        public bool isLearningEnabled = true;
    }
}
