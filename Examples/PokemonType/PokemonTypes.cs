using UnityEngine;
using System.Collections.Generic;

namespace Pluminus.Examples.Pokemon
{
    public enum PokeType 
    {
        Normal = 0, Feu, Eau, Plante, Electrik, Glace, Combat, Poison, 
        Sol, Vol, Psy, Insecte, Roche, Spectre, Dragon, Tenebres, Acier
    }

    public static class PokemonTypeChart
    {
        // Matrice Attack (Ligne) vs Defense (Colonne)
        // Ordre basé sur l'enum PokeType
        private static readonly float[,] chart = new float[17, 17] {
            // Nor, Feu, Eau, Pla, Ele, Gla, Com, Poi, Sol, Vol, Psy, Ins, Roc, Spe, Dra, Ten, Aci
            { 1f, 1f, 1f, 1f, 1f, 1f, 1f, 1f, 1f, 1f, 1f, 1f, 0.5f, 0f, 1f, 1f, 0.5f }, // Normal
            { 1f, 0.5f, 0.5f, 2f, 1f, 2f, 1f, 1f, 1f, 1f, 1f, 2f, 0.5f, 1f, 0.5f, 1f, 2f }, // Feu
            { 1f, 2f, 0.5f, 0.5f, 1f, 1f, 1f, 1f, 2f, 1f, 1f, 1f, 2f, 1f, 0.5f, 1f, 1f }, // Eau
            { 1f, 0.5f, 2f, 0.5f, 1f, 1f, 1f, 0.5f, 2f, 0.5f, 1f, 0.5f, 2f, 1f, 0.5f, 1f, 0.5f }, // Plante
            { 1f, 1f, 2f, 0.5f, 0.5f, 1f, 1f, 1f, 0f, 2f, 1f, 1f, 1f, 1f, 0.5f, 1f, 1f }, // Electrik
            { 1f, 0.5f, 0.5f, 2f, 1f, 0.5f, 1f, 1f, 2f, 2f, 1f, 1f, 1f, 1f, 2f, 1f, 0.5f }, // Glace
            { 2f, 1f, 1f, 1f, 1f, 2f, 1f, 0.5f, 1f, 0.5f, 0.5f, 0.5f, 2f, 0f, 1f, 2f, 2f }, // Combat
            { 1f, 1f, 1f, 2f, 1f, 1f, 1f, 0.5f, 0.5f, 1f, 1f, 1f, 0.5f, 0.5f, 1f, 1f, 0f }, // Poison
            { 1f, 2f, 1f, 0.5f, 2f, 1f, 1f, 2f, 1f, 0f, 1f, 0.5f, 2f, 1f, 1f, 1f, 2f }, // Sol
            { 1f, 1f, 1f, 2f, 0.5f, 1f, 2f, 1f, 1f, 1f, 1f, 2f, 0.5f, 1f, 1f, 1f, 0.5f }, // Vol
            { 1f, 1f, 1f, 1f, 1f, 1f, 2f, 2f, 1f, 1f, 0.5f, 1f, 1f, 1f, 1f, 0f, 0.5f }, // Psy
            { 1f, 0.5f, 1f, 2f, 1f, 1f, 0.5f, 0.5f, 1f, 0.5f, 2f, 1f, 1f, 0.5f, 1f, 2f, 0.5f }, // Insecte
            { 1f, 2f, 1f, 1f, 1f, 2f, 0.5f, 1f, 0.5f, 2f, 1f, 2f, 1f, 1f, 1f, 1f, 0.5f }, // Roche
            { 0f, 1f, 1f, 1f, 1f, 1f, 1f, 1f, 1f, 1f, 2f, 1f, 1f, 2f, 1f, 0.5f, 0.5f }, // Spectre
            { 1f, 1f, 1f, 1f, 1f, 1f, 1f, 1f, 1f, 1f, 1f, 1f, 1f, 1f, 2f, 1f, 0.5f }, // Dragon
            { 1f, 1f, 1f, 1f, 1f, 1f, 0.5f, 1f, 1f, 1f, 2f, 1f, 1f, 2f, 1f, 0.5f, 0.5f }, // Tenebres
            { 1f, 0.5f, 0.5f, 1f, 0.5f, 2f, 1f, 1f, 1f, 1f, 1f, 1f, 2f, 1f, 1f, 1f, 0.5f }  // Acier
        };

        public static float GetMultiplier(PokeType attack, PokeType defense1, PokeType defense2)
        {
            float m1 = chart[(int)attack, (int)defense1];
            float m2 = chart[(int)attack, (int)defense2];
            return m1 * m2;
        }

        // Il y a 136 combinaisons possibles (17 * 16 / 2) pour un double type (sans compter un type pur).
        // L'IA devra choisir parmi ces 136 actions.
        private static List<(PokeType, PokeType)> actionToTypesMap;

        public static void InitializeMapping()
        {
            if (actionToTypesMap != null) return;
            actionToTypesMap = new List<(PokeType, PokeType)>();
            for (int i = 0; i < 17; i++)
            {
                for (int j = i + 1; j < 17; j++)
                {
                    actionToTypesMap.Add(((PokeType)i, (PokeType)j));
                }
            }
        }

        public static (PokeType, PokeType) DecodeActionId(int actionId)
        {
            InitializeMapping();
            if (actionId < 0 || actionId >= actionToTypesMap.Count) return (PokeType.Normal, PokeType.Vol);
            return actionToTypesMap[actionId];
        }

        public static int TotalCombinations => 136;
    }
}
