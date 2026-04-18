using UnityEngine;
using System.Reflection;
using System.Collections.Generic;

namespace Pluminus.Sensors.Extended
{
    /// <summary>
    /// Capteur de Valeur Automatique (Multi-Paliers).
    /// Scanne un composant cible et découpe la valeur en autant de tranches que souhaité.
    /// </summary>
    [AddComponentMenu("Pluminus/Sensors/Value Sensor (Flexible)")]
    public class ValueSensor : PluminusStateSensor
    {
        [Header("Source de Données")]
        public Component targetComponent;
        public string fieldName = "health";
        public string maxFieldName = "maxHealth";

        [Header("Paliers (Ratios 0.0 à 1.0)")]
        [Tooltip("Ajoutez vos seuils de ratio ici (ex: 0.3, 0.7).")]
        public List<float> thresholds = new List<float> { 0.3f, 0.7f };

        private FieldInfo valueField;
        private PropertyInfo valueProperty;
        private FieldInfo maxField;
        private PropertyInfo maxProperty;
        private float currentRatio = 1f;

        public override int GetSubStateCount() => thresholds.Count + 1;

        protected override void Awake()
        {
            base.Awake();
            InitializeReflection();
            thresholds.Sort();
        }

        private void InitializeReflection()
        {
            if (targetComponent == null) return;
            var type = targetComponent.GetType();
            valueField = type.GetField(fieldName, BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);
            if (valueField == null) valueProperty = type.GetProperty(fieldName, BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);

            if (!string.IsNullOrEmpty(maxFieldName))
            {
                maxField = type.GetField(maxFieldName, BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);
                if (maxField == null) maxProperty = type.GetProperty(maxFieldName, BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);
            }
        }

        public override int GetCurrentSubState()
        {
            UpdateValueFromReflection();
            for (int i = 0; i < thresholds.Count; i++)
            {
                if (currentRatio <= thresholds[i]) return i;
            }
            return thresholds.Count; 
        }

        private void UpdateValueFromReflection()
        {
            if (targetComponent == null) return;
            float val = 0;
            float max = 100;

            if (valueField != null) val = System.Convert.ToSingle(valueField.GetValue(targetComponent));
            else if (valueProperty != null) val = System.Convert.ToSingle(valueProperty.GetValue(targetComponent));

            if (maxField != null) max = System.Convert.ToSingle(maxField.GetValue(targetComponent));
            else if (maxProperty != null) max = System.Convert.ToSingle(maxProperty.GetValue(targetComponent));

            currentRatio = Mathf.Clamp01(val / max);
        }

        private void OnDrawGizmosSelected()
        {
#if UNITY_EDITOR
            if (targetComponent != null)
            {
                UpdateValueFromReflection();
                UnityEditor.Handles.Label(transform.position + Vector3.up * 2f, $"Valeur : {(currentRatio * 100):F0}%");
            }
#endif
        }
    }
}
