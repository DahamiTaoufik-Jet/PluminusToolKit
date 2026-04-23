using UnityEngine;
using UnityEditor;
using Pluminus.Core;

namespace Pluminus.EditorTools
{
    /// <summary>
    /// PropertyDrawer qui n'affiche que les champs pertinents de RuleCondition selon le Type choisi.
    /// - ActionBitIsActive / ActionBitIsInactive -> Bit Index
    /// - ActionEquals / ActionNotEquals         -> Action Id
    /// - SensorEquals / SensorNotEquals         -> Sensor + Sensor State
    /// </summary>
    [CustomPropertyDrawer(typeof(RuleCondition))]
    public class PluminusRuleConditionDrawer : PropertyDrawer
    {
        private const float Spacing = 2f;

        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            EditorGUI.BeginProperty(position, label, property);

            Rect foldoutRect = new Rect(position.x, position.y, position.width, EditorGUIUtility.singleLineHeight);
            property.isExpanded = EditorGUI.Foldout(foldoutRect, property.isExpanded, label, true);

            if (!property.isExpanded)
            {
                EditorGUI.EndProperty();
                return;
            }

            EditorGUI.indentLevel++;

            SerializedProperty typeProp = property.FindPropertyRelative("type");
            SerializedProperty bitIndexProp = property.FindPropertyRelative("bitIndex");
            SerializedProperty actionIdProp = property.FindPropertyRelative("actionId");
            SerializedProperty sensorProp = property.FindPropertyRelative("sensor");
            SerializedProperty sensorStateProp = property.FindPropertyRelative("sensorState");

            float y = position.y + EditorGUIUtility.singleLineHeight + Spacing;
            float lineH = EditorGUIUtility.singleLineHeight;

            // Type toujours visible
            Rect typeRect = new Rect(position.x, y, position.width, lineH);
            EditorGUI.PropertyField(typeRect, typeProp);
            y += lineH + Spacing;

            int typeIndex = typeProp.enumValueIndex;
            var selected = (RuleCondition.ConditionType)typeIndex;

            switch (selected)
            {
                case RuleCondition.ConditionType.ActionBitIsActive:
                case RuleCondition.ConditionType.ActionBitIsInactive:
                {
                    Rect r = new Rect(position.x, y, position.width, lineH);
                    EditorGUI.PropertyField(r, bitIndexProp);
                    break;
                }

                case RuleCondition.ConditionType.ActionEquals:
                case RuleCondition.ConditionType.ActionNotEquals:
                {
                    Rect r = new Rect(position.x, y, position.width, lineH);
                    EditorGUI.PropertyField(r, actionIdProp);
                    break;
                }

                case RuleCondition.ConditionType.SensorEquals:
                case RuleCondition.ConditionType.SensorNotEquals:
                {
                    Rect r1 = new Rect(position.x, y, position.width, lineH);
                    EditorGUI.PropertyField(r1, sensorProp);
                    y += lineH + Spacing;
                    Rect r2 = new Rect(position.x, y, position.width, lineH);
                    EditorGUI.PropertyField(r2, sensorStateProp);
                    break;
                }
            }

            EditorGUI.indentLevel--;
            EditorGUI.EndProperty();
        }

        public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
        {
            if (!property.isExpanded)
                return EditorGUIUtility.singleLineHeight;

            float lineH = EditorGUIUtility.singleLineHeight;
            float total = lineH + Spacing; // Foldout
            total += lineH + Spacing;      // Type

            SerializedProperty typeProp = property.FindPropertyRelative("type");
            var selected = (RuleCondition.ConditionType)typeProp.enumValueIndex;

            switch (selected)
            {
                case RuleCondition.ConditionType.ActionBitIsActive:
                case RuleCondition.ConditionType.ActionBitIsInactive:
                case RuleCondition.ConditionType.ActionEquals:
                case RuleCondition.ConditionType.ActionNotEquals:
                    total += lineH + Spacing;
                    break;

                case RuleCondition.ConditionType.SensorEquals:
                case RuleCondition.ConditionType.SensorNotEquals:
                    total += (lineH + Spacing) * 2;
                    break;
            }

            return total;
        }
    }
}
