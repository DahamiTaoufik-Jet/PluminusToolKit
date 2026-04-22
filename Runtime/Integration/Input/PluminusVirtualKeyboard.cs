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
    /// Spoofer Hardware. Simule des pressions de touche réelles au niveau du système (New Input System).
    /// Mode TRUE NO-CODE : Permet de brancher l'IA sur un code métier existant sans jamais le modifier.
    /// </summary>
    [AddComponentMenu("Pluminus/Integration/Input/Pluminus Virtual Keyboard")]
    public class PluminusVirtualKeyboard : MonoBehaviour, IActionExecutor
    {
        [Header("Simulation Hardware (New Input System)")]
        [Tooltip("Associe chaque bit du cerveau à des touches claviers réelles à enfoncer.")]
        public List<VirtualKeyMapping> mapping = new List<VirtualKeyMapping>();

        private Keyboard virtualKeyboard;
        private int lastActionId = 0;

        private void OnEnable()
        {
            // Créé son propre clavier invisible dans Unity.
            virtualKeyboard = InputSystem.AddDevice<Keyboard>("PluminusVirtualKeyboard");
            
            // CRITIQUE : Force ce clavier à devenir Keyboard.current immédiatement.
            // Sans ça, Keyboard.current pointe vers le clavier physique et l'IA est ignorée !
            virtualKeyboard.MakeCurrent();
        }

        private void OnDisable()
        {
            if (virtualKeyboard != null)
            {
                InputSystem.RemoveDevice(virtualKeyboard);
                virtualKeyboard = null;
            }
        }

        public void ExecuteAction(int actionId)
        {
            if (virtualKeyboard == null) return;

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

            // CHANGEMENT CRITIQUE : InputState.Change applique l'état IMMÉDIATEMENT dans la même frame.
            // QueueStateEvent attendait la frame suivante, ce qui causait des désynchronisations !
            InputState.Change(virtualKeyboard, keyboardState);
            
            // Verrouille le clavier virtuel comme "Keyboard.current" à CHAQUE tick.
            // Empêche le clavier physique de reprendre le contrôle si l'utilisateur touche une touche.
            virtualKeyboard.MakeCurrent();
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
