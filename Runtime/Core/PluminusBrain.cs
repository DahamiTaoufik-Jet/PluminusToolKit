using UnityEngine;
using UnityEngine.Events;
using System.Collections.Generic;
using Pluminus.Data;
using Pluminus.Integration;

namespace Pluminus.Core
{
    public enum BrainMode
    {
        [Tooltip("L'IA apprend activement : explore, fait des erreurs, met a jour sa Q-Table.")]
        Training,

        [Tooltip("L'IA utilise sa Q-Table sans apprendre. Choisit toujours la meilleure action connue (epsilon = 0).")]
        Exploitation
    }

    /// <summary>
    /// Le composant principal à attacher sur votre ennemi dans Unity.
    /// Il fait le pont entre le moteur mathématique (QLearningEngine), la configuration, et votre jeu.
    /// </summary>
    public class PluminusBrain : MonoBehaviour
    {
        [Header("Mode")]
        [Tooltip("Training = apprend et explore. Exploitation = utilise la Q-Table telle quelle, sans apprendre.")]
        public BrainMode brainMode = BrainMode.Training;

        [Header("Configuration")]
        [Tooltip("Le profil hyperparamètres. Plusieurs ennemis peuvent partager le même BrainConfig.")]
        public BrainConfig brainConfig;
        
        [Tooltip("Le profil définissant les points gagnés ou perdus pour certaines actions.")]
        public RewardProfile rewardProfile;

        [Header("Persistance (Optionnel)")]
        [Tooltip("Le fichier de sauvegarde (QTableData) contenant la mémoire entraînée de l'IA.")]
        public QTableData memoryAsset;

        [Header("No-Code Events")]
        [Tooltip("Déclenché à chaque fois que l'IA exécute une action (Renvoie l'ID de l'action).")]
        public UnityEvent<int> OnActionExecuted;

        [Header("Composants (Auto-détectés)")]
        [Tooltip("Le script qui observe l'environnement (ex: UnityStateBuilder)")]
        public MonoBehaviour environmentObserverObject;
        [Tooltip("Le script qui exécute les actions (ex: VirtualGamepad)")]
        public MonoBehaviour actionExecutorObject;

        private IEnvironmentObserver environmentObserver;
        private IActionExecutor actionExecutor;
        
        // Le moteur interne d'apprentissage
        private QLearningEngine learningEngine;

        [Header("Rythme de Decision")]
        [Tooltip("Si coche, l'IA decide automatiquement a intervalle regulier.")]
        public bool autoTick = true;
        [Tooltip("Temps de base entre chaque decision (en secondes).")]
        public float decisionRate = 0.1f;
        [Tooltip("Si coche, l'IA decidera plus souvent quand le jeu est accelere pour eviter de rater des obstacles.")]
        public bool dynamicDecisionRate = true;
        private float tickTimer;

        [Header("Mode Heuristique (Manuel)")]
        [Tooltip("Si coché, l'IA ignore son propre cerveau et exécute les actions envoyées par le joueur (pour debug/test).")]
        public bool useHeuristic = false;

        [Header("Epsilon Scheduler")]
        [Tooltip("Si > 0, reset automatiquement l'epsilon tous les N episodes. Cree des cycles exploration/exploitation.")]
        public int resetEpsilonEveryNEpisodes = 0;
        [Tooltip("Valeur cible de l'epsilon lors du reset automatique. 0 = reprend la valeur initiale du BrainConfig.")]
        [Range(0f, 1f)]
        public float resetEpsilonTarget = 0f;

        [Header("Anti-Idle")]
        [Tooltip("Si > 0, punit l'IA et termine l'episode si elle repete la meme action N fois de suite.")]
        public int maxConsecutiveSameAction = 0;
        [Tooltip("Penalite appliquee quand l'IA idle trop (valeur negative).")]
        public float idlePenalty = -5f;

        [Header("Auto-Save")]
        [Tooltip("Si > 0 et qu'un Memory Asset est assigne, exporte automatiquement la Q-Table toutes les N minutes.")]
        public float autoSaveIntervalMinutes = 0f;

        [Header("Debug Recompenses")]
        [Tooltip("Affiche dans la console chaque appel a ApplyRewardFlag (flag recu, valeur appliquee, ou erreur si flag introuvable).")]
        public bool logRewards = false;
        
        // Historique court-terme pour l'apprentissage
        private int previousState = -1;
        private int lastActionTaken = -1;
        private int consecutiveSameActionCount = 0;
        private float currentEpsilon; // Le taux d'exploration actuel (qui diminue avec le temps)

        private float accumulatedReward = 0f; // Les points accumulés depuis la dernière action
        private int heuristicActionId = -1; // L'action injectée par le mode manuel

        // --- Analytics & Performance ---
        [Header("Statistiques d'Apprentissage")]
        public Pluminus.Data.PluminusAnalyticsData analyticsData;
        public List<float> episodeRewards = new List<float>(); // Historique des scores par épisodes
        public List<float> continuousHistory = new List<float>(); // Historique continu (temps réel)
        
        private float currentEpisodeTotalReward = 0f;
        private float sessionTotalReward = 0f;
        private int totalEpisodes = 0;
        private float statsTimer = 0f;
        private float autoSaveTimer = 0f;

        // Nouvelles metrics demandées
        private int positiveRewardCount = 0;
        private int negativeRewardCount = 0;
        private List<bool> recentRewardHistory = new List<bool>(); // Historique des 100 derniers coups (true=positif)

        private void Awake()
        {
            // 1. Récupération de l'Observer
            if (environmentObserverObject != null) environmentObserver = environmentObserverObject as IEnvironmentObserver;
            if (environmentObserver == null) environmentObserver = GetComponentInChildren<IEnvironmentObserver>();

            // 2. Récupération de l'Executor
            if (actionExecutorObject != null) actionExecutor = actionExecutorObject as IActionExecutor;
            if (actionExecutor == null) actionExecutor = GetComponentInChildren<IActionExecutor>();

            if (environmentObserver == null || actionExecutor == null)
            {
                Debug.LogError("Erreur: Le PluminusBrain n'a pas trouvé d'IEnvironmentObserver (PluminusEyes) ou d'IActionExecutor (Gamepad) sur cet objet ou ses enfants !");
                enabled = false;
                return;
            }

            // Initialise le moteur interne avec le nombre d'actions possibles
            int totalActions = actionExecutor.GetMaxActions();
            learningEngine = new QLearningEngine(totalActions);

            if (brainMode == BrainMode.Exploitation)
            {
                currentEpsilon = 0f;
                if (memoryAsset != null && memoryAsset.stateIds.Count > 0)
                {
                    ImportBrain(memoryAsset);
                }
                else
                {
                    Debug.LogWarning($"[Pluminus] '{gameObject.name}' en mode Exploitation mais aucune Q-Table chargee ! L'IA agira au hasard.");
                }
            }
            else if (brainConfig != null)
            {
                currentEpsilon = brainConfig.explorationRate;
            }
        }

        /// <summary>
        /// Injecte une action manuelle pour le mode Heuristique.
        /// </summary>
        public void SetHeuristicAction(int actionId)
        {
            heuristicActionId = actionId;
        }

        /// <summary>
        /// Méthode principale à appeler régulièrement dans votre jeu (ex: dans Update, via une Coroutine, ou à la fin d'une animation d'attaque).
        /// Elle gère le cycle complet: Observer -> Apprendre -> Décider -> Agir.
        /// </summary>
        public void TickDecision()
        {
            if (brainConfig == null) return;

            // 1. Observe la situation du jeu (Le "State")
            int currentState = environmentObserver.GetCurrentStateId();

            // 2. APPRENDRE des conséquences de la décision précédente
            if (brainMode == BrainMode.Training && brainConfig.isLearningEnabled && previousState != -1 && lastActionTaken != -1)
            {
                learningEngine.UpdateQValue(
                    previousState,
                    lastActionTaken,
                    accumulatedReward,
                    currentState,
                    brainConfig.learningRate,
                    brainConfig.discountFactor
                );

                // Réduit très légèrement le taux d'exploration pour stabiliser l'IA au fil du temps
                currentEpsilon = Mathf.Max(brainConfig.minExplorationRate, currentEpsilon * brainConfig.explorationDecayRate);

                // On ne consomme les récompenses qu'une fois réellement attribuées à une transition.
                // Sinon (pas d'action précédente), on les conserve pour la prochaine transition apprenable.
                accumulatedReward = 0f;
            }
            else if (brainMode == BrainMode.Exploitation)
            {
                accumulatedReward = 0f;
            }

            // 3. DECIDER de la prochaine action
            int chosenAction = -1;

            if (useHeuristic)
            {
                // On valide l'action injectée pour éviter toute écriture hors-bornes dans la Q-Table au tick suivant.
                if (heuristicActionId >= 0 && heuristicActionId < actionExecutor.GetMaxActions() && actionExecutor.IsActionValid(heuristicActionId))
                {
                    chosenAction = heuristicActionId;
                }
                else
                {
                    chosenAction = -1;
                }
            }
            else
            {
                chosenAction = learningEngine.DecideAction(currentState, currentEpsilon, actionExecutor.IsActionValid);
            }
            
            // 4. AGIR dans le jeu
            if (chosenAction != -1)
            {
                actionExecutor.ExecuteAction(chosenAction);
                if (OnActionExecuted != null) OnActionExecuted.Invoke(chosenAction);
            }

            // 5. Anti-Idle : detecte les repetitions excessives de la meme action
            if (maxConsecutiveSameAction > 0 && chosenAction != -1)
            {
                if (chosenAction == lastActionTaken)
                {
                    consecutiveSameActionCount++;
                    if (consecutiveSameActionCount >= maxConsecutiveSameAction)
                    {
                        Debug.Log($"<color=red>[Pluminus Anti-Idle]</color> '{gameObject.name}' -> action {chosenAction} repetee {consecutiveSameActionCount}x. Penalite {idlePenalty} + fin d'episode.");
                        AddReward(idlePenalty, true);
                        consecutiveSameActionCount = 0;
                    }
                }
                else
                {
                    consecutiveSameActionCount = 0;
                }
            }

            // 6. Mémoriser ce qu'on vient de faire pour pouvoir apprendre la prochaine fois
            previousState = currentState;
            lastActionTaken = chosenAction;
        }

        /// <summary>
        /// Ajoute une récompense ou une punition directement depuis votre code.
        /// </summary>
        /// <param name="amount">Valeur (positif = bon, négatif = mauvais)</param>
        /// <param name="isTerminal">Si vrai, l'épisode se termine immédiatement.</param>
        public void AddReward(float amount, bool isTerminal = false)
        {
            accumulatedReward += amount;
            currentEpisodeTotalReward += amount;
            sessionTotalReward += amount;

            // Stats numériques : on regarde le signe
            if (amount != 0)
            {
                bool isPositive = amount > 0;
                if (isPositive)
                {
                    positiveRewardCount++;
                    if (analyticsData != null) analyticsData.totalPositiveRewards++;
                }
                else
                {
                    negativeRewardCount++;
                    if (analyticsData != null) analyticsData.totalNegativeRewards++;
                }

                // Historique tournant pour la "Précision Récente"
                recentRewardHistory.Add(isPositive);
                if (recentRewardHistory.Count > 100) recentRewardHistory.RemoveAt(0);
            }

            if (isTerminal)
            {
                EndEpisode();
            }
        }

        private void Update()
        {
            if (!Application.isPlaying) return;

            // Auto-Tick : le cerveau decide a son propre rythme
            if (autoTick && !useHeuristic)
            {
                float currentDecisionRate = dynamicDecisionRate ? (decisionRate / Mathf.Max(1f, Time.timeScale)) : decisionRate;
                tickTimer += Time.unscaledDeltaTime;

                if (tickTimer >= currentDecisionRate)
                {
                    TickDecision();
                    tickTimer = 0;
                }
            }

            // Capture de stats pour le mode continu (toutes les secondes)
            statsTimer += Time.deltaTime;
            if (statsTimer >= 1.0f)
            {
                continuousHistory.Add(sessionTotalReward);
                if (continuousHistory.Count > 300) continuousHistory.RemoveAt(0); // 5 minutes de stats
                
                // Persistance via l'asset
                if (analyticsData != null) 
                {
                    analyticsData.AddContinuousPoint(sessionTotalReward);
#if UNITY_EDITOR
                    UnityEditor.EditorUtility.SetDirty(analyticsData);
#endif
                }

                statsTimer = 0;
            }

            // Auto-save periodique de la Q-Table
            if (autoSaveIntervalMinutes > 0f && memoryAsset != null)
            {
                autoSaveTimer += Time.unscaledDeltaTime;
                if (autoSaveTimer >= autoSaveIntervalMinutes * 60f)
                {
                    autoSaveTimer = 0f;
                    ExportBrain(memoryAsset);
#if UNITY_EDITOR
                    UnityEditor.EditorUtility.SetDirty(memoryAsset);
                    UnityEditor.AssetDatabase.SaveAssets();
#endif
                    Debug.Log($"<color=cyan>[Pluminus AutoSave]</color> '{gameObject.name}' -> Q-Table sauvegardee ({memoryAsset.stateIds.Count} etats, episode {totalEpisodes})");
                }
            }

        }

        /// <summary>
        /// Clôture l'épisode actuel et stocke les statistiques.
        /// </summary>
        public void EndEpisode()
        {
            // Un épisode est un succès si le score final est positif
            bool wasSuccess = currentEpisodeTotalReward > 0;

            episodeRewards.Add(currentEpisodeTotalReward);
            if (episodeRewards.Count > 100) episodeRewards.RemoveAt(0);
            
            // Calcul du Winrate global basé sur tous les épisodes de la session
            float currentWinRate = (float)episodeRewards.FindAll(r => r > 0).Count / episodeRewards.Count * 100f;

            // Persistance via l'asset
            if (analyticsData != null) 
            {
                analyticsData.AddEpisode(currentEpisodeTotalReward);
                if (wasSuccess) analyticsData.totalSuccesses++;
                
                // Track winrate curve
                analyticsData.winRateHistory.Add(currentWinRate);
                if (analyticsData.winRateHistory.Count > 100) analyticsData.winRateHistory.RemoveAt(0);

#if UNITY_EDITOR
                UnityEditor.EditorUtility.SetDirty(analyticsData);
#endif
            }

            totalEpisodes++;
            currentEpisodeTotalReward = 0;

            // Epsilon Scheduler : reset periodique de l'exploration
            if (brainMode == BrainMode.Training && resetEpsilonEveryNEpisodes > 0 && totalEpisodes % resetEpsilonEveryNEpisodes == 0)
            {
                float target = resetEpsilonTarget > 0f ? resetEpsilonTarget : (brainConfig != null ? brainConfig.explorationRate : 0.2f);
                currentEpsilon = target;
                Debug.Log($"<color=yellow>[Pluminus Scheduler]</color> '{gameObject.name}' -> Epsilon reset a {target:P0} (episode {totalEpisodes})");
            }

            // Réinitialise l'historique d'apprentissage pour ne pas lier la mort au nouvel état
            previousState = -1;
            lastActionTaken = -1;
            consecutiveSameActionCount = 0;
        }

        /// <summary>
        /// Méthode à appeler depuis les événements de votre jeu (ex: OnHit, OnDodge) pour donner ou retirer des points à l'IA.
        /// </summary>
        /// <param name="flag">Le nom textuel de l'événement (ex: "TookDamage")</param>
        public void ApplyRewardFlag(string flag)
        {
            if (rewardProfile == null)
            {
                if (logRewards) Debug.LogWarning($"<color=orange>[Pluminus Reward]</color> '{gameObject.name}' -> Flag '<b>{flag}</b>' ignore : aucun RewardProfile assigne !");
                return;
            }

            if (rewardProfile.TryGetReward(flag, out RewardEvent reward))
            {
                if (logRewards)
                {
                    Debug.Log($"<color=green>[Pluminus Reward]</color> '{gameObject.name}' -> '<b>{flag}</b>' = <b>{reward.rewardValue:+0.##;-0.##}</b>{(reward.isTerminalState ? " [TERMINAL]" : "")}");
                }
                AddReward(reward.rewardValue, reward.isTerminalState);
            }
            else
            {
                if (logRewards) Debug.LogWarning($"<color=red>[Pluminus Reward]</color> '{gameObject.name}' -> Flag '<b>{flag}</b>' introuvable dans le RewardProfile ! Verifiez l'orthographe.");
            }
        }

        // --- Fonctions Utilitaires ---

        public int GetTotalEpisodes() => totalEpisodes;
        public int GetPositiveRewards() => positiveRewardCount;
        public int GetNegativeRewards() => negativeRewardCount;
        public float GetLastEpisodeReward() => episodeRewards.Count > 0 ? episodeRewards[episodeRewards.Count - 1] : 0;

        public float GetRecentAccuracy()
        {
            if (recentRewardHistory.Count == 0) return 0;
            return (float)recentRewardHistory.FindAll(h => h).Count / recentRewardHistory.Count * 100f;
        }

        public QTable GetCurrentQTable() => learningEngine.GetQTable();
        
        public float GetCurrentEpsilon() => currentEpsilon;

        public void SetCurrentEpsilon(float value) => currentEpsilon = value;
        
        // --- Fonctions de Sauvegarde et Chargement ---

        /// <summary>
        /// Écrase la mémoire actuelle en chargeant un cerveau pré-entraîné (ScriptableObject).
        /// </summary>
        public void ImportBrain(QTableData loadedData)
        {
            if (loadedData == null) return;
            
            QTable newTable = new QTable(loadedData.numActions);
            for (int i = 0; i < loadedData.stateIds.Count; i++)
            {
                int sId = loadedData.stateIds[i];
                float[] sVals = loadedData.stateValues[i].values;
                newTable.table[sId] = sVals;
            }

            learningEngine.SetQTable(newTable);
            Debug.Log($"[Pluminus] Cerveau importé avec succès ({loadedData.stateIds.Count} états connus)");
        }

        /// <summary>
        /// Extrait la mémoire actuelle du moteur pour la sauvegarder dans un objet de données.
        /// </summary>
        public void ExportBrain(QTableData targetData)
        {
            if (targetData == null) return;
            
            QTable currentTable = learningEngine.GetQTable();
            targetData.numActions = currentTable.numActions;
            
            targetData.stateIds.Clear();
            targetData.stateValues.Clear();

            foreach (var kvp in currentTable.table)
            {
                targetData.stateIds.Add(kvp.Key);
                targetData.stateValues.Add(new StateActionValues { values = kvp.Value });
            }
        }
    }
}
