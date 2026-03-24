using UnityEngine;
using UnityEngine.InputSystem; // Supporte le nouveau système d'Input

namespace Pluminus.Examples.MarioAndLuigi
{
    /// <summary>
    /// Script utilitaire pour accélérer / ralentir le temps avec la barre Espace
    /// </summary>
    public class MLTimeManager : MonoBehaviour
    {
        [Tooltip("La vitesse de la simulation en mode entraînement intensif")]
        public float fastForwardSpeed = 30f;
        
        [Tooltip("La vitesse normale pour observer les résultats de l'IA")]
        public float normalSpeed = 1f;

        private bool isFastForwarding = false;

        void Update()
        {
            // On vérifie si la touche ESPACE vient d'être pressée avec le nouveau InputSystem
            if (Keyboard.current != null && Keyboard.current.spaceKey.wasPressedThisFrame)
            {
                ToggleTimeScale();
            }
        }

        public void ToggleTimeScale()
        {
            isFastForwarding = !isFastForwarding;

            if (isFastForwarding)
            {
                Time.timeScale = fastForwardSpeed;
                Debug.Log($"<color=yellow>▶▶ Vitesse d'Entraînement accélérée (x{fastForwardSpeed}) !</color>");
            }
            else
            {
                Time.timeScale = normalSpeed;
                Debug.Log($"<color=cyan>▶ Vitesse Normale (x{normalSpeed}). Visualisation en cours.</color>");
            }
        }
    }
}
