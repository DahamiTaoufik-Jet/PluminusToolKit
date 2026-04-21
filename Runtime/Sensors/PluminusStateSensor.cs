using UnityEngine;
using Pluminus.Core;

namespace Pluminus.Sensors
{
    /// <summary>
    /// Classe abstraite de base pour tous les capteurs visuels Pluminus (No-Code).
    /// Ajoutez des classes dérivées sur le même GameObject que le PluminusBrain.
    /// </summary>
    public abstract class PluminusStateSensor : MonoBehaviour
    {
        [Header("Connexion Externe")]
        [Tooltip("Laissez vide si le capteur est déjà un Enfant de l'IA. Sinon, glissez les Yeux (PluminusEyes) distants ici !")]
        public PluminusEyes targetEyes;

        [Tooltip("Nom usuel du capteur pour s'y retrouver dans l'éditeur")]
        public string sensorName = "Nouveau Capteur";

        [Header("Debug")]
        [Tooltip("Cochez pour afficher dans la console chaque basculement d'état détecté par ce capteur.")]
        public bool logStateChanges = false;
        private int lastLoggedState = -1;

        /// <summary>
        /// Retourne le nombre d'états distincts que ce capteur précis peut prendre (ex: 2 pour Booléen, N pour Distance).
        /// </summary>
        public abstract int GetSubStateCount();

        protected virtual void Awake()
        {
            // Autorise le capteur a s'inscrire lui-même à des yeux distants au lancement !
            if (targetEyes != null)
            {
                targetEyes.RegisterExternalSensor(this);
            }
        }

        /// <summary>
        /// Retourne l'état actuel de ce capteur au moment (allant de 0 à GetSubStateCount() - 1).
        /// </summary>
        public abstract int GetCurrentSubState();

        /// <summary>
        /// S'occupe de renvoyer l'état au Brain, tout en loggant automatiquement le changement dans la console si demandé.
        /// </summary>
        public int GetStateWithDebug()
        {
            int state = GetCurrentSubState();
            
            if (logStateChanges && state != lastLoggedState)
            {
                if (lastLoggedState != -1) // Ne spamme pas au premier frame
                {
                    Debug.Log($"<color=cyan>👁️ [Sensor] '{sensorName}' vient de basculer sur l'État : {state}</color>");
                }
                lastLoggedState = state;
            }
            
            return state;
        }
    }
}
