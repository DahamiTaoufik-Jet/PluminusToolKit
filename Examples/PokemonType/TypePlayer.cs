using UnityEngine;

namespace Pluminus.Examples.Pokemon
{
    public class TypePlayer : MonoBehaviour
    {
        public TypeEnemy targetEnemy;
        
        [HideInInspector]
        public PokeType currentAttack = PokeType.Normal;
        
        public bool enableLogs = true;

        void Update()
        {
            // On peut tester manuellement en appuyant sur Espace (Nouveau Input System)
            if (UnityEngine.InputSystem.Keyboard.current != null && UnityEngine.InputSystem.Keyboard.current.spaceKey.wasPressedThisFrame)
            {
                // Sélectionne une attaque aléatoire pour le test
                currentAttack = (PokeType)Random.Range(0, 17);
                if (enableLogs) Debug.Log($"<color=cyan>Joueur : Lance une attaque de type {currentAttack} !</color>");
                PerformAttack();
            }
        }

        public void TriggerAutoAttack(PokeType attack)
        {
            currentAttack = attack;
            PerformAttack();
        }

        private void PerformAttack()
        {
            if (targetEnemy != null)
                targetEnemy.ReactToAttack();
        }
    }
}
