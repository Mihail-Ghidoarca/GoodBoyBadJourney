using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "Chase-Player Chase", menuName = "Enemy Logic/Chase Logic/Player Chase")]

public class EnemyChaseToPlayer : EnemyChaseSOBase
{
    [SerializeField] private float _movementSpeed = 3f;

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
        Debug.Log("chasechasechase");
        Vector2 moveDirection = (playerTransform.position - enemy.transform.position).normalized;

        enemy.MoveEnemy(moveDirection * _movementSpeed);
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
