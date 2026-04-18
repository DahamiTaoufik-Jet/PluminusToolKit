using UnityEngine;

namespace Pluminus.Sensors.Extended
{
    [AddComponentMenu("Pluminus/Sensors/Contact Sensor (Multi-Shape)")]
    public class ContactSensor : PluminusStateSensor
    {
        public enum ShapeType { Sphere, Box, Diamond }

        [Header("Forme")]
        public ShapeType shape = ShapeType.Sphere;
        public Vector3 size = new Vector3(0.5f, 0.5f, 0.5f);
        public Vector3 offset = Vector3.zero;

        [Header("Collision")]
        public LayerMask obstacleMask;

        public override int GetSubStateCount() => 2;

        public override int GetCurrentSubState()
        {
            Vector3 worldPos = transform.position + transform.rotation * offset;
            bool hit = false;

            switch (shape)
            {
                case ShapeType.Sphere:
                    hit = Physics.CheckSphere(worldPos, size.x, obstacleMask);
                    break;
                case ShapeType.Box:
                    hit = Physics.CheckBox(worldPos, size / 2f, transform.rotation, obstacleMask);
                    break;
                case ShapeType.Diamond:
                    // Un losange est une boîte pivotée de 45 degrés
                    Quaternion diamondRotation = transform.rotation * Quaternion.Euler(0, 45, 0);
                    hit = Physics.CheckBox(worldPos, size / 2f, diamondRotation, obstacleMask);
                    break;
            }

            return hit ? 1 : 0;
        }

        private void OnDrawGizmosSelected()
        {
            Vector3 worldPos = transform.position + transform.rotation * offset;
            bool isDetected = Application.isPlaying && GetCurrentSubState() == 1;
            Gizmos.color = isDetected ? new Color(1, 0, 0, 0.5f) : new Color(0, 1, 0, 0.3f);

            Matrix4x4 oldRotation = Gizmos.matrix;
            
            switch (shape)
            {
                case ShapeType.Sphere:
                    Gizmos.DrawSphere(worldPos, size.x);
                    Gizmos.DrawWireSphere(worldPos, size.x);
                    break;
                case ShapeType.Box:
                    Gizmos.matrix = Matrix4x4.TRS(worldPos, transform.rotation, Vector3.one);
                    Gizmos.DrawCube(Vector3.zero, size);
                    Gizmos.DrawWireCube(Vector3.zero, size);
                    break;
                case ShapeType.Diamond:
                    Gizmos.matrix = Matrix4x4.TRS(worldPos, transform.rotation * Quaternion.Euler(0, 45, 0), Vector3.one);
                    Gizmos.DrawCube(Vector3.zero, size);
                    Gizmos.DrawWireCube(Vector3.zero, size);
                    break;
            }

            Gizmos.matrix = oldRotation;
        }
    }
}
