using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "Idle-Random Patrol", menuName = "Enemy Logic/Idle Logic/Random Patrol")]

public class EnemyIdleRandomPatrol : EnemyIdleSOBase
{
    [SerializeField]public float RandomMovementRange = 5f;
    [SerializeField]public float RandomMovementSpeed = 1f;
    
    private Vector3 _targetPos;
    private Vector3 _direction;

    public override void DoAnimationTriggerEventLogic(Enemy.AnimationTriggerType triggerType)
    {
        base.DoAnimationTriggerEventLogic(triggerType);
    }

    public override void DoEnterLogic()
    {
        base.DoEnterLogic();
        _targetPos = GetRandomXPoint();

    }

    public override void DoExitLogic()
    {
        base.DoExitLogic();
        
    }

    public override void DoFrameUpdateLogic()
    {
        base.DoFrameUpdateLogic();
        _direction = (_targetPos - enemy.transform.position).normalized;
        enemy.MoveEnemy(_direction * RandomMovementSpeed);
        
        if ((enemy.transform.position - _targetPos).sqrMagnitude < 0.01f)
        {
            _targetPos = GetRandomXPoint();
        }
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

    private Vector3 GetRandomXPoint()
    {
        float randomOffsetX = UnityEngine.Random.Range(-RandomMovementRange, RandomMovementRange);

        Vector3 newPosition = enemy.transform.position;
        newPosition.x += randomOffsetX;

        return newPosition;
    }
}
