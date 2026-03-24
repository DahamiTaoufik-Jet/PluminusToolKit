using UnityEngine;
using System.Collections;
using Pluminus.Core;

namespace Pluminus.Examples.MarioAndLuigi
{
    [RequireComponent(typeof(AdaptiveBrain))]
    [RequireComponent(typeof(Rigidbody2D))]
    public class MLEnemy : MonoBehaviour, Pluminus.Integration.IActionExecutor
    {
        private AdaptiveBrain brain;
        private Rigidbody2D rb;

        [Header("Configuration Saut")]
        public float jumpForce = 15f;
        
        [Header("Performance IA")]
        public float decisionTickRate = 0.05f;

        [HideInInspector] public float restingY;
        private bool isGrounded = false;

        private void Awake()
        {
            brain = GetComponent<AdaptiveBrain>();
            rb = GetComponent<Rigidbody2D>();
        }

        private void Update()
        {
            if (isGrounded) restingY = transform.position.y;
        }

        private void Start()
        {
            StartCoroutine(BrainTickRoutine());
        }

        IEnumerator BrainTickRoutine()
        {
            // L'AdaptiveBrain trouvera tout seul le PluminusUnityStateBuilder attaché pour les observations
            while (true)
            {
                brain.TickDecision();
                yield return new WaitForSeconds(decisionTickRate);
            }
        }

        // ==========================================
        // DÉTECTION DU SOL (TAG)
        // ==========================================
        private void OnCollisionStay2D(Collision2D collision)
        {
            if (collision.gameObject.CompareTag("Ground")) isGrounded = true;
        }

        private void OnCollisionExit2D(Collision2D collision)
        {
            if (collision.gameObject.CompareTag("Ground")) isGrounded = false;
        }

        // ==========================================
        // EXÉCUTEUR (Seule logique restante au développeur !)
        // ==========================================

        public int GetMaxActions() => 2; // 0: Rien, 1: Saut

        public void ExecuteAction(int actionId)
        {
            if (actionId == 1 && isGrounded)
            {
                rb.linearVelocity = new Vector2(0, jumpForce);
                Debug.Log("<color=grey>IA : *Saut* (Facture d'Énergie envoyée !)</color>");
                brain.ApplyRewardFlag("JumpTiredness"); 
            }
        }

        public bool IsActionValid(int actionId) => true;

        // ==========================================
        // COLLISIONS & RÉCOMPENSES
        // ==========================================

        public void RewardEvadeGround()
        {
            Debug.Log("<color=yellow>IA : A esquivé une attaque au sol !</color>");
            brain.ApplyRewardFlag("EvadeGround");
        }

        public void RewardEvadeAir()
        {
            Debug.Log("<color=green>IA : A ignoré une attaque volante !</color>");
            brain.ApplyRewardFlag("EvadeAir");
        }

        private void OnTriggerEnter2D(Collider2D collision) => HandleHit(collision.gameObject);

        private void OnCollisionEnter2D(Collision2D collision)
        {
            if (!collision.gameObject.CompareTag("Ground")) HandleHit(collision.gameObject);
        }

        private void HandleHit(GameObject go)
        {
            MLAttacker attacker = go.GetComponent<MLAttacker>();
            if (attacker == null) attacker = go.GetComponentInParent<MLAttacker>();

            if (attacker != null && !attacker.hasBeenResolved)
            {
                attacker.hasBeenResolved = true;

                if (attacker.isAir)
                {
                    Debug.Log("<color=red>IA : A SAUTÉ DANS L'ATTAQUE AÉRIENNE... OUCH !</color>");
                    brain.ApplyRewardFlag("HitAir"); // -5
                }
                else
                {
                    if (rb.linearVelocity.y < 0.5f && transform.position.y > attacker.transform.position.y + 0.5f)
                    {
                        Debug.Log("<color=magenta>💥 IA : STOMP PARFAIT ! JUMP SUR LA TÊTE DE L'ENNEMI !</color>");
                        brain.ApplyRewardFlag("Stomp"); // +5
                        rb.linearVelocity = new Vector2(0, jumpForce * 0.7f);
                    }
                    else
                    {
                        Debug.Log("<color=red>IA : S'est mangée l'attaque au sol de plein fouet...</color>");
                        brain.ApplyRewardFlag("HitGround"); // -5
                    }
                }
                Destroy(attacker.gameObject);
            }
        }
    }
}
