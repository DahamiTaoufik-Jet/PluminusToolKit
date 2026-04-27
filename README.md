# ReadMeComplete - Pluminus

Pluminus est un toolkit Unity pour créer des IA adaptatives (combat, défense, réaction) via **Q-Learning discret**.
Le système est orienté **No-Code** : ce qui implique une configuration par composants Unity, `ScriptableObject`, events et règles.

https://github.com/user-attachments/assets/4138fd69-3d7e-4279-9719-544358625646

---

## 1) Ce que fait Pluminus

- Observe l'environnement de l'agent via des **sensors**.
- Convertit ces observations en un **StateId** discret.
- Choisit une action selon une **Q-Table** (exploration/exploitation).
- Reçoit des récompenses/pénalités et améliore sa politique au fil du temps.
- Permet la persistance du cerveau entraîné et le suivi analytics.

En résumé :

`Sensors -> Eyes -> Brain -> Action Executor -> Reward -> Learning`

---

## 2) Architecture globale

### Core

- `PluminusBrain` : composant principal.
  - Exécute le cycle `Observe -> Learn -> Decide -> Act`.
  - Gère les rewards, épisodes, heuristique, import/export Q-Table.

- `QLearningEngine` + `QTable` : moteur mathématique Bellman + mémoire Q.

### Sensors

- `PluminusStateSensor` : base commune (`GetSubStateCount`, `GetCurrentSubState`).
- `PluminusEyes` : combine tous les sous-états capteurs en un `StateId` unique.

### Integration

- `PluminusTempoDecision` : cadence des décisions (`TickDecision`), training speed, soft reset.
- `PluminusActionRouter` (et autres executors) : mappe `actionId` vers appels gameplay.
- `PluminusRuleEngine` : applique des rewards via conditions (actions + états capteurs).

### Data (ScriptableObject)

- `BrainConfig` : hyperparamètres Q-Learning.
- `RewardProfile` + `RewardEvent` : table des rewards par flag.
- `QTableData` : persistance de la mémoire entraînée.
- `PluminusAnalyticsData` : statistiques et courbes.

---

## 3) Les 3 composants centraux

### `PluminusEyes`

- Lit les sensors de l'objet + enfants.
- Fait le produit cartésien des sous-états pour produire un `StateId`.
- Sert d'`IEnvironmentObserver` pour le Brain.

### `PluminusBrain`

- Observe (`Eyes`), apprend (Q-update), décide (policy/epsilon), agit (executor).
- API reward : `AddReward(...)`, `ApplyRewardFlag(...)`.
- Gestion des épisodes : `EndEpisode()`.
- Persistance : `ImportBrain(...)`, `ExportBrain(...)` avec `QTableData`.

### `PluminusTempoDecision`

- Appelle `TickDecision()` automatiquement à intervalle configurable.
- Ajuste la cadence en entraînement accéléré (`trainingSpeed`).
- `PerformSoftReset()` : reset agent + rigidbodies + épisode.

---

## 4) Sensors disponibles

### Dossier `Runtime/Sensors`

- `DistanceToTargetSensor`
  
  ![DistanceToTargetSensor](./Docs/ComponentScreens/Sensor%20Distance.png)

- `RaycastTagSensor`
- `TriggerTagSensor`
- `PluminusEyes`
- `PluminusStateSensor`

### Dossier `Runtime/Sensors/Extended`

- `ActionStateSensor`
- `ContactSensor`
- `ElevationSensor`
  
  ![ElevationSensor](./Docs/ComponentScreens/Etage%20Sensor.png)

- `GridSensor`
  
  ![GridSensor](./Docs/ComponentScreens/Exemple%20Grid.png)

- `QuadrantSensor`
  
  ![QuadrantSensor](./Docs/ComponentScreens/Sensor%20Quadrant.png)

- `ValueSensor`
  
  ![ValueSensor](./Docs/ComponentScreens/Value%20Sensors.png)

- `VectorSensor`
  
  ![VectorSensor](./Docs/ComponentScreens/Sensors%20de%20Calcul%20de%20vecteur%20de%20direction.png)

Notes :

- Plus vous ajoutez de sensors et de paliers, plus l'espace d'état grossit.
- Un espace d'état trop grand ralentit l'apprentissage.

---

## 5) Setup minimal (recommandé)

1. Ajouter `PluminusBrain` sur l'agent.
2. Créer/assigner `BrainConfig` et `RewardProfile`.
3. Ajouter `PluminusEyes` + sensors nécessaires.
4. Ajouter un executor d'actions (`PluminusActionRouter` par exemple).
5. Ajouter `PluminusTempoDecision` et lier le champ `brain`.
6. Brancher les rewards gameplay (`ApplyRewardFlag` ou `AddReward`).
7. (Optionnel) Assigner `QTableData` et `PluminusAnalyticsData`.

---

## 6) Workflow d'entraînement

1. Démarrer avec peu d'actions et peu de sensors.
2. Lancer des épisodes courts, observer les courbes.
3. Ajuster rewards pour guider le comportement voulu.
4. Ajuster hyperparamètres (`alpha`, `gamma`, `epsilon/decay`).
5. Exporter la Q-Table utile dans `QTableData`.
6. Rejouer en mode exploitation (ou learning réduit) pour validation.

---

## 7) Rule Engine (reward no-code)

Permet de déclencher des rewards via règles combinées, ex :

- action choisie (`ActionEquals` / `ActionNotEquals`)
- état d'un sensor (`SensorEquals` / `SensorNotEquals`)

Cas typique défense :

- Si `ActionEquals` = bouclier Est
- ET `SensorEquals` = projectile à l'Est
- ALORS appliquer `Reward Flag` positif.

---

## 8) Dashboard & analytics

Le dashboard suit notamment :

- récompense cumulée,
- winrate global / récent,
- précision récente,
- epsilon courant,
- historique d'épisodes et courbe temps réel.

Utiliser ces indicateurs pour valider si l'IA apprend réellement ou stagne.

---

## 9) Captures & documentation visuelle

Les captures sont dans : `Docs/ComponentScreens`.

- README visuel : `Docs/ComponentScreens/README.md`
- Ce README contient les images en preview Markdown.

---

## 10) Index des README spécialisés

- Vue Sensors : `Runtime/Sensors/README.md`
- Vue Data : `Runtime/Data/README.md`
- Brain/Eyes/Tempo : `Runtime/README_Brain_Eyes_Tempo.md`
- Captures composants : `Docs/ComponentScreens/README.md`

---

## 11) Bonnes pratiques

- Commencer simple, complexifier progressivement.
- Garder des flags de reward stables et explicites.
- Vérifier la cohérence `GetSubStateCount()` / `GetCurrentSubState()`.
- Utiliser `PerformSoftReset()` à chaque fin d'épisode.
- Éviter la redondance de sensors qui encodent la même info.

---

## 12) Résumé produit

Pluminus fournit une chaîne complète et modulaire pour entraîner une IA gameplay dans Unity :

- perception (`Sensors`, `Eyes`),
- décision/apprentissage (`Brain`, Q-Learning),
- exécution (`Action Router`),
- récompense (`RewardProfile`, `RuleEngine`),
- cadence/reset (`TempoDecision`),
- persistance et analytics (`QTableData`, `PluminusAnalyticsData`).

C'est adapté pour construire rapidement des comportements adaptatifs sans coder un moteur RL maison.
