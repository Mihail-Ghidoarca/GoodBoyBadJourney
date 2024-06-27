using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using QLearning;
using Unity.VisualScripting;

public class AgentController : MonoBehaviour
{
    public OpenQLearningBrain myBrainScript;

    public List<string> actions = new List<string>()
    {
        "Attack",
        "Run",
        "Wait"
    };

    private State_Class currentState;
    private State_Class newState;

    public PlayerHealth playerHealth;
    public Enemy enemy;
    //public Lurker lurker;

    private void Update()
    {
        Invoke("ExampleRound", 5);
    }

    public State_Class GetMyState()
    {
        State_Class state = new State_Class();
        state.stateString = $"{playerHealth.currentHealth},{enemy.CurrentHealth},{enemy.transform.position},{playerHealth.transform.position}";

        Debug.Log(state.stateString);
        return state;
    }

    public float RewardForActions()
    {
        float reward = 0f;

        if (playerHealth.currentHealth <= 0)
        {
            //Player died
            reward += 1f;
        }
        else if (enemy.CurrentHealth <= 0)
        {
            reward -= 1f;
        }

        return reward;
    }

    public void AgentAttack()
    {
        Debug.Log("Agent Attacked");
    }

    public void AgentWait()
    {
        Debug.Log("Agent Waited");
    }

    public void AgentRun()
    {
        Debug.Log("Agent ran");
        //enemy.CurrentHealth -= 10;
    }

    public void EnemyAttack()
    {
        Debug.Log("Player attacked back");
        //enemy.CurrentHealth -= 20;
    }

    public void ExampleRound()
    {
        currentState = GetMyState();
        string choice = myBrainScript.MakeAChoice(currentState.stateString);

        if (choice == actions[0])
        {
            AgentAttack();
        }

        else if (choice == actions[1])
        {
            AgentWait();
        }
        else if (choice != actions[2])
        {
            AgentRun();
        }

        EnemyAttack();

        newState = GetMyState();

        float reward = RewardForActions();

        Debug.Log("Score is: " + reward);

        myBrainScript.UpdateReward(reward, newState);
    }

}