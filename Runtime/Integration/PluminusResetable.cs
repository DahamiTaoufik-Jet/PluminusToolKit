using UnityEngine;

namespace Pluminus.Integration
{
    /// <summary>
    /// Composant No-Code de Reset d'Episode.
    /// Posez-le sur tout objet de la scene qui doit revenir a son etat initial entre deux episodes.
    /// Glissez sa methode ResetToInitial() dans le OnReset du PluminusTrainingManager.
    /// </summary>
    [AddComponentMenu("Pluminus/Integration/Pluminus Resetable")]
    public class PluminusResetable : MonoBehaviour
    {
        [Header("Quoi Restaurer ?")]
        [Tooltip("Restaure la position et la rotation de depart.")]
        public bool resetTransform = true;

        [Tooltip("Restaure les velocites du Rigidbody (3D et/ou 2D) a zero.")]
        public bool resetRigidbody = true;

        [Tooltip("Restaure l'etat actif/inactif du GameObject (utile pour les objets detruits via SetActive(false)).")]
        public bool resetActiveState = false;

        [Header("Point de Depart (Optionnel)")]
        [Tooltip("Si assigne, l'objet sera teleporte a ce Transform au lieu de sa position de depart. Utile pour des spawn points.")]
        public Transform overrideSpawnPoint;

        // Snapshot memoire capturee au Start
        private Vector3 savedPosition;
        private Quaternion savedRotation;
        private bool savedActiveState;

        private Rigidbody rb3d;
        private Rigidbody2D rb2d;
        private CharacterController cc;

        private void Start()
        {
            CaptureInitialState();
        }

        private void CaptureInitialState()
        {
            if (overrideSpawnPoint != null)
            {
                savedPosition = overrideSpawnPoint.position;
                savedRotation = overrideSpawnPoint.rotation;
            }
            else
            {
                savedPosition = transform.position;
                savedRotation = transform.rotation;
            }

            savedActiveState = gameObject.activeSelf;

            rb3d = GetComponent<Rigidbody>();
            rb2d = GetComponent<Rigidbody2D>();
            cc = GetComponent<CharacterController>();
        }

        /// <summary>
        /// Restaure l'objet a son etat initial. Glissez cette methode dans le OnReset du TrainingManager !
        /// </summary>
        public void ResetToInitial()
        {
            if (resetActiveState)
            {
                gameObject.SetActive(savedActiveState);
            }

            if (resetTransform)
            {
                // CharacterController bloque transform.position — on le desactive temporairement
                bool hadCC = cc != null && cc.enabled;
                if (hadCC) cc.enabled = false;

                transform.position = savedPosition;
                transform.rotation = savedRotation;

                if (hadCC) cc.enabled = true;
            }

            if (resetRigidbody)
            {
                if (rb3d != null)
                {
                    rb3d.linearVelocity = Vector3.zero;
                    rb3d.angularVelocity = Vector3.zero;
                }
                if (rb2d != null)
                {
                    rb2d.linearVelocity = Vector2.zero;
                    rb2d.angularVelocity = 0f;
                }
            }
        }
    }
}
