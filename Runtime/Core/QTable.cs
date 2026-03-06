using System;
using System.Collections.Generic;

namespace Pluminus.Core
{
    /// <summary>
    /// La "mémoire" de l'IA. 
    /// C'est un tableau de scores qui associe une situation (Etat) à un score (Q-Value) pour chaque action possible.
    /// </summary>
    [Serializable]
    public class QTable
    {
        // Un dictionnaire où la clé est l'ID de l'Etat, et la valeur est un tableau de scores pour chaque action.
        // Utiliser un dictionnaire économise de la RAM car on ne crée que les états réellement rencontrés.
        public Dictionary<int, float[]> table;
        public int numActions;

        public QTable(int actions)
        {
            table = new Dictionary<int, float[]>();
            numActions = actions;
        }

        /// <summary>
        /// Récupère les scores de toutes les actions pour un état donné. Crée la ligne de score (avec des 0) si l'état est nouveau.
        /// </summary>
        public float[] GetStateValues(int stateId)
        {
            if (!table.ContainsKey(stateId))
            {
                table[stateId] = new float[numActions];
                for (int i = 0; i < numActions; i++)
                {
                    table[stateId][i] = 0f;
                }
            }
            return table[stateId];
        }

        // Récupère le score spécifique d'une action dans un état donné
        public float GetQValue(int stateId, int actionId) => GetStateValues(stateId)[actionId];

        // Modifie le score spécifique d'une action dans un état donné
        public void SetQValue(int stateId, int actionId, float value) => GetStateValues(stateId)[actionId] = value;

        /// <summary>
        /// Trouve le score le plus élevé possible parmi toutes les actions dans un état donné. (Utilisé par l'équation de Bellman)
        /// </summary>
        public float GetMaxQValue(int stateId)
        {
            float[] values = GetStateValues(stateId);
            float max = float.MinValue;
            for (int i = 0; i < values.Length; i++)
            {
                if (values[i] > max)
                    max = values[i];
            }
            return max == float.MinValue ? 0f : max;
        }

        /// <summary>
        /// Trouve l'action avec le meilleur score pour un état donné. (Phase d'Exploitation)
        /// </summary>
        public int GetBestAction(int stateId, Func<int, bool> isValidAction = null)
        {
            float[] values = GetStateValues(stateId);
            float max = float.MinValue;
            int bestAction = 0;
            bool foundValid = false;

            for (int i = 0; i < values.Length; i++)
            {
                // Ignore les actions invalides (ex: en cooldown)
                if (isValidAction != null && !isValidAction(i)) continue;

                if (!foundValid || values[i] > max)
                {
                    max = values[i];
                    bestAction = i;
                    foundValid = true;
                }
            }
            return bestAction;
        }
    }
}
