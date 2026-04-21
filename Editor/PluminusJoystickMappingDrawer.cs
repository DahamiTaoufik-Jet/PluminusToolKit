using UnityEngine;
using UnityEditor;
using Pluminus.Integration.Input;

namespace Pluminus.EditorTools
{
    [CustomPropertyDrawer(typeof(PluminusJoystickMapping))]
    public class PluminusJoystickMappingDrawer : PropertyDrawer
    {
        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            EditorGUI.BeginProperty(position, label, property);
            
            // Dessine la flèche dépliante "Element X"
            Rect foldoutRect = new Rect(position.x, position.y, position.width, EditorGUIUtility.singleLineHeight);
            property.isExpanded = EditorGUI.Foldout(foldoutRect, property.isExpanded, label, true);
            
            if (property.isExpanded)
            {
                EditorGUI.indentLevel++;
                
                SerializedProperty directionMode = property.FindPropertyRelative("directionMode");
                SerializedProperty customAxisValue = property.FindPropertyRelative("customAxisValue");
                SerializedProperty activeButtons = property.FindPropertyRelative("activeButtons");

                float yOffset = EditorGUIUtility.singleLineHeight + 2;

                // 1. Dessine le menu déroulant Mode
                Rect dirRect = new Rect(position.x, position.y + yOffset, position.width, EditorGUIUtility.singleLineHeight);
                EditorGUI.PropertyField(dirRect, directionMode);
                yOffset += EditorGUIUtility.singleLineHeight + 2;

                // 2. LA MAGIE ICI : Ne dessiner le vecteur que si CustomVector (index 0) est sélectionné
                if (directionMode.enumValueIndex == (int)JoystickDirectionMode.CustomVector)
                {
                    // L'interface Vecteur 2D prend parfois un peu plus d'une ligne selon la version d'Unity
                    float vecHeight = EditorGUI.GetPropertyHeight(customAxisValue, true);
                    Rect customRect = new Rect(position.x, position.y + yOffset, position.width, vecHeight);
                    EditorGUI.PropertyField(customRect, customAxisValue, true);
                    yOffset += vecHeight + 2;
                }

                // 3. Dessiner la liste des boutons
                float buttonsHeight = EditorGUI.GetPropertyHeight(activeButtons, true);
                Rect buttonsRect = new Rect(position.x, position.y + yOffset, position.width, buttonsHeight);
                EditorGUI.PropertyField(buttonsRect, activeButtons, true);

                EditorGUI.indentLevel--;
            }

            EditorGUI.EndProperty();
        }

        public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
        {
            if (!property.isExpanded)
                return EditorGUIUtility.singleLineHeight;

            float height = EditorGUIUtility.singleLineHeight + 2; // Barre de titre de l'élément (Element X)

            SerializedProperty directionMode = property.FindPropertyRelative("directionMode");
            SerializedProperty customAxisValue = property.FindPropertyRelative("customAxisValue");
            SerializedProperty activeButtons = property.FindPropertyRelative("activeButtons");

            // Ajoute la hauteur du champ Direction Mode
            height += EditorGUIUtility.singleLineHeight + 2;

            // Ajoute la hauteur du vecteur SEULEMENT s'il est affiché
            if (directionMode.enumValueIndex == (int)JoystickDirectionMode.CustomVector)
            {
                height += EditorGUI.GetPropertyHeight(customAxisValue, true) + 2;
            }

            // Ajoute la taille exacte de la liste des boutons
            height += EditorGUI.GetPropertyHeight(activeButtons, true);

            return height;
        }
    }
}
