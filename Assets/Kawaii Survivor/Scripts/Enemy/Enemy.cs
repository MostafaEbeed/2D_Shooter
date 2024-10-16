using System;
using TMPro;
using UnityEngine;

[RequireComponent (typeof(EnemyMovement))]
public class Enemy : MonoBehaviour
{
    [Header("Components")]
    private EnemyMovement movement;

    [Header("Health")]
    [SerializeField] private int maxHealth;
    private int health;
    [SerializeField] private TextMeshPro healthText;

    [Header("Elements")]
    private Player player;  

    [Header("Spawn Sequence Related")]
    [SerializeField] private SpriteRenderer spriteRenderer;
    [SerializeField] private SpriteRenderer spawnIndicator;
    [SerializeField] private Collider2D collider;
    private bool hasSpawned;

    [Header("Effects")]
    [SerializeField] private ParticleSystem passAwayParticles;

    [Header("Attack")]
    [SerializeField] private int damage;
    [SerializeField] private float attackFrequency;
    [SerializeField] private float playerDetectionRadius;
    private float attackDelay;
    private float attackTimer;

    [Header("Debug")]
    [SerializeField] private bool displayGizmos;

    [Header("Actions")]
    public static Action<int, Vector2> onDamageTaken;

    void Start()
    {
        health = maxHealth;
        healthText.text = health.ToString();

        movement = GetComponent<EnemyMovement>();

        player = FindFirstObjectByType<Player>();

        if (player == null)
        {
            Debug.LogWarning("No Player Found");
            Destroy(gameObject);
        }

        StartSpawnSequence();

        attackDelay = 1f / attackFrequency;
    }

    void Update()
    {
        if (attackTimer >= attackDelay)
            TryAttack();
        else
            Wait();
    }

    private void SpawnSequnceCompleted()
    {
        SetRenderersVisibility(true);
        hasSpawned = true;

        collider.enabled = true;

        movement.StorePlayer(player);
    }

    private void StartSpawnSequence()
    {
        SetRenderersVisibility(false);

        Vector3 targetScale = spawnIndicator.transform.localScale * 1.2f;
        LeanTween.scale(spawnIndicator.gameObject, targetScale, 0.3f)
            .setLoopPingPong(4)
            .setOnComplete(SpawnSequnceCompleted);
    }

    private void SetRenderersVisibility(bool visibility = true)
    {
        spriteRenderer.enabled = visibility;
        spawnIndicator.enabled = !visibility;
    }

    private void Wait()
    {
        attackTimer += Time.deltaTime;
    }

    private void TryAttack()
    {
        float distanceToPlayer = Vector2.Distance(transform.position, player.transform.position);

        if (distanceToPlayer <= playerDetectionRadius)
            Attack();
    }

    private void Attack()
    {
        attackTimer = 0f;

        player.TakeDamage(damage);
    }

    private void PassAway()
    {
        passAwayParticles.transform.SetParent(null);
        passAwayParticles.Play();

        Destroy(gameObject);
    }

    private void OnDrawGizmos()
    {
        if (!displayGizmos) return;

        Gizmos.color = Color.red;

        Gizmos.DrawWireSphere(transform.position, playerDetectionRadius);
    }

    public void TakeDamage(int damage)
    {
        int realDamage = Mathf.Min(damage, health);
        health -= realDamage;

        healthText.text = health.ToString();

        onDamageTaken?.Invoke(damage, transform.position);

        if(health <= 0)
        {
            PassAway();
        }
    }
}
