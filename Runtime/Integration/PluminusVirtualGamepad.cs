using UnityEngine;
using UnityEngine.Events;
using System.Collections.Generic;

namespace Pluminus.Integration
{
    [System.Serializable]
    public class VirtualButton
    {
        public string buttonName = "Bouton";
        public UnityEvent onAction;
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

        public void ExecuteAction(int actionId)
        {
            // Parcourt chaque bouton et vérifie si son bit est actif dans l'actionId
            for (int i = 0; i < buttons.Count; i++)
            {
                int bitValue = (1 << i);
                if ((actionId & bitValue) != 0)
                {
                    buttons[i].onAction.Invoke();
                }
            }
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
