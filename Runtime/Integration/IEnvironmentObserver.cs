namespace Pluminus.Integration
{
    /// <summary>
    /// Interface à implémenter sur votre script de gestion des sens de l'ennemi.
    /// Elle sert à transformer les données du jeu (Distance, HP, Anim du joueur) en un numéro unique (ID).
    /// </summary>
    public interface IEnvironmentObserver
    {
        /// <summary>
        /// Doit analyser le jeu et renvoyer un ID représentant la "situation" actuelle.
        /// Exemple: Distance Mêlée + Joueur Low HP = Etat 1.
        /// </summary>
        /// <returns>Un nombre entier (State ID) représentant l'environnement actuel.</returns>
        int GetCurrentStateId();

        /// <summary>
        /// Définit combien de situations différentes l'IA peut rencontrer au total (ex: 200).
        /// Plus ce nombre est grand, plus l'IA mettra de temps à apprendre.
        /// </summary>
        int GetMaxStates();
    }
}
