using UnityEngine;

namespace Pluminus.Sensors.Extended
{
    /// <summary>
    /// Capteur de Valeur Continue (Discrétisation de variables).
    /// Utile pour transformer des HP ou de la Mana en "Critique", "Moyen", "Plein".
    /// </summary>
    [AddComponentMenu("Pluminus/Sensors/Value Sensor")]
    public class ValueSensor : PluminusStateSensor
    {
        [Header("Paliers de Valeurs")]
        [Tooltip("Si la valeur est en dessous de ce ratio (0.0 à 1.0), on considère que c'est l'État Critique (0)")]
        [Range(0f, 1f)]
        public float criticalThreshold = 0.3f;
        
        [Tooltip("Si la valeur est au dessus de critique, mais en dessous de celui-là, on est à l'État Moyen (1). Au-dessus c'est l'État Plein (2).")]
        [Range(0f, 1f)]
        public float highThreshold = 0.7f;

        // La valeur courante normalisée de 0 à 1.
        private float currentRatio = 1f;

        public override int GetSubStateCount()
        {
            // 3 états : Critique, Moyen, Plein
            return 3;
        }

        public override int GetCurrentSubState()
        {
            if (currentRatio < criticalThreshold) return 0; // Critique (ex: HP bas)
            if (currentRatio < highThreshold) return 1;     // Moyen (ex: HP moyens)
            return 2;                                       // Plein/OK (ex: Full HP)
        }

        /// <summary>
        /// Doit être appelé par le composant de vie/statistiques de votre jeu à chaque fois qu'il prend des dégâts/se soigne.
        /// </summary>
        /// <param name="ratio">Le ratio entre 0 (vide) et 1 (plein) de la ressource mesurée.</param>
        public void UpdateValueRatio(float ratio)
        {
            currentRatio = Mathf.Clamp01(ratio);
        }
    }
}
