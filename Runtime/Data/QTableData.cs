using System;
using System.Collections.Generic;
using UnityEngine;

namespace Pluminus.Data
{
    /// <summary>
    /// Représentation sérialisable de la matrice d'apprentissage (Q-Table) 
    /// pour être sauvegardée de façon persistante dans les assets Unity.
    /// </summary>
    [CreateAssetMenu(fileName = "NewQTableData", menuName = "Pluminus/Trained Brain (QTable)")]
    public class QTableData : ScriptableObject
    {
        [Tooltip("Nombre d'actions gérées par ce cerveau lors de son entraînement.")]
        public int numActions;

        // Unity ne sait pas sérialiser un Dictionnaire "par défaut", on le décompose en 2 listes liées.
        [HideInInspector]
        public List<int> stateIds = new List<int>();
        
        [HideInInspector]
        public List<StateActionValues> stateValues = new List<StateActionValues>();
    }

    [Serializable]
    public struct StateActionValues
    {
        public float[] values;
    }
}
