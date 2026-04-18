using UnityEngine;
using UnityEngine.Events;
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

        // Les modules développés par le joueur pour lier son jeu à l'IA
        private IEnvironmentObserver environmentObserver;
        private IActionExecutor actionExecutor;
        
        // Le moteur interne d'apprentissage
        private QLearningEngine learningEngine;

        // Historique court-terme pour l'apprentissage
        private int previousState = -1;
        private int lastActionTaken = -1;
        private float currentEpsilon; // Le taux d'exploration actuel (qui diminue avec le temps)

        private float accumulatedReward = 0f; // Les points accumulés depuis la dernière action

        private void Awake()
        {
            // Récupère automatiquement les interfaces sur le GameObject
            environmentObserver = GetComponent<IEnvironmentObserver>();
            actionExecutor = GetComponent<IActionExecutor>();

            if (environmentObserver == null || actionExecutor == null)
            {
                Debug.LogError("Erreur: L'AdaptiveBrain a besoin d'un script implémentant IEnvironmentObserver et d'un script implémentant IActionExecutor sur le même GameObject !");
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
            int chosenAction = learningEngine.DecideAction(currentState, currentEpsilon, actionExecutor.IsActionValid);
            
            // 4. AGIR dans le jeu
            actionExecutor.ExecuteAction(chosenAction);
            if (OnActionExecuted != null) OnActionExecuted.Invoke(chosenAction);

            // 5. Mémoriser ce qu'on vient de faire pour pouvoir apprendre la prochaine fois
            previousState = currentState;
            lastActionTaken = chosenAction;
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
                accumulatedReward += reward.rewardValue; // Ajoute les points

                // Si c'est un game over (ex: mort), on casse la chaîne d'apprentissage
                if (reward.isTerminalState)
                {
                    previousState = -1;
                    lastActionTaken = -1;
                }
            }
        }

        // --- Fonctions Utilitaires ---

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
