using UnityEngine;
using Pluminus.Core;
using Pluminus.Integration;

namespace Pluminus.Examples.Pokemon
{
    [RequireComponent(typeof(PluminusBrain))]
    public class TypeEnemy : MonoBehaviour, IEnvironmentObserver, IActionExecutor
    {
        private PluminusBrain brain;
        public TypePlayer player;

        public bool enableLogs = true;

        [HideInInspector] public int totalRewards = 0;
        [HideInInspector] public int totalPunishments = 0;
        [HideInInspector] public bool lastActionWasGood = false;
        
        private void Awake()
        {
            brain = GetComponent<PluminusBrain>();
            PokemonTypeChart.InitializeMapping();
        }

        public void ReactToAttack()
        {
            brain.TickDecision();
        }

        // --- IEnvironmentObserver ---
        public int GetMaxStates()
        {
            // L'état est simplement le type de l'attaque entrante (17 types = 17 états)
            return 17;
        }

        public int GetCurrentStateId()
        {
            return (int)player.currentAttack;
        }

        // --- IActionExecutor ---
        public int GetMaxActions()
        {
            // 136 combinaisons de doubles-types possibles
            return PokemonTypeChart.TotalCombinations;
        }

        public void ExecuteAction(int actionId)
        {
            PokeType incomingAttack = player.currentAttack;
            
            // Décoder le choix de l'IA
            var (typeA, typeB) = PokemonTypeChart.DecodeActionId(actionId);
            
            // Calculer les dégâts
            float multiplier = PokemonTypeChart.GetMultiplier(incomingAttack, typeA, typeB);

            // Gérer les récompenses en fonction de l'efficience de la résistance
            if (multiplier == 0f)
            {
                if (enableLogs) Debug.Log($"<color=green>L'IA a choisi {typeA}/{typeB}. Parfaite immunité (x0) contre {incomingAttack} !</color>");
                brain.ApplyRewardFlag("Immune"); // Devra valoir +3
                RegisterResult(true);
            }
            else if (multiplier <= 0.25f)
            {
                if (enableLogs) Debug.Log($"<color=green>L'IA a choisi {typeA}/{typeB}. Double Résistance (x0.25) contre {incomingAttack} !</color>");
                brain.ApplyRewardFlag("DoubleResist"); // Devra valoir +2
                RegisterResult(true);
            }
            else if (multiplier <= 0.5f)
            {
                if (enableLogs) Debug.Log($"<color=yellow>L'IA a choisi {typeA}/{typeB}. Résistance (x0.5) contre {incomingAttack} !</color>");
                brain.ApplyRewardFlag("Resist"); // Devra valoir +1
                RegisterResult(true);
            }
            else if (multiplier == 1f)
            {
                if (enableLogs) Debug.Log($"<color=orange>L'IA a choisi {typeA}/{typeB}. Dégâts Neutres (x1) contre {incomingAttack}.</color>");
                brain.ApplyRewardFlag("Neutral"); // Devra valoir -1
                RegisterResult(false);
            }
            else
            {
                if (enableLogs) Debug.Log($"<color=red>L'IA a choisi {typeA}/{typeB}. FAIBLESSE (x2 ou x4) contre {incomingAttack} !</color>");
                brain.ApplyRewardFlag("Weak"); // Devra valoir -2
                RegisterResult(false);
            }
        }

        private void RegisterResult(bool isGood)
        {
            if (isGood) totalRewards++;
            else totalPunishments++;
            lastActionWasGood = isGood;
        }

        public bool IsActionValid(int actionId)
        {
            return true;
        }
    }
}
