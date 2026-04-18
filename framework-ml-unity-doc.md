# Documentation Détaillée : Framework Q-Learning No-Code pour Unity

---

## 1. Vision et Philosophie du Produit
L'objectif de ce framework est de **démocratiser l'Apprentissage par Renforcement** (Reinforcement Learning) au sein du moteur Unity. Il repose sur un algorithme de **Q-Learning classique (Q-Table)**. 

### Principes Fondamentaux :
* **Accessibilité (No-Code / Low-Code) :** Un Game Designer doit pouvoir configurer une IA complète (perception, décision, action) via l'inspecteur Unity par simple glisser-déposer.
* **Le défi de la Discrétisation :** L'outil transforme un monde continu (positions fluides, physique complexe) en un ensemble fini d'états et d'actions (identifiants numériques stricts) exploitables par la Q-Table.

---

## 2. Couche de Perception : Les Capteurs (Sensors)
L'agent perçoit le monde via des capteurs qui réduisent les données complexes en entiers (IDs).

### A. Capteurs Spatiaux et Géométriques
* **Grid Sensor (Capteur Matriciel) :** Projette une grille (3x3, 5x5, etc.) autour de l'agent. Analyse chaque case pour renvoyer un état (Vide/Occupé) selon les masques de collision.
* **Raycast Sensor (Lidar unidirectionnel) :** Tire un rayon rectiligne et renvoie l'ID de la catégorie d'objet touché (ex: Mur, Ennemi, Neutre).
* **Detector Sensor (Zone de Déclenchement) :** Radar de proximité binaire (sphère ou boîte) signalant la présence d'une entité spécifique.

### B. Capteurs de Repérage et d'Orientation
* **Quadrant / Angle Sensor :** Découpe l'espace en "parts de gâteau" (4 à 16 secteurs). Identifie dans quel secteur se trouve la cible pour orienter l'agent sans calculs d'angles complexes.
* **Distance Sensor :** Évalue l'éloignement via des anneaux concentriques (Corps-à-corps, Moyenne portée, Longue portée, Hors de vue).
* **Elevation Sensor :** Compare la hauteur (Axe Y) entre l'agent et sa cible (Haut, Bas, Même niveau).

### C. Capteurs Physiques et Systémiques
* **Vector Sensor (Vélocité/Inertie) :** Convertit les forces d'un *Rigidbody* en paliers de vitesse (Immobile, Lent, Rapide) et directions locales (Avance, Recule).
* **Contact Sensor :** Confirmation physique binaire (ex: pieds touchant le sol ou contact avec un mur).
* **Value Sensor (Variables) :** Discrétise les statistiques (Points de vie, munitions) en tranches d'états (Critique, Moyen, Plein).
* **Action State Sensor :** Traduit la phase actuelle d'une action complexe (Préparation, Frappe, Récupération) pour synchroniser les réactions de l'IA.

---

## 3. Couche de Décision : Le Profil Cérébral (Brain Profile)
Toutes les perceptions convergent vers ce composant stocké en tant que *Scriptable Object*.

### Les Hyperparamètres
* **Taux d'Apprentissage (Alpha) :** Vitesse à laquelle l'IA remplace ses anciennes connaissances par les nouvelles.
* **Facteur d'Anticipation (Discount Factor) :** Équilibre entre récompense immédiate et stratégie à long terme.
* **Taux d'Exploration (Epsilon) Initial:** Dose de hasard (curiosité) injectée dans les choix au début de l'entraînement.
* **Taux d'Exploration Minimum (Min Exploration Rate) :** Valeur plancher (ex: 5%) garantissant que l'IA conserve toujours une petite part de hasard (RNG). Cela évite la suroptimisation et lui permet de s'adapter aux changements imprévus.
* **Déclin d'Exploration (Exploration Rate Decay) :**  Le coefficient de réduction qui fait progressivement baisser la curiosité au profit de l'expérience.

### Le Moniteur de Complexité (State Space Monitor)
Fonction de sécurité calculant en temps réel le produit des états possibles de tous les capteurs. Une jauge couleur (Vert, Orange, Rouge) prévient l'utilisateur en cas de risque d'**Explosion Combinatoire** (trop de données pour la Q-Table).

---

## 4. Couche d'Action : La Manette Virtuelle (Virtual Gamepad)
L'IA interagit avec le jeu en envoyant des identifiants d'action. L'outil propose deux paradigmes d'exécution pour s'adapter à tous les types de gameplay.

* **Mode "Simulation de Manette" (Architecture Multi-Branches): ** Idéal pour les agents qui doivent imiter un joueur humain avec des mouvements continus (ex: jeu de plateforme, FPS).
### Actions Simultanées : 
L'IA peut combiner plusieurs choix (ex: déplacer le joystick vers la droite ET presser le bouton de tir).

### Interface Visuelle : 
L'utilisateur configure une manette virtuelle dans l'inspecteur, liant les axes et les boutons à ses propres scripts via des UnityEvents.
* **Mapping par Événements (UnityEvents) :** Système 100% visuel où l'utilisateur lie chaque bouton virtuel aux fonctions de son propre code (ex: fonction `Sauter()`) via l'inspecteur.
* **Mode Heuristique :** Permet au développeur de piloter manuellement l'agent pour valider les contrôles et la faisabilité du niveau.
### B. Mode "Exécution Brute" (Action Router / Liste Discrète)
Idéal pour les entités possédant un vaste répertoire d'actions mutuellement exclusives (ex: un Boss de RPG avec des dizaines de compétences tactiques).

* **Réceptacle Dynamique :** L'utilisateur définit une liste d'événements de la taille de son choix (ex: de 0 à 99 slots).

* **Déclenchement par Index :** Le cerveau renvoie un ID unique (ex: 12). Le réceptacle appelle instantanément la fonction liée au slot n°12. Ce système offre une complexité d'exécution ultra-rapide (O(1)) et une modularité infinie.

### C. Mode Heuristique (Contrôle Humain):
Option transversale permettant au développeur de désactiver l'IA pour prendre personnellement le contrôle de la Manette ou du Routeur. Essentiel pour valider la jouabilité du niveau avant de lancer un entraînement.
---

## 5. La Motivation : Le Système de Récompenses
Définit le but de l'IA via un système modulaire.

* **Reward Table :** Catalogue de récompenses (Scriptable Object) associant des noms d'événements à des valeurs de points (ex: "Trouver_Clef" = +50).
* **Reward Dispensers :** Composants de scène (triggers, zones) qui déclenchent l'envoi des points au Cerveau sans nécessiter de modification de code.

---

## 6. L'Espace-Temps : Environnement et Synchronisation
Gère la logistique de l'entraînement et la remise à zéro des épisodes.

* **Verrouillage d'Action (Input Lock) :** Suspend la réflexion de l'IA pendant les actions longues (animations) pour éviter de polluer la mémoire de l'agent.
* **Snapshot & Soft Reset :** Capture une "photo" des positions et variables initiales. Téléporte instantanément les éléments à leur point de départ en fin d'épisode pour éviter de recharger la scène.
* **Modes d'Entraînement :** Choix entre un mode **Épisodique** (cycles avec début et fin) et un mode **Continu** (survie infinie).

---

## 7. L'Analyse : Le Tableau de Bord d'Entraînement
Interface intégrée à l'éditeur Unity permettant de suivre les métriques clés en temps réel :

* **Progression des Récompenses :** Courbe du score cumulé.
* **Efficacité Temporelle :** Mesure de la vitesse d'exécution ou du temps de survie.
* **Déclin de Curiosité :** Suivi de la transition entre le hasard (Epsilon) et l'expérience.
* **Couverture de Connaissance :** Pourcentage des situations explorées par rapport à la capacité totale du cerveau.

---
