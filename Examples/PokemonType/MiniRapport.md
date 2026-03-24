# Rapport d'Entraînement : Affinité Pokémon

Ce document analyse les résultats exceptionnels de l'IA testant l'affinité des types (17 attaques possibles vs 136 choix de défenses possibles).

*(N'hésitez pas à glisser vos captures d'écran de l'AutoTrainer dans ce dossier pour la postérité !)*

## 1. La Problématique de "l'Optimum Local"

Dans notre première expérience (Sword & Shoot), l'IA n'avait que deux choix : Parer ou Esquiver. Le résultat était binaire (Bon ou Mauvais). 

Ici, avec la logique des Types Pokémon, il existe une **"échelle globale"** de réussite :
* **Dégâts x0.5** (Résistance) ➔ C'est Mieux que Neutre ! *(Récompense +1)*
* **Dégâts x0.25** (Double Résistance) ➔ C'est Super ! *(Récompense +5)*
* **Dégâts x0** (Immunité) ➔ C'est Parfait ! *(Récompense +10)*

**Le Phénomène :** Au début de son entraînement, l'IA testait au hasard. En tombant sur une simple "Résistance", elle recevait un feed-back positif. Satisfaite par ce gain inattendu, elle décidait parfois qu'elle n'avait pas besoin de chercher plus loin, validant définitivement une vulgaire Résistance là où une Immunité totale existait. En *Machine Learning*, on dit qu'elle s'est bloquée dans un **"Optimum Local"** par paresse de chercher en dehors de sa zone de confort.

## 2. Le Remède : `Exploration Decay Rate` à 0.999

Pour lutter contre ce phénomène, il ne faut surtout pas punir l'IA de trouver des résistances. Il faut l'encourager à **continuer d'explorer son environnement** plus longtemps de manière chaotique pour maximiser ses chances d'effleurer la récompense ultime !

C'est là que le réglage de l'`Exploration Decay Rate` est devenu crucial.
En le passant de `0.9` (Baisse rapide) à **`0.999`** (Baisse microscopique) :
On force le curseur *Epsilon* à rester à 100% "Curieux" pendant beaucoup plus longtemps. Au lieu d'arrêter d'explorer après 100 attaques, l'IA essaye des combinaisons délirantes pendant près de **10 000 attaques** avant que sa curiosité ne baisse à zéro. 

Pendant ce chaos géant de 10 000 coups, il était mathématiquement garanti qu'elle finisse par tomber "par accident" sur la Double Résistance, recevant l'immense gain de +5 et écrasant du même coup l'ancienne Q-Value. 

## 3. L'Aboutissement (Le Plafond de Verre)
La console a fini par cracher le Master-Plan de l'IA : elle s'est montrée d'une précision chirurgicale sur les 17 types.
Preuve de son intelligence : elle a compris et isolé le cas du type **Dragon**. Ce type n'ayant aucune immunité ni double-résistance de "Type Acier" dans la table fournie, l'IA a purement exploré l'entièreté des 136 plans possibles pour aboutir à la stricte conclusion mathématique que `Dégâts x0.5` était techniquement la récompense plafond (Optimum Global) atteignable pour cette situation.

**Conclusion :** Même les problèmes impliquant plusieurs centaines d'intersections possibles peuvent être maîtrisés instantanément par l'IA Pluminus sans le moindre réseau neuronal profond... du moment qu'on lui laisse la chance d'explorer !
