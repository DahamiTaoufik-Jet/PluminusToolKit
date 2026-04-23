using UnityEngine;

namespace Pluminus.Sensors.Extended
{
    /// <summary>
    /// Capteur No-Code Multi-Forme (Box/Sphere).
    /// N'utilise AUCUN Collider Unity : interroge directement le moteur physique avec Physics.OverlapBox/OverlapSphere
    /// dans sa propre zone géométrique. Chaque sensor est 100% indépendant des autres et ne touche à aucun collider existant.
    /// </summary>
    [AddComponentMenu("Pluminus/Sensors/Contact Sensor (Multi-Shape)")]
    public class ContactSensor : PluminusStateSensor
    {
        public enum ShapeType { Sphere, Box }

        [Header("Forme (en unités monde, Size = dimensions totales)")]
        public ShapeType shape = ShapeType.Box;
        public Vector3 size = new Vector3(1f, 1f, 1f);
        public Vector3 offset = Vector3.zero;

        [Header("Détection")]
        [Tooltip("Layers à scanner. Laissez sur 'Everything' si vous voulez ne filtrer que par Tag.")]
        public LayerMask detectionMask = ~0;

        [Tooltip("Le Tag de l'objet à détecter (ex: Bullet). Laissez vide pour ne pas filtrer par tag.")]
        public string targetTag = "Bullet";

        [Tooltip("Si coché, les colliders appartenant à l'agent porteur (ce GameObject et sa hiérarchie) sont ignorés. Évite l'auto-détection persistante.")]
        public bool ignoreSelfHierarchy = true;

        [Tooltip("Si coché, les colliders en mode Trigger sont aussi détectés (utile si les projectiles sont des Triggers).")]
        public bool detectTriggers = true;

        // Buffer réutilisé pour éviter toute allocation GC à chaque scan.
        private static readonly Collider[] _hitsBuffer = new Collider[32];

        private Transform _selfRoot;
        private bool isCurrentlyInside = false;
        private bool pulseMemory = false;

        protected override void Awake()
        {
            base.Awake();
            _selfRoot = transform.root;
        }

        public override int GetSubStateCount() => 2;

        public override int GetCurrentSubState()
        {
            // VRAI si une cible est dedans au dernier scan, OU vient de passer assez vite pour être manquée par le prochain tick.
            return (isCurrentlyInside || pulseMemory) ? 1 : 0;
        }

        private void FixedUpdate()
        {
            Vector3 worldPos = transform.TransformPoint(offset);
            var triggerMode = detectTriggers ? QueryTriggerInteraction.Collide : QueryTriggerInteraction.Ignore;
            int count;

            if (shape == ShapeType.Sphere)
            {
                count = Physics.OverlapSphereNonAlloc(worldPos, size.x * 0.5f, _hitsBuffer, detectionMask, triggerMode);
            }
            else // Box
            {
                count = Physics.OverlapBoxNonAlloc(worldPos, size * 0.5f, _hitsBuffer, transform.rotation, detectionMask, triggerMode);
            }

            bool found = false;
            bool filterByTag = !string.IsNullOrEmpty(targetTag);

            for (int i = 0; i < count; i++)
            {
                var col = _hitsBuffer[i];
                if (col == null) continue;
                if (ignoreSelfHierarchy && col.transform.root == _selfRoot) continue;
                if (filterByTag && !col.CompareTag(targetTag)) continue;

                found = true;
                break;
            }

            isCurrentlyInside = found;
            if (found) pulseMemory = true;
        }

        private void LateUpdate()
        {
            // Fin de frame : on efface la mémoire du pulse (cohérent avec TriggerTagSensor).
            pulseMemory = false;
        }

        private void OnDrawGizmosSelected()
        {
            bool isDetected = Application.isPlaying && (isCurrentlyInside || pulseMemory);
            Gizmos.color = isDetected ? new Color(1, 0, 0, 0.5f) : new Color(0, 1, 0, 0.3f);

            Vector3 worldPos = transform.TransformPoint(offset);
            Matrix4x4 oldMatrix = Gizmos.matrix;

            if (shape == ShapeType.Box)
            {
                Gizmos.matrix = Matrix4x4.TRS(worldPos, transform.rotation, Vector3.one);
                Gizmos.DrawCube(Vector3.zero, size);
                Gizmos.color = isDetected ? Color.red : Color.green;
                Gizmos.DrawWireCube(Vector3.zero, size);
            }
            else
            {
                Gizmos.DrawSphere(worldPos, size.x * 0.5f);
                Gizmos.color = isDetected ? Color.red : Color.green;
                Gizmos.DrawWireSphere(worldPos, size.x * 0.5f);
            }

            Gizmos.matrix = oldMatrix;
        }
    }
}
