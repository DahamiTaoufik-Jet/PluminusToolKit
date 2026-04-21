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
        /// Retourne le vecteur de mouvement de direction (Vector3).
        /// S'adapte automatiquement à la vue 2D ou 3D selon la configuration du manager abstrait.
        /// </summary>
        Vector3 GetAxis();

        /// <summary>
        /// Retourne Vrai si le bouton demandé est maintenu/actif sur ce tick. (ex: 'Jump', 'Fire')
        /// </summary>
        bool GetButton(string actionName);
    }
}
