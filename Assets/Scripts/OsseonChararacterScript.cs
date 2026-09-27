using UnityEngine;

public class OsseonChararacterScript : MonoBehaviour, IPlayableCharacter
{
    // Osseon Specific Variables

    [Header("Osseon Attack & Hitbox Settings")]
    [SerializeField] private float attackDuration = 0.25f;
    [SerializeField] private float dashAttackDuration = 0.2f;
    [SerializeField] private float attackForwardDashForce = 4f;
    [SerializeField] private float inputBufferWindow = 0.15f;

    public AttackHitboxSettings attackSettings { get; set; }

    [Header("Osseon Hit Detection")]
    [SerializeField] private float attackRange = 1.2f;
    [SerializeField] private float attackRadius = 0.8f;
    [SerializeField] private int attackDamage = 10;
    [SerializeField] private int dashAttackDamage = 18;
    [SerializeField] private float normalKnockbackForce = 12f;
    [SerializeField] private float dashKnockbackForce = 22f;

    public HitDetectionSettings hitDetectionSettings { get; set; }

    [Header("Osseon Components")]
    [SerializeField] private Transform spriteContainer;
    public Animator anim { get; set; }
    public SpriteRenderer spriteRenderer { get; set; }


    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        attackSettings = new(attackDuration, dashAttackDuration, attackForwardDashForce, inputBufferWindow);
        hitDetectionSettings = new(attackRange, attackRadius, attackDamage, dashAttackDamage, normalKnockbackForce, dashKnockbackForce);

        if (spriteContainer != null)
        {
            spriteRenderer = spriteContainer.GetComponent<SpriteRenderer>();
            anim = spriteContainer.GetComponent<Animator>();
        }
        else
        {
            spriteContainer = transform;
            spriteRenderer = GetComponent<SpriteRenderer>();
            anim = GetComponent<Animator>();
        }
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
