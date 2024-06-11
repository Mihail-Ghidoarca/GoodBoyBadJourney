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
        "Flee"
    };

    private State_Class currentState;
    private State_Class newState;

    private PlayerHealth playerHealth;
    private int enemyHealth;

    public State_Class GetMyState()
    {
        State_Class state = new State_Class();

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

    }

    public void AgentDefend()
    {

    }

    public void AgentRun()
    {

    }

    public void EnemyAttack()
    {

    }

    public void ExampleRound()
    {
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

        myBrainScript.UpdateReward(reward, newState);
    }

}
