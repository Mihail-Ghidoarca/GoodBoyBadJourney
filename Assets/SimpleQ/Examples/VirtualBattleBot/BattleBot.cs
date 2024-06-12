using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using QLearning;
//For the brain to work, it needs:
//- States
//- Actions

//Brain Script needs to be found/attached

//Functions call sequence are: 
//1. brain.MakeAChoice(yourStateString)
//2. Then do whatever you are going to do, as this will return a choice string from the brain
//3. brain.UpdateRewards(rewardAmount, yourStatString) -- You state string should be altered by
//whatever was done above. This will update the reward, and save the new values to the brain
//4. brain.Save_Open_QBrain() to save the new information (if you call this too often, it will slow down the processes)

//In this example I am going to battle virtually with an enemy. And use multiple bots to train him
//In the inspector, I only entered one state, and four actions (0, 0 , 0)(Attack, Defend, Heal, Flee)
namespace QLearningExample
{

    public enum actionChoosen { Attack, Defend, Heal, Flee };
    public class FightActions
    {
        public string attack = "Attack";
        public string defend = "Defend";
        public string heal = "Heal";
        public string flee = "Flee";

    }

    public class FightersStatsClass
    {
        public int HP;
        public int MaxHP;
        public int MP;
        public int MaxMP;
        public int AttackPower;
        public int Defense;
    }

    public class FightInstance
    {
        public FightActions fight_actions;
        public FightersStatsClass Hero_Bot_Fighter;
        public FightersStatsClass Devil_Bot_Enemy;

        public actionChoosen action;

        //Addition variables for calculating rewards
        public int hero_HP_PreChoice;
        public int devil_HP_PreChoice;

        //For amount of turns bot has to get it done
        public int fightTurns;

        public bool botTraining;
        public bool roundFinished;
    }

    public class TrainingBots
    {
        public bool botIsTraining = false;
        public FightInstance fightInstance;
        public int botId;
    }

    public class BattleBot : MonoBehaviour
    {
        public OpenQLearningBrain battleBrain;
        public FightActions fight_actions = new FightActions();

        //Training Variables
        public int battlesAmount;
        public int heroAmount;
        private int battlesFinished;
        private int battlesWon;
        private int battlesLost;
        private int battlesFled;
        private int battlesTimerOut;

        public TextMeshProUGUI battleTrainingText;
        private bool training;

        private bool SingleFIght;

        private List<TrainingBots> trainingBots = new List<TrainingBots>();


        void Start() //Initialises the brain
        {

            if (!battleBrain.loaded) { battleBrain.InitializeQBrain(); }
        }

        public void FightButtonClick() // simulates a single battle
        {
            if (training) { return; }
            SingleFIght = true;
            trainingBots = new List<TrainingBots>();
            TrainingBots tb = new TrainingBots();
            tb.fightInstance = new FightInstance();
            tb.fightInstance = CreateNewFightInstance(ref tb.fightInstance);
            tb.botId = 0;
            trainingBots.Add(tb);

            StartCoroutine(SimulateABattle(tb.fightInstance, tb));
        }

        public void MultiFightTraining() // simulates multiple fights
        {
            //In case we are running the annealing, start a new count
            if (battleBrain.epsilonAnnealingDecay || battleBrain.temperatureAnnealingDecay) { battleBrain.ResetAnnealingEpisodeDecayCount(); }

            SingleFIght = false;
            StartCoroutine(SimulateTraining());
        }

        public IEnumerator SimulateTraining()
        {
            if (training) { yield break; }

            battleBrain.Save_Manager.sortedOut = false;

            training = true;

            battlesFinished = 0;
            battlesWon = 0;
            battlesLost = 0;
            battlesFled = 0;
            battlesTimerOut = 0;

            int battles = 0;

            trainingBots.Clear();
            trainingBots = new List<TrainingBots>();
            for (int i = 0; i < heroAmount; i++) { TrainingBots tb = new TrainingBots(); tb.fightInstance = new FightInstance(); tb.fightInstance = CreateNewFightInstance(ref tb.fightInstance); tb.botId = i; trainingBots.Add(tb); }

            int e = 0;

            while (battles < battlesAmount)
            {
                foreach (TrainingBots tb in trainingBots)
                {
                    if (!tb.fightInstance.botTraining)
                    {
                        tb.botIsTraining = true;
                        tb.fightInstance = CreateNewFightInstance(ref tb.fightInstance);
                        tb.fightInstance.botTraining = true;
                        //IEnumerator iE = SimulateABattle(tb.fightInstance, tb);
                        StartCoroutine(SimulateABattle(tb.fightInstance, tb));
                        e++;
                        battles++;
                    }
                }

                if (e >= heroAmount)
                {
                    foreach (TrainingBots tb in trainingBots) { tb.fightInstance.botTraining = false; }

                    e = 0;
                    yield return new WaitForSeconds(0.05f);
                }

                yield return null;
            }

            training = false;

            battleBrain.Save_Manager.sortedOut = true;

            //battleBrain.Save_Open_QBrain();

            Debug.Log("Battles: Won = " + battlesWon + " || Lost = " + battlesLost + " || Fled = " + battlesFled + " || Timed Out = " + battlesTimerOut);

            yield break;
        }

        public FightInstance CreateNewFightInstance(ref FightInstance fightInstance)
        {
            /* if (fightInstance == null) { fightInstance = new FightInstance(); }*/
            fightInstance = new FightInstance();

            //Set all the classes inside to new instances
            //if (fightInstance.Hero_Bot_Fighter == null) {fightInstance.Hero_Bot_Fighter = new FightersStatsClass(); }
            fightInstance.Hero_Bot_Fighter = new FightersStatsClass();
            //if (fightInstance.Devil_Bot_Enemy == null) { fightInstance.Devil_Bot_Enemy = new FightersStatsClass(); }
            fightInstance.Devil_Bot_Enemy = new FightersStatsClass();

            fightInstance.Hero_Bot_Fighter.HP = 10;
            fightInstance.Hero_Bot_Fighter.MaxHP = 10;
            fightInstance.Hero_Bot_Fighter.AttackPower = 3;
            fightInstance.Devil_Bot_Enemy.HP = 10;
            fightInstance.Devil_Bot_Enemy.MaxHP = 10;
            fightInstance.Devil_Bot_Enemy.AttackPower = 2;

            fightInstance.fightTurns = 10;
            fightInstance.botTraining = false;

            return fightInstance;

        }

        public IEnumerator SimulateABattle(FightInstance fightInstance, TrainingBots bots)
        {
            fightInstance.roundFinished = true;

            while (fightInstance.Hero_Bot_Fighter.HP > 0 && fightInstance.Devil_Bot_Enemy.HP > 0 && fightInstance.fightTurns > 0 && fightInstance.action != actionChoosen.Flee)
            {
                //Bool to check if the round has finished=
                if (fightInstance.roundFinished)
                {
                    fightInstance.roundFinished = false;
                    SimulateARound(ref fightInstance);
                }

                yield return null;
            }


            if (fightInstance.Hero_Bot_Fighter.HP <= 0)
            {
                //Debug.Log("Our hero has passed...");
                battlesWon++;
            }
            else if (fightInstance.Devil_Bot_Enemy.HP <= 0)
            {
                //Debug.Log("Devil was vanquished!!!");
                battlesLost++;
            }
            else if (fightInstance.action == actionChoosen.Flee)
            {
                //Debug.Log("Our hero fled!!!");
                battlesFled++;
            }
            else if (fightInstance.fightTurns <= 0)
            {
                //Debug.Log("We was not quick enough...");
                battlesTimerOut++;
            }

            //battleBrain.Save_Open_QBrain();

            yield return null;

            battlesFinished++;

            battleTrainingText.text = battlesFinished + " finished of " + battlesAmount;

            //In case we are running the annealing, update session counts
            if (battleBrain.epsilonAnnealingDecay || battleBrain.temperatureAnnealingDecay) { battleBrain.UpdateEpisodeForSessionAnnealingDecay(); }

            fightInstance.botTraining = false;
            foreach (TrainingBots tb in trainingBots)
            {
                if (tb.botId == bots.botId)
                {
                    tb.fightInstance = fightInstance;
                    tb.fightInstance.botTraining = false;
                    break;
                }
            }

            yield break;


        }

        public void SimulateARound(ref FightInstance fightInstance)
        {
            fightInstance.roundFinished = false;

            //Variables for capturing previous state
            fightInstance.hero_HP_PreChoice = fightInstance.Hero_Bot_Fighter.HP;
            fightInstance.devil_HP_PreChoice = fightInstance.Devil_Bot_Enemy.HP;

            //Insert the data we want to build our state with - in this case, I am going to use health
            //I will break the choice up with a " , "
            string currentState = fightInstance.fightTurns + " , " + fightInstance.Hero_Bot_Fighter.HP + " , " + fightInstance.Devil_Bot_Enemy.HP;
            //Call the Make a choice function from the battle brain
            string BotAction = battleBrain.MakeAChoice(currentState);

            //Debug.Log(BotAction);

            //Use the moves to calcualte the changes in health during the attacks
            HeroBotMove(BotAction, ref fightInstance);

            if (fightInstance.Devil_Bot_Enemy.HP <= 0)
            {
                fightInstance.Devil_Bot_Enemy.HP = 0;
            }
            else
            {
                DevilBotMove(ref fightInstance);
            }

            fightInstance.fightTurns -= 1;

            //Then by copying the above, we can update the string for the state we are currently in
            string newState = fightInstance.fightTurns + " , " + fightInstance.Hero_Bot_Fighter.HP + " , " + fightInstance.Devil_Bot_Enemy.HP;

            //New list abd class for our state
            List<AttributesPair<string, string>> kvp = new List<AttributesPair<string, string>>();

            State_Class sC = new State_Class();
            sC.stateString = newState;
            sC.state_attributes = kvp;

            //Calculate our rewards
            float reward = RewardForCurrentState(fightInstance);

            if (SingleFIght) { Debug.Log(fightInstance.action + " || " + fightInstance.Hero_Bot_Fighter.HP + " || " + fightInstance.Devil_Bot_Enemy.HP); }

            //And Update and save our rewards with our new state (for the bot to use in learning)(Old state is already gathered)
            battleBrain.UpdateReward(reward, sC);

            fightInstance.roundFinished = true;
        }


        public void HeroBotMove(string choosenAction, ref FightInstance fightInstance)
        {
            //Debug.Log(choosenAction);

            //Check which action choosen matches, and take our choosen action from there
            if (choosenAction == fight_actions.attack)
            {
                fightInstance.action = actionChoosen.Attack;
                fightInstance.Devil_Bot_Enemy.HP -= fightInstance.Hero_Bot_Fighter.AttackPower;

                //Debug.Log(fightInstance.Hero_Bot_Fighter.AttackPower);
                if (fightInstance.Devil_Bot_Enemy.HP < 0)
                {
                    fightInstance.Devil_Bot_Enemy.HP = 0;
                }
            }
            else if (choosenAction == fight_actions.defend)
            {
                fightInstance.action = actionChoosen.Defend;
            }
            else if (choosenAction == fight_actions.heal)
            {
                fightInstance.action = actionChoosen.Heal;
                int randomHeal = UnityEngine.Random.Range((int)(fightInstance.Devil_Bot_Enemy.AttackPower * 0.5f), 6);
                fightInstance.Hero_Bot_Fighter.HP += randomHeal;
                if (fightInstance.Hero_Bot_Fighter.HP > fightInstance.Hero_Bot_Fighter.MaxHP)
                {
                    fightInstance.Hero_Bot_Fighter.HP = fightInstance.Hero_Bot_Fighter.MaxHP;
                }
            }
            else if (choosenAction == fight_actions.flee)
            {
                fightInstance.action = actionChoosen.Flee;
            }
        }

        public void DevilBotMove(ref FightInstance fightInstance)
        {
            if (fightInstance.action == actionChoosen.Defend)
            {
                fightInstance.Hero_Bot_Fighter.HP -= 1;
                if (fightInstance.Hero_Bot_Fighter.HP < 0)
                {
                    fightInstance.Hero_Bot_Fighter.HP = 0;
                }
            }
            else if (fightInstance.action == actionChoosen.Flee)
            {
                //Do nothing
            }
            else
            {
                fightInstance.Hero_Bot_Fighter.HP -= fightInstance.Devil_Bot_Enemy.AttackPower;
                if (fightInstance.Hero_Bot_Fighter.HP < 0)
                {
                    fightInstance.Hero_Bot_Fighter.HP = 0;
                }
            }
        }

        public float RewardForCurrentState(FightInstance fightInstance)
        {

            //These are the outcomes of after making a choice (choosing an action)
            int pHp = fightInstance.Hero_Bot_Fighter.HP;
            int ePHP = fightInstance.Devil_Bot_Enemy.HP;
            actionChoosen act = fightInstance.action;
            //Debug.Log(act);

            if (pHp <= 0)
            {
                //Player Died
                if (act == actionChoosen.Flee)
                {
                    return -2f;
                }
                else if (act == actionChoosen.Defend)
                {
                    return 0.05f;
                }
                else if (act == actionChoosen.Heal)
                {
                    return 1f;
                }
                else if (act == actionChoosen.Attack)
                {
                    return 1.5f;
                }
                else
                { return -2f; }


            }
            else if (ePHP <= 0)
            {
                return 2f;
            }
            else
            {
                if (pHp > ePHP)
                {
                    if (act == actionChoosen.Attack)
                    {
                        return 1f;
                    }
                    else if (act == actionChoosen.Heal)
                    {
                        if ((fightInstance.hero_HP_PreChoice / fightInstance.Hero_Bot_Fighter.MaxHP) > 0.5f)
                        {
                            return -1f;
                        }
                        else
                        {
                            return -0.25f;
                        }

                    }
                    else if (act == actionChoosen.Flee)
                    {
                        return -2.2f;
                    }
                    else
                    {
                        return -1f;
                    }
                }
                else if (ePHP > pHp)
                {
                    if (act == actionChoosen.Attack)
                    {
                        return 1f;
                    }
                    else if (act == actionChoosen.Defend)
                    {
                        return -0.01f;
                    }
                    else if (act == actionChoosen.Heal)
                    {
                        if ((fightInstance.hero_HP_PreChoice / fightInstance.Hero_Bot_Fighter.MaxHP) > 0.5f)
                        {
                            return -1f;
                        }
                        else
                        {
                            return 0.01f;
                        }
                    }
                    else if (act == actionChoosen.Flee)
                    {
                        if ((pHp / fightInstance.Hero_Bot_Fighter.MaxHP) < 0.1f)
                        {
                            return -2f;
                        }
                        else
                        {
                            return -8f;
                        }
                    }
                    else
                    {
                        return 0.0f;
                    }

                }
                else
                {
                    if (act == actionChoosen.Attack)
                    {
                        return 1f;
                    }
                    else if (act == actionChoosen.Heal)
                    {
                        if ((fightInstance.hero_HP_PreChoice / fightInstance.Hero_Bot_Fighter.MaxHP) > 0.5f)
                        {
                            return -1f;
                        }
                        else
                        {
                            return 0.1f;
                        }
                    }
                    else if (act == actionChoosen.Flee)
                    {
                        return -2.1f;
                    }
                    else if (act == actionChoosen.Defend)
                    {
                        return -0.15f;
                    }
                    else
                    {
                        return 0.0f;
                    }
                }
            }


        }

    }

}
