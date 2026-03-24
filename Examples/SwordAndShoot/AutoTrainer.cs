using UnityEngine;

namespace Pluminus.Examples
{
    /// <summary>
    /// Script permettant d'automatiser l'entraînement de l'IA sur un grand nombre d'itérations.
    /// Idéal pour vérifier la convergence mathématique de l'agent.
    /// </summary>
    public class AutoTrainer : MonoBehaviour
    {
        [Tooltip("Le joueur automatisé")]
        public SimplePlayer player;

        [Tooltip("L'ennemi qui va s'entraîner")]
        public SimpleEnemy enemy;

        [Tooltip("Le nombre d'attaques à simuler d'un coup")]
        public int totalIterations = 100000;

        [ContextMenu("Lancer l'entraînement massif (Script seulement)")]
        public void StartTraining()
        {
            if (player == null || enemy == null)
            {
                Debug.LogError("Veuillez assigner le Player et Enemy dans l'AutoTrainer !");
                return;
            }

            // 1. On coupe temporairement les logs pour ne pas faire crasher l'éditeur Unity avec 100k messages
            player.enableLogs = false;
            enemy.enableLogs = false;

            // 2. On réinitialise les compteurs de rapport de l'ennemi
            enemy.successfulDefends = 0;
            enemy.failedDefends = 0;

            int recentSuccesses = 0;
            int recentFails = 0;

            Debug.Log($"<color=yellow>⏳ Début de l'entraînement intensif : {totalIterations} attaques...</color>");

            // 3. Boucle d'entraînement ultra rapide (En mémoire RAM, ça prend quelques millisecondes)
            for (int i = 0; i < totalIterations; i++)
            {
                // Choix aléatoire entre l'Épée et le Projectile
                AttackType randomAttack = Random.value > 0.5f ? AttackType.Sword : AttackType.Projectile;

                // On force le joueur à attaquer l'ennemi
                player.TriggerAutoAttack(randomAttack);

                // Pour tester si l'IA est devenue intelligente à la fin, on isole les 1000 derniers résultats
                if (i >= totalIterations - 1000)
                {
                    if (enemy.lastActionWasSuccess) recentSuccesses++;
                    else recentFails++;
                }
            }

            // 4. Rapport Final
            Debug.Log($"<color=green>✅ Entraînement Terminé !</color>");
            Debug.Log($"<b>Stats Globales (Inclut le temps long d'exploration au début)</b> : {enemy.successfulDefends} Succès | {enemy.failedDefends} Échecs");
            Debug.Log($"<b>Stats Récentes (Sur les 1000 dernières attaques)</b> : <color=green>{recentSuccesses} Succès</color> | <color=red>{recentFails} Échecs</color> ({((float)recentSuccesses / 10f):F1}% de réussite en fin d'entrainement)");

            // 5. On réactive les logs et on laisse l'IA prête pour le manuel
            player.enableLogs = true;
            enemy.enableLogs = true;
        }
    }
}
