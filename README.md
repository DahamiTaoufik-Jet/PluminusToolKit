# Pluminus - Adaptive Combat AI Toolkit

Pluminus est un plugin Unity léger permettant de créer des intelligences artificielles (IA) de combat adaptatives pour des jeux d'action, en utilisant le Machine Learning algorithmique (Q-Learning discret). L'IA apprend en temps réel de ses erreurs et de ses succès.

## Architecture Actuelle (MVP)

Le projet est divisé en trois piliers principaux :

### 1. La Configuration (Data)
*   **`BrainConfig`** (ScriptableObject) : Les réglages du cerveau. Définit la vitesse d'apprentissage (Alpha), la vision à long terme (Gamma) et le taux d'exploration aléatoire (Epsilon).
*   **`RewardProfile`** (ScriptableObject) : La table des lois. Définit le nombre de points distribués pour chaque action (ex: Esquiver = +1 point, Prendre un coup = -2 points).
*   **`QTableData`** (ScriptableObject) : La carte mémoire. Permet de sauvegarder de façon permanente ce que l'IA a appris lors d'une session (Persistance).

### 2. Le Moteur Mathématique (Core)
*   **`QLearningEngine`** : Le cœur algorithmique (C# pur). Il applique l'Équation de Bellman pour mettre à jour les probabilités de réussite d'une action.
*   **`QTable`** : Le tableau local stockant les scores temporaires.
*   **`PluminusBrain`** (MonoBehaviour) : Le chef d'orchestre. C'est le composant principal à glisser sur vos ennemis. Il relie votre jeu au moteur mathématique.

### 3. Les Interfaces Développeur (Integration)
*   **`IEnvironmentObserver`** : À implémenter sur votre ennemi pour traduire l'environnement 3D/2D (Distance, HP) en un "État" mathématique simple (un entier).
*   **`IActionExecutor`** : À implémenter pour permettre au Cerveau de déclencher vos vraies animations Unity (Attaquer, Fuir, Bloquer).

## Comment l'IA apprend-elle ? (Concepts d'Apprentissage par Renforcement)

### L'Équation de Bellman

La prise de décision dans Pluminus repose sur l'**Équation de Bellman**, le fondement mathématique du Q-learning. 
Plutôt que d'évaluer une action uniquement sur son résultat immédiat, cet algorithme calcule une "Quality Value" (Q-Value) globale. Cette valeur prend en compte la récompense directe de l'action choisie, additionnée de la *meilleure valeur estimée* de l'état futur dans lequel cette action nous amènera. 
Cela permet à l'IA d'apprendre des séquences d'actions (combos) et d'anticiper les conséquences de ses choix à moyen terme.

### Hyperparamètres du Cerveau (`BrainConfig`)

Le comportement et la courbe d'apprentissage de votre IA sont modulables via trois hyperparamètres clés :

*  **Taux d'Exploration (Epsilon - ε)**
    *   **Définition :** La probabilité conditionnelle que l'agent choisisse une action aléatoire plutôt que l'action optimale connue.
    *   **En pratique :** Au début de l'entraînement (ε proche de 1), l'agent explore intensivement son espace d'état. Grâce au paramètre de "Decay", cet Epsilon diminue progressivement. En fin de cycle, l'agent privilégie purement l'Exploitation (ε proche de 0) en exécutant la politique (policy) optimale qu'il a calculée.
*  **Taux d'Apprentissage (Alpha - α)**
    *   **Définition :** Le poids accordé aux nouvelles informations acquises par rapport aux connaissances passées.
    *   **En pratique :** Un Alpha bas (ex: 0.1) créera une IA robuste dont les convictions évoluent lentement au fil de multiples itérations. Un Alpha élevé (ex: 0.9) créera une IA hyper-réactive qui écrasera immédiatement sa stratégie précédente à la moindre erreur constatée.
*  **Facteur d'Escompte (Gamma - γ)**
    *   **Définition :** L'importance pondérée accordée aux récompenses futures espérées par rapport aux récompenses immédiates.
    *   **En pratique :** Un Gamma à 0 force l'agent à être totalement opportuniste (ne considérant que l'instant T). Un Gamma proche de 1 (ex: 0.9) obligera l'agent à développer des stratégies complexes à long terme, capable de sacrifier une récompense immédiate pour se positionner vers une récompense massive ultérieurement.
