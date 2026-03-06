namespace Pluminus.Integration
{
    /// <summary>
    /// Interface à implémenter sur votre script d'animation/attaque de l'ennemi.
    /// Elle permet à l'IA de déclencher les vraies actions en jeu.
    /// </summary>
    public interface IActionExecutor
    {
        /// <summary>
        /// Déclenche l'action correspondante dans le jeu (jouer une animation, tirer un projectile).
        /// </summary>
        /// <param name="actionId">Le numéro de l'action choisie par l'IA (ex: 0 = Attack, 1 = Block).</param>
        void ExecuteAction(int actionId);

        /// <summary>
        /// Définit le nombre total d'actions possibles que cet ennemi sait faire (ex: 5 actions).
        /// </summary>
        int GetMaxActions();
        
        /// <summary>
        /// Vérifie si une action est autorisée en ce moment.
        /// Très utile pour gérer des temps de recharge (cooldowns) ou empêcher d'attaquer si on est étourdi (stun).
        /// </summary>
        bool IsActionValid(int actionId);
    }
}
