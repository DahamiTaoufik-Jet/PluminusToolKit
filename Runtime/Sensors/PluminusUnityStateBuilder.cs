using UnityEngine;
using System.Collections.Generic;
using Pluminus.Integration;

namespace Pluminus.Sensors
{
    /// <summary>
    /// Le Cœur du système No-Code Pluminus. Ce composant remplace le besoin de programmer IEnvironmentObserver.
    /// Il lit automatiquement tous les `PluminusStateSensor` attachés à ce GameObject et multiplie leurs états
    /// pour générer dynamiquement l'ID d'État Final (StateId) pour l'AdaptiveBrain.
    /// </summary>
    public class PluminusUnityStateBuilder : MonoBehaviour, IEnvironmentObserver
    {
        private PluminusStateSensor[] loadedSensors;

        private void Awake()
        {
            // Récupère automatiquement tous les capteurs No-Code empilés sur l'objet !
            loadedSensors = GetComponents<PluminusStateSensor>();
            
            if (loadedSensors.Length == 0)
            {
                Debug.LogWarning("PluminusUnityStateBuilder: Aucun capteur n'est attaché à ce GameObject. L'état sera toujours 0.");
            }
        }

        public int GetMaxStates()
        {
            if (loadedSensors == null) loadedSensors = GetComponents<PluminusStateSensor>();
            if (loadedSensors.Length == 0) return 1;

            int totalStates = 1;
            foreach (var sensor in loadedSensors)
            {
                int count = sensor.GetSubStateCount();
                if (count > 0)
                {
                    totalStates *= count; // Produit cartésien des états possibles
                }
            }

            return totalStates; // Le nombre total astronomique de possibilités !
        }

        public int GetCurrentStateId()
        {
            if (loadedSensors.Length == 0) return 0;

            int finalStateId = 0;
            int currentMultiplier = 1;

            // Algorithme de combinaison de bases dynamiques
            // Ex: SensorA a 3 états. SensorB a 2 états. SensorC a 4 états.
            // finalState = StateA + (StateB * 3) + (StateC * 3 * 2).
            for (int i = 0; i < loadedSensors.Length; i++)
            {
                int sensorState = loadedSensors[i].GetCurrentSubState();
                int maxSubStates = loadedSensors[i].GetSubStateCount();

                // Sécurité
                if (sensorState < 0) sensorState = 0;
                if (sensorState >= maxSubStates) sensorState = maxSubStates - 1;

                finalStateId += sensorState * currentMultiplier;
                currentMultiplier *= maxSubStates;
            }

            return finalStateId;
        }
    }
}
