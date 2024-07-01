using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public interface ILurker
{
    public void ChangeAttackState(string attackState);
    public void ChangeChaseState(string chaseState);
    public void ChangeIdleState(string idleState);
}