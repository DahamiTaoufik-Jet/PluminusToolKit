using UnityEngine;
using System.Collections.Generic;

namespace Pluminus.Sensors
{
    public enum TargetSelectionMode
    {
        [Tooltip("Suit l'objet le plus proche du capteur")]
        Closest,

        [Tooltip("Suit l'objet le plus éloigné du capteur")]
        Farthest,

        [Tooltip("Suit le PREMIER objet qui est entré dans la zone de détection (le plus ancien)")]
        FirstEntered,

        [Tooltip("Suit le DERNIER objet qui est entré dans la zone de détection (le plus récent)")]
        LastEntered
    }

    /// <summary>
    /// Capteur de Distance Flexible.
    /// Découpe la distance vers une cible en autant de zones que souhaité.
    /// Supporte 4 modes de sélection : Plus Proche, Plus Loin, Premier Entré, Dernier Entré.
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

        [Header("Mode de Sélection")]
        [Tooltip("Comment choisir la cible parmi les objets avec le même Tag ?")]
        public TargetSelectionMode selectionMode = TargetSelectionMode.Closest;

        [Header("Seuils de Distance")]
        [Tooltip("Ajoutez vos paliers de distance ici (ex: 2, 5, 10, 20).")]
        public List<float> thresholds = new List<float> { 2f, 5f, 10f };

        [Header("Visualisation")]
        public bool showGizmos = true;

        // Historique d'apparition pour les modes FirstEntered / LastEntered
        private List<Transform> entryOrder = new List<Transform>();

        public override int GetSubStateCount()
        {
            if (target == null && !autoFindByTag) return 1;
            return thresholds.Count + 2; // +1 pour "hors portée", +1 pour "aucune cible"
        }

        protected override void Awake()
        {
            base.Awake();
            thresholds.Sort();
        }

        private void Update()
        {
            // Met à jour l'historique d'entrée pour les modes First/Last
            if (!autoFindByTag || string.IsNullOrEmpty(targetTag)) return;

            GameObject[] candidates = GameObject.FindGameObjectsWithTag(targetTag);

            // Ajouter les nouveaux objets à la fin de la liste (ordre d'apparition)
            foreach (var obj in candidates)
            {
                if (!entryOrder.Contains(obj.transform))
                {
                    entryOrder.Add(obj.transform);
                }
            }

            // Nettoyer les objets détruits
            entryOrder.RemoveAll(t => t == null);
        }

        /// <summary>
        /// Sélectionne la cible selon le mode choisi dans l'inspecteur.
        /// </summary>
        private Transform SelectTargetByMode()
        {
            if (string.IsNullOrEmpty(targetTag)) return null;
            
            GameObject[] candidates = GameObject.FindGameObjectsWithTag(targetTag);
            if (candidates.Length == 0) return null;

            switch (selectionMode)
            {
                case TargetSelectionMode.Closest:
                    return FindByDistance(candidates, closest: true);

                case TargetSelectionMode.Farthest:
                    return FindByDistance(candidates, closest: false);

                case TargetSelectionMode.FirstEntered:
                    // Le premier de la liste = le plus ancien
                    if (entryOrder.Count > 0) return entryOrder[0];
                    return candidates[0].transform;

                case TargetSelectionMode.LastEntered:
                    // Le dernier de la liste = le plus récent
                    if (entryOrder.Count > 0) return entryOrder[entryOrder.Count - 1];
                    return candidates[candidates.Length - 1].transform;

                default:
                    return FindByDistance(candidates, closest: true);
            }
        }

        private Transform FindByDistance(GameObject[] candidates, bool closest)
        {
            Transform result = null;
            float bestDist = closest ? float.MaxValue : float.MinValue;

            foreach (var obj in candidates)
            {
                float dist = Vector3.Distance(transform.position, obj.transform.position);
                if ((closest && dist < bestDist) || (!closest && dist > bestDist))
                {
                    bestDist = dist;
                    result = obj.transform;
                }
            }
            return result;
        }

        public override int GetCurrentSubState()
        {
            // Si une cible manuelle est assignée, on la garde. Sinon, on cherche par Tag selon le mode.
            Transform currentTarget = target;
            if (currentTarget == null && autoFindByTag)
            {
                currentTarget = SelectTargetByMode();
            }
            
            if (currentTarget == null) return 0;

            float dist = Vector3.Distance(transform.position, currentTarget.position);

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
