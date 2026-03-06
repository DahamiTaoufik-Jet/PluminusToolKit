using UnityEngine;
using UnityEngine.InputSystem;
using Pluminus.Integration;
using Pluminus.Core;

namespace Pluminus.Examples
{
    /// <summary>
    /// Un script d'exemple concret pour tester Pluminus. 
    /// Ce Slime virtuel combat un joueur imaginaire.
    /// Il possède un brain et doit apprendre quand Attaquer(0), Bloquer(1) ou Fuir(2).
    /// </summary>
    [RequireComponent(typeof(AdaptiveBrain))]
    public class DummySlimeEnemy : MonoBehaviour, IEnvironmentObserver, IActionExecutor
    {
        private AdaptiveBrain brain;

        // --- Données simulées du jeu ---
        [Header("Simulation de Jeu (Test)")]
        [Tooltip("La distance actuelle par rapport au joueur")]
        public float distanceToPlayer = 5f;
        
        [Tooltip("Les points de vie actuels du slime")]
        public float currentHealth = 100f;

        [Header("Contrôles (New Input System)")]
        public InputAction tickInput = new InputAction("Tick", type: InputActionType.Button, binding: "<Keyboard>/space");
        public InputAction punishInput = new InputAction("Punish", type: InputActionType.Button, binding: "<Keyboard>/z");
        public InputAction rewardInput = new InputAction("Reward", type: InputActionType.Button, binding: "<Keyboard>/x");

        private void Awake()
        {
            brain = GetComponent<AdaptiveBrain>();
        }

        private void OnEnable()
        {
            tickInput.Enable();
            punishInput.Enable();
            rewardInput.Enable();
        }

        private void OnDisable()
        {
            tickInput.Disable();
            punishInput.Disable();
            rewardInput.Disable();
        }

        private void Update()
        {
            // Dans un vrai jeu, vous appelleriez TickDecision() quand l'ennemi a fini son attaque
            // Ici, pour le test on appuie sur la touche configurée (Espace par défaut) pour le forcer à "réfléchir"
            if (tickInput.WasPressedThisFrame())
            {
                brain.TickDecision();
            }

            // Touches pour simuler des événements de jeu et voir si l'IA apprend
            if (punishInput.WasPressedThisFrame()) 
            {
                Debug.Log("Joueur frappe le Slime ! (Punition)");
                currentHealth -= 20f;
                brain.ApplyRewardFlag("TookDamage"); // Doit être configuré à -1 dans le RewardProfile
            }
            if (rewardInput.WasPressedThisFrame())
            {
                Debug.Log("Le Slime esquive bien ! (Récompense)");
                brain.ApplyRewardFlag("DodgedSuccessfully"); // Doit être configuré à +1
            }
        }

        // ==========================================
        // IMPLEMENTATION : IEnvironmentObserver
        // Transformer le jeu en "Situation" (State ID)
        // ==========================================

        public int GetMaxStates()
        {
            // Nous avons 2 distances (Melee, Range) * 2 états de santé (High, Low) = 4 situations possibles.
            return 4;
        }

        public int GetCurrentStateId()
        {
            // 0 = Melee + High HP
            // 1 = Melee + Low HP
            // 2 = Range + High HP
            // 3 = Range + Low HP

            bool isMelee = distanceToPlayer < 2f;
            bool isLowHp = currentHealth < 30f;

            if (isMelee && !isLowHp) return 0;
            if (isMelee && isLowHp) return 1;
            if (!isMelee && !isLowHp) return 2;
            else return 3; // (!isMelee && isLowHp)
        }

        // ==========================================
        // IMPLEMENTATION : IActionExecutor
        // Transformer "l'Id Action" de l'IA en code Unity
        // ==========================================

        public int GetMaxActions()
        {
            return 3; // 0: Attaquer, 1: Bloquer, 2: Fuir
        }

        public void ExecuteAction(int actionId)
        {
            switch (actionId)
            {
                case 0:
                    Debug.Log("⚔️ L'IA a décidé : ATTAQUER !");
                    // Ex: anim.SetTrigger("Attack");
                    break;
                case 1:
                    Debug.Log("🛡️ L'IA a décidé : BLOQUER !");
                    // Ex: anim.SetBool("IsBlocking", true);
                    break;
                case 2:
                    Debug.Log("🏃 L'IA a décidé : FUIR !");
                    // Ex: navMeshAgent.SetDestination(safeSpot);
                    // On peut lui donner une récompense immédiate s'il a bien fait de fuir !
                    if (currentHealth < 30f) 
                    {
                        Debug.Log("-> Et c'était un bon choix car il avait peu de PV ! (+2 points)");
                        brain.ApplyRewardFlag("GoodFlee");
                    }
                    else
                    {
                        Debug.Log("-> Mauvais choix de fuir, il avait plein de PV ! (-1 point)");
                        brain.ApplyRewardFlag("BadFlee");
                    }
                    break;
            }
        }

        public bool IsActionValid(int actionId)
        {
            // Peut-on faire cette action ?
            return true; // Pour ce test, toutes les actions sont toujours possibles
        }
    }
}
