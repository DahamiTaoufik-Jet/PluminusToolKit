using UnityEngine;

namespace Pluminus.Sensors.Extended
{
    /// <summary>
    /// Capteur avec une Grille 3x3 (ou personnalisée).
    /// Version Damier : Cubes collés les uns aux autres.
    /// </summary>
    [AddComponentMenu("Pluminus/Sensors/Grid Sensor (3x3 Tiled)")]
    public class GridSensor : PluminusStateSensor
    {
        [Header("Taille du Damier")]
        [Tooltip("La taille totale de la grille 3x3.")]
        public float gridSize = 3f;
        public float verticalOffset = 0.5f;

        [Header("Collision")]
        public LayerMask obstacleMask;

        [Header("Toggles de Cellules (Fixe 3x3)")]
        [Tooltip("Activez les cases souhaitées. (Lecture de Gauche à Droite, Haut en Bas)")]
        public bool[] activeCells = new bool[9] { 
            true, true, true, 
            true, false, true, 
            true, true, true 
        };

        private void OnValidate()
        {
            if (activeCells == null || activeCells.Length != 9)
            {
                System.Array.Resize(ref activeCells, 9);
            }
        }

        public override int GetSubStateCount()
        {
            int activeCount = 0;
            if (activeCells == null || activeCells.Length != 9) return 1;
            foreach (bool b in activeCells) if (b) activeCount++;
            return (int)Mathf.Pow(2, activeCount);
        }

        public override int GetCurrentSubState()
        {
            int state = 0;
            int bitIndex = 0;
            float cellSize = gridSize / 3f;

            for (int y = 1; y >= -1; y--) // De Haut en Bas
            {
                for (int x = -1; x <= 1; x++) // De Gauche à Droite
                {
                    int cellIndex = (1 - y) * 3 + (x + 1);
                    if (activeCells[cellIndex])
                    {
                        Vector3 offset = (transform.right * x * cellSize) + (transform.forward * y * cellSize);
                        Vector3 checkPos = transform.position + offset + Vector3.up * verticalOffset;

                        // On teste un cube de la taille d'une cellule
                        if (Physics.CheckBox(checkPos, new Vector3(cellSize, cellSize, cellSize) / 2.1f, transform.rotation, obstacleMask))
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
            float cellSize = gridSize / 3f;
            for (int y = 1; y >= -1; y--)
            {
                for (int x = -1; x <= 1; x++)
                {
                    int cellIndex = (1 - y) * 3 + (x + 1);
                    Vector3 offset = (transform.right * x * cellSize) + (transform.forward * y * cellSize);
                    Vector3 checkPos = transform.position + offset + Vector3.up * verticalOffset;

                    if (activeCells[cellIndex])
                    {
                        bool isOccupied = Application.isPlaying && Physics.CheckBox(checkPos, new Vector3(cellSize, cellSize, cellSize) / 2.1f, transform.rotation, obstacleMask);
                        Gizmos.color = isOccupied ? new Color(1, 0, 0, 0.6f) : new Color(0, 1, 0, 0.3f);
                        
                        // Dessine le cube
                        Matrix4x4 oldRotation = Gizmos.matrix;
                        Gizmos.matrix = Matrix4x4.TRS(checkPos, transform.rotation, Vector3.one);
                        Gizmos.DrawCube(Vector3.zero, new Vector3(cellSize, cellSize, cellSize) * 0.95f);
                        Gizmos.DrawWireCube(Vector3.zero, new Vector3(cellSize, cellSize, cellSize) * 0.95f);
                        Gizmos.matrix = oldRotation;
                    }
                }
            }
        }
    }
}
