using UnityEngine;
using System.Collections.Generic;

namespace Pluminus.Sensors.Extended
{
    [System.Serializable]
    public class LevelConfig
    {
        public string label = "Étage";
        public float heightOffset = 0f; // Hauteur relative au centre
        public float levelSizeY = 0.5f; // Taille verticale
        public Vector2 horizontalSize = new Vector2(5f, 5f); // Taille X / Z
    }

    [AddComponentMenu("Pluminus/Sensors/Elevation Sensor (Multi-Floor)")]
    public class ElevationSensor : PluminusStateSensor
    {
        [Header("Cible")]
        public Transform target;
        public bool autoFindPlayerTag = true;

        [Header("Étages (Floors)")]
        [Tooltip("Configurez vos différents niveaux de détection.")]
        public List<LevelConfig> floors = new List<LevelConfig> {
            new LevelConfig { label = "Même Niveau", heightOffset = 0, levelSizeY = 0.5f },
            new LevelConfig { label = "Étage Supérieur", heightOffset = 3f, levelSizeY = 1.0f }
        };

        [Header("Visualisation")]
        public bool showGizmos = true;

        public override int GetSubStateCount()
        {
            // 0 = Aucun étage, 1..N = Index de l'étage
            return floors.Count + 1;
        }

        protected override void Awake()
        {
            base.Awake();
            if (target == null && autoFindPlayerTag)
            {
                GameObject player = GameObject.FindGameObjectWithTag("Player");
                if (player != null) target = player.transform;
            }
        }

        public override int GetCurrentSubState()
        {
            if (target == null) return 0;

            for (int i = 0; i < floors.Count; i++)
            {
                if (IsInsideFloor(floors[i])) return i + 1;
            }

            return 0; // Hors zones configurées
        }

        private bool IsInsideFloor(LevelConfig floor)
        {
            Vector3 relPos = target.position - transform.position;
            
            // Verifie la hauteur
            float floorBottom = floor.heightOffset - (floor.levelSizeY / 2f);
            float floorTop = floor.heightOffset + (floor.levelSizeY / 2f);
            
            if (relPos.y < floorBottom || relPos.y > floorTop) return false;

            // Verifie l'horizontale (X/Z)
            if (Mathf.Abs(relPos.x) > floor.horizontalSize.x / 2f) return false;
            if (Mathf.Abs(relPos.z) > floor.horizontalSize.y / 2f) return false;

            return true;
        }

        private void OnDrawGizmosSelected()
        {
            if (!showGizmos) return;

            int currentState = Application.isPlaying ? GetCurrentSubState() : -1;

            for (int i = 0; i < floors.Count; i++)
            {
                bool isActive = (currentState == i + 1);
                Gizmos.color = isActive ? new Color(1, 1, 0, 0.6f) : new Color(0, 1, 0, 0.2f);
                
                Vector3 floorCenter = transform.position + Vector3.up * floors[i].heightOffset;
                Vector3 floorSize = new Vector3(floors[i].horizontalSize.x, floors[i].levelSizeY, floors[i].horizontalSize.y);
                
                Gizmos.DrawCube(floorCenter, floorSize);
                Gizmos.DrawWireCube(floorCenter, floorSize);

#if UNITY_EDITOR
                UnityEditor.Handles.Label(floorCenter + Vector3.up * (floors[i].levelSizeY / 2f), floors[i].label);
#endif
            }
        }
    }
}
