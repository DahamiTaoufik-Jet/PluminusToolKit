using UnityEngine;
using System.Reflection;

namespace Pluminus.Sensors.Extended
{
    /// <summary>
    /// Capteur de Valeur Automatique.
    /// Scanne un composant cible et lit une variable (float/int) pour calculer l'état.
    /// </summary>
    [AddComponentMenu("Pluminus/Sensors/Value Sensor (Auto)")]
    public class ValueSensor : PluminusStateSensor
    {
        [Header("Source de Données")]
        [Tooltip("Le composant qui contient la valeur (ex: PlayerStats, Health, etc.)")]
        public Component targetComponent;
        
        [Tooltip("Le nom exact de la variable ou propriété à scanner (ex: currentHealth)")]
        public string fieldName = "health";

        [Tooltip("Le nom de la variable Max (optionnel) pour calculer le ratio. Si vide, utilise 100.")]
        public string maxFieldName = "maxHealth";

        [Header("Paliers de Valeurs")]
        [Range(0f, 1f)] public float criticalThreshold = 0.3f;
        [Range(0f, 1f)] public float highThreshold = 0.7f;

        private FieldInfo valueField;
        private PropertyInfo valueProperty;
        private FieldInfo maxField;
        private PropertyInfo maxProperty;

        private float currentRatio = 1f;

        public override int GetSubStateCount() => 3;

        protected override void Awake()
        {
            base.Awake();
            InitializeReflection();
        }

        private void InitializeReflection()
        {
            if (targetComponent == null) return;

            var type = targetComponent.GetType();
            
            // Cherche le champ ou la propriété pour la valeur actuelle
            valueField = type.GetField(fieldName, BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);
            if (valueField == null) valueProperty = type.GetProperty(fieldName, BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);

            // Cherche le champ ou la propriété pour la valeur max
            if (!string.IsNullOrEmpty(maxFieldName))
            {
                maxField = type.GetField(maxFieldName, BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);
                if (maxField == null) maxProperty = type.GetProperty(maxFieldName, BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);
            }
        }

        public override int GetCurrentSubState()
        {
            UpdateValueFromReflection();

            if (currentRatio < criticalThreshold) return 0; // Critique
            if (currentRatio < highThreshold) return 1;     // Moyen
            return 2;                                       // Plein
        }

        private void UpdateValueFromReflection()
        {
            if (targetComponent == null) return;

            float val = 0;
            float max = 100;

            // Lecture de la valeur actuelle
            if (valueField != null) val = System.Convert.ToSingle(valueField.GetValue(targetComponent));
            else if (valueProperty != null) val = System.Convert.ToSingle(valueProperty.GetValue(targetComponent));

            // Lecture de la valeur max
            if (maxField != null) max = System.Convert.ToSingle(maxField.GetValue(targetComponent));
            else if (maxProperty != null) max = System.Convert.ToSingle(maxProperty.GetValue(targetComponent));

            currentRatio = Mathf.Clamp01(val / max);
        }

        private void OnDrawGizmosSelected()
        {
            if (targetComponent != null)
            {
#if UNITY_EDITOR
                UpdateValueFromReflection();
                Color c = currentRatio < criticalThreshold ? Color.red : (currentRatio < highThreshold ? Color.yellow : Color.green);
                UnityEditor.Handles.Label(transform.position + Vector3.up * 2f, $"Valeur : {(currentRatio * 100):F0}%", new GUIStyle { normal = { textColor = c }, fontStyle = FontStyle.Bold });
#endif
            }
        }
    }
}
