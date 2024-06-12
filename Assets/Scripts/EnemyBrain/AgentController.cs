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
        "Defend",
        "Run"
    };

    private State_Class currentState;
    private State_Class newState;

    public PlayerHealth playerHealth;
    private int enemyHealth;

    public Lurker lurker;

    public State_Class GetMyState()
    {
        State_Class state = new State_Class();

        playerHealth.maxHealth = 60;
        enemyHealth = 40;
        state.stateString = playerHealth.currentHealth + "," + enemyHealth;


        return state;
    }

    public float RewardForActions()
    {
        float reward = 0f;

        if(playerHealth.currentHealth <= 0)
        {
            //Player died
            reward -= 1f;
        }
        else if(enemyHealth <= 0)
        {
            reward += 1f;
        }

        return reward;
    }

    public void AgentAttack()
    {
        Debug.Log("Agent attacked");
        playerHealth.currentHealth -= 20;
    }

    public void AgentDefend()
    {
        Debug.Log("Agent defended");
    }

    public void AgentRun()
    {
        Debug.Log("Agent ran");
        enemyHealth -= 10;
    }

    public void EnemyAttack()
    {
        Debug.Log("Player attacked back");
        enemyHealth -= 20;
    }

    public void InitBrain()
    {

    }

    public void ExampleRound()
    {
        Debug.Log("AAAAAAAAAAAAAAAAAA");
        currentState = GetMyState();

        string choice = myBrainScript.MakeAChoice(currentState.stateString);

        if(choice == actions[0])
        {
            AgentAttack();
        }

        else if (choice == actions[1])
        {
            AgentDefend();
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
