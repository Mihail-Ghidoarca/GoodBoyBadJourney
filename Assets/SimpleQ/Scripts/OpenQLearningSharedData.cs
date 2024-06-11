using System.Collections;
using System.Collections.Concurrent;
using System.Collections.Generic;
using UnityEngine;
using QLearning;

public class OpenQLearningSharedData : MonoBehaviour
{
    //Shared Open Q Brain
    public ConcurrentDictionary<State_Class, ConcurrentDictionary<string, float>> Shared_Open_QBRain = new ConcurrentDictionary<State_Class, ConcurrentDictionary<string, float>>();

    //Shared Replay Buffer
    public ConcurrentDictionary<Replay_Buffer_Key, Replay_Buffer_Values> Shared_Replay_Buffer = new ConcurrentDictionary<Replay_Buffer_Key, Replay_Buffer_Values>();

    //A bool to see if the Update Q Table function has already been called by someone lately
    [HideInInspector]public bool QTableUpdateCalled;

    [HideInInspector] public bool dataLoadedYet = false;

    //Shared data Update Called Flag Coroutine - for retunring the flag to false after a few moments
    public IEnumerator UpdateQTableCalledFlag()
    {
        //Give the Update a three second cool off too ensure no bottle necks
        yield return new WaitForSeconds(3f);
        //Set the flag back to false
        QTableUpdateCalled = false;

        yield break;
    }
}