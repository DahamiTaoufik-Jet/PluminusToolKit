using UnityEngine;
using System.Collections.Generic;

namespace Pluminus.Sensors.Extended
{
    [System.Serializable]
    public class StateAxis
    {
        [Tooltip("Nom de cet axe (ex: 'Type Action', 'Phase', 'Posture')")]
        public string axisName = "Axe";

        [Tooltip("Nombre d'etats possibles sur cet axe (ex: 4 = Rien/Slash/Thrust/Shield)")]
        [Min(1)]
        public int stateCount = 3;
    }

    /// <summary>
    /// Capteur Multi-Axes (Array State).
    /// Expose plusieurs dimensions d'etat (ex: Type d'Action x Phase).
    /// Le code metier du dev n'a qu'a appeler SetAxis(index, valeur) pour signaler un changement.
    /// PluminusEyes lit le resultat combine automatiquement.
    /// </summary>
    [AddComponentMenu("Pluminus/Sensors/Array State Sensor")]
    public class ArrayStateSensor : PluminusStateSensor
    {
        [Header("Axes d'Etat")]
        [Tooltip("Definissez vos dimensions ici (ex: Axe 0 = Type Action, Axe 1 = Phase).")]
        public List<StateAxis> axes = new List<StateAxis>
        {
            new StateAxis { axisName = "Type Action", stateCount = 4 },
            new StateAxis { axisName = "Phase", stateCount = 3 }
        };

        private int[] currentValues;

        protected override void Awake()
        {
            base.Awake();
            currentValues = new int[axes.Count];
        }

        public override int GetSubStateCount()
        {
            if (axes.Count == 0) return 1;

            int total = 1;
            foreach (var axis in axes)
            {
                total *= axis.stateCount;
            }
            return total;
        }

        public override int GetCurrentSubState()
        {
            if (axes.Count == 0 || currentValues == null) return 0;

            // Mixed Radix Encoding (meme principe que PluminusEyes)
            int state = 0;
            int multiplier = 1;

            for (int i = 0; i < axes.Count; i++)
            {
                int val = (i < currentValues.Length) ? currentValues[i] : 0;
                val = Mathf.Clamp(val, 0, axes[i].stateCount - 1);

                state += val * multiplier;
                multiplier *= axes[i].stateCount;
            }

            return state;
        }

        /// <summary>
        /// La seule ligne a ajouter dans le code metier.
        /// Ex: sensor.SetAxis(0, 2) pour "Thrust", sensor.SetAxis(1, 1) pour "Preparation".
        /// </summary>
        public void SetAxis(int axisIndex, int value)
        {
            if (currentValues == null || axisIndex < 0 || axisIndex >= currentValues.Length) return;
            currentValues[axisIndex] = value;
        }

        /// <summary>
        /// Remet tous les axes a zero d'un coup.
        /// </summary>
        public void ResetAll()
        {
            if (currentValues == null) return;
            for (int i = 0; i < currentValues.Length; i++)
            {
                currentValues[i] = 0;
            }
        }

        private void OnDrawGizmosSelected()
        {
#if UNITY_EDITOR
            if (axes == null || axes.Count == 0) return;

            string label = "";
            for (int i = 0; i < axes.Count; i++)
            {
                int val = (currentValues != null && i < currentValues.Length) ? currentValues[i] : 0;
                label += $"{axes[i].axisName}: {val}  ";
            }

            UnityEditor.Handles.Label(
                transform.position + Vector3.up * 2.5f,
                label,
                new GUIStyle { normal = { textColor = Color.cyan }, alignment = TextAnchor.MiddleCenter }
            );
#endif
        }
    }
}
