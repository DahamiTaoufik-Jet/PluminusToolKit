using UnityEngine;

namespace Pluminus.Sensors.Extended
{
    /// <summary>
    /// Capteur avec une Grille 3x3 (ou personnalisée).
    /// Permet de détecter les obstacles tout autour de l'agent.
    /// </summary>
    [AddComponentMenu("Pluminus/Sensors/Grid Sensor (3x3)")]
    public class GridSensor : PluminusStateSensor
    {
        [Header("Dimensions de la Grille")]
        public float gridSize = 3f;
        public float cellRadius = 0.4f;
        public float verticalOffset = 0.5f;

        [Header("Masque de Collision")]
        public LayerMask obstacleMask;

        [Header("Toggles de Cellules (Optimisation)")]
        [Tooltip("Désactivez les cases inutiles pour réduire le nombre d'États de l'IA.")]
        public bool[] activeCells = new bool[9] { 
            true, true, true, 
            true, false, true, 
            true, true, true 
        };

        public override int GetSubStateCount()
        {
            // Le nombre d'états est 2 puissance (nombre de cases actives)
            int activeCount = 0;
            foreach (bool b in activeCells) if (b) activeCount++;
            return (int)Mathf.Pow(2, activeCount);
        }

        public override int GetCurrentSubState()
        {
            int state = 0;
            int bitIndex = 0;

            for (int y = -1; y <= 1; y++)
            {
                for (int x = -1; x <= 1; x++)
                {
                    int cellIndex = (y + 1) * 3 + (x + 1);
                    if (activeCells[cellIndex])
                    {
                        Vector3 offset = (transform.right * x * (gridSize / 2f)) + (transform.forward * y * (gridSize / 2f));
                        Vector3 checkPos = transform.position + offset + Vector3.up * verticalOffset;

                        if (Physics.CheckSphere(checkPos, cellRadius, obstacleMask))
                        {
                            state |= (1 << bitIndex);
                        }
                        bitIndex++;
                    }
                }
            }
            return state;
        }

        private void OnDrawGizmosSelected()
        {
            for (int y = -1; y <= 1; y++)
            {
                for (int x = -1; x <= 1; x++)
                {
                    int cellIndex = (y + 1) * 3 + (x + 1);
                    Vector3 offset = (transform.right * x * (gridSize / 2f)) + (transform.forward * y * (gridSize / 2f));
                    Vector3 checkPos = transform.position + offset + Vector3.up * verticalOffset;

                    if (activeCells[cellIndex])
                    {
                        bool isOccupied = Application.isPlaying && Physics.CheckSphere(checkPos, cellRadius, obstacleMask);
                        Gizmos.color = isOccupied ? new Color(1, 0, 0, 0.6f) : new Color(0, 1, 0, 0.3f);
                        Gizmos.DrawSphere(checkPos, cellRadius);
                        Gizmos.DrawWireSphere(checkPos, cellRadius);
                    }
                    else
                    {
                        Gizmos.color = new Color(0.5f, 0.5f, 0.5f, 0.1f);
                        Gizmos.DrawWireSphere(checkPos, cellRadius * 0.5f);
                    }
                }
            }
        }
    }
}
