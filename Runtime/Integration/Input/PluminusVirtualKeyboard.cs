using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.LowLevel;
using System.Collections.Generic;

namespace Pluminus.Integration.Input
{
    [System.Serializable]
    public class VirtualKeyMapping
    {
        [Tooltip("Nom descriptif de cette action simulée (ex: 'Sauter', 'Bouclier Nord')")]
        public string actionName = "Action";
        
        [Tooltip("La ou les touches physiques à enfoncer quand l'IA choisit cette action.")]
        public List<UnityEngine.InputSystem.Key> keysToPress = new List<UnityEngine.InputSystem.Key>();
    }

    /// <summary>
    /// Spoofer Hardware. Simule des pressions de touche réelles en injectant l'état
    /// directement dans le clavier physique existant (New Input System).
    /// Mode TRUE NO-CODE : Permet de brancher l'IA sur un code métier existant sans jamais le modifier.
    /// </summary>
    [AddComponentMenu("Pluminus/Integration/Input/Pluminus Virtual Keyboard")]
    public class PluminusVirtualKeyboard : MonoBehaviour, IActionExecutor
    {
        [Header("Simulation Hardware (New Input System)")]
        [Tooltip("Associe chaque bit du cerveau à des touches claviers réelles à enfoncer.")]
        public List<VirtualKeyMapping> mapping = new List<VirtualKeyMapping>();

        // On capture une référence FIXE au clavier physique au démarrage.
        // On n'en crée jamais un second : plus de conflit Keyboard.current !
        private Keyboard targetKeyboard;

        private void OnEnable()
        {
            // Capture le clavier existant.
            targetKeyboard = Keyboard.current;
            
            // ROOT CAUSE FIX : Quand l'utilisateur clique dans l'Inspector ou le Dashboard,
            // Unity fait un "soft reset" et relâche TOUTES les touches de TOUS les claviers.
            // Cette ligne dit à Unity : "Ignore les changements de focus, ne touche pas aux inputs."
            // C'est indispensable pour l'entraînement IA en arrière-plan !
            InputSystem.settings.backgroundBehavior = InputSettings.BackgroundBehavior.IgnoreFocus;
            
#if UNITY_EDITOR
            // En mode Éditeur, configure aussi le comportement pour que le clavier
            // reste actif même quand la Game View n'a pas le focus.
            InputSystem.settings.editorInputBehaviorInPlayMode = InputSettings.EditorInputBehaviorInPlayMode.AllDevicesRespectGameViewFocus;
#endif
        }

        private void OnDisable()
        {
            // Relâche toutes les touches proprement à l'arrêt
            if (targetKeyboard != null)
            {
                InputState.Change(targetKeyboard, new KeyboardState());
            }
            targetKeyboard = null;
        }

        public void ExecuteAction(int actionId)
        {
            // Si le clavier n'a pas encore été capturé (ex: OnEnable avant que Unity ne crée le device)
            if (targetKeyboard == null) targetKeyboard = Keyboard.current;
            if (targetKeyboard == null) return;

            // Reconstruit l'état complet du clavier pour ce tick.
            KeyboardState keyboardState = new KeyboardState();

            for (int i = 0; i < mapping.Count; i++)
            {
                int bitValue = (1 << i);
                if ((actionId & bitValue) != 0)
                {
                    foreach (var k in mapping[i].keysToPress)
                    {
                        keyboardState.Set(k, true);
                    }
                }
            }

            // Injection IMMÉDIATE dans le clavier physique.
            // Keyboard.current pointe TOUJOURS vers ce clavier → plus aucun conflit !
            InputState.Change(targetKeyboard, keyboardState);
        }

        public int GetMaxActions()
        {
            return (int)Mathf.Pow(2, mapping.Count);
        }

        public bool IsActionValid(int actionId)
        {
            return actionId >= 0 && actionId < GetMaxActions();
        }
    }
}
