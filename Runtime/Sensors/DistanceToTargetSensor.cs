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
        [Tooltip("Glissez la cible manuellement ici. Si vide, le capteur cherchera automatiquement par Tag.")]
        public Transform target;
        
        [Tooltip("Si la cible est vide, cherche automatiquement un objet avec ce Tag dans la scène.")]
        public bool autoFindByTag = true;
        
        [Tooltip("Le Tag à chercher si aucune cible n'est assignée (ex: 'Player', 'Bullet', 'Enemy')")]
        public string targetTag = "Player";

        [Header("Seuils de Distance")]
        [Tooltip("Ajoutez vos paliers de distance ici (ex: 2, 5, 10, 20).")]
        public List<float> thresholds = new List<float> { 2f, 5f, 10f };

        [Header("Visualisation")]
        public bool showGizmos = true;

        public override int GetSubStateCount()
        {
            if (target == null) return 1;
            return thresholds.Count + 1;
        }

        protected override void Awake()
        {
            base.Awake();
            FindTargetIfNeeded();
            thresholds.Sort();
        }

        /// <summary>
        /// Trouve l'objet LE PLUS PROCHE parmi tous ceux avec le tag cible.
        /// </summary>
        private Transform FindClosestWithTag()
        {
            if (string.IsNullOrEmpty(targetTag)) return null;
            
            GameObject[] candidates = GameObject.FindGameObjectsWithTag(targetTag);
            if (candidates.Length == 0) return null;

            Transform closest = null;
            float closestDist = float.MaxValue;

            foreach (var obj in candidates)
            {
                float dist = Vector3.Distance(transform.position, obj.transform.position);
                if (dist < closestDist)
                {
                    closestDist = dist;
                    closest = obj.transform;
                }
            }
            return closest;
        }

        public override int GetCurrentSubState()
        {
            // Si une cible manuelle est assignée, on la garde. Sinon, on cherche le plus proche par Tag.
            Transform currentTarget = target;
            if (currentTarget == null && autoFindByTag)
            {
                currentTarget = FindClosestWithTag();
            }
            
            if (currentTarget == null) return 0;

            float dist = Vector3.Distance(transform.position, currentTarget.position);

            for (int i = 0; i < thresholds.Count; i++)
            {
                if (dist <= thresholds[i]) return i + 1;
            }

            return thresholds.Count + 1; // Hors de portée (dernière zone)
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
