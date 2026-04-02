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
        private List<PluminusStateSensor> loadedSensors = new List<PluminusStateSensor>();

        public IReadOnlyList<PluminusStateSensor> GetLoadedSensors() => loadedSensors;

        private void Awake()
        {
            // Récupère d'abord les capteurs locaux (Lui-même et ses enfants)
            PluminusStateSensor[] childrenSensors = GetComponentsInChildren<PluminusStateSensor>();
            foreach (var s in childrenSensors)
            {
                if (!loadedSensors.Contains(s)) loadedSensors.Add(s);
            }
            
            if (loadedSensors.Count == 0)
            {
                Debug.LogWarning("PluminusUnityStateBuilder: Aucun capteur n'est attaché. L'état restera toujours 0.");
            }
        }

        public void RegisterExternalSensor(PluminusStateSensor externalSensor)
        {
            if (externalSensor != null && !loadedSensors.Contains(externalSensor))
            {
                loadedSensors.Add(externalSensor);
            }
        }

        public int GetMaxStates()
        {
            if (loadedSensors.Count == 0) return 1;

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
            if (loadedSensors.Count == 0) return 0;

            int finalStateId = 0;
            int currentMultiplier = 1;

            // Arrimage dimensionnel des capteurs connectés
            for (int i = 0; i < loadedSensors.Count; i++)
            {
                int sensorState = loadedSensors[i].GetStateWithDebug();
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
