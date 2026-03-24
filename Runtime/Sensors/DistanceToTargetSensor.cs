using UnityEngine;
using System.Collections.Generic;

namespace Pluminus.Sensors
{
    /// <summary>
    /// Un capteur No-Code qui vérifie la distance avec un objet (via Transform direct ou via le Tag le plus proche).
    /// </summary>
    public class DistanceToTargetSensor : PluminusStateSensor
    {
        public enum TargetMode { UseTransform, UseTag }

        [Header("Configuration de Cible")]
        public TargetMode targetMode = TargetMode.UseTransform;
        public Transform manualTarget;
        public string searchTag = "Enemy";

        [Header("Seuils de Distance (Du plus lointain au plus critique)")]
        [Tooltip("Si la distance dépasse le seuil le plus grand, l'état max est retourné (Souvent 'Loin').")]
        public List<float> distanceThresholds = new List<float>() { 8f, 4f, 2f };

        public override int GetSubStateCount()
        {
            // Nombre de zones = Nombre de seuils + 1.
            // Ex: Si on a [8, 4, 2]. On a : >8, <=8, <=4, <=2 => 4 états.
            return distanceThresholds.Count + 1;
        }

        public override int GetCurrentSubState()
        {
            float dist = float.MaxValue;

            if (targetMode == TargetMode.UseTransform && manualTarget != null)
            {
                dist = Vector3.Distance(transform.position, manualTarget.position);
            }
            else if (targetMode == TargetMode.UseTag && !string.IsNullOrEmpty(searchTag))
            {
                GameObject[] targets = GameObject.FindGameObjectsWithTag(searchTag);
                foreach (var t in targets)
                {
                    float d = Vector3.Distance(transform.position, t.transform.position);
                    if (d < dist) dist = d;
                }
            }

            // dist est évaluée contre les seuils, on assume que les seuils sont triés par le dev ou on les trie.
            // Pour être user-friendly, on les classe du plus grand au plus petit (Lointain -> Proche).
            distanceThresholds.Sort((a, b) => b.CompareTo(a)); 

            // Si Seuils = 8, 4, 2
            // dist = 10 -> retourne 0 (État Lointain max)
            // dist = 6 -> retourne 1
            // dist = 3 -> retourne 2
            // dist = 1 -> retourne 3 (État Critique)
            
            for (int i = 0; i < distanceThresholds.Count; i++)
            {
                if (dist > distanceThresholds[i])
                {
                    return i;
                }
            }

            // S'il est plus petit que tous les seuils
            return distanceThresholds.Count;
        }

        private void OnDrawGizmosSelected()
        {
            Gizmos.color = Color.cyan;
            foreach (float threshold in distanceThresholds)
            {
                Gizmos.DrawWireSphere(transform.position, threshold);
            }
        }
    }
}
