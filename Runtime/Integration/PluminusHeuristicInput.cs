using UnityEngine;
using System.Collections.Generic;
using Pluminus.Core;

namespace Pluminus.Integration
{
    [System.Serializable]
    public class KeyMapping
    {
        public KeyCode key;
        public int actionId;
    }

    /// <summary>
    /// Contrôleur Heuristique (Manuel).
    /// Permet au joueur de piloter l'agent au clavier pour tester le jeu.
    /// </summary>
    [AddComponentMenu("Pluminus/Integration/Pluminus Heuristic Input")]
    public class PluminusHeuristicInput : MonoBehaviour
    {
        private PluminusBrain brain;

        [Header("Mapping des Touches")]
        [Tooltip("Définit quelle touche clavier déclenche quel ID d'action.")]
        public List<KeyMapping> mappings = new List<KeyMapping>();

        private void Awake()
        {
            brain = GetComponent<PluminusBrain>();
        }

        private void Update()
        {
            if (brain == null || !brain.useHeuristic) return;

            // On réinitialise l'action à chaque frame (ou on garde la dernière ?)
            // Pour l'instant on cherche si une touche est pressée
            foreach (var mapping in mappings)
            {
                if (Input.GetKey(mapping.key))
                {
                    brain.SetHeuristicAction(mapping.actionId);
                    return; 
                }
            }

            // Si aucune touche n'est pressée
            brain.SetHeuristicAction(-1);
        }
    }
}
