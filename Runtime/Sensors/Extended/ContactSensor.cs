using UnityEngine;

namespace Pluminus.Sensors.Extended
{
    [AddComponentMenu("Pluminus/Sensors/Contact Sensor")]
    public class ContactSensor : PluminusStateSensor
    {
        [Header("Zone de Contact")]
        [Tooltip("Position locale de la sphère de détection.")]
        public Vector3 offset = new Vector3(0, -0.1f, 0);
        public float detectionRadius = 0.2f;

        [Header("Collision")]
        public LayerMask obstacleMask;

        public override int GetSubStateCount() => 2;

        public override int GetCurrentSubState()
        {
            Vector3 checkPosition = transform.position + transform.rotation * offset;
            return Physics.CheckSphere(checkPosition, detectionRadius, obstacleMask) ? 1 : 0;
        }

        private void OnDrawGizmosSelected()
        {
            Vector3 checkPosition = transform.position + transform.rotation * offset;
            bool isDetected = Application.isPlaying && GetCurrentSubState() == 1;
            
            Gizmos.color = isDetected ? new Color(1, 0, 0, 0.6f) : new Color(0, 1, 0, 0.4f);
            Gizmos.DrawSphere(checkPosition, detectionRadius);
            Gizmos.DrawWireSphere(checkPosition, detectionRadius);
            
#if UNITY_EDITOR
            string contactText = isDetected ? "CONTACT" : "LIBRE";
            UnityEditor.Handles.Label(checkPosition + Vector3.down * detectionRadius, contactText);
#endif
        }
    }
}
