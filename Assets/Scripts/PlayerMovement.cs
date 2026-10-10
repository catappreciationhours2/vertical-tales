using System.Collections;
using Unity.VisualScripting;
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
    [SerializeField] private GameObject osseonGameObject;
    [SerializeField] private GameObject mycenaGameObject;
    private Rigidbody2D myRigidBody;
    private Animator anim;
    private SpriteRenderer spriteRenderer;

    // I don't know why, but I wanted to make this general in case we added another character??
    // IDK - Chcuk
    private enum Character { Osseon, Mycena }
    private int numCharacters = 2;

    [Header("switching")]
    [SerializeField] private Character currentCharacter;
    [SerializeField] private float switchCooldownTime = 3.0f;
    private bool canSwitch = true;
    private float switchCooldownTimer;

    private int currentCharacterIndex; // 0 - Osseon, 1 - Mycena
    private IPlayableCharacter osseonScript;
    private IPlayableCharacter mycenaScript;
    private IPlayableCharacter currentCharacterScript;

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

        // Gets the characters scripts from their gameobjects
        osseonScript = osseonGameObject.GetComponent<OsseonChararacterScript>();
        mycenaScript = mycenaGameObject.GetComponent<MycenaCharacterScript>();

        // TEMPARARY, I don't knwo how we want to do this yet, so i'm having it be osseon by default
        SwitchCharacter(Character.Osseon);

        // For Saftey
        if (spriteContainer == null)
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

        // Basic switching, haven't implimented it with any of the other actions
        if (Input.GetKeyDown(KeyCode.V) && canSwitch)
        {
            canSwitch = false;
            switchCooldownTimer = switchCooldownTime;
            SwitchCharacter(GetNextCharacter(currentCharacterIndex));
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

        if (!canSwitch)
        {
            switchCooldownTimer -= Time.deltaTime;
            if (switchCooldownTimer <= 0f)
            {
                canSwitch = true;
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

    // Changes the character by switching all neccesary attributes
    private void SwitchCharacter(Character character)
    {
        // Sets the new current character
        currentCharacter = character;
        currentCharacterIndex = (int)character;
        Debug.Log($"Current Character Index: {currentCharacterIndex}");

        // Gets the scripts for the new character
        switch (character)
        {
            case Character.Osseon:
                osseonGameObject.SetActive(true);
                mycenaGameObject.SetActive(false);
                currentCharacterScript = osseonScript;
                break;
            case Character.Mycena:
                osseonGameObject.SetActive(false);
                mycenaGameObject.SetActive(true);
                currentCharacterScript = mycenaScript;
                break;
        }

        // Graphics 
        anim = currentCharacterScript.GetAnimator();
        spriteRenderer = currentCharacterScript.GetSpriteRenderer();
        originalSpriteScale = spriteContainer.localScale;

        // Attack and hitbox
        AttackHitboxSettings s = currentCharacterScript.GetAttackHitboxSettings();
        attackDuration = s.attackDuration;
        dashAttackDuration = s.dashAttackDuration;
        inputBufferWindow = s.inputBufferWindow;
        attackForwardDashForce = s.attackForwardDashForce;

        // other stuff im tired
        HitDetectionSettings h = currentCharacterScript.GetHitDetectionSettings();
        attackDamage = h.attackDamage;
        dashAttackDamage = h.dashAttackDamage;
        attackRange = h.attackRange;
        attackRadius = h.attackRadius;
        normalKnockbackForce = h.normalKnockbackForce;
        dashKnockbackForce = h.dashKnockbackForce;
    }

    private Character GetNextCharacter(int startingIndex)
    {
        //Returns the character next in line
        return (Character)((startingIndex + 1) % numCharacters);
    }
}