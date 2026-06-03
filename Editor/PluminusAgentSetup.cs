using UnityEngine;
using UnityEditor;
using Pluminus.Core;
using Pluminus.Sensors;
using Pluminus.Integration;

namespace Pluminus.EditorTools
{
    /// <summary>
    /// Menu contextuel pour poser tous les composants Pluminus de base sur un GameObject en un clic.
    /// Clic-droit sur un GameObject dans la Hierarchy -> Pluminus -> Setup Agent.
    /// </summary>
    public static class PluminusAgentSetup
    {
        [MenuItem("GameObject/Pluminus/Setup Agent", false, 10)]
        private static void SetupAgent()
        {
            GameObject selected = Selection.activeGameObject;
            if (selected == null)
            {
                EditorUtility.DisplayDialog("Pluminus", "Selectionnez un GameObject dans la Hierarchy d'abord !", "OK");
                return;
            }

            Undo.RegisterCompleteObjectUndo(selected, "Pluminus Setup Agent");

            int added = 0;

            if (selected.GetComponent<PluminusBrain>() == null)
            {
                Undo.AddComponent<PluminusBrain>(selected);
                added++;
            }

            if (selected.GetComponent<PluminusEyes>() == null)
            {
                Undo.AddComponent<PluminusEyes>(selected);
                added++;
            }

            if (selected.GetComponent<PluminusResetable>() == null)
            {
                Undo.AddComponent<PluminusResetable>(selected);
                added++;
            }

            if (added > 0)
            {
                Debug.Log($"<color=cyan>[Pluminus]</color> Setup Agent sur '{selected.name}' : {added} composant(s) ajoute(s) (Brain, Eyes, Resetable). Ajoutez vos Sensors et un Action Executor manuellement.");
            }
            else
            {
                Debug.Log($"<color=cyan>[Pluminus]</color> '{selected.name}' a deja tous les composants de base.");
            }
        }

        [MenuItem("GameObject/Pluminus/Setup Agent", true)]
        private static bool ValidateSetupAgent()
        {
            return Selection.activeGameObject != null;
        }
    }
}
