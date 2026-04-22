using UnityEngine;

namespace Pluminus.Sensors
{
    /// <summary>
    /// Capteur No-Code 3D qui vérifie si un objet avec un tag spécifique est actuellement dans une zone Trigger 3D.
    /// Renvoie 1 si vrai, 0 si faux.
    /// Nécessite un Collider (3D) en mode "Is Trigger" sur ce GameObject.
    /// </summary>
    [RequireComponent(typeof(Collider))]
    [AddComponentMenu("Pluminus/Sensors/Trigger Tag Sensor 3D")]
    public class TriggerTagSensor3D : PluminusStateSensor
    {
        [Tooltip("Le Tag de l'objet à détecter dans la zone visuelle (ex: Bullet)")]
        public string targetTag = "Enemy";

        private bool isCurrentlyInside = false;
        private bool pulseMemory = false;

        public override int GetSubStateCount()
        {
            return 2; // Booléen : 0 = Vide, 1 = Occupé
        }

        public override int GetCurrentSubState()
        {
            // L'IA observe l'état à chaque tick de décision.
            // On renvoie VRAI si l'objet est dedans actuellement OU s'il est passé très vite (pulse).
            bool finalState = isCurrentlyInside || pulseMemory;
            
            // On efface la mémoire de l'éclair après l'observation pour le prochain tick
            pulseMemory = false;
            
            return finalState ? 1 : 0;
        }

        private void OnTriggerStay(Collider other)
        {
            if (!string.IsNullOrEmpty(targetTag) && other.CompareTag(targetTag))
            {
                isCurrentlyInside = true;
                pulseMemory = true;
            }
        }

        private void OnTriggerExit(Collider other)
        {
            if (!string.IsNullOrEmpty(targetTag) && other.CompareTag(targetTag))
            {
                isCurrentlyInside = false;
            }
        }
    }
}
