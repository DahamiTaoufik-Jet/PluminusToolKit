using UnityEngine;

namespace Pluminus.Sensors.Extended
{
    /// <summary>
    /// Capteur Matriciel Linéaire (Version Légère du Grid Sensor).
    /// Teste 3 cases (Gauche, Centre, Droite) devant l'agent.
    /// L'utilisation d'une vraie grille 3x3 génère 512 états, ce qui sature trop la Q-Table en No-Code conventionnel.
    /// </summary>
    [AddComponentMenu("Pluminus/Sensors/Grid Sensor (Linear)")]
    public class GridSensor : PluminusStateSensor
    {
        [Header("Grille Frontale (3 cases)")]
        [Tooltip("Distance où la détection aura lieu (devant le Transform).")]
        public float forwardOffset = 1.5f;
        
        [Tooltip("Largeur de chaque case (distance entre la gauche, le centre et la droite).")]
        public float spacing = 1.0f;
        
        [Tooltip("Rayon de chaque test.")]
        public float cellRadius = 0.4f;

        [Tooltip("Masque des éléments considérés comme 'Occupant' la coordonnée.")]
        public LayerMask obstacleMask;

        public override int GetSubStateCount()
        {
            // 3 cellules booléennes = 2^3 = 8 états (de 0 à 7)
            return 8;
        }

        public override int GetCurrentSubState()
        {
            int state = 0;
            Vector3 centerPos = transform.position + transform.forward * forwardOffset;

            // Construit un ID binaire : b010 par exemple
            // Cellule Gauche (Bit 0)
            if (CheckCell(centerPos - transform.right * spacing)) state |= (1 << 0);
            
            // Cellule Centre (Bit 1)
            if (CheckCell(centerPos)) state |= (1 << 1);
            
            // Cellule Droite (Bit 2)
            if (CheckCell(centerPos + transform.right * spacing)) state |= (1 << 2);

            return state;
        }

        private bool CheckCell(Vector3 worldPos)
        {
            return Physics.CheckSphere(worldPos, cellRadius, obstacleMask);
        }

        private void OnDrawGizmosSelected()
        {
            Vector3 centerPos = transform.position + transform.forward * forwardOffset;
            
            DrawCellGizmo(centerPos - transform.right * spacing);
            DrawCellGizmo(centerPos);
            DrawCellGizmo(centerPos + transform.right * spacing);
        }

        private void DrawCellGizmo(Vector3 pos)
        {
            bool isOccupied = Physics.CheckSphere(pos, cellRadius, obstacleMask);
            Gizmos.color = isOccupied ? new Color(1, 0, 0, 0.5f) : new Color(0, 1, 0, 0.2f);
            Gizmos.DrawSphere(pos, cellRadius);
        }
    }
}
