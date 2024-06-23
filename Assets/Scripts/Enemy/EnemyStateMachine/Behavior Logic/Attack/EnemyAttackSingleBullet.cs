using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "Attack-Single Bullet Attack", menuName = "Enemy Logic/Attack Logic/Single Bullet Attack")]
public class EnemyAttackSingleBullet : EnemyAttackSOBase
{
    [SerializeField] private Rigidbody2D Bullet;
    [SerializeField] private float _timeBetweenShots = 2f;
    [SerializeField] private float _timeUntilExit = 3;
    [SerializeField] private float _distanceToCountExit = 3f;
    [SerializeField] private float _bulletSpeed = 10f;
    [SerializeField ] private float attackDamage = 20;


    private float _timer;
    private float _exitTimer;

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
        enemy.MoveEnemy(Vector2.zero);
        //Debug.Log("salsallaslsal");
        //Debug.Log("am scos: " + GlobalVars.GetAction());
        if (_timer > _timeBetweenShots)
        {
            _timer = 0f;
            Vector2 dir = (playerTransform.position - new Vector3(0, 1.25f, 0) - enemy.transform.position).normalized;
            Rigidbody2D bullet = GameObject.Instantiate(Bullet, enemy.transform.position + new Vector3(0, 1f, 0), Quaternion.identity);
            bullet.velocity = dir * _bulletSpeed;
        }

        if (Vector2.Distance(playerTransform.position, enemy.transform.position) > _distanceToCountExit)
        {
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
}
