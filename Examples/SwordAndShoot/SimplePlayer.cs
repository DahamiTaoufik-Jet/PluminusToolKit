using UnityEngine;
using UnityEngine.InputSystem;

namespace Pluminus.Examples
{
    public enum AttackType { None, Sword, Projectile }

    /// <summary>
    /// Contrôleur basique pour le joueur permettant d'attaquer avec une épée ou de tirer un projectile.
    /// Utilise désormais le nouveau InputSystem (InputSystem_Actions).
    /// </summary>
    public class SimplePlayer : MonoBehaviour
    {
        [Tooltip("Glisser/Déposer le composant SimpleEnemy ici.")]
        public SimpleEnemy targetEnemy;

        [HideInInspector]
        public AttackType CurrentAttack = AttackType.None;
        
        public bool enableLogs = true;

        private InputSystem_Actions inputActions;

        private void Awake()
        {
            inputActions = new InputSystem_Actions();
        }

        private void OnEnable()
        {
            inputActions.Enable();
        }

        private void OnDisable()
        {
            inputActions.Disable();
        }

        void Update()
        {
            // Vérifie si l'ennemi est assigné (sinon on ne fait rien)
            if (targetEnemy == null) return;

            // L'action "Previous" est assignée à la touche 1 par défaut dans l'InputSystem_Actions
            if (inputActions.Player.Previous.WasPressedThisFrame())
            {
                CurrentAttack = AttackType.Sword;
                if (enableLogs) Debug.Log("<color=cyan>Joueur : Lance une attaque à l'épée !</color>");
                PerformAttack();
            }
            // L'action "Next" est assignée à la touche 2 par défaut dans l'InputSystem_Actions
            else if (inputActions.Player.Next.WasPressedThisFrame())
            {
                CurrentAttack = AttackType.Projectile;
                if (enableLogs) Debug.Log("<color=cyan>Joueur : Tire un projectile !</color>");
                PerformAttack();
            }
        }

        // --- Méthode appelée par l'AutoTrainer (silencieuse) ---
        public void TriggerAutoAttack(AttackType attack)
        {
            CurrentAttack = attack;
            PerformAttack();
        }

        private void PerformAttack()
        {
            // Pousse l'ennemi à prendre une décision instantanément face à cette attaque
            targetEnemy.ReactToAttack();

            // On remet l'état à None après la résolution
            CurrentAttack = AttackType.None;
        }
    }
}
