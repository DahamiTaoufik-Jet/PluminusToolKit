using UnityEngine;

namespace Pluminus.Sensors.Extended
{
    /// <summary>
    /// Capteur de Contact Physique.
    /// Renvoie un état binaire vérifiant si un contact est établi dans une zone via OverlapSphere.
    /// Très utile pour vérifier si l'agent touche le sol (Grounded) ou s'il est collé à un mur.
    /// </summary>
    [AddComponentMenu("Pluminus/Sensors/Contact Sensor")]
    public class ContactSensor : PluminusStateSensor
    {
        [Header("Paramètres de Détection")]
        [Tooltip("Position centrale de la zone de contact par rapport au Transform de cet objet.")]
        public Vector3 offset = new Vector3(0, -0.1f, 0);

        [Tooltip("Rayon de la sphère de détection.")]
        public float detectionRadius = 0.2f;

        [Tooltip("Masque des couches considérées comme solides (ex: Ground, Obstacle).")]
        public LayerMask obstacleMask;

        public override int GetSubStateCount()
        {
            // 2 États: En l'air (0), Au Sol / En contact (1)
            return 2;
        }

        public override int GetCurrentSubState()
        {
            Vector3 checkPosition = transform.position + transform.rotation * offset;
            bool hasContact = Physics.CheckSphere(checkPosition, detectionRadius, obstacleMask);
            
            return hasContact ? 1 : 0;
        }

        private void OnDrawGizmosSelected()
        {
            Gizmos.color = new Color(0, 1, 0, 0.5f);
            Vector3 checkPosition = transform.position + transform.rotation * offset;
            Gizmos.DrawSphere(checkPosition, detectionRadius);
        }
    }
}
