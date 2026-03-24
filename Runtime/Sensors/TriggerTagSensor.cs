using UnityEngine;

namespace Pluminus.Sensors
{
    /// <summary>
    /// Capteur No-Code qui vérifie si un objet avec un tag spécifique est actuellement dans une zone Trigger.
    /// Renvoie 1 si vrai, 0 si faux.
    /// Nécessite un Collider2D en mode "Is Trigger" sur ce GameObject.
    /// </summary>
    [RequireComponent(typeof(Collider2D))]
    public class TriggerTagSensor : PluminusStateSensor
    {
        [Tooltip("Le Tag de l'objet à détecter dans la zone visuelle")]
        public string targetTag = "Enemy";

        private bool isTriggered = false;

        public override int GetSubStateCount()
        {
            return 2; // Booléen : 0 = Vide, 1 = Occupé
        }

        public override int GetCurrentSubState()
        {
            return isTriggered ? 1 : 0;
        }

        private void OnTriggerStay2D(Collider2D collision)
        {
            if (!string.IsNullOrEmpty(targetTag) && collision.CompareTag(targetTag))
            {
                isTriggered = true;
            }
        }

        private void OnTriggerExit2D(Collider2D collision)
        {
            if (!string.IsNullOrEmpty(targetTag) && collision.CompareTag(targetTag))
            {
                isTriggered = false;
            }
        }

        // Remise à zéro à chaque update si Unity foire le Exit (sécurité)
        // Mais en Physics2D, Stay se déclenche après, donc c'est ok de rater une frame parfois, 
        // ou d'implémenter un petit timer de decay. Pour la pureté, on laisse Trigger.
    }
}
