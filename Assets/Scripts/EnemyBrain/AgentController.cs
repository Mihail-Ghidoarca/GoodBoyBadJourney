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
    public EnemyDamage enemyDamage;
    //public Lurker lurker;
    private float pStartingHealth = -1;
    private float timer;
    private float timeForRoundExec = 5f;

    private void Update()
    {
        timer += Time.deltaTime;
        if (timer > timeForRoundExec)
        {
            ExampleRound();
            timer = 0f;
        }
    }

    public State_Class GetMyState()
    {
        State_Class state = new State_Class();
        state.stateString = $"{playerHealth.currentHealth},{enemy.CurrentHealth},{enemy.transform.position},{playerHealth.transform.position}";

        Debug.Log(state.stateString);
        return state;
    }

    public float RewardForActions(float playerStartHealth)
    {
        float reward = 0f;

        // if the enemy has a higher percentage of HP than the player, reward gets improved
        if (((playerStartHealth - playerHealth.currentHealth) / playerHealth.maxHealth) / 
            ((enemy.MaxHealth - enemy.CurrentHealth) / enemy.MaxHealth) > 1)
        {
            reward += 1f;
        }

        // if the enemy has the same health percentage than the player, reward gets slighlty diminished
        if (((playerStartHealth - playerHealth.currentHealth) / playerHealth.maxHealth) /
            ((enemy.MaxHealth - enemy.CurrentHealth) / enemy.MaxHealth) == 1)
        {
            reward -= 0.25f; //
        }

        // if the enemy has a lower percentage of HP than the player, reward gets diminished
        else if (((playerStartHealth - playerHealth.currentHealth) / playerHealth.maxHealth) /
            ((enemy.MaxHealth - enemy.CurrentHealth) / enemy.MaxHealth) < 1)
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

    public void PlayerAction()
    {
        Debug.Log(GlobalVars.actionStack.Peek());
        
    }
    
    public void EnemyChooseAction(string choice)
    {
        if (choice == actions[0])
        {
            AgentAttack();
            //playerHealth.TakeDamage(enemyDamage.damage);
        }

        else if (choice == actions[1])
        {
            AgentWait();
        }
        else if (choice == actions[2])
        {
            AgentRun();
        }

    }


    private void SetPlayerStartingHealth(int playerCurrentHealth) 
    {
        if (pStartingHealth == -1)
        {
            pStartingHealth = playerCurrentHealth;
            return;
        }
        else
            return;
    }

    public void ExampleRound()
    {
        SetPlayerStartingHealth(playerHealth.currentHealth);
        currentState = GetMyState();
        string choice = myBrainScript.MakeAChoice(currentState.stateString);

        EnemyChooseAction(choice);
        PlayerAction();

        newState = GetMyState();

        float reward = RewardForActions(pStartingHealth);

        Debug.Log("Score is: " + reward);

        myBrainScript.UpdateReward(reward, newState);
    }

}