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
    private float pStartingHealth = -1;
    private float timer;
    private float timeForRoundExec = 5f;
    private float distanceToPlayer;
    private void Start()
    {
        enemy = GetComponent<Enemy>();
        if (!myBrainScript.loaded) { myBrainScript.InitializeQBrain(); }
    }

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

        // if the enemy has the same health percentage than the player, reward gets slightly diminished
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
        if (enemy is ILurker lurkerEnemy)
        {
            Debug.Log(distanceToPlayer);
            if (distanceToPlayer <= 5f)
            {
                lurkerEnemy.ChangeAttackState("melee");

            }
            else
            {
                lurkerEnemy.ChangeAttackState("ranged");
            }
        }
    }

    public void AgentWait()
    {
        Debug.Log("Agent Waited");
        
    }

    public void AgentRun()
    {
        Debug.Log("Agent ran");
        GetDistanceToPlayer();
        if (enemy is ILurker lurkerEnemy)
        {
            if (distanceToPlayer <= 5f)
            {
                lurkerEnemy.ChangeChaseState("chase");
            }
            else
            {
                lurkerEnemy.ChangeChaseState("run");
            }
        }

    }

    public string PlayerAction()
    {
        return GlobalVars.actionStack.Peek().ToString();
    }

    public void EnemyChooseAction(string choice)
    {
        if (choice == actions[0])
        {
            AgentAttack();
        }

        else if (choice == actions[1])
        {
            AgentRun();
        }
        else if (choice == actions[2])
        {
            AgentWait();
        }
        else
        {
            Debug.Log("choice is " + choice);
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

    private void GetDistanceToPlayer()
    {
        distanceToPlayer = Mathf.Abs(enemy.transform.position.x - playerHealth.gameObject.transform.position.x);
    }

    private void EnemyMakeChoice(string choice)
    {
        if(PlayerAction() == "Jump")
        {
            EnemyChooseAction("Wait");
        }
        else if(PlayerAction() == "MeleeAttack")
        {
            EnemyChooseAction("Attack");
        }
        else
        {
            EnemyChooseAction("Run");
        }
    }

    public void ExampleRound()
    {
        SetPlayerStartingHealth(playerHealth.currentHealth);
        currentState = GetMyState();
        string choice = myBrainScript.MakeAChoice(currentState.stateString);

        EnemyMakeChoice(choice);
        PlayerAction();
        //EnemyChooseAction(choice);

        newState = GetMyState();

        float reward = RewardForActions(pStartingHealth);

        Debug.Log("Score is: " + reward);

        myBrainScript.UpdateReward(reward, newState);
        myBrainScript.Save_Open_QBrain();
    }

}
