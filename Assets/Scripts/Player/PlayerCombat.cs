using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Windows;

public class PlayerCombat : MonoBehaviour
{
    public Animator animator;
 
    [SerializeField] Transform SideAttackTransform, UpAttackTransform, DownAttackTransform;
    [SerializeField] Vector2 SideAttackArea, UpAttackArea, DownAttackArea;
    [SerializeField] private Rigidbody2D rb;
    [SerializeField] private Transform attackTransform;
    [SerializeField] private float attackRange = 1.5f;
    [SerializeField] private LayerMask attackableLayer;
    [SerializeField] private float damageAmount = 20f;
    [SerializeField] private float timeBtwAttacks = 0.15f;
    [SerializeField] GameObject slashEffect;
    public float knockbackForce = 5f;

    public bool ShouldBeDamaging { get; private set; } = false;

    private List<IDamageable> iDamageables = new List<IDamageable>();

    private float attackTimeCounter;

    RaycastHit2D[] hits;

    private float moveInput, yInput;

    private void Awake()
    {
        rb.GetComponent<Rigidbody2D>();
    }
    private void Start()
    {
        animator.GetComponent<Animator>();
        attackTimeCounter = timeBtwAttacks;
    }
    void Update()
    {
        GetInputs();
        if (UserInput.instance.controls.Attacking.Attack.WasPressedThisFrame() && attackTimeCounter >= timeBtwAttacks)
        {
            GlobalVars.actionQueue.Enqueue(PlayerAction.MeleeAttack);
            attackTimeCounter = 0f;
            animator.SetTrigger("attack");
        }
        attackTimeCounter += Time.deltaTime;
    }
    void GetInputs()
    {
        moveInput = UserInput.instance.moveInput.x;
        yInput = UserInput.instance.moveInput.y;
    }
    public IEnumerator DamageWhileSlashIsActive()
    {
        ShouldBeDamaging = true;
        while (ShouldBeDamaging)
        {
            if (yInput >= Mathf.Abs(moveInput) && yInput >= 0.3)
            {
                hits = Physics2D.CircleCastAll(UpAttackTransform.position, attackRange, transform.right, 0f, attackableLayer);
                SlashEffectAngle(slashEffect, 90, UpAttackTransform);
            }
            else if (yInput <= -Mathf.Abs(moveInput) && yInput <= -0.3)
            {
                hits = Physics2D.CircleCastAll(DownAttackTransform.position, attackRange, transform.right, 0f, attackableLayer);
                SlashEffectAngle(slashEffect, -90, DownAttackTransform);
            }
            else
            {
                hits = Physics2D.CircleCastAll(SideAttackTransform.position, attackRange, transform.right, 0f, attackableLayer);
                Instantiate(slashEffect, SideAttackTransform);
            }

            for (int i = 0; i < hits.Length; i++)
            {
                IDamageable iDamageable = hits[i].collider.gameObject.GetComponent<IDamageable>();

                if (iDamageable != null)
                {
                    iDamageable.TakeDamage(damageAmount);
                    iDamageables.Add(iDamageable);
                    ApplyKnockback(hits[i]);
                }
            }
            yield return null;
        }

        ReturnAttackablesToDamageable();
    }
    void SlashEffectAngle(GameObject _slashEffect, int _effectAngle, Transform _attackTransform)
    {
        _slashEffect = Instantiate(_slashEffect, _attackTransform);
        _slashEffect.transform.eulerAngles = new Vector3(0f, 0f, _effectAngle);
        _slashEffect.transform.localScale = new Vector2(0.1f, 0.3f);
    }

    private void ReturnAttackablesToDamageable()
    {
        foreach (IDamageable thingThatWasDamaged in iDamageables)
        {
            thingThatWasDamaged.HasTakenDamage = false;
        }

        iDamageables.Clear();
    }

    private void ApplyKnockback(RaycastHit2D hit)
    {
        Vector2 knockbackDirection = (Vector2)transform.position - hit.point;
        Debug.Log("knockback = " + knockbackDirection);
        rb.AddForce(2f * knockbackForce * knockbackDirection, ForceMode2D.Impulse);
    }

    void OnDrawGizmos()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(SideAttackTransform.position, attackRange);
        Gizmos.DrawWireCube(DownAttackTransform.position, DownAttackArea);
        Gizmos.DrawWireCube(UpAttackTransform.position, UpAttackArea);
    }

    #region Animation Triggers
    public void ShouldBeDamagingToTrue()
    {
        ShouldBeDamaging = true;
    }

    public void ShouldBeDamagingToFalse()
    {
        ShouldBeDamaging = false;
    }

    #endregion

}
