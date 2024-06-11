using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using QLearning;

namespace QLearningExample
{

    public class AgentController : MonoBehaviour
    {
        //Instance of our brain
        public OpenQLearningBrain ourBrainScript;

        //List of our actions - OPTIONAL
        public List<string> actions = new List<string>()
    {
        "Attack",
        "Defend",
        "Flee"
    };

        //State Classes to save us time creating creating new vars every time we use them - OPTIONAL
        private State_Class currentState;
        private State_Class newState;

        //Variables for health
        private int playerHealth;
        private int enemyHealth;

        //Amount of sessions
        public int amountOfSessions;
        private int sessionsSoFar;

        public IEnumerator RunTrainingSessions()
        {
            //Reset any variables first
            sessionsSoFar = 0;
            ResetHealth();

            //For Annealing Decay
            ourBrainScript.ResetAnnealingEpisodeDecayCount();

            //While we have yet to meet the numbe of sessions, keep checking health and calling the battle round
            while (sessionsSoFar < amountOfSessions)
            {
                if (playerHealth <= 0 || enemyHealth <= 0)
                {
                    //Update our run for a new session
                    ResetHealth();

                    //Update session loop 
                    sessionsSoFar++;

                    //For Annealing Decay
                    ourBrainScript.UpdateEpisodeForSessionAnnealingDecay();

                    //For Episode Decay
                    ourBrainScript.ResetDecayInteractionCounter();

                }
                else
                {
                    //Continue to call our attack loop
                    ExampleRound();
                }

                yield return new WaitForSeconds(0.1f);
            }

            //OPTIONAL - save when the whole session has finished
            ourBrainScript.Save_Open_QBrain();


            yield break;
        }

        public void ExampleRound()
        {
            //Get our current state using the function we created
            currentState = GetOurState();

            //Get a choice string from the brain by feeding in the state (or in this example, only the string)
            string choice = ourBrainScript.MakeAChoice(currentState.stateString);

            //Our Code for acting
            if (choice == actions[0])
            {
                //Attack
                AgentAttack();
            }
            else if (choice == actions[1])
            {
                //Defend
                AgentDefend();
            }
            else if (choice == actions[2])
            {
                //Fless
                AgentFlee();
            }

            //Of course our enemy would also make his move next
            EnemyAttack();

            //Now we use the get state function to create our new state after everything has been donw
            newState = GetOurState();

            //We get our reward for the outcome
            float reward = RewardForActions();

            //And update our Rewards (Updating our Exeperiences) with the new state and our reward
            ourBrainScript.UpdateReward(reward, newState);

            //And that is a single training loop which can be looped again and again to train the agent
        }

        //Function fore returning our current class
        public State_Class GetOurState()
        {
            State_Class state = new State_Class();

            //Update the string state according to how we want the data laid out
            state.stateString = playerHealth + "," + enemyHealth;


            return state;
        }

        //Function for getting our reward 
        public float RewardForActions()
        {
            //We are using the player and enemy alive state for this reward
            //However, we could infact use any variables for rewards
            //Health before and after a choice could be tracked e.t.c

            float reward = 0f;

            if (playerHealth <= 0)
            {
                //Player died
                reward -= 1f;
            }
            else if (enemyHealth <= 0)
            {
                //Enemy died
                reward += 1f;
            }

            return reward;
        }

        //Function for resetting health parameters
        public void ResetHealth()
        {
            playerHealth = 10;
            enemyHealth = 10;
        }

        //Example Functions
        public void AgentAttack()
        {
            //Attack logic (or anims even)
        }
        public void AgentDefend()
        {
            //Defend Logic
        }
        public void AgentFlee()
        {
            //Flee logic
        }
        public void EnemyAttack()
        {
            //Enemy Attack Logic
        }

    }

}
