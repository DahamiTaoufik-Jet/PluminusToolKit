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
            // Créé son propre clavier invisible dans Unity pour ne pas spammer/casser l'historique du vrai clavier.
            // Le New Input System combinera ses frappes avec les manettes existantes !
            virtualKeyboard = InputSystem.AddDevice<Keyboard>("PluminusVirtualKeyboard");
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

            // Analyse le bitmask de l'action envoyée par le cerveau
            for (int i = 0; i < mapping.Count; i++)
            {
                int bitValue = (1 << i);
                bool isPressedNow = (actionId & bitValue) != 0;
                bool wasPressedBefore = (lastActionId & bitValue) != 0;

                // Vient juste d'être activé ce tick
                if (isPressedNow && !wasPressedBefore)
                {
                    ChangeKeysState(mapping[i].keysToPress, 1f);
                }
                // Vient juste d'être désactivé ce tick
                else if (!isPressedNow && wasPressedBefore)
                {
                    ChangeKeysState(mapping[i].keysToPress, 0f);
                }
            }

            lastActionId = actionId;
        }

        /// <summary>
        /// Injecte un signal brut "Enfoncé" / "Relaché" directement dans le pipeline Input de Unity.
        /// </summary>
        private void ChangeKeysState(List<UnityEngine.InputSystem.Key> keys, float stateValue)
        {
            foreach (var k in keys)
            {
                var control = virtualKeyboard[k];
                if (control != null)
                {
                    // Update state sans réécrire tout le bloc mémoire clavier
                    InputSystem.QueueDeltaStateEvent(control, stateValue);
                }
            }
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
