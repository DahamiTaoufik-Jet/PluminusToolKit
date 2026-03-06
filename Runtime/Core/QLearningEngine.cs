namespace Pluminus.Core
{
    /// <summary>
    /// Le moteur mathématique principal (pur C#).
    /// Il gère la prise de décision et la mise à jour des scores via l'équation de Bellman.
    /// </summary>
    public class QLearningEngine
    {
        private QTable qTable; // La mémoire de l'IA

        public QLearningEngine(int numActions)
        {
            qTable = new QTable(numActions);
        }

        public QTable GetQTable() => qTable;
        public void SetQTable(QTable table) => qTable = table;

        /// <summary>
        /// Applique la formule mathématique du Q-Learning (Équation de Bellman) pour mettre à jour la mémoire.
        /// Formule: Nouveau Q = Q actuel + Alpha * (Récompense + Gamma * Max Q suivant - Q actuel)
        /// </summary>
        public void UpdateQValue(int previousState, int actionTaken, float reward, int currentState, float alpha, float gamma)
        {
            // 1. Quel était le score de l'action qu'on vient de faire ?
            float currentQ = qTable.GetQValue(previousState, actionTaken);
            
            // 2. Quelle est la meilleure opportunité dans la NOUVELLE situation actuelle ?
            float maxNextQ = qTable.GetMaxQValue(currentState);

            // 3. Calcul du nouveau score
            float newQ = currentQ + alpha * (reward + gamma * maxNextQ - currentQ);
            
            // 4. Enregistrement dans la mémoire
            qTable.SetQValue(previousState, actionTaken, newQ);
        }

        /// <summary>
        /// Décide de la prochaine action à effectuer, en gérant le compromis "Exploration vs Exploitation".
        /// </summary>
        public int DecideAction(int currentState, float epsilon, System.Func<int, bool> isValidAction = null)
        {
            // --- EXPLORATION ---
            // On tire un nombre au hasard. S'il est sous "epsilon", on fait un choix aléatoire.
            if (UnityEngine.Random.value < epsilon)
            {
                int randomAction = UnityEngine.Random.Range(0, qTable.numActions);
                // On vérifie que cette action aléatoire est autorisée
                if (isValidAction == null || isValidAction(randomAction)) 
                {
                    return randomAction;
                }
            }

            // --- EXPLOITATION ---
            // Si on n'explore pas, ou si l'action aléatoire était invalide, on prend la meilleure action connue.
            return qTable.GetBestAction(currentState, isValidAction);
        }
    }
}
