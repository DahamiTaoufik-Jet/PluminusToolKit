using UnityEngine;

namespace Pluminus.Integration.Input
{
    /// <summary>
    /// Interface standard Fournisseur d'Entrées pour Pluminus.
    /// Les développeurs peuvent appeler GetComponent<IPluminusInput>().GetAxis() dans leurs scripts physqiues
    /// pour supporter la manette ET l'Intelligence Artificielle de Pluminus sans effort.
    /// </summary>
    public interface IPluminusInput
    {
        /// <summary>
        /// Retourne le vecteur de mouvement de direction (-1 à 1 sur X et Y, comme Input.GetAxisRaw).
        /// </summary>
        Vector2 GetAxis();

        /// <summary>
        /// Retourne Vrai si le bouton demandé est maintenu/actif sur ce tick. (ex: 'Jump', 'Fire')
        /// </summary>
        bool GetButton(string actionName);
    }
}
