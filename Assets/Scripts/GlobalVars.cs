using JetBrains.Annotations;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Random = System.Random;
public static class GlobalVars
{
    public static Queue<PlayerAction> actionQueue = new Queue<PlayerAction>(); 

    private static Random random = new Random();

    private const int maxActionsInQueue = 150;

    public static void AddToActionArray(PlayerAction actionCode)
    {
        //Debug.Log(actionQueue.Count);
        if(actionQueue.Count == maxActionsInQueue)
        {
            var lastAction = actionQueue.Dequeue();
            actionCounter[(int)lastAction]--;
        }
        actionQueue.Enqueue(actionCode);
        actionCounter[(int)actionCode]++;

    }

    public static int[] actionCounter = new int[5];

    public static PlayerAction? GetAction()
    {
        if(actionQueue.Count >= maxActionsInQueue)
        {
            var actionToBeCompared = random.Next(maxActionsInQueue);

            int sum = 0;

            for (int i = 0; i < actionCounter.Length; i++)
            {
                sum += actionCounter[i];
                if(actionToBeCompared < sum)
                {
                    return (PlayerAction)i;
                }
            }
        }

        return null;
    }
}
