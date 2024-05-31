using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "Chase-Player Chase", menuName = "Enemy Logic/Chase Logic/Player Chase")]

public class EnemyChaseToPlayer : EnemyChaseSOBase
{

    private bool isGrounded;
    private bool shouldJump;
    private bool isPlayerAbove;
    [SerializeField] private float _movementSpeed = 3f;
    public float jumpForce = 2f;
    public LayerMask groundLayer;

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

        isGrounded = Physics2D.BoxCast(enemy.transform.position, enemy.coll.bounds.size, 0f, Vector2.down, groundLayer);
        Debug.Log(isGrounded);
        bool isPlayerAbove = Physics2D.BoxCast(enemy.transform.position, enemy.coll.bounds.size, 3f, Vector2.up, GameObject.FindGameObjectWithTag("Player").layer);
        
        Vector2 moveDirection = (playerTransform.position - enemy.transform.position).normalized;
        if (isGrounded)
        {
            enemy.MoveEnemy(moveDirection * _movementSpeed);

            RaycastHit2D groundInFront = Physics2D.BoxCast(enemy.transform.position, enemy.coll.bounds.size, 2f, new Vector2(moveDirection.x, 0), groundLayer);

            RaycastHit2D gapAhead = Physics2D.BoxCast(enemy.transform.position + new Vector3(moveDirection.x, 0, 0), enemy.coll.bounds.size, 2f, Vector2.down, groundLayer);

            RaycastHit2D platformAbove = Physics2D.BoxCast(enemy.transform.position, enemy.coll.bounds.size, 3f, Vector2.up, groundLayer);

            if (!groundInFront.collider && !gapAhead.collider)
            {
                shouldJump = true;
            }
            else if (isPlayerAbove && platformAbove.collider)
            {
                shouldJump = true;
            }
        }
        
        if(!enemy.IsAggroed)
        {
            enemy.StateMachine.ChangeState(enemy.IdleState);
        }
    }

    public override void DoPhysicsUpdateLogic()
    {
        base.DoPhysicsUpdateLogic();
        if(isGrounded && shouldJump)
        {
            shouldJump = false;
            Vector2 direction = (playerTransform.position - enemy.transform.position).normalized;
            Vector2 jumpDirection = direction * jumpForce;

            enemy.RB.AddForce(new Vector2(jumpDirection.x, jumpForce), ForceMode2D.Impulse);
        }
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
