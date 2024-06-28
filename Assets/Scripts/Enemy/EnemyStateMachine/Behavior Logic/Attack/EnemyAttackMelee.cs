using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEditor.SearchService;
using UnityEngine;
using UnityEngine.SceneManagement;

[CreateAssetMenu(fileName = "Attack-Melee Attack", menuName = "Enemy Logic/Attack Logic/Melee Attack")]
public class EnemyAttackMelee : EnemyAttackSOBase
{
    [SerializeField] private float _timeBetweenAttacks = 1.5f;
    [SerializeField] private float _timeUntilExit = 2f;
    [SerializeField] private float _distanceToCountExit = 3f;
    [SerializeField] private BoxCollider2D rangeCollider;
    private float _timer;
    private float _exitTimer;
    [SerializeField]public float detectionRange = 5f;
    [SerializeField] private float _attackRange = 1.5f;
    [SerializeField] private int _attackDamage = 20;
    public PlayerHealth playerHealth;
    public float speed = 1f;

    public override void DoAnimationTriggerEventLogic(Enemy.AnimationTriggerType triggerType)
    {
        base.DoAnimationTriggerEventLogic(triggerType);
    }

    public override void DoEnterLogic()
    {
        base.DoEnterLogic();
    }

    public override void DoExitLogic()
    {
        base.DoExitLogic();
    }

    public override void DoFrameUpdateLogic()
    {
        base.DoFrameUpdateLogic();

        float distanceToPlayer = Mathf.Abs(playerTransform.position.x - enemy.RB.position.x);

        if (distanceToPlayer <= _attackRange)
        {
            if(distanceToPlayer < 2f)
                enemy.MoveEnemy(Vector2.zero);
            if (_timer > _timeBetweenAttacks)
            {
                _timer = 0f;
                playerTransform.GetComponent<PlayerHealth>().TakeDamage(_attackDamage);
            }
        }

        Vector2 target = new Vector2(playerTransform.position.x, enemy.RB.position.y);
        Vector2 newPos = Vector2.MoveTowards(enemy.RB.position, target, speed);
        enemy.MoveEnemy(newPos);


        if (Vector2.Distance(playerTransform.position, enemy.RB.position) <= _attackRange)
        {
            enemy.animator.SetTrigger("Attack");
            _exitTimer += Time.deltaTime;

            if (_exitTimer > _timeUntilExit)
            {
                enemy.StateMachine.ChangeState(enemy.ChaseState);
            }
        }
        else
        {
            _exitTimer = 0f;
        }

        _timer += Time.deltaTime;
    }

    public override void DoPhysicsUpdateLogic()
    {
        base.DoPhysicsUpdateLogic();
    }

    public override void Initialize(GameObject gameObject, Enemy enemy)
    {
        base.Initialize(gameObject, enemy);
    }

    public override void ResetValues()
    {
        base.ResetValues();
    }

    private void PerformMeleeAttack()
    {
        // Play attack animation, if any
        enemy.animator.SetTrigger("Attack");
        Debug.Log("SALUT");
        float distanceToPlayer = playerTransform.position.x - enemy.RB.position.x;
        if (distanceToPlayer <= _attackRange)
        {
            playerHealth.TakeDamage(_attackDamage);
        }
    }

}
