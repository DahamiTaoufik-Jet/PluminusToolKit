using UnityEngine;

namespace Pluminus.Sensors.Extended
{
    /// <summary>
    /// Capteur d'État d'Action.
    /// Traduit la phase actuelle d'une action complexe (ex: Préparation, Frappe, Récupération).
    /// </summary>
    [AddComponentMenu("Pluminus/Sensors/Action State Sensor")]
    public class ActionStateSensor : PluminusStateSensor
    {
        [Header("Configuration de l'Action")]
        [Tooltip("Nombre total de phases/états possibles pour ce capteur. (Ex: 3 = Repos, Attaque, Esquive)")]
        [Min(1)]
        public int numberOfStates = 3;

        private int currentState = 0;

        public override int GetSubStateCount()
        {
            return numberOfStates;
        }

        public override int GetCurrentSubState()
        {
            // Sécurité pour éviter un out of bounds
            return Mathf.Clamp(currentState, 0, numberOfStates - 1);
        }

        /// <summary>
        /// Méthode à appeler depuis vos propres scripts ou via des Animation Events pour informer l'IA de l'état actuel.
        /// </summary>
        /// <param name="newStateId">L'ID de l'état (entre 0 et numberOfStates - 1)</param>
        public void SetActionState(int newStateId)
        {
            if (newStateId >= 0 && newStateId < numberOfStates)
            {
                currentState = newStateId;
            }
            else
            {
                Debug.LogWarning($"[ActionStateSensor] Tentative d'assigner l'état invalide {newStateId}. Max: {numberOfStates - 1}");
            }
        }
    }
}
