using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEditor.U2D;
using UnityEngine;


public class Lurker : Enemy
{
    [SerializeField] private EnemyAttackSOBase EnemyMeleeAttackBase;
    [SerializeField] private EnemyAttackSOBase EnemyRangedAttackBase;

    private void ChangeAttackState()
    {
        EnemyAttackBaseInstance.Initialize(gameObject, this);
    }
}
