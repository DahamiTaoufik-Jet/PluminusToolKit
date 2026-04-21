using UnityEngine;
using System.Collections.Generic;

namespace Pluminus.Core
{
    [System.Serializable]
    public class ActionRewardRule
    {
        [Tooltip("L'ID de l'action qui déclenche la vérification (ex: 1 pour Sauter)")]
        public int triggerActionId;
        
        [Tooltip("Optionnel: Le capteur No-Code à surveiller au moment de l'action pour vérifier une condition")]
        public Sensors.PluminusStateSensor requiredSensorCondition;
        
        [Tooltip("L'état spécifique que le capteur doit avoir (ex: 0 pour dire que l'ennemi est Lointain)")]
        public int requiredSensorState;

        [Tooltip("Le Flag de récompense à appliquer au cerveau si les conditions sont remplies (Idem RewardProfile)")]
        public string rewardFlagToApply;
    }

    /// <summary>
    /// Moteur de Règles (No-Code). S'attache sur le GameObject de l'IA (là où se trouve PluminusBrain).
    /// Gère la distribution de récompenses (ex: JumpTiredness) selon l'environnement de façon 100% visuelle.
    /// </summary>
    [RequireComponent(typeof(PluminusBrain))]
    public class PluminusRuleEngine : MonoBehaviour
    {
        [Header("Configuration des Règles No-Code")]
        public List<ActionRewardRule> rules = new List<ActionRewardRule>();
        
        [Tooltip("La priorité globale : Si l'IA voit une règle valide, elle ignore son cerveau temporairement.")]
        public float ruleWeight = 1.0f;

        private PluminusBrain brain;

        private void Awake()
        {
            brain = GetComponent<PluminusBrain>();
            if (brain != null && brain.OnActionExecuted != null)
            {
                // On se branche pour écouter l'IA de l'extérieur du code !
                brain.OnActionExecuted.AddListener(EvaluateRules);
            }
        }

        private void EvaluateRules(int actionId)
        {
            foreach (var rule in rules)
            {
                if (rule.triggerActionId == actionId)
                {
                    bool conditionMet = true;
                    
                    if (rule.requiredSensorCondition != null)
                    {
                        if (rule.requiredSensorCondition.GetCurrentSubState() != rule.requiredSensorState)
                        {
                            conditionMet = false;
                        }
                    }

                    // Application du verdict
                    if (conditionMet && !string.IsNullOrEmpty(rule.rewardFlagToApply))
                    {
                        Debug.Log($"<color=magenta>[Pluminus RuleEngine]</color> Action {actionId} validée -> Récompense '<b>{rule.rewardFlagToApply}</b>' appliquée !");
                        brain.ApplyRewardFlag(rule.rewardFlagToApply);
                    }
                }
            }
        }
    }
}
