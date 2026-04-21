using UnityEngine;
using System.Collections.Generic;

namespace Pluminus.Integration.Input
{
    public enum JoystickDirectionMode
    {
        CustomVector,
        Up,
        Down,
        Left,
        Right,
        Stop
    }

    [System.Serializable]
    public class PluminusJoystickMapping
    {
        [Tooltip("Mode de direction : Choisir une direction rapide (Haut, Bas...) ou un Vecteur XY Personnalisé.")]
        public JoystickDirectionMode directionMode = JoystickDirectionMode.CustomVector;

        [Tooltip("Direction personnalisée (X,Y). Utilisée uniquement si le mode est sur CustomVector.")]
        public Vector2 customAxisValue;

        [Tooltip("Les noms des boutons simulés comme pressés avec cette action (ex: 'Jump', 'Fire')")]
        public List<string> activeButtons = new List<string>();

        /// <summary>
        /// Calcule le Vector2 final selon le mode choisi par le développeur.
        /// </summary>
        public Vector2 GetActualDirection()
        {
            switch (directionMode)
            {
                case JoystickDirectionMode.Up: return new Vector2(0, 1);
                case JoystickDirectionMode.Down: return new Vector2(0, -1);
                case JoystickDirectionMode.Left: return new Vector2(-1, 0);
                case JoystickDirectionMode.Right: return new Vector2(1, 0);
                case JoystickDirectionMode.Stop: return Vector2.zero;
                case JoystickDirectionMode.CustomVector:
                default:
                    return customAxisValue;
            }
        }
    }

    /// <summary>
    /// Remplace le PluminusActionRouter pour les jeux de mouvement continus (Top-Down, Platformer, FPS).
    /// Fournit un joystick IA qui convertit les choix du cerveau en directions lisibles (Vector2) via IPluminusInput.
    /// </summary>
    [AddComponentMenu("Pluminus/Integration/Input/Pluminus Joystick AI")]
    public class PluminusJoystickAI : MonoBehaviour, IActionExecutor, IPluminusInput
    {
        [Header("Configuration Directionnelle de l'IA")]
        [Tooltip("Configure ici ce que l'IA simule comme poussée de joystick à chaque ActionID choisi (Action 0 = Element 0).")]
        public List<PluminusJoystickMapping> actionMappings = new List<PluminusJoystickMapping>();

        // L'état actuel du joystick simulé par l'IA
        private Vector2 currentAxis = Vector2.zero;
        private List<string> currentButtons = new List<string>();

        // === IMPLEMENTATION DU CERVEAU (IActionExecutor) =======================
        // C'est appelé automatiquement par PluminusTempoDecision
        // =======================================================================
        
        public void ExecuteAction(int actionId)
        {
            if (actionId >= 0 && actionId < actionMappings.Count)
            {
                currentAxis = actionMappings[actionId].GetActualDirection();
                currentButtons = actionMappings[actionId].activeButtons;
            }
            else
            {
                currentAxis = Vector2.zero;
                currentButtons.Clear();
            }
        }

        public int GetMaxActions()
        {
            return actionMappings.Count;
        }

        public bool IsActionValid(int actionId)
        {
            return actionId >= 0 && actionId < actionMappings.Count;
        }

        // === IMPLEMENTATION DU CODE DEVELOPPEUR (IPluminusInput) ===============
        // C'est appelé par le script 'PlayerController' ou 'Movement' du développeur
        // =======================================================================
        
        public Vector2 GetAxis()
        {
            return currentAxis;
        }

        public bool GetButton(string actionName)
        {
            return currentButtons.Contains(actionName);
        }
    }
}
