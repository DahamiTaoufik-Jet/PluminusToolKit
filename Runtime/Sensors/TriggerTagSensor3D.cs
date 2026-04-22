using UnityEngine;

namespace Pluminus.Sensors
{
    /// <summary>
    /// Capteur No-Code 3D Auto-Configuré.
    /// Gère son propre BoxCollider pour détecter les objets par Tag.
    /// </summary>
    [RequireComponent(typeof(BoxCollider))]
    [AddComponentMenu("Pluminus/Sensors/Trigger Tag Sensor 3D")]
    public class TriggerTagSensor3D : PluminusStateSensor
    {
        [Header("Détection")]
        [Tooltip("Le Tag de l'objet à détecter (ex: Bullet)")]
        public string targetTag = "Bullet";

        [Header("Configuration de la Zone")]
        [Tooltip("La taille de la zone de détection.")]
        public Vector3 zoneSize = new Vector3(1f, 1f, 1f);
        
        [Tooltip("Le décalage du centre de la zone.")]
        public Vector3 zoneCenter = Vector3.zero;

        private BoxCollider _internalCollider;
        private bool isCurrentlyInside = false;
        private bool pulseMemory = false;

        private void Reset()
        {
            SetupCollider();
        }

        private void OnValidate()
        {
            SetupCollider();
        }

        private void SetupCollider()
        {
            if (_internalCollider == null) _internalCollider = GetComponent<BoxCollider>();
            
            if (_internalCollider != null)
            {
                _internalCollider.isTrigger = true;
                _internalCollider.size = zoneSize;
                _internalCollider.center = zoneCenter;
            }
        }

        public override int GetSubStateCount() => 2;

        public override int GetCurrentSubState()
        {
            bool finalState = isCurrentlyInside || pulseMemory;
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
