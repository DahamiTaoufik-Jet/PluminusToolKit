using UnityEngine;
using System.Collections.Generic;

namespace Pluminus.Sensors.Extended
{
    /// <summary>
    /// Capteur Vectoriel Flexible.
    /// Découpe la vitesse de mouvement en zones personnalisables.
    /// </summary>
    [AddComponentMenu("Pluminus/Sensors/Vector Sensor (Flexible)")]
    public class VectorSensor : PluminusStateSensor
    {
        [Header("Cible")]
        public Rigidbody targetRigidbody;

        [Header("Paliers de Vitesse (m/s)")]
        [Tooltip("Ajoutez vos seuils de vitesse ici (ex: 0.1, 2, 5).")]
        public List<float> speedThresholds = new List<float> { 0.1f, 3.0f };

        public override int GetSubStateCount()
        {
            // N seuils = 2(Avance/Recule) * N + 1(Immobile)
            return (speedThresholds.Count * 2) + 1;
        }

        protected override void Awake()
        {
            base.Awake();
            if (targetRigidbody == null) targetRigidbody = GetComponentInParent<Rigidbody>();
            speedThresholds.Sort();
        }

        public override int GetCurrentSubState()
        {
            if (targetRigidbody == null) return 0;

            Vector3 worldVelocity = targetRigidbody.velocity;
            float speed = worldVelocity.magnitude;

            // Etat 0 : Immobile (sous le premier seuil)
            if (speed < speedThresholds[0]) return 0;

            float directionDot = Vector3.Dot(targetRigidbody.transform.forward, worldVelocity.normalized);
            bool isMovingForward = directionDot >= 0;

            // Trouve le palier de vitesse
            int speedLevel = 0;
            for (int i = 0; i < speedThresholds.Count; i++)
            {
                if (speed >= speedThresholds[i]) speedLevel = i;
            }

            // Calcul de l'index final
            // Etats pairs = Avance, Etats impairs = Recule ? Non, faisons plus simple :
            // 0 = Immobile
            // 1..N = Avance (Lent..Rapide)
            // N+1..2N = Recule (Lent..Rapide)
            int n = speedThresholds.Count;
            if (isMovingForward) return 1 + speedLevel;
            else return 1 + n + speedLevel;
        }
    }
}
