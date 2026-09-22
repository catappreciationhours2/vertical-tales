using System.Collections;
using UnityEngine;

public class NewMonoBehaviourScript : MonoBehaviour
{
    [Header("Movement & Input")]
    public float speed = 5f;
    private Vector3 change;
    private Vector2 lookDirection = Vector2.down; // Default facing front/down

    [Header("Dash Settings")]
    [SerializeField] private float dashSpeed = 16f;
    [SerializeField] private float dashDuration = 0.15f;
    [SerializeField] private float dashCooldown = 0.4f;
    [SerializeField] private Vector3 dashStretchScale = new Vector3(1.4f, 0.7f, 1f);

    [Header("Attack & Hitbox Settings")]
    [SerializeField] private float attackDuration = 0.25f;
    [SerializeField] private float dashAttackDuration = 0.2f;
    [SerializeField] private float attackForwardDashForce = 4f;
    [SerializeField] private float inputBufferWindow = 0.15f;
    
    [Header("Hit Detection")]
    [SerializeField] private float attackRange = 1.2f;      
    [SerializeField] private float attackRadius = 0.8f;    
    [SerializeField] private LayerMask enemyLayers;        
    [SerializeField] private int attackDamage = 10;
    [SerializeField] private int dashAttackDamage = 18;
    [SerializeField] private float normalKnockbackForce = 12f;
    [SerializeField] private float dashKnockbackForce = 22f;

    [Header("Components")]
    [SerializeField] private Transform spriteContainer;
    private Rigidbody2D myRigidBody;
    private Animator anim;
    private SpriteRenderer spriteRenderer;

    // Internal State Machine
    private enum CombatState { IdleMove, Dashing, Attacking, DashAttacking }
    private CombatState currentState = CombatState.IdleMove;

    private float nextDashTime;
    private float stateTimer;
    private Vector2 activeDashDirection;

    // Buffer Memory
    private bool attackBuffered;
    private float bufferTimer;

    private Vector3 originalSpriteScale;

    void Start()
    {
        myRigidBody = GetComponent<Rigidbody2D>();

        if (spriteContainer != null)
        {
            spriteRenderer = spriteContainer.GetComponent<SpriteRenderer>();
            anim = spriteContainer.GetComponent<Animator>();
            originalSpriteScale = spriteContainer.localScale;
        }
        else
        {
            spriteContainer = transform;
            spriteRenderer = GetComponent<SpriteRenderer>();
            anim = GetComponent<Animator>();
            originalSpriteScale = Vector3.one;
        }
    }

    void Update()
    {
        GatherInput();
        UpdateBuffer();
        HandleStateTransitions();
    }

    void FixedUpdate()
    {
        ExecuteStatePhysics();
    }

    private void GatherInput()
    {
        if (currentState == CombatState.IdleMove || currentState == CombatState.Dashing)
        {
            change = Vector3.zero;
            change.x = Input.GetAxisRaw("Horizontal");
            change.y = Input.GetAxisRaw("Vertical");

            if (change != Vector3.zero)
            {
                lookDirection = change.normalized;

                if (anim != null)
                {
                    anim.SetFloat("MoveX", change.x);
                    anim.SetFloat("MoveY", change.y);
                    anim.SetBool("moving", true);
                }
            }
            else
            {
                if (anim != null)
                {
                    anim.SetBool("moving", false);
                }
            }
        }

        // Dash Trigger -> Key L
        if (Input.GetKeyDown(KeyCode.L) && Time.time >= nextDashTime)
        {
            StartDash();
        }

        // Queue Attack Input -> Key E
        if (Input.GetKeyDown(KeyCode.E))
        {
            attackBuffered = true;
            bufferTimer = inputBufferWindow;
        }
    }

    private void UpdateBuffer()
    {
        if (attackBuffered)
        {
            bufferTimer -= Time.deltaTime;
            if (bufferTimer <= 0f)
            {
                attackBuffered = false;
            }
        }
    }

    private void HandleStateTransitions()
    {
        if (currentState == CombatState.Dashing || currentState == CombatState.Attacking || currentState == CombatState.DashAttacking)
        {
            stateTimer -= Time.deltaTime;

            if (currentState == CombatState.Dashing && attackBuffered)
            {
                attackBuffered = false;
                StartAttack(isDashAttack: true);
                return;
            }

            if (stateTimer <= 0)
            {
                ResetState();
            }
        }

        if (currentState == CombatState.IdleMove && attackBuffered)
        {
            attackBuffered = false;
            StartAttack(isDashAttack: false);
        }
    }

    private void ExecuteStatePhysics()
    {
        switch (currentState)
        {
            case CombatState.IdleMove:
                if (change != Vector3.zero)
                {
                    myRigidBody.MovePosition(transform.position + change.normalized * speed * Time.fixedDeltaTime);
                }
                break;

            case CombatState.Dashing:
                myRigidBody.linearVelocity = activeDashDirection * dashSpeed;
                break;

            case CombatState.Attacking:
            case CombatState.DashAttacking:
                myRigidBody.linearVelocity = Vector2.Lerp(myRigidBody.linearVelocity, Vector2.zero, Time.fixedDeltaTime * 15f);
                break;
        }
    }

    private void ResetState()
    {
        currentState = CombatState.IdleMove;
        myRigidBody.linearVelocity = Vector2.zero;
        spriteContainer.localScale = originalSpriteScale;
        spriteContainer.localRotation = Quaternion.identity;
    }

    private void StartDash()
    {
        currentState = CombatState.Dashing;
        stateTimer = dashDuration;
        nextDashTime = Time.time + dashCooldown;

        activeDashDirection = change != Vector3.zero ? (Vector2)change.normalized : lookDirection;
        spriteContainer.localScale = dashStretchScale;
    }

    private void StartAttack(bool isDashAttack)
    {
        currentState = isDashAttack ? CombatState.DashAttacking : CombatState.Attacking;
        stateTimer = isDashAttack ? dashAttackDuration : attackDuration;

        myRigidBody.linearVelocity = lookDirection * (isDashAttack ? attackForwardDashForce * 1.5f : attackForwardDashForce);
        spriteContainer.localScale = originalSpriteScale;
        spriteContainer.localRotation = Quaternion.identity;

        // Visual Slash
        StartCoroutine(SpawnSlashVisual(isDashAttack));

        // Detect if attack lands on target
        DetectHit(isDashAttack);
    }

    private void DetectHit(bool isDashAttack)
    {
        Vector2 attackPoint = (Vector2)transform.position + (lookDirection * attackRange);
        Collider2D[] hitEnemies = Physics2D.OverlapCircleAll(attackPoint, attackRadius, enemyLayers);

        if (hitEnemies.Length > 0)
        {
            int damage = isDashAttack ? dashAttackDamage : attackDamage;
            float knockback = isDashAttack ? dashKnockbackForce : normalKnockbackForce;

            foreach (Collider2D enemy in hitEnemies)
            {
                if (enemy.TryGetComponent<DummyEnemy>(out DummyEnemy enemyComponent))
                {
                    // Passes Damage, Player Position (for direction), and Knockback Force
                    enemyComponent.TakeDamage(damage, transform.position, knockback);
                }
            }
        }
    }

    private IEnumerator SpawnSlashVisual(bool isDashAttack)
    {
        GameObject slashObj = new GameObject("ProceduralSlash");
        slashObj.transform.position = transform.position + (Vector3)(lookDirection * 1.1f);

        float angle = Mathf.Atan2(lookDirection.y, lookDirection.x) * Mathf.Rad2Deg;
        slashObj.transform.rotation = Quaternion.Euler(0, 0, angle);

        LineRenderer line = slashObj.AddComponent<LineRenderer>();
        line.useWorldSpace = false;
        line.startWidth = isDashAttack ? 0.35f : 0.25f;
        line.endWidth = 0.02f;
        line.material = new Material(Shader.Find("Sprites/Default"));
        
        Color slashColor = isDashAttack ? new Color(0.2f, 0.9f, 1f, 0.9f) : new Color(1f, 1f, 1f, 0.9f);
        line.startColor = slashColor;
        line.endColor = slashColor;

        int points = 10;
        line.positionCount = points;
        float radius = isDashAttack ? 1.2f : 0.9f;
        float arcDegrees = 110f;

        for (int i = 0; i < points; i++)
        {
            float progress = (float)i / (points - 1);
            float currentAngle = (-arcDegrees / 2f + progress * arcDegrees) * Mathf.Deg2Rad;
            Vector3 pos = new Vector3(Mathf.Cos(currentAngle) * radius, Mathf.Sin(currentAngle) * radius, 0);
            line.SetPosition(i, pos);
        }

        float elapsed = 0f;
        float duration = isDashAttack ? dashAttackDuration : attackDuration;

        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            float t = elapsed / duration;

            slashObj.transform.localScale = Vector3.one * (0.8f + t * 0.5f);
            
            Color fadedColor = slashColor;
            fadedColor.a = Mathf.Lerp(0.9f, 0f, t);
            line.startColor = fadedColor;
            line.endColor = fadedColor;

            yield return null;
        }

        Destroy(slashObj);
    }

    // Draws red attack circle in Scene View so you can see your attack range
    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.red;
        Vector2 attackPoint = (Vector2)transform.position + (lookDirection * attackRange);
        Gizmos.DrawWireSphere(attackPoint, attackRadius);
    }
}