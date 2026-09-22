using System.Collections;
using UnityEngine;

[RequireComponent(typeof(Rigidbody2D))]
public class DummyEnemy : MonoBehaviour
{
    [Header("Health Settings")]
    [SerializeField] private int maxHealth = 50;
    private int currentHealth;

    [Header("Hit Flash Settings")]
    [SerializeField] private Color hitFlashColor = Color.white; // Flashes white on hit
    [SerializeField] private float flashDuration = 0.12f;

    [Header("Knockback Settings")]
    [SerializeField] private float knockbackResistance = 0.2f; // 0 = full force, 1 = immune

    private SpriteRenderer spriteRenderer;
    private Color originalColor;
    private Rigidbody2D rb;

    void Start()
    {
        currentHealth = maxHealth;
        spriteRenderer = GetComponent<SpriteRenderer>();
        rb = GetComponent<Rigidbody2D>();

        // Cache the original base color assigned in the Inspector (e.g. Red)
        if (spriteRenderer != null)
        {
            originalColor = spriteRenderer.color;
        }

        // Configure Rigidbody2D for clean physics-based knockback
        rb.gravityScale = 0f;            // Top-down game mode
        rb.linearDamping = 10f;                   // High drag creates smooth deceleration after push
        rb.constraints = RigidbodyConstraints2D.FreezeRotation; // Prevents dummy from spinning
    }

    public void TakeDamage(int damage, Vector2 attackerPosition, float knockbackForce)
    {
        currentHealth -= damage;
        Debug.Log($"Enemy hit! Health: {currentHealth}/{maxHealth}");

        // color flash on hit
        if (spriteRenderer != null)
        {
            StopAllCoroutines();
            StartCoroutine(FlashHitColor());
        }

        // knockback dir
        Vector2 pushDirection = ((Vector2)transform.position - attackerPosition).normalized;
        float finalForce = knockbackForce * (1f - Mathf.Clamp01(knockbackResistance));
        
        rb.linearVelocity = Vector2.zero; 
        rb.AddForce(pushDirection * finalForce, ForceMode2D.Impulse);

        // death check
        if (currentHealth <= 0)
        {
            Debug.Log("Enemy Destroyed!");
            Destroy(gameObject);
        }
    }

    private IEnumerator FlashHitColor()
    {
        // flash color and back to base
        spriteRenderer.color = hitFlashColor;
        yield return new WaitForSeconds(flashDuration);
        spriteRenderer.color = originalColor;
    }
}