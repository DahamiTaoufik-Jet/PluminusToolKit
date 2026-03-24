using UnityEngine;

namespace Pluminus.Sensors
{
    /// <summary>
    /// Classe abstraite de base pour tous les capteurs visuels Pluminus (No-Code).
    /// Ajoutez des classes dérivées sur le même GameObject que le PluminusUnityStateBuilder.
    /// </summary>
    public abstract class PluminusStateSensor : MonoBehaviour
    {
        [Tooltip("Nom usuel du capteur pour s'y retrouver dans l'éditeur")]
        public string sensorName = "Nouveau Capteur";

        /// <summary>
        /// Retourne le nombre d'états distincts que ce capteur précis peut prendre (ex: 2 pour Booléen, N pour Distance).
        /// </summary>
        public abstract int GetSubStateCount();

        /// <summary>
        /// Retourne l'état actuel de ce capteur au moment (allant de 0 à GetSubStateCount() - 1).
        /// </summary>
        public abstract int GetCurrentSubState();
    }
}
