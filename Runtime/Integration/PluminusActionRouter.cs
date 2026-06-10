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

        // Masque d'actions : si non-vide, seuls les index a true sont autorises
        private bool[] actionMask;
        private int lastExecutedAction = -1;
        private float lastExecutedTime;

        public int LastExecutedAction => lastExecutedAction;
        public float LastExecutedTime => lastExecutedTime;

        public void ExecuteAction(int actionId)
        {
            if (actionId >= 0 && actionId < actions.Count)
            {
                lastExecutedAction = actionId;
                lastExecutedTime = Time.time;
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
            if (actionId < 0 || actionId >= actions.Count) return false;
            if (actionMask != null && actionMask.Length == actions.Count) return actionMask[actionId];
            return true;
        }

        /// <summary>
        /// Active ou desactive une action par son index.
        /// Les actions desactivees ne seront jamais choisies par le cerveau.
        /// </summary>
        public void SetActionEnabled(int actionId, bool enabled)
        {
            EnsureMask();
            if (actionId >= 0 && actionId < actionMask.Length)
                actionMask[actionId] = enabled;
        }

        /// <summary>
        /// Active toutes les actions.
        /// </summary>
        public void EnableAllActions()
        {
            EnsureMask();
            for (int i = 0; i < actionMask.Length; i++)
                actionMask[i] = true;
        }

        /// <summary>
        /// Desactive toutes les actions sauf celles listees.
        /// </summary>
        public void EnableOnlyActions(int[] allowedIds)
        {
            EnsureMask();
            for (int i = 0; i < actionMask.Length; i++)
                actionMask[i] = false;
            for (int i = 0; i < allowedIds.Length; i++)
            {
                int id = allowedIds[i];
                if (id >= 0 && id < actionMask.Length)
                    actionMask[id] = true;
            }
        }

        private void EnsureMask()
        {
            if (actionMask == null || actionMask.Length != actions.Count)
            {
                actionMask = new bool[actions.Count];
                for (int i = 0; i < actionMask.Length; i++)
                    actionMask[i] = true;
            }
        }
    }
}
