using UnityEngine;
using System;
using UnityEngine.Serialization;

namespace Kawaii_Survivor.Scripts.Enemy
{
    public abstract class Enemy : MonoBehaviour
    {
        [Header("Components")]
        protected EnemyMovement movement;
        
        [Header("Health")]
        [SerializeField] protected int maxHealth;
        protected int health;
        
        [Header("Elements")]
        protected Player player;
        
        [Header("Spawn Sequence Related")]
        [SerializeField] protected SpriteRenderer spriteRenderer;
        [SerializeField] protected SpriteRenderer spawnIndicator;
        [FormerlySerializedAs("collider")] [SerializeField] protected Collider2D enemyCollider;
        protected bool hasSpawned;
        
        [Header("Effects")]
        [SerializeField] protected ParticleSystem passAwayParticles;

        [Header("Attack")]
        [SerializeField] protected float playerDetectionRadius;

        [Header("Actions")]
        public static Action<int, Vector2, bool> onDamageTaken;
        public static Action<Vector2> onPassedAway;

        [Header("Debug")]
        [SerializeField] protected bool displayGizmos;
        
        protected virtual void Start()
        {
            health = maxHealth;
            
            movement = GetComponent<EnemyMovement>();
            
            player = FindFirstObjectByType<Player>();

            if (player == null)
            {
                Debug.LogWarning("No Player Found");
                Destroy(gameObject);
            }
            
            StartSpawnSequence();
        }

        protected bool CanAttack()
        {
            return spriteRenderer.enabled;
        }
        
        private void StartSpawnSequence()
        {
            SetRenderersVisibility(false);

            Vector3 targetScale = spawnIndicator.transform.localScale * 1.2f;
            LeanTween.scale(spawnIndicator.gameObject, targetScale, 0.3f)
                .setLoopPingPong(4)
                .setOnComplete(SpawnSequnceCompleted);
        }
        
        private void SpawnSequnceCompleted()
        {
            SetRenderersVisibility(true);
            hasSpawned = true;

            enemyCollider.enabled = true;

            movement.StorePlayer(player);
        }
        
        private void SetRenderersVisibility(bool visibility = true)
        {
            spriteRenderer.enabled = visibility;
            spawnIndicator.enabled = !visibility;
        }
        
        public void TakeDamage(int damage, bool isCriticalHit)
        {
            int realDamage = Mathf.Min(damage, health);
            health -= realDamage;

            onDamageTaken?.Invoke(damage, transform.position, isCriticalHit);

            if (health <= 0)
            {
                PassAway();
            }
        }
        
        public void PassAway()
        {
            onPassedAway?.Invoke(transform.position);

            PassAwayAfterWave();
        }

        public void PassAwayAfterWave()
        {
            passAwayParticles.transform.SetParent(null);
            passAwayParticles.Play();

            LeanTween.cancel(gameObject);
            
            Destroy(gameObject);
        }

        private void OnDrawGizmos()
        {
            if (!displayGizmos) return;

            Gizmos.color = Color.magenta;
            Gizmos.DrawWireSphere(transform.position, playerDetectionRadius);
        }
    }
}
