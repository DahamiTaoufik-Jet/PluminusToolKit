using UnityEngine;

namespace Pluminus.Sensors
{
    /// <summary>
    /// Capteur No-Code qui lance un "Laser" invisible dans une direction.
    /// Renvoie 1 si le laser touche le tag demandé, 0 sinon.
    /// </summary>
    public class RaycastTagSensor : PluminusStateSensor
    {
        [Tooltip("Le Tag de la cible à 'voir'")]
        public string targetTag = "Enemy";

        [Tooltip("La direction du champ de vision")]
        public Vector2 rayDirection = Vector2.right;

        [Tooltip("La longueur de la vue")]
        public float maxDistance = 5f;

        [Tooltip("Les calques physiques que le rayon peut heurter")]
        public LayerMask obstacleMask = Physics2D.AllLayers;

        public override int GetSubStateCount()
        {
            return 2; // 0 = Ne voit rien, 1 = Voit la cible
        }

        public override int GetCurrentSubState()
        {
            RaycastHit2D hit = Physics2D.Raycast(transform.position, rayDirection, maxDistance, obstacleMask);
            
            if (hit.collider != null && hit.collider.CompareTag(targetTag))
            {
                return 1;
            }
            
            return 0;
        }

        private void OnDrawGizmosSelected()
        {
            Gizmos.color = Color.magenta;
            Gizmos.DrawRay(transform.position, rayDirection.normalized * maxDistance);
        }
    }
}
