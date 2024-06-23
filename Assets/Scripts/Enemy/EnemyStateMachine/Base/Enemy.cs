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
    public bool HasTakenDamage { get; set; }
    public float knockbackForce = 10f;
    #endregion

    #region ScriptableObject Variables

    [SerializeField] private EnemyIdleSOBase EnemyIdleBase;
    [SerializeField] private EnemyChaseSOBase EnemyChaseBase;
    [SerializeField] private EnemyAttackSOBase EnemyAttackBase;

    public EnemyIdleSOBase EnemyIdleBaseInstance { get; set; }
    public EnemyChaseSOBase EnemyChaseBaseInstance { get; set; }
    public EnemyAttackSOBase EnemyAttackBaseInstance { get; set; }

    #endregion

    public void Awake()
    {

        EnemyIdleBaseInstance = Instantiate(EnemyIdleBase);
        EnemyChaseBaseInstance = Instantiate(EnemyChaseBase);
        EnemyAttackBaseInstance = Instantiate(EnemyAttackBase);

        StateMachine = new EnemyStateMachine();

        IdleState = new EnemyIdleState(this, StateMachine);
        ChaseState = new EnemyChaseState(this, StateMachine);
        AttackState = new EnemyAttackState(this, StateMachine);
    }

    public void Start()
    {
        CurrentHealth = MaxHealth;
        RB = GetComponent<Rigidbody2D>();
        coll = GetComponent<Collider2D>();
        EnemyIdleBaseInstance.Initialize(gameObject, this);
        EnemyChaseBaseInstance.Initialize(gameObject, this);
        EnemyAttackBaseInstance.Initialize(gameObject, this);

        StateMachine.Initialize(IdleState);
    }

    public void Update()
    {
        StateMachine.CurrentEnemyState.FrameUpdate();
    }

    public void FixedUpdate()
    {
        StateMachine.CurrentEnemyState.PhysicsUpdate();
    }

    #region Health and Die functions
    public void TakeDamage(float damageAmount)
    {
        HasTakenDamage = true;
        animator.SetTrigger("Hurt");
        RB.AddForce(RB.position + new Vector2(100f, 15f), ForceMode2D.Impulse);
        CurrentHealth -= damageAmount;
        if (CurrentHealth <= 0f) {
            animator.SetTrigger("Death");
            Die();
        }
    }

    public void Die()
    {
        Destroy(gameObject);
    }



    #endregion
    #region Movement Functions

    public void MoveEnemy(Vector2 velocity)
    {
         
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
