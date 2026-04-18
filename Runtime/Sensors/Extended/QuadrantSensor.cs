using UnityEngine;

namespace Pluminus.Sensors.Extended
{
    [AddComponentMenu("Pluminus/Sensors/Quadrant Sensor (Precise)")]
    public class QuadrantSensor : PluminusStateSensor
    {
        [Header("Cible (Target)")]
        public Transform target;
        public bool autoFindPlayerTag = true;

        [Header("Configuration")]
        [Tooltip("Nombre de secteurs. Supporte jusqu'à 32 pour une précision extrême.")]
        [Range(2, 32)]
        public int numberOfSectors = 8;

        [Header("Visualisation")]
        public Color diskColor = new Color(0, 1, 1, 0.1f);
        public float visualRadius = 3f;

        public override int GetSubStateCount() => numberOfSectors + 1;

        protected override void Awake()
        {
            base.Awake();
            if (target == null && autoFindPlayerTag)
            {
                GameObject player = GameObject.FindGameObjectWithTag("Player");
                if (player != null) target = player.transform;
            }
        }

        public override int GetCurrentSubState()
        {
            if (target == null) return 0;

            Vector3 directionToTarget = target.position - transform.position;
            directionToTarget.y = 0;
            
            if (directionToTarget.sqrMagnitude < 0.001f) return 0;

            float angle = Vector3.SignedAngle(transform.forward, directionToTarget.normalized, Vector3.up);
            if (angle < 0) angle += 360f;

            float sectorSize = 360f / numberOfSectors;
            float shiftedAngle = angle + (sectorSize / 2f);
            if (shiftedAngle >= 360f) shiftedAngle -= 360f;

            return Mathf.FloorToInt(shiftedAngle / sectorSize) + 1;
        }

        private void OnDrawGizmosSelected()
        {
            Gizmos.color = diskColor;
            Matrix4x4 oldMatrix = Gizmos.matrix;
            Gizmos.matrix = Matrix4x4.TRS(transform.position, Quaternion.identity, new Vector3(1, 0.01f, 1));
            Gizmos.DrawSphere(Vector3.zero, visualRadius);
            Gizmos.matrix = oldMatrix;

            float sectorSize = 360f / numberOfSectors;
            Gizmos.color = new Color(diskColor.r, diskColor.g, diskColor.b, 0.5f);
            
            for (int i = 0; i < numberOfSectors; i++)
            {
                float angle = (i * sectorSize) - (sectorSize / 2f);
                Vector3 dir = Quaternion.Euler(0, angle, 0) * transform.forward;
                Gizmos.DrawLine(transform.position, transform.position + dir * visualRadius);
            }

            if (target != null)
            {
                Gizmos.color = Color.yellow;
                Gizmos.DrawLine(transform.position, target.position);
            }
        }
    }
}
