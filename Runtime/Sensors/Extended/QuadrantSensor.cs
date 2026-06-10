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
        [Tooltip("Nombre de secteurs dans l'arc de detection.")]
        [Range(2, 32)]
        public int numberOfSectors = 8;

        [Tooltip("Angle total de l'arc de detection en degres. 360 = cercle complet, 180 = demi-cercle, 90 = quart de cercle.")]
        [Range(1f, 360f)]
        public float arcAngle = 360f;

        [Tooltip("Decalage de l'arc par rapport a l'avant du transform (0 = centre sur forward).")]
        [Range(-180f, 180f)]
        public float arcOffset = 0f;

        [Header("Visualisation")]
        public Color diskColor = new Color(0, 1, 1, 0.1f);
        public float visualRadius = 3f;

        // Etat 0 = cible absente ou hors arc
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

            // Applique l'offset et ramene dans [-180, 180]
            float relative = angle - arcOffset;
            if (relative > 180f) relative -= 360f;
            if (relative < -180f) relative += 360f;

            float halfArc = arcAngle / 2f;

            // Hors de l'arc de detection
            if (relative < -halfArc || relative > halfArc) return 0;

            // Normalise dans [0, arcAngle]
            float normalized = relative + halfArc;
            float sectorSize = arcAngle / numberOfSectors;

            int sector = Mathf.FloorToInt(normalized / sectorSize);
            if (sector >= numberOfSectors) sector = numberOfSectors - 1;

            return sector + 1;
        }

        private void OnDrawGizmosSelected()
        {
            float halfArc = arcAngle / 2f;
            float sectorSize = arcAngle / numberOfSectors;

            // Dessine l'arc
            Gizmos.color = diskColor;
            int segments = 40;
            float startAngle = -halfArc + arcOffset;
            float endAngle = halfArc + arcOffset;
            float step = (endAngle - startAngle) / segments;

            Vector3 prevPoint = transform.position + Quaternion.Euler(0, startAngle, 0) * transform.forward * visualRadius;
            for (int i = 1; i <= segments; i++)
            {
                float a = startAngle + step * i;
                Vector3 point = transform.position + Quaternion.Euler(0, a, 0) * transform.forward * visualRadius;
                Gizmos.DrawLine(prevPoint, point);
                prevPoint = point;
            }

            // Lignes de bord de l'arc
            Vector3 leftEdge = Quaternion.Euler(0, startAngle, 0) * transform.forward;
            Vector3 rightEdge = Quaternion.Euler(0, endAngle, 0) * transform.forward;
            Gizmos.DrawLine(transform.position, transform.position + leftEdge * visualRadius);
            Gizmos.DrawLine(transform.position, transform.position + rightEdge * visualRadius);

            // Lignes de separation des secteurs
            Gizmos.color = new Color(diskColor.r, diskColor.g, diskColor.b, 0.5f);
            for (int i = 1; i < numberOfSectors; i++)
            {
                float a = startAngle + sectorSize * i;
                Vector3 dir = Quaternion.Euler(0, a, 0) * transform.forward;
                Gizmos.DrawLine(transform.position, transform.position + dir * visualRadius);
            }

            // Ligne vers la cible
            if (target != null)
            {
                Gizmos.color = Color.yellow;
                Gizmos.DrawLine(transform.position, target.position);
            }
        }
    }
}
