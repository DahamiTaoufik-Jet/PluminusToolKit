using UnityEngine;
using System.Collections.Generic;

namespace Pluminus.Core
{
    /// <summary>
    /// Une condition individuelle à vérifier pour qu'une règle soit valide.
    /// </summary>
    [System.Serializable]
    public class RuleCondition
    {
        public enum ConditionType
        {
            [Tooltip("Vérifie si un Bit spécifique est ACTIF dans l'action choisie par l'IA (utile pour PluminusVirtualGamepad en bitmask)")]
            ActionBitIsActive,

            [Tooltip("Vérifie si un Bit spécifique est INACTIF dans l'action choisie par l'IA (utile pour PluminusVirtualGamepad en bitmask)")]
            ActionBitIsInactive,

            [Tooltip("Vérifie si l'IA a choisi EXACTEMENT cette action (utile pour PluminusActionRouter en actions discrètes)")]
            ActionEquals,

            [Tooltip("Vérifie si l'IA a choisi N'IMPORTE QUELLE action SAUF celle-ci")]
            ActionNotEquals,

            [Tooltip("Vérifie si un capteur donné retourne un état précis (ex: Le capteur distance voit 'Proche')")]
            SensorEquals,

            [Tooltip("Vérifie si un capteur donné retourne un état DIFFÉRENT d'un état précis")]
            SensorNotEquals
        }

        [Tooltip("Le type de condition à vérifier")]
        public ConditionType type;

        [Tooltip("Pour ActionBit : L'index du bouton dans le bitmask (0 = premier bouton, 1 = deuxième...)")]
        public int bitIndex;

        [Tooltip("Pour ActionEquals / ActionNotEquals : L'ID exact de l'action à vérifier (ex: 0 = Idle, 1 = ShieldN, ...)")]
        public int actionId;

        [Tooltip("Pour Sensor : Le capteur No-Code à surveiller")]
        public Sensors.PluminusStateSensor sensor;

        [Tooltip("Pour Sensor : L'état que le capteur doit avoir (ou ne pas avoir)")]
        public int sensorState;
    }

    /// <summary>
    /// Une règle complète : Si TOUTES les conditions sont réunies, on applique une récompense.
    /// </summary>
    [System.Serializable]
    public class RewardRule
    {
        [Tooltip("Nom de la règle pour s'y retrouver dans l'inspecteur")]
        public string ruleName = "Nouvelle Règle";

        [Tooltip("TOUTES ces conditions doivent être vraies en même temps pour que la règle se déclenche")]
        public List<RuleCondition> conditions = new List<RuleCondition>();

        [Tooltip("Le Flag de récompense à appliquer au cerveau si les conditions sont remplies (doit exister dans le RewardProfile)")]
        public string rewardFlagToApply;

        [Tooltip("Si coché, cette règle ne se déclenchera qu'une seule fois, puis se réactivera quand ses conditions redeviennent fausses (Anti-Spam)")]
        public bool fireOnce = false;

        // Mémoire interne pour le mode FireOnce
        [System.NonSerialized] public bool hasAlreadyFired = false;
    }

    /// <summary>
    /// Moteur de Règles 2.0 (No-Code).
    /// Distribue des récompenses basées sur les DÉCISIONS de l'IA combinées à l'état de l'environnement.
    /// Supporte le Bitmask (touches multiples), les conditions multiples, et l'anti-spam de points.
    /// </summary>
    [AddComponentMenu("Pluminus/Core/Pluminus Rule Engine")]
    [RequireComponent(typeof(PluminusBrain))]
    public class PluminusRuleEngine : MonoBehaviour
    {
        [Header("Configuration des Règles No-Code")]
        public List<RewardRule> rules = new List<RewardRule>();

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
                bool allConditionsMet = true;

                foreach (var condition in rule.conditions)
                {
                    if (!EvaluateCondition(condition, actionId))
                    {
                        allConditionsMet = false;
                        break;
                    }
                }

                if (allConditionsMet)
                {
                    // Anti-Spam : si FireOnce est actif et qu'on l'a déjà déclenché, on saute
                    if (rule.fireOnce && rule.hasAlreadyFired) continue;

                    if (!string.IsNullOrEmpty(rule.rewardFlagToApply))
                    {
                        Debug.Log($"<color=magenta>[Pluminus RuleEngine]</color> Règle '<b>{rule.ruleName}</b>' validée -> Récompense '<b>{rule.rewardFlagToApply}</b>' appliquée !");
                        brain.ApplyRewardFlag(rule.rewardFlagToApply);
                    }

                    if (rule.fireOnce) rule.hasAlreadyFired = true;
                }
                else
                {
                    // Les conditions ne sont plus remplies : on réactive le mode FireOnce
                    if (rule.fireOnce) rule.hasAlreadyFired = false;
                }
            }
        }

        private bool EvaluateCondition(RuleCondition condition, int actionId)
        {
            switch (condition.type)
            {
                case RuleCondition.ConditionType.ActionBitIsActive:
                    return (actionId & (1 << condition.bitIndex)) != 0;

                case RuleCondition.ConditionType.ActionBitIsInactive:
                    return (actionId & (1 << condition.bitIndex)) == 0;

                case RuleCondition.ConditionType.ActionEquals:
                    return actionId == condition.actionId;

                case RuleCondition.ConditionType.ActionNotEquals:
                    return actionId != condition.actionId;

                case RuleCondition.ConditionType.SensorEquals:
                    if (condition.sensor == null) return false;
                    return condition.sensor.GetCurrentSubState() == condition.sensorState;

                case RuleCondition.ConditionType.SensorNotEquals:
                    if (condition.sensor == null) return false;
                    return condition.sensor.GetCurrentSubState() != condition.sensorState;

                default:
                    return false;
            }
        }
    }
}
