using UnityEngine;

namespace Pluminus.Sensors.Extended
{
    /// <summary>
    /// Capteur d'Élévation.
    /// Compare la position Y de l'agent à celle de sa cible pour savoir si elle est plus haute, au même niveau, ou plus basse.
    /// </summary>
    [AddComponentMenu("Pluminus/Sensors/Elevation Sensor")]
    public class ElevationSensor : PluminusStateSensor
    {
        [Header("Cible")]
        public Transform target;

        [Header("Paramètres")]
        [Tooltip("Marge de tolérance (en unités) pour considérer que les objets sont 'Au même niveau'.")]
        public float sameLevelTolerance = 0.5f;

        public override int GetSubStateCount()
        {
            // 4 états : 0 = Pas de Cible, 1 = Cible plus basse, 2 = Même Niveau, 3 = Cible plus haute
            return 4;
        }

        public override int GetCurrentSubState()
        {
            if (target == null) return 0; // Pas de cible

            float agentHigh = transform.position.y;
            float targetHigh = target.position.y;
            float difference = targetHigh - agentHigh;

            if (difference > sameLevelTolerance)
            {
                return 3; // Plus Haut
            }
            if (difference < -sameLevelTolerance)
            {
                return 1; // Plus bas
            }
            
            return 2; // Même niveau
        }
    }
}
