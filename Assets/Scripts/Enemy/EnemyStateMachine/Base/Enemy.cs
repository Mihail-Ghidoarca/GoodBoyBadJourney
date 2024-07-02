using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class Enemy : MonoBehaviour, IDamageable, IEnemyMovable, ITriggerCheckable
{
    [field: SerializeField] public float MaxHealth { get; set; }
    public float CurrentHealth { get; set; }
    public Rigidbody2D RB { get; set; }
    public bool isFacingRight { get; set; } = false;
    public Collider2D coll { get; set;}
    [field: SerializeField] public Animator animator { get; set; }
    #region State Machine Variables
    public EnemyStateMachine StateMachine { get; set; }
    public EnemyIdleState IdleState {  get; set; }
    public EnemyAttackState AttackState { get; set; }
    public EnemyChaseState ChaseState { get; set; }
    public bool IsAggroed { get; set; }
    public bool IsWithinStrikingDistance { get; set; }
    public bool IsMelee { get; set; }
    public bool HasTakenDamage { get; set; }
    public float knockbackForce = 10f;
    [SerializeField] public BoxCollider2D rangeCollider;
    #endregion
    #region ScriptableObject Variables

    [SerializeField] protected EnemyIdleSOBase EnemyIdleBase;
    [SerializeField] protected EnemyChaseSOBase EnemyChaseBase;
    [SerializeField] protected EnemyAttackSOBase EnemyAttackBase;
    public EnemyIdleSOBase EnemyIdleBaseInstance { get; set; }
    public EnemyChaseSOBase EnemyChaseBaseInstance { get; set; }
    public EnemyAttackSOBase EnemyAttackBaseInstance { get; set; }

    #endregion

    public virtual void Awake()
    {
        EnemyIdleBaseInstance = Instantiate(EnemyIdleBase);
        EnemyChaseBaseInstance = Instantiate(EnemyChaseBase);
        EnemyAttackBaseInstance = Instantiate(EnemyAttackBase);

        StateMachine = new EnemyStateMachine();

        IdleState = new EnemyIdleState(this, StateMachine);
        ChaseState = new EnemyChaseState(this, StateMachine);
        AttackState = new EnemyAttackState(this, StateMachine);
    }

    public virtual void Start()
    {
        CurrentHealth = MaxHealth;
        RB = GetComponent<Rigidbody2D>();
        coll = GetComponent<Collider2D>();
        EnemyIdleBaseInstance.Initialize(gameObject, this);
        EnemyChaseBaseInstance.Initialize(gameObject, this);
        EnemyAttackBaseInstance.Initialize(gameObject, this);

        StateMachine.Initialize(IdleState);
    }

    public virtual void Update()
    {
        StateMachine.CurrentEnemyState.FrameUpdate();
        if(RB.velocity == Vector2.zero)
        {
            animator.SetBool("isMoving", false);
        }
    }

    public virtual void FixedUpdate()
    {
        StateMachine.CurrentEnemyState.PhysicsUpdate();
    }

    #region Health and Die functions
    public void TakeDamage(float damageAmount)
    {
        HasTakenDamage = true;
        animator.SetTrigger("Hurt");
        RB.AddForce(knockbackForce * new Vector2(3f, 1f), ForceMode2D.Impulse);

        CurrentHealth -= damageAmount;
        if (CurrentHealth <= 0f) {
            animator.SetTrigger("Death");
            Die();
            GlobalVars.playerScore += 1;
        }
    }

    public void Die()
    {
        gameObject.SetActive(false);
        Destroy(gameObject);
    }

    #endregion
    #region Movement Functions

    public void MoveEnemy(Vector2 velocity)
    {
        animator.SetBool("isMoving", true);
        RB.velocity = new Vector2(velocity.x, 0f);
        CheckForLeftOrRightFacing(velocity);
    }

    public void CheckForLeftOrRightFacing(Vector2 velocity)
    {
        if (isFacingRight && velocity.x < 0f)
        {
            Vector3 rotator = new Vector3(transform.rotation.x, 0f, transform.rotation.z);
            transform.rotation = Quaternion.Euler(rotator);
            isFacingRight = !isFacingRight;
        }
        else if (!isFacingRight && velocity.x > 0f)
        {
            Vector3 rotator = new Vector3(transform.rotation.x, 180f, transform.rotation.z);
            transform.rotation = Quaternion.Euler(rotator);
            isFacingRight = !isFacingRight;
        }

    }
   
    #endregion

    #region Distance Checks

    public void SetAggroStatus(bool isAggroed)
    {
        IsAggroed = isAggroed;
    }

    public void SetStrikingDistanceBool(bool isWithinStrikingDistance)
    {
        IsWithinStrikingDistance = isWithinStrikingDistance;
    }

    public void SetMeleeCheck(bool isMelee)
    {
        IsMelee = isMelee;
    }
    #endregion

    #region Animation Triggers

    private void AnimationTriggerEvent(AnimationTriggerType triggerType)
    {
        StateMachine.CurrentEnemyState.AnimationTriggerEvent(triggerType);
    }
    
    public enum AnimationTriggerType
    {
        EnemyDamaged,
        PlayFootstepSound
    }
    #endregion
}
