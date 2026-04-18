using UnityEngine;

namespace Pluminus.Sensors
{
    /// <summary>
    /// Capteur Raycast 3D.
    /// Lance un rayon dans une direction définie par des angles Euler.
    /// </summary>
    [AddComponentMenu("Pluminus/Sensors/Raycast Tag Sensor (3D)")]
    public class RaycastTagSensor : PluminusStateSensor
    {
        [Header("Détection")]
        public string targetTag = "Player";
        public float maxDistance = 10f;
        public LayerMask obstacleMask = -1;

        [Header("Direction (Angles)")]
        [Tooltip("Rotation de la vue en degrés (X=Haut/Bas, Y=Gauche/Droite)")]
        public Vector2 directionAngles = Vector2.zero;

        public override int GetSubStateCount() => 2;

        public override int GetCurrentSubState()
        {
            Vector3 direction = GetDirection();
            if (Physics.Raycast(transform.position, direction, out RaycastHit hit, maxDistance, obstacleMask))
            {
                if (hit.collider.CompareTag(targetTag)) return 1;
            }
            return 0;
        }

        private Vector3 GetDirection()
        {
            // On calcule la direction basée sur la rotation de l'objet + les angles X et Y
            return transform.rotation * Quaternion.Euler(directionAngles.x, directionAngles.y, 0) * Vector3.forward;
        }

        private void OnDrawGizmosSelected()
        {
            Vector3 direction = GetDirection();
            bool isHitting = Application.isPlaying && GetCurrentSubState() == 1;

            Gizmos.color = isHitting ? Color.red : Color.magenta;
            Gizmos.DrawRay(transform.position, direction * maxDistance);
            
            // Dessine une petite pointe pour le "bout" du rayon
            Gizmos.DrawWireSphere(transform.position + direction * maxDistance, 0.1f);
        }
    }
}
