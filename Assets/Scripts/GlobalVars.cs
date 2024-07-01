using JetBrains.Annotations;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Random = System.Random;
public static class GlobalVars
{
    public static Stack<PlayerAction> actionStack = new Stack<PlayerAction>(); 

    private static Random random = new Random();

    private const int maxActionsInStack = 1;
    
    public static void AddToActionArray(PlayerAction actionCode)
    {
        PlayerAction lastAction;
        if (actionStack.TryPeek(out lastAction))
        {
            actionStack.Pop();
            actionStack.Push(actionCode);
        }
        else
            actionStack.Push(actionCode);
    }
}
