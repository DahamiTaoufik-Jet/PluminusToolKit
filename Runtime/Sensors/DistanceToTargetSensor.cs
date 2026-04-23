using UnityEngine;
using System.Collections.Generic;

namespace Pluminus.Sensors
{
    /// <summary>
    /// Capteur de Distance Flexible.
    /// Découpe la distance vers une cible en autant de zones que souhaité.
    /// </summary>
    [AddComponentMenu("Pluminus/Sensors/Distance Sensor (List)")]
    public class DistanceToTargetSensor : PluminusStateSensor
    {
        [Header("Cible (Target)")]
        public Transform target;
        public bool autoFindPlayerTag = true;

        [Header("Seuils de Distance")]
        [Tooltip("Ajoutez vos paliers de distance ici (ex: 2, 5, 10, 20).")]
        public List<float> thresholds = new List<float> { 2f, 5f, 10f };

        [Header("Visualisation")]
        public bool showGizmos = true;

        public override int GetSubStateCount()
        {
            // N seuils = N+1 états (0=Cible manquante, puis état par zone)
            if (target == null) return 1;
            return thresholds.Count + 1;
        }

        protected override void Awake()
        {
            base.Awake();
            if (target == null && autoFindPlayerTag)
            {
                GameObject player = GameObject.FindGameObjectWithTag("Player");
                if (player != null) target = player.transform;
            }
            // Tri automatique par sécurité
            thresholds.Sort();
        }

        public override int GetCurrentSubState()
        {
            if (target == null) return 0;

            float dist = Vector3.Distance(transform.position, target.position);

            for (int i = 0; i < thresholds.Count; i++)
            {
                if (dist <= thresholds[i]) return i + 1;
            }

            // Hors de portée : on reste dans la dernière zone définie (cohérent avec GetSubStateCount = thresholds.Count + 1).
            return thresholds.Count;
        }

        private void OnDrawGizmosSelected()
        {
            if (!showGizmos) return;

            for (int i = 0; i < thresholds.Count; i++)
            {
                float t = (float)i / thresholds.Count;
                Gizmos.color = Color.Lerp(Color.green, Color.red, t);
                DrawCircleGizmo(thresholds[i]);
            }

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
