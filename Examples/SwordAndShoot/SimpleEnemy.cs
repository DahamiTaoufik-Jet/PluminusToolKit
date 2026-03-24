using UnityEngine;
using Pluminus.Core;
using Pluminus.Integration;

namespace Pluminus.Examples
{
    /// <summary>
    /// L'IA ennemie qui doit apprendre à parer l'épée et esquiver le projectile venant du SimplePlayer.
    /// </summary>
    [RequireComponent(typeof(AdaptiveBrain))]
    public class SimpleEnemy : MonoBehaviour, IEnvironmentObserver, IActionExecutor
    {
        private AdaptiveBrain brain;
        
        [Tooltip("Glisser/Déposer le joueur de la scène ici.")]
        public SimplePlayer player;

        public bool enableLogs = true;

        [HideInInspector] public int successfulDefends = 0;
        [HideInInspector] public int failedDefends = 0;
        [HideInInspector] public bool lastActionWasSuccess = false;

        private void Awake()
        {
            brain = GetComponent<AdaptiveBrain>();
        }

        // Cette méthode est appelée par le Player quand il appuie sur '1' ou '2'.
        public void ReactToAttack()
        {
            // Demande au cerveau d'observer la situation (quel type d'attaque ?),
            // d'apprendre des résultats de la précédente attaque,
            // et de décider d'une nouvelle action (parer ou esquiver).
            brain.TickDecision();
        }

        // ==========================================
        // 1. OBSERVATION DE L'ENVIRONNEMENT
        // ==========================================

        public int GetMaxStates()
        {
            // Le joueur a 3 états possibles :
            // 0 = Rien / Attente
            // 1 = Epée (Attaque en cours)
            // 2 = Projectile (Attaque en cours)
            return 3;
        }

        public int GetCurrentStateId()
        {
            if (player.CurrentAttack == AttackType.Sword)
            {
                return 1;
            }
            if (player.CurrentAttack == AttackType.Projectile)
            {
                return 2;
            }
            return 0;
        }

        // ==========================================
        // 2. EXÉCUTION DE L'ACTION ET GESTION DES RÉCOMPENSES
        // ==========================================

        public int GetMaxActions()
        {
            // L'ennemi a 2 actions possibles : 
            // 0 = Parer (Block)
            // 1 = Esquiver (Dodge)
            return 2;
        }

        public void ExecuteAction(int actionId)
        {
            // L'IA vient de choisir son action.
            // On vérifie immédiatement si le choix était judicieux ou non par rapport à l'attaque du joueur.
            AttackType incomingAttack = player.CurrentAttack;

            if (actionId == 0) // L'IA a choisi de PARER
            {
                if (incomingAttack == AttackType.Sword)
                {
                    if (enableLogs) Debug.Log("<color=green>✓ L'ennemi PARE l'attaque à l'épée ! -> SUCCÈS</color>");
                    brain.ApplyRewardFlag("GoodChoice"); // Récompense l'IA (+1 point)
                    successfulDefends++;
                    lastActionWasSuccess = true;
                }
                else if (incomingAttack == AttackType.Projectile)
                {
                    if (enableLogs) Debug.Log("<color=red>✗ L'ennemi tente de PARER le projectile, mais une explosion passe à travers sa garde ! -> ÉCHEC</color>");
                    brain.ApplyRewardFlag("BadChoice"); // Punit l'IA (-1 point)
                    failedDefends++;
                    lastActionWasSuccess = false;
                }
            }
            else if (actionId == 1) // L'IA a choisi d'ESQUIVER
            {
                if (incomingAttack == AttackType.Projectile)
                {
                    if (enableLogs) Debug.Log("<color=green>✓ L'ennemi ESQUIVE parfaitement le projectile ! -> SUCCÈS</color>");
                    brain.ApplyRewardFlag("GoodChoice");
                    successfulDefends++;
                    lastActionWasSuccess = true;
                }
                else if (incomingAttack == AttackType.Sword)
                {
                    if (enableLogs) Debug.Log("<color=red>✗ L'ennemi tente d'ESQUIVER, mais le grand coup circulaire d'épée le rattrape ! -> ÉCHEC</color>");
                    brain.ApplyRewardFlag("BadChoice");
                    failedDefends++;
                    lastActionWasSuccess = false;
                }
            }
        }

        public bool IsActionValid(int actionId)
        {
            // Pas de restriction, l'IA peut toujours essayer de parer ou esquiver.
            return true;
        }
    }
}
