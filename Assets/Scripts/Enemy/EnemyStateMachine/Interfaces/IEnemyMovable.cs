using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public interface IEnemyMovable
{
    Rigidbody2D RB { get; set; }

    bool isFacingRight { get; set; }

    void MoveEnemy(Vector2 velocity);
    void CheckForLeftOrRightFacing(Vector2 velocity);
}
