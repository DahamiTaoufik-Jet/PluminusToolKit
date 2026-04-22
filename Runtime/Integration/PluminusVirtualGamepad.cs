using UnityEngine;
using UnityEngine.Events;
using System.Collections.Generic;

namespace Pluminus.Integration
{
    [System.Serializable]
    public class VirtualButton
    {
        public string buttonName = "Bouton";
        [Tooltip("S'active en boucle tant que l'IA maintient ce bouton enfoncé")]
        public UnityEvent onAction;

        [Tooltip("S'active 1 seule fois au moment où l'IA appuie sur ce bouton")]
        public UnityEvent onPressed;

        [Tooltip("S'active 1 seule fois au moment où l'IA relâche ce bouton")]
        public UnityEvent onReleased;
    }

    /// <summary>
    /// Manette Virtuelle (Gamepad).
    /// Mode 'Simulation de Manette' : Utilise le Bitmasking pour déclencher plusieurs actions simultanément.
    /// Action 1 (Bouton 0), Action 2 (Bouton 1), Action 3 (Bouton 0 + 1), etc.
    /// </summary>
    [AddComponentMenu("Pluminus/Integration/Pluminus Virtual Gamepad")]
    public class PluminusVirtualGamepad : MonoBehaviour, IActionExecutor
    {
        [Header("Boutons de la Manette")]
        [Tooltip("Chaque bouton ici est lié à un bit. Bouton 1 = Bit 0 (valeur 1), Bouton 2 = Bit 1 (valeur 2)...")]
        public List<VirtualButton> buttons = new List<VirtualButton>();

        private int lastActionId = 0; // Mémoire de la frame précédente pour détecter Pression/Relâchement

        public void ExecuteAction(int actionId)
        {
            // Parcourt chaque bouton et vérifie si son bit est actif dans l'actionId
            for (int i = 0; i < buttons.Count; i++)
            {
                int bitValue = (1 << i);
                bool isPressedNow = (actionId & bitValue) != 0;
                bool wasPressedBefore = (lastActionId & bitValue) != 0;

                if (isPressedNow)
                {
                    // Action continue
                    buttons[i].onAction?.Invoke();

                    // Vient juste d'être appuyé
                    if (!wasPressedBefore)
                    {
                        buttons[i].onPressed?.Invoke();
                    }
                }
                else
                {
                    // Vient juste d'être relâché
                    if (wasPressedBefore)
                    {
                        buttons[i].onReleased?.Invoke();
                    }
                }
            }

            lastActionId = actionId;
        }

        public int GetMaxActions()
        {
            // Le nombre total de combinaisons est 2^N
            return (int)Mathf.Pow(2, buttons.Count);
        }

        public bool IsActionValid(int actionId)
        {
            // En mode manette, toutes les combinaisons de bits sont théoriquement valides
            return actionId >= 0 && actionId < GetMaxActions();
        }
    }
}

