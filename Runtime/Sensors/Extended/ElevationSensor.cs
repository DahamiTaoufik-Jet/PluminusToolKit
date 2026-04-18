using UnityEngine;

namespace Pluminus.Sensors.Extended
{
    [AddComponentMenu("Pluminus/Sensors/Elevation Sensor")]
    public class ElevationSensor : PluminusStateSensor
    {
        [Header("Cible (Target)")]
        [Tooltip("L'objet à comparer. Si vide et 'Auto-Find' est coché, cherchera le Player.")]
        public Transform target;
        public bool autoFindPlayerTag = true;

        [Header("Paramètres d'Élévation")]
        [Tooltip("Marge de tolérance (en unités) pour considérer que les objets sont 'Au même niveau'.")]
        public float sameLevelTolerance = 0.5f;

        [Header("Visualisation")]
        public bool showGizmos = true;
        public float visualRange = 5f;

        public override int GetSubStateCount() => 4;

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
            if (target == null) return 0; // State 0 = Missing Target

            float diff = target.position.y - transform.position.y;

            if (diff > sameLevelTolerance) return 3; // Plus Haut
            if (diff < -sameLevelTolerance) return 1; // Plus Bas
            return 2; // Même Niveau
        }

        private void OnDrawGizmosSelected()
        {
            if (!showGizmos) return;

            Vector3 center = transform.position;
            
            // Dessine la zone "Même Niveau" (Boîte transparente)
            Gizmos.color = new Color(0, 1, 0, 0.2f);
            Gizmos.DrawCube(center, new Vector3(visualRange, sameLevelTolerance * 2f, visualRange));
            
            // Dessine les limites haute et basse
            Gizmos.color = Color.green;
            Vector3 p1 = center + new Vector3(-visualRange/2, sameLevelTolerance, -visualRange/2);
            Vector3 p2 = center + new Vector3(visualRange/2, sameLevelTolerance, visualRange/2);
            // On dessine juste les contours pour la clarté
            Gizmos.DrawWireCube(center, new Vector3(visualRange, sameLevelTolerance * 2f, visualRange));

            if (target != null)
            {
                Gizmos.color = Color.yellow;
                Gizmos.DrawLine(center, new Vector3(center.x, target.position.y, center.z));
                Gizmos.DrawSphere(new Vector3(center.x, target.position.y, center.z), 0.1f);
                
#if UNITY_EDITOR
                string stateText = "";
                switch(GetCurrentSubState())
                {
                    case 1: stateText = "Plus Bas"; break;
                    case 2: stateText = "Même Niveau"; break;
                    case 3: stateText = "Plus Haut"; break;
                }
                UnityEditor.Handles.Label(center + Vector3.up * (sameLevelTolerance + 0.5f), $"Élévation : {stateText}");
#endif
            }
        }
    }
}
