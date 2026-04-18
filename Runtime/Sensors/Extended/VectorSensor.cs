using UnityEngine;

namespace Pluminus.Sensors.Extended
{
    /// <summary>
    /// Capteur Vectoriel (Vitesse de déplacement).
    /// Discrétise la force de mouvement du Rigidbody pour des analyses contextuelles (Ex: IA doit esquiver quand l'ennemi court).
    /// </summary>
    [AddComponentMenu("Pluminus/Sensors/Vector Sensor")]
    public class VectorSensor : PluminusStateSensor
    {
        [Header("Cible (Qui l'on observe)")]
        [Tooltip("L'objet avec un Rigidbody dont on veut analyser la vitesse. Laissez vide pour analyser propre parent.")]
        public Rigidbody targetRigidbody;

        [Header("Paramètres de Vitesse")]
        [Tooltip("Vitesse en dessous de laquelle l'objet est considéré immmobile.")]
        public float idleThreshold = 0.1f;
        
        [Tooltip("Vitesse au dessus de laquelle l'objet est considéré Rapide (entre les deux c'est 'Lent').")]
        public float fastThreshold = 3.0f;

        public override int GetSubStateCount()
        {
            // 5 états possibles :
            // 0: Immobile
            // 1: Avance (Lent)
            // 2: Avance (Rapide)
            // 3: Recule (Lent)
            // 4: Recule (Rapide)
            return 5;
        }

        protected override void Awake()
        {
            base.Awake();
            if (targetRigidbody == null)
            {
                targetRigidbody = GetComponentInParent<Rigidbody>();
            }
        }

        public override int GetCurrentSubState()
        {
            if (targetRigidbody == null) return 0; // Sécurité

            Vector3 worldVelocity = targetRigidbody.velocity;

            // Vitesse pure
            float speed = worldVelocity.magnitude;

            if (speed < idleThreshold)
            {
                return 0; // Immobile
            }

            // Pour savoir si l'Agent avance ou recule par rapport à sa propre rotation
            // On calcule le dot product (produit scalaire) entre sa direction (Forward) et son vecteur de mouvement.
            // S'il est positif, il avance. S'il est négatif, il recule.
            float directionDot = Vector3.Dot(targetRigidbody.transform.forward, worldVelocity.normalized);
            bool isMovingForward = directionDot >= 0;

            bool isFast = speed > fastThreshold;

            if (isMovingForward)
            {
                return isFast ? 2 : 1; // Avance Rapide(2) ou Lent(1)
            }
            else
            {
                return isFast ? 4 : 3; // Recule Rapide(4) ou Lent(3)
            }
        }
    }
}
