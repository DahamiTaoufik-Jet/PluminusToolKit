using UnityEngine;

namespace Pluminus.Sensors.Extended
{
    /// <summary>
    /// Capteur No-Code Multi-Forme (Box/Sphere).
    /// Gère son propre Collider 3D (Trigger) et utilise OnTriggerStay/Exit pour une détection indépendante.
    /// Chaque instance possède SON propre collider : plusieurs ContactSensor peuvent cohabiter sans se "contaminer".
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
        [Tooltip("Le Tag de l'objet à détecter (ex: Bullet). Laissez vide pour détecter n'importe quel collider qui entre dans la zone.")]
        public string targetTag = "Bullet";

        [Tooltip("Si coché, les colliders appartenant à l'agent porteur (ce GameObject et sa hiérarchie) sont ignorés. Évite l'auto-détection persistante.")]
        public bool ignoreSelfHierarchy = true;

        private bool isCurrentlyInside = false;
        private bool pulseMemory = false;

        private void Reset() => SetupCollider();
        private void OnValidate() => SetupCollider();

        private void SetupCollider()
        {
            // Désactive les colliders dont la forme ne correspond plus au choix.
            Collider[] existing = GetComponents<Collider>();
            foreach (var coll in existing)
            {
                if ((shape == ShapeType.Box && !(coll is BoxCollider)) ||
                    (shape == ShapeType.Sphere && !(coll is SphereCollider)))
                {
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

        public override int GetSubStateCount() => 2;

        public override int GetCurrentSubState()
        {
            // Cohérent avec TriggerTagSensor : VRAI si une cible est dedans OU vient de passer trop vite pour être vue par le prochain tick.
            return (isCurrentlyInside || pulseMemory) ? 1 : 0;
        }

        private void OnTriggerStay(Collider other)
        {
            if (!IsValidTarget(other)) return;
            isCurrentlyInside = true;
            pulseMemory = true;
        }

        private void OnTriggerExit(Collider other)
        {
            if (!IsValidTarget(other)) return;
            // Si un autre collider valide reste dans la zone, OnTriggerStay remettra isCurrentlyInside à true à la prochaine frame physique.
            isCurrentlyInside = false;
        }

        private bool IsValidTarget(Collider other)
        {
            if (other == null) return false;
            if (ignoreSelfHierarchy && other.transform.root == transform.root) return false;
            if (!string.IsNullOrEmpty(targetTag) && !other.CompareTag(targetTag)) return false;
            return true;
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
