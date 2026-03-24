using UnityEngine;

namespace Pluminus.Examples.Pokemon
{
    public class TypeAutoTrainer : MonoBehaviour
    {
        public TypePlayer player;
        public TypeEnemy enemy;
        public int totalIterations = 100000;

        [ContextMenu("Lancer l'entraînement massif")]
        public void StartTraining()
        {
            if (player == null || enemy == null) return;

            player.enableLogs = false;
            enemy.enableLogs = false;

            enemy.totalRewards = 0;
            enemy.totalPunishments = 0;

            int recentSuccesses = 0;
            int recentNeutralOrWeak = 0;

            Debug.Log($"<color=yellow>⏳ Entraînement de l'IA ({totalIterations} attaques aléatoires)...</color>");

            for (int i = 0; i < totalIterations; i++)
            {
                // À la demande : On bombarde la console pour les 150 derniers coups afin de voir en direct 
                // les exactes combinaisons qu'il choisit à la fin de son entraînement !
                if (i == totalIterations - 150)
                {
                    player.enableLogs = true;
                    enemy.enableLogs = true;
                    Debug.Log("\n<color=yellow>--- DÉBUT DES LOGS EN TEMPS RÉEL (150 derniers coups) ---</color>");
                }

                PokeType randomAttack = (PokeType)Random.Range(0, 17);
                player.TriggerAutoAttack(randomAttack);

                if (i >= totalIterations - 1000)
                {
                    if (enemy.lastActionWasGood) recentSuccesses++;
                    else recentNeutralOrWeak++;
                }
            }

            Debug.Log($"\n<color=green>✅ Mémorisation Terminée !</color>");
            Debug.Log($"<b>Bilan Global de l'Apprentissage</b> : {enemy.totalRewards} Choix Résistants | {enemy.totalPunishments} Mauvais Choix (Neutre/Faible)");
            
            float successRate = ((float)recentSuccesses / 10f);
            Debug.Log($"<b>Fiabilité Récente (Sur les 1000 derniers coups)</b> : {successRate:F1}% de réussite.");

            // Épluchage de la Q-Table pour faire un rapport détaillé des connaissances acquises
            Debug.Log("\n<color=magenta>--- RAPPORT DES 17 STRATÉGIES FINALES RETENUES PAR L'IA ---</color>");
            var brain = enemy.GetComponent<Pluminus.Core.AdaptiveBrain>();
            var qTable = brain.GetCurrentQTable();
            
            for (int state = 0; state < 17; state++)
            {
                PokeType incomingAttack = (PokeType)state;
                // On extrait la meilleure action apprise par le cerveau pour cet état (null signifie qu'aucune action n'est interdite)
                int bestActionId = qTable.GetBestAction(state, null);
                
                var (defA, defB) = PokemonTypeChart.DecodeActionId(bestActionId);
                float multiplier = PokemonTypeChart.GetMultiplier(incomingAttack, defA, defB);
                
                string qualite = multiplier == 0f ? "<color=green>Immunité Totale (x0)</color>" : multiplier <= 0.25f ? "<color=green>Excellente Résistance (x0.25)</color>" : "<color=yellow>Bonne Résistance (x0.5)</color>";
                
                Debug.Log($"Si l'attaque entrante est [<b>{incomingAttack}</b>] ➔ L'IA choisira de devenir [<b>{defA}/{defB}</b>]. Dégâts subis : {qualite}");
            }

            // Remise en état des logs pour l'utilisation manuelle
            player.enableLogs = true;
            enemy.enableLogs = true;
        }
    }
}
