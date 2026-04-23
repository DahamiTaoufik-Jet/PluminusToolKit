using UnityEngine;

namespace Pluminus.Sensors.Extended
{
    /// <summary>
    /// Capteur No-Code Multi-Forme (Box/Sphere).
    /// Gère son propre Collider 3D et utilise OnTriggerStay pour une détection robuste.
    /// Supporte la détection par Tag (Bullet, Player, etc.).
    /// </summary>
    [AddComponentMenu("Pluminus/Sensors/Contact Sensor (Multi-Shape)")]
    public class ContactSensor : PluminusStateSensor
    {
        public enum ShapeType { Sphere, Box }

        [Header("Forme")]
        public ShapeType shape = ShapeType.Box;
        public Vector3 size = new Vector3(1f, 1f, 1f);
        public Vector3 offset = Vector3.zero;

        [Header("Détection")]
        [Tooltip("Le Tag de l'objet à détecter (ex: Bullet)")]
        public string targetTag = "Bullet";

        private bool isCurrentlyInside = false;
        private bool pulseMemory = false;

        private void Reset() => SetupCollider();
        private void OnValidate() => SetupCollider();

        private void SetupCollider()
        {
            // Nettoie l'ancien collider silencieusement si la forme a changé
            Collider[] existing = GetComponents<Collider>();
            foreach (var coll in existing)
            {
                if ((shape == ShapeType.Box && !(coll is BoxCollider)) ||
                    (shape == ShapeType.Sphere && !(coll is SphereCollider)))
                {
                    // On ne peut pas facilement détruire en OnValidate, 
                    // donc on prévient juste l'utilisateur ou on désactive.
                    coll.enabled = false;
                }
            }

            if (shape == ShapeType.Box)
            {
                BoxCollider box = GetComponent<BoxCollider>();
                if (box == null) box = gameObject.AddComponent<BoxCollider>();
                box.isTrigger = true;
                box.size = size;
                box.center = offset;
                box.enabled = true;
            }
            else
            {
                SphereCollider sphere = GetComponent<SphereCollider>();
                if (sphere == null) sphere = gameObject.AddComponent<SphereCollider>();
                sphere.isTrigger = true;
                sphere.radius = size.x / 2f;
                sphere.center = offset;
                sphere.enabled = true;
            }
        }

        [Tooltip("Si rempli, seuls les colliders portant ce Tag comptent. Laissez vide pour détecter n'importe quel objet du LayerMask.")]
        public string targetTag = "";

        [Tooltip("Si coché, les colliders appartenant à l'agent porteur (ce GameObject et sa hiérarchie) sont ignorés. Évite l'auto-détection persistante.")]
        public bool ignoreSelfHierarchy = true;

        // Buffer réutilisé pour éviter de générer du garbage à chaque Tick.
        private static readonly Collider[] _hitsBuffer = new Collider[16];

        // Racine de l'agent (GameObject le plus haut dans la hiérarchie) — calculée au premier besoin.
        private Transform _selfRoot;

        public override int GetSubStateCount() => 2;

        public override int GetCurrentSubState()
        {
            Vector3 worldPos = transform.position + transform.rotation * offset;
            int count = 0;

        private void OnTriggerStay(Collider other)
        {
            if (!string.IsNullOrEmpty(targetTag) && other.CompareTag(targetTag))
            {
                case ShapeType.Sphere:
                    count = Physics.OverlapSphereNonAlloc(worldPos, size.x, _hitsBuffer, obstacleMask);
                    break;
                case ShapeType.Box:
                    count = Physics.OverlapBoxNonAlloc(worldPos, size / 2f, _hitsBuffer, transform.rotation, obstacleMask);
                    break;
            }
        }

            if (count == 0) return 0;

            if (ignoreSelfHierarchy && _selfRoot == null) _selfRoot = transform.root;
            bool filterByTag = !string.IsNullOrEmpty(targetTag);

            for (int i = 0; i < count; i++)
            {
                var col = _hitsBuffer[i];
                if (col == null) continue;

                // Ignore tout collider appartenant à la hiérarchie du porteur (anti auto-détection).
                if (ignoreSelfHierarchy && col.transform.root == _selfRoot) continue;

                if (filterByTag && !col.CompareTag(targetTag)) continue;

                return 1;
            }
            return 0;
        }

        // Les Gizmos sont gérés par Unity via les Colliders, mais on peut ajouter un feedback visuel
        private void OnDrawGizmosSelected()
        {
            bool isDetected = Application.isPlaying && isCurrentlyInside;
            Gizmos.color = isDetected ? new Color(1, 0, 0, 0.5f) : new Color(0, 1, 0, 0.3f);
            
            Vector3 worldPos = transform.position + transform.rotation * offset;
            if (shape == ShapeType.Box)
            {
                Matrix4x4 oldRotation = Gizmos.matrix;
                Gizmos.matrix = Matrix4x4.TRS(worldPos, transform.rotation, Vector3.one);
                Gizmos.DrawCube(Vector3.zero, size);
                Gizmos.matrix = oldRotation;
            }
            else
            {
                Gizmos.DrawSphere(worldPos, size.x / 2f);
            }
        }
    }
}
