# Rapport d'Entraînement : Sword & Shoot

Ce document résume la configuration et les résultats de l'exemple "Sword & Shoot", démontrant l'efficacité du moteur Q-Learning de Pluminus.

*(Vous pouvez ajouter vos captures d'écran dans ce dossier pour compléter ce rapport).*

## 1. Configuration du Cerveau (`BrainConfig`)
Les hyperparamètres suivants ont permis à l'IA d'apprendre de façon quasi-instantanée :

*   **Learning Rate (Alpha) : `0.8`**
    *   L'IA accorde une confiance énorme (80%) à ses nouvelles expériences. Dès qu'elle subit un échec, elle corrige immédiatement sa probabilité de refaire cette action.
*   **Discount Factor (Gamma) : `0`**
    *   L'IA est totalement "opportuniste". Étant donné que dans cet exemple la punition ou la récompense tombe *instantanément* après l'action, l'IA n'a pas besoin de calculer de stratégie sur le long terme.
*   **Exploration Rate (Epsilon initial) : `1`**
    *   Au tout premier coup, l'IA choisit son action totalement au hasard (100% de chance d'explorer). C'est vital pour qu'elle découvre quelles actions rapportent des points.
*   **Min Exploration Rate : `0`**
    *   L'objectif final est que l'IA ne fasse plus aucun choix au hasard une fois qu'elle a compris les règles.
*   **Exploration Decay Rate : `0.9`**
    *   À chaque itération, la chance d'explorer chute de 10%. Après seulement 40 à 50 coups environ, l'esprit de l'IA est mathématiquement focalisé sur la pure exploitation de ses connaissances.

## 2. Profil de Récompenses (`RewardProfile`)
Le système fonctionne sur une logique binaire très simple :
*   **`GoodChoice` (Valeur : `1`)** : Accordé si l'IA Pare une Épée ou Esquive un Projectile.
*   **`BadChoice` (Valeur : `-1`)** : Accordé si l'IA se trompe de défense. L'impact de ce `-1` couplé au Learning Rate de `0.8` suffit à la dissuader de recommencer.

## 3. Constatations des Résultats (`AutoTrainer`)
Lors d'un test massif générant 1000 à 100 000 attaques en quelques millisecondes :
*   **Apprentissage foudroyant :** L'IA ne cumule qu'entre 3 et 6 échecs *au total*.
*   **Plafonnement de performance :** Une fois la brève période d'exploration terminée (Epsilon proche de 0), l'agent se stabilise à un taux de réussite de **99,4% à 100%**.
*   **Conclusion :** Le composant `AdaptiveBrain` remplit parfaitement son rôle pour des prises de décisions réflexes et binaires en combat, sans aucune charge CPU notable.
