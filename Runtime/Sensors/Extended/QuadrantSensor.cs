using UnityEngine;

namespace Pluminus.Sensors.Extended
{
    /// <summary>
    /// Capteur d'Angle / Secteur (Quadrant).
    /// Découpe l'espace autour de l'agent en N 'parts de gâteau' pour repérer d'où vient la cible (ex: Devant = 1, Droite = 2, etc.).
    /// </summary>
    [AddComponentMenu("Pluminus/Sensors/Quadrant Sensor")]
    public class QuadrantSensor : PluminusStateSensor
    {
        [Header("Cible")]
        public Transform target;

        [Header("Paramètres du Quadrant")]
        [Tooltip("Nombre de secteurs. 4 = Devant/Derrière/Gauche/Droite. 8 ajoute les diagonales.")]
        [Range(2, 16)]
        public int numberOfSectors = 4;

        public override int GetSubStateCount()
        {
            // Secteurs + 1 état pour 'Pas de cible'
            return numberOfSectors + 1;
        }

        public override int GetCurrentSubState()
        {
            if (target == null) return 0; // State 0 = Missing Target

            Vector3 directionToTarget = target.position - transform.position;
            directionToTarget.y = 0; // On l'ignore pour s'assurer qu'on ne gère que le plan 2D Horizontal
            
            if (directionToTarget.sqrMagnitude < 0.001f) return 0; // Trop proche

            // Calcule l'angle signé entre -180 et 180 par rapport au forward
            float angle = Vector3.SignedAngle(transform.forward, directionToTarget.normalized, Vector3.up);

            // Convertir l'angle [-180, 180] en [0, 360]
            if (angle < 0) angle += 360f;

            // Découpe le gâteau en fonction du nombre de parts (centré sur le forward)
            float sectorSize = 360f / numberOfSectors;
            
            // On décale l'angle de la moitié d'un secteur pour que le secteur 0 soit bien centré pile Devant
            float shiftedAngle = angle + (sectorSize / 2f);
            if (shiftedAngle >= 360f) shiftedAngle -= 360f;

            int sectorIndex = Mathf.FloorToInt(shiftedAngle / sectorSize);

            // Secteur 0 = Index 1 (car 0 est gardé pour Pas de cible)
            return sectorIndex + 1;
        }
    }
}
