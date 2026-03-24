using UnityEngine;
using System.Collections;

namespace Pluminus.Examples.MarioAndLuigi
{
    public class MLSpawner : MonoBehaviour
    {
        public GameObject attackerPrefab;
        public MLEnemy targetAI;

        public float spawnMinDelay = 1.0f;
        public float spawnMaxDelay = 2.5f;

        private GameObject currentAttacker;

        void Start()
        {
            StartCoroutine(SpawnRoutine());
        }

        IEnumerator SpawnRoutine()
        {
            while (true)
            {
                // Dès le début de la boucle, on bloque tant que le précédent n'est pas mort/esquivé
                while (currentAttacker != null)
                {
                    yield return null; 
                }

                // Une fois l'écran vidé, on applique le timer aléatoire pour le prochain
                yield return new WaitForSeconds(Random.Range(spawnMinDelay, spawnMaxDelay));

                if (attackerPrefab != null && currentAttacker == null)
                {
                    // 1 chance sur 2 d'être en l'air, 1 chance sur 2 d'être rapide
                    bool isAir = Random.value > 0.5f;
                    bool isFast = Random.value > 0.5f;

                    currentAttacker = Instantiate(attackerPrefab, transform.position, Quaternion.identity);
                    currentAttacker.GetComponent<MLAttacker>().Setup(isAir, isFast, targetAI);
                }
            }
        }
    }
}
