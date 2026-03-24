using UnityEngine;

namespace Pluminus.Examples.MarioAndLuigi
{
    // On retire sciemment tous les [RequireComponent] automatiques. 
    // C'est à vous d'ajouter manuellement le Rigidbody2D (Kinematic) et le Collider2D (Is Trigger).
    public class MLAttacker : MonoBehaviour
    {
        public bool isAir;
        public bool isFast;
        private float speed;
        
        [HideInInspector] public MLEnemy targetAI;
        [HideInInspector] public bool hasBeenResolved = false;

        public void Setup(bool air, bool fast, MLEnemy aiObject)
        {
            isAir = air;
            isFast = fast;
            targetAI = aiObject;
            
            speed = isFast ? 12f : 6f; // Vitesse rapide = 12, lente = 6
            
            // On se place exactement à la hauteur enregistrée par l'IA lors de son dernier appui sur le sol "Ground"
            float baseHeight = targetAI != null ? targetAI.restingY : transform.position.y;

            if (isAir)
            {
                transform.position = new Vector3(transform.position.x, baseHeight + 2.0f, 0);
            }
            else
            {
                transform.position = new Vector3(transform.position.x, baseHeight, 0);
            }
            
            // Feedback visuel : Rouge = Rapide, Orange = Lent. Carré = Sol, Cercle (ou transparent) = Air
            var sr = GetComponent<SpriteRenderer>();
            sr.color = isFast ? new Color(1f, 0.2f, 0.2f) : new Color(1f, 0.6f, 0.2f);
        }

        void Update()
        {
            // Avance vers la gauche
            transform.Translate(Vector3.left * speed * Time.deltaTime);

            if (targetAI != null && !hasBeenResolved && transform.position.x < targetAI.transform.position.x - 2f)
            {
                hasBeenResolved = true;
                if (isAir)
                {
                    targetAI.RewardEvadeAir();
                }
                else
                {
                    targetAI.RewardEvadeGround();
                }
                Destroy(gameObject);
            }

            if (transform.position.x < -15f)
            {
                Destroy(gameObject);
            }
        }
    }
}
