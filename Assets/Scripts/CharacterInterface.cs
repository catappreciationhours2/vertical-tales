using Unity.VisualScripting;
using UnityEngine;

// Used to store the basic attack and hitbox settings
public struct AttackHitboxSettings
{
    // Attack Constants
    public float attackDuration { get; set; }
    public float dashAttackDuration { get; set; }
    public float attackForwardDashForce { get; set; }
    public float inputBufferWindow { get; set; }

    public AttackHitboxSettings(float attackDur, float dashAttackDur, float attackForwardDashForce, float inputBufferWindow)
    {
        this.attackDuration = attackDur;
        this.dashAttackDuration = dashAttackDur;
        this.attackForwardDashForce = attackForwardDashForce;
        this.inputBufferWindow = inputBufferWindow;
    }
}

public struct HitDetectionSettings
{
    public float attackRange { get; set; }
    public float attackRadius { get; set; }
    public int attackDamage { get; set; }
    public int dashAttackDamage { get; set; }
    public float normalKnockbackForce { get; set; }
    public float dashKnockbackForce { get; set; }

    public HitDetectionSettings(float atkRange, float atkRadius, int atkDmg, int dashAtkDmg, float nKnockback, float dKnockback)
    {
        this.attackRange = atkRange;
        this.attackRadius = atkRadius;
        this.attackDamage = atkDmg;
        this.dashAttackDamage = dashAtkDmg;
        this.normalKnockbackForce = nKnockback;
        this.dashKnockbackForce = dKnockback;
    }
}

public interface IPlayableCharacter
{
    // Attack Constants
    AttackHitboxSettings attackSettings { get; set; }

    // Hit detection constants
    HitDetectionSettings hitDetectionSettings { get; set; }

    // Required Components
    Animator anim { get; set; }
    SpriteRenderer spriteRenderer { get; set; }
    
    // Graphics Getters
    Animator GetAnimator() { return anim; }
    SpriteRenderer GetSpriteRenderer() { return spriteRenderer; }

    // Attack and hitbox getters
    AttackHitboxSettings GetAttackHitboxSettings() { return attackSettings; }
    HitDetectionSettings GetHitDetectionSettings() { return hitDetectionSettings; }


}
