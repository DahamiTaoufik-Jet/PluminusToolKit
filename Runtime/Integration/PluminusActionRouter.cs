using UnityEngine;
using UnityEngine.Events;
using System.Collections.Generic;

namespace Pluminus.Integration
{
    /// <summary>
    /// Réceptacle d'Actions (Router).
    /// Mode 'Simulation Brute' : Mappe un ID d'action directement vers un UnityEvent à l'index correspondant.
    /// Idéal pour les Boss de RPG ou les IA avec des attaques discrètes.
    /// </summary>
    [AddComponentMenu("Pluminus/Integration/Pluminus Action Router")]
    public class PluminusActionRouter : MonoBehaviour, IActionExecutor
    {
        [Header("Liste des Actions")]
        [Tooltip("Chaque événement ici correspond à un ID d'action de l'IA (Index 0 = Action 0, etc.)")]
        public List<UnityEvent> actions = new List<UnityEvent>();

        public void ExecuteAction(int actionId)
        {
            if (actionId >= 0 && actionId < actions.Count)
            {
                actions[actionId].Invoke();
            }
            else
            {
                Debug.LogWarning($"[ActionRouter] Tentative d'exécution de l'ID d'action invalide : {actionId}");
            }
        }

        public int GetMaxActions()
        {
            return actions.Count;
        }

        public bool IsActionValid(int actionId)
        {
            // Par défaut dans le Router, toutes les actions configurées sont valides.
            // On pourrait ajouter des cooldowns ici plus tard.
            return actionId >= 0 && actionId < actions.Count;
        }
    }
}
