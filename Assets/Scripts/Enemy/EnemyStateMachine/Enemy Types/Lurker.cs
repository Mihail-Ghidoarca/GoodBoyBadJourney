using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;


public class Lurker : Enemy, ILurker
{
    public List<EnemyAttackSOBase> Attacks = new();
    public List<EnemyChaseSOBase> Chases = new();
    public List<EnemyIdleSOBase> Idles = new();
    private EnemyAttackSOBase enemyAuxState;
    public void ChangeAttackState(string attackState)
    {
        if (attackState is "melee")
        {
            EnemyAttackBaseInstance = Instantiate(Attacks[0]);
            EnemyAttackBaseInstance.Initialize(gameObject, this);
        }
        else
        {
            EnemyAttackBaseInstance = Instantiate(Attacks[1]);
            EnemyAttackBaseInstance.Initialize(gameObject, this);
        }
    }

    public void ChangeChaseState(string chaseState) 
    {
        if (chaseState is "chase")
        {
            Debug.Log("SALUT4");            
            EnemyChaseBaseInstance = Instantiate(Chases[0]);
            EnemyChaseBaseInstance.Initialize(gameObject, this);
        }
        else
        {
            Debug.Log("SALUT5");
            EnemyChaseBaseInstance = Instantiate(Chases[1]);
            EnemyChaseBaseInstance.Initialize(gameObject, this);
        }
    }

    public void ChangeIdleState(string idleState)
    {
        if (idleState is "patrol")
        {
            Debug.Log("SALUT1");
            EnemyIdleBaseInstance = Instantiate(Idles[0]);
            EnemyIdleBaseInstance.Initialize(gameObject, this);
        }
        else
        {
            Debug.Log("SALUT2");
            EnemyIdleBaseInstance = Instantiate(Idles[1]);
            EnemyIdleBaseInstance.Initialize(gameObject, this);
        }
    }
}
