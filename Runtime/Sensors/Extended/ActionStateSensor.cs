using UnityEngine;

namespace Pluminus.Sensors.Extended
{
    /// <summary>
    /// Capteur d'État d'Action.
    /// Sert de 'pont' pour que l'IA connaisse sa phase actuelle (ex: Attaque, Idle, Stun).
    /// </summary>
    [AddComponentMenu("Pluminus/Sensors/Action State Sensor")]
    public class ActionStateSensor : PluminusStateSensor
    {
        [TextArea(3, 5)]
        public string Note = "INFO: Ce capteur permet à l'IA de savoir ce qu'elle fait déjà (ex: État 1 = Attaque en cours). Appelez 'SetActionState(id)' depuis vos scripts ou Animation Events.";

        [Header("Configuration")]
        [Min(1)] public int numberOfStates = 3;

        private int currentState = 0;

        public override int GetSubStateCount() => numberOfStates;
        public override int GetCurrentSubState() => Mathf.Clamp(currentState, 0, numberOfStates - 1);

        public void SetActionState(int newStateId)
        {
            currentState = newStateId;
        }

        private void OnDrawGizmosSelected()
        {
#if UNITY_EDITOR
            UnityEditor.Handles.Label(transform.position + Vector3.up * 2.5f, $"Phase Action : {GetCurrentSubState()}", new GUIStyle { normal = { textColor = Color.white }, alignment = TextAnchor.MiddleCenter });
#endif
        }
    }
}
