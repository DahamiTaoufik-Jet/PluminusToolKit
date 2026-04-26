# Pluminus - Framework Q-Learning No-Code pour Unity

Pluminus est un plugin Unity léger permettant de créer des intelligences artificielles (IA) de combat adaptatives pour des jeux d'action, en utilisant le Machine Learning algorithmique (Q-Learning discret). L'IA apprend en temps réel de ses erreurs et de ses succès. L'objectif principal de ce framework est de **démocratiser l'Apprentissage par Renforcement** au sein d'Unity grâce à une approche No-Code / Low-Code.

## 📂 Architecture du Projet

Le projet est structuré sous forme de package Unity (`com.dahamitaoufik.pluminus`) et se divise en trois répertoires principaux :

### 1. `Runtime/` (Le Moteur)
Contient toute la logique d'exécution du framework en jeu.
*   **Core** : Le moteur mathématique (`QLearningEngine`, `QTable`) qui gère l'algorithme d'apprentissage (Équation de Bellman).
*   **Data** : Les configurations et données persistantes via des *ScriptableObjects* (`BrainConfig`, `RewardProfile`, `QTableData`).
*   **Sensors** (Couche de perception) : Transforme l'environnement 3D/2D continu en états discrets (ex: *Grid Sensor*, *Raycast Sensor*, *Distance Sensor*, *Vector Sensor*).
*   **Integration** : Les interfaces (`IEnvironmentObserver`, `IActionExecutor`) et composants liant le cerveau de l'IA aux comportements du jeu.

### 2. `Editor/` (Les Outils Développeur)
Une suite d'outils s'intégrant directement dans l'interface d'Unity pour faciliter la configuration et le monitoring de l'entraînement :
*   **Tableau de bord d'entraînement** (`PluminusPerformanceDashboard`) : Suivi en temps réel des métriques clés (score cumulé, efficacité, déclin de curiosité).
*   **Inspecteurs Personnalisés** : Interfaces visuelles pour paramétrer le cerveau de l'IA (`PluminusBrainEditor`) et la vision/les capteurs (`PluminusEyesEditor`) sans coder.

### 3. `Examples/` (Cas d'Usages)
Des scènes et scripts de démonstration pour illustrer l'intégration du framework dans différents styles de jeux :
*   **SwordAndShoot** : Implémentations d'ennemis classiques (ex: `DummySlimeEnemy`).
*   **MarioAndLuigi** / **PokemonType** : Exemples d'adaptabilité de l'IA à des gameplays variés.

## 🧠 Les 4 Piliers du Framework

### 1. Perception (Sensors)
L'agent analyse son environnement via divers capteurs (spatiaux, physiques, repérage) qui transforment des données complexes en IDs simples. Le *State Space Monitor* intégré alerte en cas de risque d'explosion combinatoire.

### 2. Décision (Brain Profile & Q-Learning)
L'IA s'appuie sur la fameuse équation de Bellman, contrôlée par trois hyperparamètres modulables :
*   **Epsilon (ε) - Taux d'Exploration** : L'envie de l'IA de tenter de nouvelles actions au hasard.
*   **Alpha (α) - Taux d'Apprentissage** : L'importance accordée aux nouvelles expériences face aux anciennes.
*   **Gamma (γ) - Facteur d'Escompte** : La capacité de l'IA à privilégier une stratégie à long terme sur un gain immédiat.

### 3. Action (Manette Virtuelle & Exécution Brute)
L'outil propose deux paradigmes d'exécution :
*   **Manette Virtuelle** : L'IA simule des inputs physiques (déplacements, sauts) liés via des *UnityEvents*, idéal pour des mouvements continus.
*   **Routeur d'Actions** : L'IA sélectionne un ID d'action spécifique dans une liste (ex: compétence de Boss), offrant une complexité de O(1) et une modularité infinie.

### 4. Motivation (Système de Récompenses)
Un système modulaire permet de guider l'apprentissage de l'agent. Des *Reward Dispensers* dans la scène attribuent des points positifs ou négatifs selon les actions accomplies par l'IA, sans nécessiter de script externe.
