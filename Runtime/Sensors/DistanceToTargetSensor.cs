using UnityEngine;

namespace Pluminus.Sensors
{
    /// <summary>
    /// Capteur de Distance.
    /// Découpe la distance vers une cible en anneaux (Corps à corps, Portée moyenne, etc.).
    /// </summary>
    [AddComponentMenu("Pluminus/Sensors/Distance Local Sensor")]
    public class DistanceToTargetSensor : PluminusStateSensor
    {
        [Header("Cible (Target)")]
        [Tooltip("La cible à mesurer. Si vide et 'Auto-Find' est coché, cherchera le Player.")]
        public Transform target;
        public bool autoFindPlayerTag = true;

        [Header("Seuils de Distance")]
        public float meleeRange = 2f;
        public float midRange = 5f;
        public float longRange = 10f;

        [Header("Visualisation")]
        public bool showGizmos = true;

        public override int GetSubStateCount()
        {
            // 5 états : 0=Pas de cible, 1=CàC, 2=Moyen, 3=Long, 4=Hors de vue
            return 5;
        }

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

            float dist = Vector3.Distance(transform.position, target.position);

            if (dist <= meleeRange) return 1;
            if (dist <= midRange) return 2;
            if (dist <= longRange) return 3;
            return 4;
        }

        private void OnDrawGizmosSelected()
        {
            if (!showGizmos) return;

            // Dessine les anneaux de distance
            Gizmos.color = new Color(0, 1, 0, 0.2f);
            DrawCircleGizmo(meleeRange);
            
            Gizmos.color = new Color(1, 1, 0, 0.15f);
            DrawCircleGizmo(midRange);
            
            Gizmos.color = new Color(1, 0.5f, 0, 0.1f);
            DrawCircleGizmo(longRange);

            if (target != null)
            {
                Gizmos.color = Color.yellow;
                Gizmos.DrawLine(transform.position, target.position);
            }
        }

        private void DrawCircleGizmo(float radius)
        {
            Matrix4x4 oldMatrix = Gizmos.matrix;
            Gizmos.matrix = Matrix4x4.TRS(transform.position, Quaternion.identity, new Vector3(1, 0.01f, 1));
            Gizmos.DrawWireSphere(Vector3.zero, radius);
            Gizmos.matrix = oldMatrix;
        }
    }
}
