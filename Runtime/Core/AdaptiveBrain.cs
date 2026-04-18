using UnityEngine;
using UnityEngine.Events;
using System.Collections.Generic;
using Pluminus.Data;
using Pluminus.Integration;

namespace Pluminus.Core
{
    /// <summary>
    /// Le composant principal à attacher sur votre ennemi dans Unity.
    /// Il fait le pont entre le moteur mathématique (QLearningEngine), la configuration, et votre jeu.
    /// </summary>
    public class AdaptiveBrain : MonoBehaviour
    {
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

        [Header("Mode Heuristique (Manuel)")]
        [Tooltip("Si coché, l'IA ignore son propre cerveau et exécute les actions envoyées par le joueur (pour debug/test).")]
        public bool useHeuristic = false;
        
        // Historique court-terme pour l'apprentissage
        private int previousState = -1;
        private int lastActionTaken = -1;
        private float currentEpsilon; // Le taux d'exploration actuel (qui diminue avec le temps)

        private float accumulatedReward = 0f; // Les points accumulés depuis la dernière action
        private int heuristicActionId = -1; // L'action injectée par le mode manuel

        // --- Analytics & Performance ---
        [Header("Statistiques d'Apprentissage")]
        public List<float> episodeRewards = new List<float>(); // Historique des scores par épisodes
        public List<float> continuousHistory = new List<float>(); // Historique continu (temps réel)
        private float currentEpisodeTotalReward = 0f;
        private float sessionTotalReward = 0f;
        private int totalEpisodes = 0;
        private float statsTimer = 0f;

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
                Debug.LogError("Erreur: L'AdaptiveBrain n'a pas trouvé d'IEnvironmentObserver (StateBuilder) ou d'IActionExecutor (Gamepad) sur cet objet ou ses enfants !");
                enabled = false;
                return;
            }

            // Initialise le moteur interne avec le nombre d'actions possibles
            int totalActions = actionExecutor.GetMaxActions();
            learningEngine = new QLearningEngine(totalActions);

            if (brainConfig != null)
            {
                currentEpsilon = brainConfig.explorationRate; // Démarre au taux d'exploration choisi
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
            if (brainConfig.isLearningEnabled && previousState != -1 && lastActionTaken != -1)
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
            }

            // Remet le score de récompenses à 0 pour la prochaine action
            accumulatedReward = 0f;

            // 3. DECIDER de la prochaine action
            int chosenAction = -1;

            if (useHeuristic)
            {
                chosenAction = heuristicActionId;
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

            // 5. Mémoriser ce qu'on vient de faire pour pouvoir apprendre la prochaine fois
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

            if (isTerminal)
            {
                EndEpisode();
            }
        }

        private void Update()
        {
            if (!Application.isPlaying) return;

            // Capture de stats pour le mode continu (toutes les secondes)
            statsTimer += Time.deltaTime;
            if (statsTimer >= 1.0f)
            {
                continuousHistory.Add(sessionTotalReward);
                if (continuousHistory.Count > 300) continuousHistory.RemoveAt(0); // 5 minutes de stats
                statsTimer = 0;
            }
        }

        /// <summary>
        /// Clôture l'épisode actuel et stocke les statistiques.
        /// </summary>
        public void EndEpisode()
        {
            episodeRewards.Add(currentEpisodeTotalReward);
            if (episodeRewards.Count > 100) episodeRewards.RemoveAt(0); // Garde les 100 derniers
            
            totalEpisodes++;
            currentEpisodeTotalReward = 0;
            
            // Réinitialise l'historique d'apprentissage pour ne pas lier la mort au nouvel état
            previousState = -1;
            lastActionTaken = -1;
        }

        /// <summary>
        /// Méthode à appeler depuis les événements de votre jeu (ex: OnHit, OnDodge) pour donner ou retirer des points à l'IA.
        /// </summary>
        /// <param name="flag">Le nom textuel de l'événement (ex: "TookDamage")</param>
        public void ApplyRewardFlag(string flag)
        {
            // Cherche la valeur en points liée à ce mot clé dans le RewardProfile
            if (rewardProfile != null && rewardProfile.TryGetReward(flag, out RewardEvent reward))
            {
                AddReward(reward.rewardValue, reward.isTerminalState);
            }
        }

        // --- Fonctions Utilitaires ---

        public int GetTotalEpisodes() => totalEpisodes;
        public float GetLastEpisodeReward() => episodeRewards.Count > 0 ? episodeRewards[episodeRewards.Count - 1] : 0;

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
