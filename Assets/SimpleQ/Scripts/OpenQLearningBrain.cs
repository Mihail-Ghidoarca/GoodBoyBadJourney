using System.Collections;
using System.Collections.Generic;
using System.Collections.Concurrent;
using System.IO;
using UnityEngine;
using System.Threading.Tasks;
using Random = System.Random;
using System.Linq;
using System;
using UnityEditor;
using System.Text;
using QLearning;

//Open Q is a an asset for building ML models. It is free to use, and a learning tool to help you gain a better understanding on Machine Learning and AI
//Feel free to change, adapt, and add to the code to suit your needs. Or use as is, and build a model with a brain ready to be grow and learn.


//States can be co-ordinates, health, wealth, exp and so much more!!!
//Their attributes can be all the above, plus!!!
//And their actions, can be adjusted soley for them

//Examples:
//Co-ordinates can be saved as "x , y" (or "x, y"). Your choice, as long as you inputting them the same
//the bot will learn


//States as strings
//If it were set simple to ints, it would likely be easier to adapt to most code
//However, if we were to add more variables, then we would have to expand the (e.g) int, into 4 ints for playerHP, enemyHp, defense e.t.c
//In this sceneario, you select the variables you wish, and put them in a string format, to be compared to other such things.
//Following the example above, when an enemy attacks, we could covert the result into a single string of (e.g) "58, 70, 10, 12)

//WE use System Random, the static unity version is slightly quicker
//Howeever, in cases where we are using random Init states for seed
//generation, we would be affecting the models learning, as any randomness 
//would be the smae within the same seed, making it a loop training session where everything is the same


//Playback Buffer
//Our playback buffer contains:
//State (s): The state of the environment at a specific time step.
//Action(a): The action taken by the agent.
//Reward (r): The reward received by the agent for taking the action.
//Next State (s'): The state of the environment after the action was taken.
//Done (d): A flag indicating whether the episode ended after taking the action.

//The Replay BUffer container uses a state and action as the key, with the rest
//of the experience (Replay_Buffer_Values) used as the values

//The playback buffer can also used to help better dynamically calculate exploration vs expoliation values


//Prioritized Learning - Sample Batch Selection, allows you to specify what data the 
//model is focusing on. This can be changed in the inspector, or through script.

//E.g. Map = State. Positions = Attributes. We only want to learn this map, so in the code
//we set our stateStringReplayBufferString to our current Map, and only pull out experiences of 
//that map, using the stateStringReplayBuffer



//Main Script (Class) for monobehaviour inheritance
public class OpenQLearningBrain : MonoBehaviour
{

    [Header("Save File Path")]
    //The save directory for our new brain
    public string brainsSaveDirectory = "OpenBotBrain.brain";

    [SerializeField]
    public List<AttributesPair<string, string>> sn = new List<AttributesPair<string, string>>();

    //[Header("States and Actions List")]
    //The list to update in inspector
    [SerializeField] public List<Open_QBrain_Class> State_And_Actions = new List<Open_QBrain_Class>();

    //The qtable to contain our brain (Key = State, Value = Dictionary(Key = Action, Value = Q-Value)
    [SerializeField]
    public ConcurrentDictionary<State_Class, ConcurrentDictionary<string, float>> Open_QBrain_Container = new ConcurrentDictionary<State_Class, ConcurrentDictionary<string, float>>();

    //The replay buffer dictionary
    [SerializeField]
    public ConcurrentDictionary<Replay_Buffer_Key, Replay_Buffer_Values> Open_QBrain_Replay_Buffer = new ConcurrentDictionary<Replay_Buffer_Key, Replay_Buffer_Values>();

    //For our Rewards
    [HideInInspector] public float rewardGranted;
    private float minRewardSeen;
    private float maxRewardSeen;

    //States And Actions Capturing
    private State_Class old_state;
    private State_Class new_state;
    private string action_Choosen;


    //Variables for learning
    //[Header("Q-Learning Values")]
    [Header("How Quickly the Bot Adapts")] public float learningRate = 0.1f;
    [Header("The Importance of Future Rewards")] public float dicountFactor = 0.9f;
    [Header("Choices Between Learning Experience")] public int choicesBetweenBatchLearning = 200;
    private int choices_taken_count_batch_learning;
    [Header("Replay Batch Sample Amount")] public int replayBatchSampleSize = 100;

    //Loaded - a bool check to see if we have already loaded up the brain
    [HideInInspector] public bool loaded = false;

    //Our dave manager variable - initialized properly in the Inirialise Brain function
    public OpenQLearningBrainSaveManager Save_Manager = new OpenQLearningBrainSaveManager();

    //Actions we may not want to take
    [HideInInspector] public List<string> actions_not_to_take = new List<string>();
    [HideInInspector] public bool dontIncludeActions = false;


    #region Exploration vs Exploitation 

    //Four our exploration vs exploitation calucaltions - two choices
    //[Header("Exploration vs Exploitation")]


    /// <summary>
    /// Greedy-Epsilon Exploration
    /// </summary>

    [Header("Greedy-Epsilon Exploration")]
    public bool Greedy_Epsilon = true;
    [Tooltip("Exploration vs explotation rate")] public float epsilon = 0.1f;

    public bool dynamicEpsilon = false;
    //A higher value leads to faster decay and vice versa
    [Tooltip("How quickly the epsilon rate drops")] public float epsilonDecayRate = 0.005f;

    //Epsilon Annealing Decay Variable
    public bool epsilonAnnealingDecay = false;
    [Tooltip("Total Session Variable")] public int epsilonDynamicDecayTotalSessionValue;
    public float epsilonStart = 1.0f;
    public float epsilonEnd = 0.1f;

    //Epsilon Episode Decay Variable
    public bool epsilonEpisodeDecay = false;
    [Tooltip("Total Interactions in an Episode")] public int epsilonDynamicEpisode_InteractionsAmount = 0;

    //Epsilon Episode Experience Variable
    public bool epsilonExperienceFluctuations = false;
    public float ageWeight_Epsilon = 0.5f; // This is how much affect the age has on the experience (between 0-1)

    /// <summary>
    /// Boltzmann Exploration
    /// </summary>

    [Header("Boltzmann Exploration")]
    public bool Boltzmann_Exploration = false;
    [Tooltip("Exploration vs explotation rate")] public float temperature = 1f;

    //Dynamic Temperature options and variables
    public bool dynamicTemperature = true;

    //Temperature Annealing Decay Variables
    public bool temperatureAnnealingDecay = true;
    [Tooltip("Total Session Variable")] public int temperatureDynamicDecayTotalSessionValue = 0;
    public float temperatureStart = 1.0f;
    public float temperatureEnd = 0.1f;


    public bool temperatureEpisodeDecay = false;
    [Tooltip("Total Interactions in an Episode")] public int temperatureDynamic_EpisodeInteractionsAmount = 0;
    [Tooltip("Rate at which it will decay")] public float temperatureDecayRate = 0.1f;

    public bool temperatureExperienceFluctuations = false;
    public float ageWeight_Temperature = 0.5f; // This is how much affect the age has on the experience (between 0-1)

    //Joint variables both functions share
    private int decayAnnealing_current_episode_counter = 0; //For the amount of episodes this session
    private int decayEpisode_current_action_counter = 0; //For the amount of interaction taken in the episode
    //This list allows us to use the current data, and update it only when we have updated the qtable
    private List<KeyValuePair<Replay_Buffer_Key, Replay_Buffer_Values>> experienceFluctationExeperiences = new List<KeyValuePair<Replay_Buffer_Key, Replay_Buffer_Values>>();


    #endregion


   //Prioritised Learning Variables - Replay Buffer Sample Selection

   [Header("Random Experiences")] public bool randomReplayBuffer = true;
    [Header("State String Experience")] public bool stateStringReplayBuffer;
    public string stateStringReplayBufferString;
    [Header("Attribute Experience")] public bool singleAttributeReplayBuffer;
    public string singleAttributeReplayBufferKey;
    public string singleAttributeReplayBufferValue;

    //Replay Buffer Removal Policy Variables
    [Header("Replay Buffer Max Experiences Size")] public int ReplayBufferSize = 10000;
    [Header("Experience Amount to Remove")]public int amountOfExperiencesToRemove = 1500;
    [Header("Using Experience ID Sorted")] public bool experienceRemovalIdSorted = false;
    [Header("Random Experiences Removed")] public bool RB_RandomRemoval = true;
    [Header("Older Experiences Removed First")] public bool RB_FirstIn_FirstOut;
    [Header("Newer Experiences Removed First")] public bool RB_LastIn_FirstOut;
    [Header("Experiences With Specific Experiences")]public bool RB_Prioritised_AttributeRemoval;
    public string RB_Prioritised_AttributeRemovalKey;
    public string RB_Prioritised_AttributeRemovalValue;



    //Sorting the output will ensure its order, but will slow down the runs if save is called too often.
    [Header("Output Sorted")]
    public bool sortedOutput = false;


    //Shared Data 
    //[Header("Shared Data Variables")]
    public bool sharedData = false;
    //The instance of the shared data for the brain to use
    [SerializeField]public OpenQLearningSharedData sharedDataSource;
    


    //Functions for initalizing the q brain

    // Start is called before the first frame update
    /*public void Start()
    {
        //If you wish to initialize from the start
        if (!loaded) { InitializeQBrain(); }
    }*/

    //Function for initalizing the brain and replay buffer
    public void InitializeQBrain()
    {
        //Set out containers reference to null, if reinitializing, this resets their reference (which might be the shared reference)
        Open_QBrain_Container = new ConcurrentDictionary<State_Class, ConcurrentDictionary<string, float>>();
        Open_QBrain_Replay_Buffer = new ConcurrentDictionary<Replay_Buffer_Key, Replay_Buffer_Values>();

        //Check if we are using shared data and reset the load bool
        if (sharedData) { sharedDataSource.dataLoadedYet = false; }

        //Set our path variable
        Save_Manager.pathDir = brainsSaveDirectory;
        //Set our path if we haven't already
        if (Save_Manager.path == null)
        {
            Save_Manager.path = Path.Combine(Application.persistentDataPath, Save_Manager.pathDir);
        }

        //If our states and actions list is empty (fine) if we have a brain ready to load up, else it will return an error
        if (State_And_Actions.Count <= 0 && !File.Exists(Save_Manager.path))
        {
            Debug.LogError("Please ensure their are States and Actions. Or a brain file path set.");
            return;
        }

        //If we do have an exisiting brain, load it up
        if(File.Exists(Save_Manager.path))
        {
            if(sharedData && !sharedDataSource.dataLoadedYet)
            {
                //Set the bool to true
                sharedDataSource.dataLoadedYet = true;
                //Load up our exisiting brain (q-table)
                Save_Manager.LoadOQLBrain(ref Open_QBrain_Container);
                //Load up or replay buffer experiences
                Save_Manager.LoadOQLBReplay(ref Open_QBrain_Replay_Buffer);

            }
            
            if(!sharedData)
            {
                //Load up our exisiting brain (q-table)
                Save_Manager.LoadOQLBrain(ref Open_QBrain_Container);
                //Load up or replay buffer experiences
                Save_Manager.LoadOQLBReplay(ref Open_QBrain_Replay_Buffer);
            }

        }
        else
        {
            //Initialize our brain for the first time, and save the newly intialized brain
            InitialzeANewOpenBrain();
        }

        //Initalize the max and min rewards seen for exploration decay
        minRewardSeen = float.MaxValue;
        maxRewardSeen = float.MinValue;

        //Set our save managers sorted function
        Save_Manager.sortedOut = sortedOutput;

        //If our data is going to be shared be several separate instances of this brain
        if (sharedData)
        {
            //Check the shared data component has been added to an over-seer object - or set false and back out
            if(sharedDataSource == null) { Debug.LogError("You are trying to share data with no Shared Data Component. Individual Learning has been activated"); sharedData = false; loaded = true; return; }

            //If our brain (loaded data) has entries, and our shared has not been populated yet
            if(Open_QBrain_Container.Count > 0 && sharedDataSource.Shared_Open_QBRain.Count <= 0)
            {
                //Clear the list to ensure they are empty and ready
                sharedDataSource.Shared_Open_QBRain.Clear();
                sharedDataSource.Shared_Replay_Buffer.Clear();

                //Debug Check to see that only one bot is calling this
                Debug.Log("I called this.");

                //Populate our Shared Brain Container (QTable)
                foreach(var kvp in Open_QBrain_Container)
                {
                    sharedDataSource.Shared_Open_QBRain.TryAdd(kvp.Key, kvp.Value);
                }

                //Populate our Shared Replay Buffer (Experiences)
                foreach (var kvp in Open_QBrain_Replay_Buffer)
                {
                    sharedDataSource.Shared_Replay_Buffer.TryAdd(kvp.Key, kvp.Value);
                }
            }

            //Assign our containers to reference the shared containers
            Open_QBrain_Container = sharedDataSource.Shared_Open_QBRain;
            Open_QBrain_Replay_Buffer = sharedDataSource.Shared_Replay_Buffer;

            //Ensure our save function bool is false
            sharedDataSource.QTableUpdateCalled = false;
        }
        else
        {
            //Clear shared lists if present and no longer using
            if (sharedDataSource != null && sharedDataSource.Shared_Open_QBRain.Count > 0) { sharedDataSource.Shared_Open_QBRain.Clear(); sharedDataSource.Shared_Replay_Buffer.Clear();}
        }

        /*foreach(var o in Open_QBrain_Container)
        {
            Debug.Log(o.Key);
        }*/

        //Set our bool to true
        loaded = true;
    }

    //Function for initalizing a new brain (IF YOU HAVE A BRAIN, THIS WILL WIPE IT CLEAN WITH A FRESH ONE).
    public void InitialzeANewOpenBrain()
    {
        //Ensure brain is clear
        Open_QBrain_Container.Clear();

        //A temp dictionary for our new values
        ConcurrentDictionary<string, float> new_action_qValues = new ConcurrentDictionary<string, float>();

        //Loop through our states and actions list
        foreach(Open_QBrain_Class s_AQ in State_And_Actions)
        {
            //Get our state string
            string state = s_AQ.State.stateString;
            //And it's attributes (if any)
            List<AttributesPair<string, string>> kvp = s_AQ.State.state_attributes;
            if(kvp == null) { kvp = new List<AttributesPair<string, string>>(); }

            //And create our state class
            State_Class sC = new State_Class();
            sC.stateString = state;
            sC.state_attributes = kvp;


            //Get all the actions associated, and set their qValue to 0.0f - adding to our temp dicitonary
            new_action_qValues.Clear();
            foreach(string action in s_AQ.Actions)
            {
                new_action_qValues.TryAdd(action, 0.0f);
            }

            //Add values to open brain 
            Open_QBrain_Container.TryAdd(sC, new_action_qValues);


        }

        //Save our newly built brain
        Save_Manager.SaveOQLBrain(Open_QBrain_Container);

    }




    //Functions to be called by the user's bot script (you reading and using this, you are the user)

    //Function for making a choice
    public string MakeAChoice(string stateString, List<AttributesPair<string, string>> attributes = null)
    {
        //Set our reward variable back to 0

        //If we don't have a state yet
        //Not sure if we should do anything here
        if(attributes == null) { attributes = new List<AttributesPair<string, string>>();}

        //Create our new state class
        State_Class state = new State_Class();
        state.stateString = stateString;
        state.state_attributes = attributes;


        //Get our curret state (we use the Get Current in case it is a new state)
        old_state = GetCurrentState(state);

        //Choose an action
        action_Choosen = ChooseAnAction(state);

        //Counter for our steps for the batch learning
        choices_taken_count_batch_learning++;
        //Counter for our current step Count for Episode dcay
        if(temperatureEpisodeDecay || epsilonEpisodeDecay) { decayEpisode_current_action_counter++; }


        return action_Choosen;
    }

    //For Updating our reward, and our qTable
    public void UpdateReward(float rewarded, State_Class currentState, bool done = false)
    {
        //Update our reward variable
        rewardGranted = 0;
        rewardGranted += rewarded;

        //Update our state variable
        new_state = GetCurrentState(currentState);

        //Update our experiences - replay buffer
        AddExperienceToReplayBuffer(currentState, rewardGranted, done);

        //Update min and max rewards seen
        if (rewarded < minRewardSeen) { minRewardSeen = rewarded; }
        if (rewardGranted > maxRewardSeen) { maxRewardSeen = rewardGranted; }

        //And Update our qTable variable - if the steps take match or are more
        if (choices_taken_count_batch_learning >= choicesBetweenBatchLearning)
        {
            //Do a check for shared learning, to ensure we are not overcalling this message close together
            if(!sharedData)
            {
                UpdateQTable();
            }
            else if(sharedData)
            {
                //If now has call the update lately
                if(!sharedDataSource.QTableUpdateCalled)
                {
                    //Set it to called
                    sharedDataSource.QTableUpdateCalled = true;
                    //Update the table
                    UpdateQTable();
                    //Start the countdown until it can be called again
                    StartCoroutine(sharedDataSource.UpdateQTableCalledFlag());
                }
            }
            //Reset the count
            choices_taken_count_batch_learning = 0;
            //Debug.Log(Open_QBrain_Replay_Buffer.Count);
        }

        //For dynamically updating our exploration vs exploitation rates
        if (Greedy_Epsilon && dynamicEpsilon)
        {
            //Greedy epsilon uses the rewards of the state/actions and actions taken to dynamically adjust the epsilon rate
            //These are some of the basic implementations
            if(epsilonExperienceFluctuations) { Epsilon_Dynamic_Experience_Decay(); }
            else if(epsilonAnnealingDecay) { Epsilon_Dynamic_Annealing_Decay(); }
            else if(epsilonEpisodeDecay) { Epsilon_Dynamic_Episode_Decay(); }

        }
        else if (Boltzmann_Exploration && dynamicTemperature)
        {
            //Boltzmann Exploration decay takes in qvalues and our reward to dynamically adjust the temperature rate
            //It is a basic implementation
            if(temperatureExperienceFluctuations) { BoltzmannTemeperature_Dynamic_Experience_Decay(); }
            else if(temperatureAnnealingDecay) { BoltzmannTemeperature_Dynamic_Annealing_Decay(); }
            else if(temperatureEpisodeDecay) { BoltzmannTemeperature_Dynamic_Episode_Decay(); }
            
        }

        //This causes a bottle neck because of the list sorting and other string functions if called too frequently
        /*//Save our updated qTable
        OpenQLearningBrainSave_Manager.SaveOQLBrain(Open_QBrain_Container);*/
    }

    //For Saving Both Brain And Buffer replay
    public void Save_Open_QBrain()
    {
        Save_Manager.sortedOut = sortedOutput;
        Save_Manager.SaveOQLBrain(Open_QBrain_Container);
        Save_Manager.SaveOQLReplay(Open_QBrain_Replay_Buffer);
    }

    //Episode Controls - important for setting when you are using episodes for exploring/exploiting and other areas of your code
    public void ResetAnnealingEpisodeDecayCount()
    {
        decayAnnealing_current_episode_counter = 0;

    }
    public void UpdateEpisodeForSessionAnnealingDecay()
    {
        decayAnnealing_current_episode_counter++;
    }
    //Interactions Controls
    public void ResetDecayInteractionCounter()
    {
        decayEpisode_current_action_counter = 0;
        epsilon = 1f;
        temperature = 1f;
    }


    //Action Functions

    //Choose an action uses one (of two) available methods to help balance the brains choice between exploration and exploitation
    public string ChooseAnAction(State_Class state)
    {
        //If the greedy epsilon has been choosen, else use the Boltzmann
        if(Greedy_Epsilon)
        {
            //Get a random value
            Random rand = new Random();
            float explore = (float)rand.NextDouble();
            //Debug.Log(explore);

            //Check if it less than our epsilon - epsilon is either set in inspector or updated dynamically in the Update Rewards function
            if (explore < epsilon)
            {
                //Get a random action
                int r = rand.Next(0, Open_QBrain_Container[state].Keys.Count);
                //Assign it
                string randomAction = Open_QBrain_Container[state].Keys.ElementAt(r);
                //Return it
                return randomAction;
            }
            else
            {
                //Set our action and best qvalue variables
                string bestChoice = string.Empty;
                float bestValue = float.MinValue;
                float qValue = 0f;

                //Iterate through our list of actions that can be taken in the state
                foreach(var action_qValue in Open_QBrain_Container[state])
                {
                    //Get the action qValue and check it against the best value
                    qValue = action_qValue.Value;
                    if(qValue > bestValue)
                    {
                        //Assign the variables if it is greater
                        bestValue = qValue;
                        bestChoice = action_qValue.Key;
                    }
                }

                //Return our action
                return bestChoice;
            }
        }
        else
        {
            //Botlzmann Exploration
            //temperature = 1.0f; // Set in inspector and/or dynamically

            // Get Q-values for all actions in the current state
            ConcurrentDictionary<string, float> actionValues = Open_QBrain_Container[state];

            // Calculate action probabilities
            float sumOfProbabilities = 0.0f;
            ConcurrentDictionary<string, float> actionProbabilities = new ConcurrentDictionary<string, float>();
            foreach (KeyValuePair<string, float> actionValue in actionValues)
            {
                float probability = Mathf.Exp(actionValue.Value / temperature);
                actionProbabilities.TryAdd(actionValue.Key, probability);
                sumOfProbabilities += probability;
            }

            // Normalize probabilities (optional, can be skipped if sumOfProbabilities is close to 1)
            foreach (KeyValuePair<string, float> actionProb in actionProbabilities)
            {
                actionProbabilities[actionProb.Key] = actionProb.Value / sumOfProbabilities;
            }

            // Select action based on probability distribution
            Random rand = new Random();
            float randomValue = (float)rand.NextDouble();
            string bestChoice = string.Empty;
            float currentProbability = 0.0f;
            foreach (KeyValuePair<string, float> actionProb in actionProbabilities)
            {
                currentProbability += actionProb.Value;
                if (randomValue < currentProbability)
                {
                    bestChoice = actionProb.Key;
                    return bestChoice;
                }
            }

            // If no action selected due to rounding errors (unlikely), return a random action (fallback)
            bestChoice = Open_QBrain_Container[state].Keys.ElementAt(rand.Next(0, Open_QBrain_Container[state].Keys.Count));
            return bestChoice;
        }
    }

    //State Functions

    //Function for getting our current state - currently this is not very necessary,
    //since we can easily find out our state, but is good for adding in the case of meeting an unkown state
    public State_Class GetCurrentState(State_Class state)
    {
        //Create an empty variable
        State_Class currentState = state;

        //If our current state does not match any state, we will add it to the brain using anothers states
        if (Open_QBrain_Container.ContainsKey(currentState))
        {
            return currentState;
        }
        else
        {
            //Create a new set of actions and qvalues (actions from state's actions) and save to our table
            ConcurrentDictionary<string, float> new_qValues_and_Actions = new ConcurrentDictionary<string, float>();

            //Use example in the State_Actions to assign actions
            for(int i = 0; i < State_And_Actions[0].Actions.Count; i++)
            {
                new_qValues_and_Actions.TryAdd(State_And_Actions[0].Actions[i], 0.0f);
            }

            /*//Random key int to use as an index to get a string key for the dictionary
            Random rand = new Random();
            int r = rand.Next(0, Open_QBrain_Container.Keys.Count);
            State_Class rS = Open_QBrain_Container.ElementAt(r).Key;

            //Make a list from the actions, loop through, adding them to the new dictionary, with qValues of 0.0f
            List<string> actions = Open_QBrain_Container[rS].Keys.ToList();
            foreach (string action in actions)
            {
                new_qValues_and_Actions.TryAdd(action, 0.0f);
            }*/



            //Add our current (new) state and our current table, and save the table
            Open_QBrain_Container.TryAdd(currentState, new_qValues_and_Actions);
            //Save_Manager.SaveOQLBrain(Open_QBrain_Container);


            return currentState;
        }

    }


    //Q Value functions

    //Update QTable
    public void UpdateQTable()
    {
        // Sample a minibatch of experiences from the replay buffer
        List<KeyValuePair<Replay_Buffer_Key, Replay_Buffer_Values>> sampledExperiences = GetPlayBackSample();

        State_Class sC;
        string stateString; List<AttributesPair<string, string>> sA; string action; float reward; State_Class nextState; bool done;
        float currentQValue = 0; 

        foreach(KeyValuePair<Replay_Buffer_Key, Replay_Buffer_Values> experience in sampledExperiences)
        {
            stateString = experience.Key.State.stateString;
            sA = experience.Key.State.state_attributes;
            sC = new State_Class();
            sC.stateString = stateString;
            sC.state_attributes = sA;

            sC = GetCurrentState(sC);

            action = experience.Key.Action;
            reward = experience.Value.Reward; 
            nextState = experience.Value.NextState;
            done = experience.Value.Done;


            // Calculate target Q-value using Bellman equation (with experience replay)
            float targetQValue;
            if (!done)
            {
                float maxQNext = GetMaxQValue(nextState);
                targetQValue = reward + dicountFactor * maxQNext;
            }
            else
            {
                targetQValue = reward;
            }

            currentQValue = GetQValue(sC, action);
            // Update Q-value for the state-action pair in the experience
            SetQValue(sC, action, targetQValue, currentQValue);


        }

        //Check if we have reached out size limit
        if (Open_QBrain_Replay_Buffer.Count >= ReplayBufferSize)
        {
            //If so, we can remove, and check if it is done yet.
            bool doneYet = ReplayBufferExperienceRemoval();
        }

        //Save the new data - again, might have to take this out
        Save_Open_QBrain();
    }
    //Get Q-Values
    float GetQValue(State_Class state, string action)
    {
        ConcurrentDictionary<string, float> currentStateAndAction = Open_QBrain_Container[state];

        //if (currentStateAndAction[action] == null) { Debug.LogError("You are trying to save/access states with no actions. Follow this error message to where your script is calling the brain, and see what may be wrong there. (Also, your brain file likely has erronous data now, consider deleting it.)"); }

        return currentStateAndAction[action];
    }
    //Get Max Q-Value
    float GetMaxQValue(State_Class newState)
    {
        //Find our next state in the dictionary, and collect it's ditionary of actions ang q-values
        ConcurrentDictionary<string, float> currentStateAndAction = Open_QBrain_Container[newState];

        float maxQ = float.MinValue; // Initialize with minimum value

        // Loop through all possible actions and check against/change the maxQ value

        foreach(KeyValuePair<string, float> kvp in currentStateAndAction)
        {
            maxQ = Mathf.Max(maxQ, currentStateAndAction[kvp.Key]);
        }
        

        return maxQ;
    }
    //Set Q-Value
    void SetQValue(State_Class state, string action, float targetQValue, float currentQValue)
    {
        ConcurrentDictionary<string, float> currentStateAndAction = Open_QBrain_Container[state];

        // Calculate the new Q-value using learning rate (using the q value from our current state, not our replay state)
        float newQValue = currentQValue + learningRate * (targetQValue - currentQValue);

        // Update the Q-table entry with the new Q-value
        currentStateAndAction[action] = newQValue;
    }




    //Buffer Replay Functions

    //Function for adding an experience to the replay buffer
    public void AddExperienceToReplayBuffer(State_Class state, float reward, bool done) 
    {
        
        //Create new repaly buffers (key/value) to add to our replay container
        Replay_Buffer_Key rbK = new Replay_Buffer_Key(old_state, action_Choosen);
        Replay_Buffer_Values pbV = new Replay_Buffer_Values(reward, state, done);

        //Add the new id for our experience
        rbK.ExperienceID = Open_QBrain_Replay_Buffer.Count;

        Open_QBrain_Replay_Buffer.TryAdd(rbK, pbV);

        //If the flag is done, and we have not saved (i.e. our steps are not enough) - Again this can cause bottle
        //necks is the done flag is being used for constantly captuiring successes - over-cakking the save function
        //like in our dodgebot example
        /*if (done)
        {
            //Save the file
            Save_Open_QBrain();
        }*/

    }

    //Get a sample of experience from our Buffer Replay
    List<KeyValuePair<Replay_Buffer_Key, Replay_Buffer_Values>> GetPlayBackSample()
    {
        List<KeyValuePair<Replay_Buffer_Key, Replay_Buffer_Values>> newL = new List<KeyValuePair<Replay_Buffer_Key, Replay_Buffer_Values>>();


        if(Open_QBrain_Replay_Buffer.Count <= replayBatchSampleSize)
        {
            foreach(KeyValuePair<Replay_Buffer_Key, Replay_Buffer_Values> keyValuePair in Open_QBrain_Replay_Buffer)
            {
                newL.Add(keyValuePair);
            }

            return newL;
        }
        else
        {

            //list.IndexOf(list.Max()); for max float from a list

            int sampleGrabTries = 0;
            Random rand = new Random();
            List<KeyValuePair<Replay_Buffer_Key, Replay_Buffer_Values>> keys = new List<KeyValuePair<Replay_Buffer_Key, Replay_Buffer_Values>>(Open_QBrain_Replay_Buffer);
            KeyValuePair<Replay_Buffer_Key, Replay_Buffer_Values> kvp = new KeyValuePair<Replay_Buffer_Key, Replay_Buffer_Values>();

            //If we simply want a random batch of replay experiences
            if (randomReplayBuffer)
            {
                for (int i = 0; i < replayBatchSampleSize; i++)
                {
                    int r = rand.Next(0, keys.Count);
                    kvp = keys[r];

                    if (newL.Contains(kvp))
                    {
                        if (sampleGrabTries < 1000)
                        {
                            sampleGrabTries++;
                            i--;
                        }
                        else
                        {
                            break;
                        }

                    }
                    else
                    {
                        newL.Add(kvp);
                    }
                }
               
            }

            //For getting experiences using a single attribute as an example
            if (singleAttributeReplayBuffer)
            {
                //Since we are using a concurrent dictionary, nothing in there is ordered,
                //meaning we are getting a ranom sample of a single attributes memory
                int i = 0;
                foreach (KeyValuePair<Replay_Buffer_Key, Replay_Buffer_Values> kp in Open_QBrain_Replay_Buffer)
                {
                    foreach (AttributesPair<string, string> sPair in kp.Key.State.state_attributes)
                    {
                        if (sPair.Key == singleAttributeReplayBufferKey && sPair.Value == singleAttributeReplayBufferValue)
                        {
                            if (!newL.Contains(kp))
                            {
                                newL.Add(kp);
                                i++;
                                if (i >= replayBatchSampleSize - 1) { break; }
                            }
                        }
                    }
                    if (i >= replayBatchSampleSize - 1) { break; }
                }
            }

            //For getting experiences from a single string state
            if (stateStringReplayBuffer)
            {
                //Since we are using a concurrent dictionary, nothing in there is ordered,
                //meaning we are getting a ranom sample of the state string
                int i = 0;
                foreach (KeyValuePair<Replay_Buffer_Key, Replay_Buffer_Values> kp in Open_QBrain_Replay_Buffer)
                {
                    if (kp.Key.State.stateString == singleAttributeReplayBufferKey)
                    {
                        if (!newL.Contains(kp))
                        {
                            newL.Add(kp);
                            i++;
                            if (i >= replayBatchSampleSize - 1) { break; }
                        }
                    }
                    if (i >= replayBatchSampleSize - 1) { break; }
                }
            }

            return newL;


        }
    }

    public bool ReplayBufferExperienceRemoval()
    {
        bool replayFinishedRemoving = false;

        //Use the global variable to assign how many experience we want to remove
        int amountWeAreRemoving = amountOfExperiencesToRemove;

        //A check to update the amount to remove, incase it will remove enough to cross the max size amount
        if((Open_QBrain_Replay_Buffer.Count - amountWeAreRemoving) > ReplayBufferSize)
        {
            //Debug.Log(Open_QBrain_Replay_Buffer.Count + " || " + amountWeAreRemoving + " || " + replayBatchSampleSize);
            //Adjust the amount to be equal to the count minus the max (+ 1 for the sake of putting us a little under)
            amountWeAreRemoving = Open_QBrain_Replay_Buffer.Count - ReplayBufferSize;
        }

        if (RB_FirstIn_FirstOut)
        {
            if (experienceRemovalIdSorted == false) { experienceRemovalIdSorted = true; }
            //Sort ourselves into an ordered list - and clear our replay buffer
            var sortedList = Open_QBrain_Replay_Buffer.OrderBy(pair => pair.Key.ExperienceID).ToList();
            Open_QBrain_Replay_Buffer.Clear();
            for (int i = 0; i < amountWeAreRemoving; i++)
            {
                //Since we have order the list ascending, our index will alway be 0, as we will always be removing the first item
                int index = 0;

                //Remove it from our list
                sortedList.Remove(sortedList[index]);

            }

            //Repopulate the dictionary with our new values
            for (int i = 0; i < sortedList.Count; i++)
            {
                sortedList[i].Key.ExperienceID = i;
                Open_QBrain_Replay_Buffer.TryAdd(sortedList[i].Key, sortedList[i].Value);
            }
        }
        else if (RB_LastIn_FirstOut)
        {
            if(experienceRemovalIdSorted == false) { experienceRemovalIdSorted = true; }

            //Sort ourselves into an ordered list - and clear our replay buffer
            var sortedList = Open_QBrain_Replay_Buffer.OrderBy(pair => pair.Key.ExperienceID).ToList();
            Open_QBrain_Replay_Buffer.Clear();
            for (int i = 0; i < amountWeAreRemoving; i++)
            {
                //Since we have order the list ascending, our index will alway be the last item, as we will always be removing the last item
                int index = sortedList.Count - 1;

                //Remove it from our list
                sortedList.Remove(sortedList[index]);

            }

            //Repopulate the dictionary with our new values - since we have just taken from the top, we don't have to adjust experience ids
            Open_QBrain_Replay_Buffer = new ConcurrentDictionary<Replay_Buffer_Key, Replay_Buffer_Values>(sortedList.ToDictionary(pair => pair.Key, pair => pair.Value));
        }
        else if (RB_Prioritised_AttributeRemoval)
        {
            List<Replay_Buffer_Key> toRemove = new List<Replay_Buffer_Key>();
            int i = 0;
            foreach (var kvp in Open_QBrain_Replay_Buffer)
            {
                foreach (var att in kvp.Key.State.state_attributes)
                {
                    if (att.Key == RB_Prioritised_AttributeRemovalKey && att.Key == RB_Prioritised_AttributeRemovalValue)
                    {
                        toRemove.Add(kvp.Key);
                        i++;
                        if (i >= amountWeAreRemoving) { break; }
                    }
                }
                if (i >= amountWeAreRemoving) { break; }
            }


            //Remove them
            foreach (var k in toRemove)
            {
                ((IDictionary)Open_QBrain_Replay_Buffer).Remove(k);
            }

            //If you wish to keep the replay buffer id's ordered - this will slow things down in large replay buffers
            if(experienceRemovalIdSorted)
            {
                //Order it into a list and clear
                var sortedList = Open_QBrain_Replay_Buffer.OrderBy(pair => pair.Key.ExperienceID).ToList();
                Open_QBrain_Replay_Buffer.Clear();

                //And repopulate
                for (int j = 0; j < sortedList.Count; j++)
                {
                    sortedList[j].Key.ExperienceID = j;
                    Open_QBrain_Replay_Buffer.TryAdd(sortedList[j].Key, sortedList[j].Value);
                }
            }
            
        }
        else if (RB_RandomRemoval)
        {
            Random rand = new Random();
            List<Replay_Buffer_Key> rKeys = Open_QBrain_Replay_Buffer.Keys.ToList();

            for (int i = 0; i < amountWeAreRemoving; i++)
            {
                int r = rand.Next(rKeys.Count);
                var k = rKeys[r];
                if (k != null)
                {
                    ((IDictionary)Open_QBrain_Replay_Buffer).Remove(k);
                    rKeys.Remove(rKeys[r]);
                }

            }

            //If you wish to keep the replay buffer id's ordered - this will slow things down in large replay buffers
            if (experienceRemovalIdSorted)
            {
                //Order it into a list and clear
                var sortedList = Open_QBrain_Replay_Buffer.OrderBy(pair => pair.Key.ExperienceID).ToList();
                Open_QBrain_Replay_Buffer.Clear();

                //And repopulate
                for (int j = 0; j < sortedList.Count; j++)
                {
                    sortedList[j].Key.ExperienceID = j;
                    Open_QBrain_Replay_Buffer.TryAdd(sortedList[j].Key, sortedList[j].Value);

                }
            }

        }
        else
        {
            //Create your own method and olace it here!!!
        }

        replayFinishedRemoving = true;

        return replayFinishedRemoving;

    }

    


    /// <summary>
    /// Exploration Versus Exploitation Variable Decay Functions
    /// </summary>


    //Epsilon Dynamic Decay Functions

    //Dynamic Experience Decay
    void Epsilon_Dynamic_Experience_Decay()
    {
        // Sample a batch of experiences from the replay buffer - check if the replay had run an experience yet
        if(choices_taken_count_batch_learning < choicesBetweenBatchLearning && choices_taken_count_batch_learning > 0)
        {
            //If our list had not been populated, we poulate it
            if(experienceFluctationExeperiences.Count <=0)
            {
                experienceFluctationExeperiences = GetPlayBackSample();
                //Assign our max Q and our age to a normalized value (between 0-1)
                float maxQ = 0f;
                float age = Open_QBrain_Replay_Buffer.Count / (float)ReplayBufferSize;
                float meanQ = 0f;
                float ageWeight = ageWeight_Epsilon;

                int experienceCount = experienceFluctationExeperiences.Count;

                //Loop through our experiences to get our max
                foreach (var kvp in experienceFluctationExeperiences)
                {
                    //Clip the range to account for large negatives or positive rewards
                    float reward = Mathf.Clamp(kvp.Value.Reward, -10f, 10f);
                    meanQ += reward;
                    if (reward > maxQ)
                    {
                        maxQ = reward;
                    }
                }

                float ageDiscount = Mathf.Clamp01(age * ageWeight); // Clamp ageDiscount between 0 and 1

                // Calculate weighted mean reward (consider using "totalReward / experienceCount" if preferred)
                float meanReward = (meanQ * (1f - ageDiscount)) / experienceCount;

                // Calculate epsilon based on weighted mean and max reward (adjust formula as needed)
                epsilon = Mathf.Clamp01(meanReward / maxQ);
            }

            //Else, continue using the current list of experiences
        }
        else
        {
            //WE update the list with a new sample batch
            experienceFluctationExeperiences.Clear();
            experienceFluctationExeperiences = GetPlayBackSample();
            //Assign our max Q and our age to a normalized value (between 0-1)
            float maxQ = 0f;
            float age = Open_QBrain_Replay_Buffer.Count / (float)ReplayBufferSize;
            float meanQ = 0f;
            float ageWeight = ageWeight_Epsilon;

            int experienceCount = experienceFluctationExeperiences.Count;

            //Loop through our experiences to get our max
            foreach (var kvp in experienceFluctationExeperiences)
            {
                //Clip the range to account for large negatives or positive rewards
                float reward = Mathf.Clamp(kvp.Value.Reward, -10f, 10f);
                meanQ += reward;
                if (reward > maxQ)
                {
                    maxQ = reward;
                }
            }

            float ageDiscount = Mathf.Clamp01(age * ageWeight); // Clamp ageDiscount between 0 and 1

            // Calculate weighted mean reward (consider using "totalReward / experienceCount" if preferred)
            float meanReward = (meanQ * (1f - ageDiscount)) / experienceCount;

            // Calculate epsilon based on weighted mean and max reward (adjust formula as needed)
            epsilon = Mathf.Clamp01(meanReward / maxQ);
        }

        


    }

    //Dynamic Annealing Decay - Decay Based on Episodes / Total Session Episodes
    void Epsilon_Dynamic_Annealing_Decay()
    {
        //Lerp our current episode counter / total session episode from start to end
        epsilon = Mathf.Lerp(epsilonStart, epsilonEnd, (float)decayAnnealing_current_episode_counter / epsilonDynamicDecayTotalSessionValue);
        //Reset the value automatically
        if(decayAnnealing_current_episode_counter >= epsilonDynamicDecayTotalSessionValue) { decayAnnealing_current_episode_counter = 0; }

    }
    //Dynamic Episodic Decay - Decay based on Interactions / Total Episode Interactions
    void Epsilon_Dynamic_Episode_Decay()
    {
        
        // Normalize the counter value based on provided episode length
        float normalizedCounter = Mathf.Clamp01((float)decayEpisode_current_action_counter / (float)epsilonDynamicEpisode_InteractionsAmount);

        //Get our decay modifier and begin decaying our epsilon
        float decayModifier = 1.0f - (normalizedCounter * epsilonDecayRate);
        epsilon = decayModifier;

        //Reset the counter
        if(decayEpisode_current_action_counter >= epsilonDynamicEpisode_InteractionsAmount) { decayEpisode_current_action_counter = 0; }
    }

    //Boltzmann Dynanic Decay Finctions

    //Dynamic Experience Decay
    void BoltzmannTemeperature_Dynamic_Experience_Decay()
    {
        // Sample a batch of experiences from the replay buffer - check if the replay had run an experience yet
        if (choices_taken_count_batch_learning < choicesBetweenBatchLearning && choices_taken_count_batch_learning > 0)
        {
            //If our list had not been populated, we poulate it
            if (experienceFluctationExeperiences.Count <= 0)
            {
                experienceFluctationExeperiences = GetPlayBackSample();

                //Assign our max Q and our age to a normalized value (between 0-1)
                float maxQ = 0f;
                float age = Open_QBrain_Replay_Buffer.Count / (float)ReplayBufferSize;
                float meanQ = 0f;
                float ageWeight = ageWeight_Temperature;

                int experienceCount = experienceFluctationExeperiences.Count;

                //Loop through our experiences to get our max
                foreach (var kvp in experienceFluctationExeperiences)
                {
                    //Clip the range to account for large negatives or positive rewards
                    float reward = Mathf.Clamp(kvp.Value.Reward, -10f, 10f);
                    meanQ += reward;
                    if (reward > maxQ)
                    {
                        maxQ = reward;
                    }
                }

                float ageDiscount = Mathf.Clamp01(age * ageWeight); // Clamp ageDiscount between 0 and 1

                // Calculate weighted mean reward (consider using "totalReward / experienceCount" if preferred)
                float meanReward = (meanQ * (1f - ageDiscount)) / experienceCount;

                // Calculate temperature based on weighted mean and max reward (adjust formula as needed)
                temperature = Mathf.Clamp01(meanReward / maxQ);
            }

            //Else, continue using the current list of experiences
        }
        else
        {
            //WE update the list with a new sample batch
            experienceFluctationExeperiences.Clear();
            experienceFluctationExeperiences = GetPlayBackSample();


            //Assign our max Q and our age to a normalized value (between 0-1)
            float maxQ = 0f;
            float age = Open_QBrain_Replay_Buffer.Count / (float)ReplayBufferSize;
            float meanQ = 0f;
            float ageWeight = ageWeight_Temperature;

            int experienceCount = experienceFluctationExeperiences.Count;

            //Loop through our experiences to get our max
            foreach (var kvp in experienceFluctationExeperiences)
            {
                //Clip the range to account for large negatives or positive rewards
                float reward = Mathf.Clamp(kvp.Value.Reward, -10f, 10f);
                meanQ += reward;
                if (reward > maxQ)
                {
                    maxQ = reward;
                }
            }

            float ageDiscount = Mathf.Clamp01(age * ageWeight); // Clamp ageDiscount between 0 and 1

            // Calculate weighted mean reward (consider using "totalReward / experienceCount" if preferred)
            float meanReward = (meanQ * (1f - ageDiscount)) / experienceCount;

            // Calculate temperature based on weighted mean and max reward (adjust formula as needed)
            temperature = Mathf.Clamp01(meanReward / maxQ);
        }

        
    }
    //Dynamic Annealing Decay - Decay Based on Episodes / Total Session Episodes
    void BoltzmannTemeperature_Dynamic_Annealing_Decay()
    {
        //Lerp our current episode counter / total session episode from start to end
        temperature = Mathf.Lerp(temperatureStart, temperatureEnd, (float)decayAnnealing_current_episode_counter / temperatureDynamicDecayTotalSessionValue);
        //Reset the value automatically
        if (decayAnnealing_current_episode_counter >= temperatureDynamicDecayTotalSessionValue) { decayAnnealing_current_episode_counter = 0; }

    }
    //Dynamic Episodic Decay - Decay based on Interactions / Total Episode Interactions
    void BoltzmannTemeperature_Dynamic_Episode_Decay()
    {
        // Normalize the counter value based on provided episode length
        float normalizedCounter = Mathf.Clamp01((float)decayEpisode_current_action_counter / (float)temperatureDynamic_EpisodeInteractionsAmount);

        //Get our decay modifier and begin decaying our epsilon
        float decayModifier = 1.0f - (normalizedCounter * temperatureDecayRate);
        temperature = decayModifier;

        //Reset the counter
        if (decayEpisode_current_action_counter >= temperatureDynamic_EpisodeInteractionsAmount) { decayEpisode_current_action_counter = 0; }

    }



}



/// <summary>
/// Saving and Loading
/// </summary>


//To load in the brain string be created from holder classes - to make it easier for the JSON Utility.


//Our simple save functions Class - using Stream writer/reader
public class OpenQLearningBrainSaveManager
{
    public string pathDir;
    public string path;
    public bool sortedOut = false;

    public string path_replay_buffer_Dir;
    public string replay_path_addition = "replay_Buffer_";
    public string path_replay_buffer;

    public void SaveOQLBrain(ConcurrentDictionary<State_Class, ConcurrentDictionary<string, float>> bQtable)
    {
        //Set our path if we haven't already
        if (path == null)
        {
            path = Path.Combine(Application.persistentDataPath, pathDir);
        }

        //Create entries list of the
        List<QTable_Data_Holder> bEntries = new List<QTable_Data_Holder>();


        //Preload variabls
        string sState;
        List<AttributesPair<string, string>> attributePairs = new List<AttributesPair<string, string>>();
        AttributesPair<string, string> Ap;
        List<AttributesPair<string, float>> actionValues = new List<AttributesPair<string, float>>();
        AttributesPair<string, float> Av;
        QTable_Data_Holder QDH;

        //Loop trhough
        foreach (var kvp in bQtable)
        {
            //Set the state string
            sState = kvp.Key.stateString;

            //Get any attributes and put into a string list
            attributePairs = new List<AttributesPair<string, string>>();
            foreach (var att in kvp.Key.state_attributes)
            {
                Ap = new AttributesPair<string, string>(att.key, att.value);
                attributePairs.Add(Ap);
            }

            //Get our action actions and floats
            actionValues = new List<AttributesPair<string, float>>();
            foreach (var aQ in kvp.Value)
            {
                Av = new AttributesPair<string, float>(aQ.Key, aQ.Value);
                actionValues.Add(Av);
                //Debug.Log(Av.Key + " " + Av.Value);
            }

            QDH = new QTable_Data_Holder(sState, attributePairs, actionValues);

            bEntries.Add(QDH);


        }

        //If we want to sort the output before saving
        if (sortedOut)
        {
            //Sort both out lists
            bEntries.Sort();
        }

        // String for serialization
        string jsonString;

        try
        {
            //Using Stream Writer to write our simple brain
            using (StreamWriter writer = new StreamWriter(path))
            {
                // Loop through bEntries and serialize each entry individually
                foreach (var entry in bEntries)
                {
                    jsonString = JsonUtility.ToJson(entry);
                    // Write the serialized entry to the file
                    writer.WriteLine(jsonString);
                }
            }
        }
        catch
        {
            Debug.Log("Not multithread, are we???");
        }


    }

    public void LoadOQLBrain(ref ConcurrentDictionary<State_Class, ConcurrentDictionary<string, float>> bQtable)
    {
        //Set our path if we haven't already
        if (path == null)
        {
            path = Path.Combine(Application.persistentDataPath, pathDir);
        }

        //If our path does not exist, return out of the function
        if (!File.Exists(path)) return;

        //Ensure our table is clear
        bQtable.Clear();

        //Set the variables
        State_Class sC;
        ConcurrentDictionary<string, float> actionVals = new ConcurrentDictionary<string, float>();
        string sState;
        List<AttributesPair<string, string>> attributePairs = new List<AttributesPair<string, string>>();
        AttributesPair<string, string> Ap;
        List<AttributesPair<string, float>> actionValues = new List<AttributesPair<string, float>>();
        QTable_Data_Holder entry;

        try
        {
            

            //Read each line and add to the list
            using (StreamReader reader = new StreamReader(path))
            {
                //String JSON
                string jsonString;
                while((jsonString = reader.ReadLine()) != null) 
                {
                    //Entry From JSON
                    entry = JsonUtility.FromJson<QTable_Data_Holder>(jsonString);
                    sC = new State_Class();
                    sState = entry.state_String;
                    sC.stateString = sState;

                    attributePairs = new List<AttributesPair<string, string>>();
                    foreach(var aP in entry.attribute_Pairs)
                    {
                        string s = aP.value;
                        Ap = new AttributesPair<string, string>(aP.Key, aP.value);
                        attributePairs.Add(aP);
                        
                    }

                    sC.state_attributes = attributePairs;

                    actionVals = new ConcurrentDictionary<string, float>();
                    foreach (var kvp in entry.action_values)
                    {
                        actionVals.TryAdd(kvp.Key, kvp.Value); 
                    }
                    

                    bQtable.TryAdd(sC, actionVals);
                    
                }
                
            }

        }
        catch
        {
            //Can place a catch here in case something goes wrong
        }
    }

    public void SaveOQLReplay(ConcurrentDictionary<Replay_Buffer_Key, Replay_Buffer_Values> bufferData)
    {
        //Set our buffer directory if we have not yet
        if(path_replay_buffer == null) 
        {
            path_replay_buffer_Dir = replay_path_addition + pathDir;

            path_replay_buffer = Path.Combine(Application.persistentDataPath, path_replay_buffer_Dir);
        }

        // Create a list to hold serialized JSON strings
        List<RB_Data_Holder> serializedEntries = new List<RB_Data_Holder>();
        RB_Data_Holder rbDH;
        string sState;
        List<AttributesPair<string, string>> stateAttributes = new List<AttributesPair<string, string>>();
        string act;
        int expID;
        float rewardVal;
        string nState;
        List<AttributesPair<string, string>> newStateAttributes = new List<AttributesPair<string, string>>();
        bool doneYet = false;


        // Serialize each buffer entry individually
        foreach (var kvp in bufferData)
        {

            sState = kvp.Key.State.stateString;

            stateAttributes.Clear();
            foreach (var sA in kvp.Key.State.state_attributes)
            {
                stateAttributes.Add(sA);
            }

            act = kvp.Key.Action;

            expID = kvp.Key.ExperienceID;

            rewardVal = kvp.Value.Reward;

            nState = kvp.Value.NextState.stateString;

            newStateAttributes.Clear();
            foreach (var sB in kvp.Value.NextState.state_attributes)
            {
                newStateAttributes.Add(sB);
            }

            doneYet = kvp.Value.Done;

            rbDH = new RB_Data_Holder(sState, stateAttributes, act, expID, rewardVal, nState, newStateAttributes, doneYet);

            // Add to the list of serialized entries
            serializedEntries.Add(rbDH);
        }

        try
        {
            // Using Stream Writer to write our simple brain
            using (StreamWriter writer = new StreamWriter(path_replay_buffer))
            {
                // Loop through serialized entries and write them to the file
                foreach (var serializedEntry in serializedEntries)
                {
                    // Convert the RB_Data_Holder object to JSON string
                    string jsonString = JsonUtility.ToJson(serializedEntry);
                    writer.WriteLine(jsonString);
                }

            }
        }
        catch (Exception e)
        {
            Debug.LogError("Error saving replay buffer: " + e.Message);
        }

       
    }

    public void LoadOQLBReplay(ref ConcurrentDictionary<Replay_Buffer_Key, Replay_Buffer_Values> bufferData)
    {
        //Set our buffer directory if we have not yet
        if (path_replay_buffer == null)
        {
            path_replay_buffer_Dir = replay_path_addition + pathDir;

            path_replay_buffer = Path.Combine(Application.persistentDataPath, path_replay_buffer_Dir);
        }

        //If the file doesn't exist for some reason, return out
        if (!File.Exists(path_replay_buffer)) { return; }

        //Ensure our table is clear
        bufferData.Clear();

        //Varaibles to collect
        RB_Data_Holder ent;
        Replay_Buffer_Key rbk;
        Replay_Buffer_Values rbv;
        State_Class state_C;
        State_Class new_State_C;
        string act;
        int expID;
        float reward;
        bool doneD;

        //Read through the data
        using (StreamReader reader = new StreamReader(path_replay_buffer))
        {
            //Get the string
            string entry;
            while ((entry = reader.ReadLine()) != null)
            {
                //Deserialize from JSON
                ent = JsonUtility.FromJson<RB_Data_Holder>(entry);

                //Assign variables
                state_C = new State_Class();
                state_C.stateString = ent.stateString;
                state_C.state_attributes = ent.stateAttributes;

                act = ent.action;

                expID = ent.experiID;

                reward = ent.reward;

                new_State_C = new State_Class();
                new_State_C.stateString = ent.newStateString;
                new_State_C.state_attributes = ent.newStateAttributes;

                doneD = ent.done;

                //Assign the buffer Key and Value
                rbk = new Replay_Buffer_Key(state_C, act, expID);
                rbv = new Replay_Buffer_Values(reward, new_State_C, doneD);

                //Add to the dictionary
                bufferData.TryAdd(rbk, rbv);
            }
        }

    }
}



/// <summary>
/// Additional Classes
/// </summary>


namespace QLearning
{

    //Classes for the QBrain and Replay Buffer

    //Open_QBrain Class - our state and actions
    [System.Serializable]
    public class Open_QBrain_Class
{
    //Our State and Actions
    [SerializeField]
    public State_Class State = new State_Class();
    [SerializeField]
    public List<string> Actions = new List<string>();

}


    //Our State Class - state string and attributes
    [System.Serializable]
    public class State_Class
    {
        //OUr State String and Attributes Pair list
        [SerializeField]
        public string stateString;
        [SerializeField]
        public List<AttributesPair<string, string>> state_attributes = new List<AttributesPair<string, string>>();

        //Since we are using a custom class as a key, we need to override the Equals function to take in our
        //string and attributes when checking the class. We also need to override our GetHashCode function to 
        //combine our state string and attributes (KeyValuePairs in the list

        //Function for overriding the equals fucntion for our State_Class
        public override bool Equals(object obj)
        {
            //Ensure we are passing in an object (reference) with the correct type 
            if (obj == null || GetType() != obj.GetType())
            {
                return false;
            }

            //Get our state_class (object)
            State_Class other = (State_Class)obj;

            // Check state string equality
            if (stateString != other.stateString) { return false; }

            // Check KeyValuePair List equality
            if (state_attributes.Count != other.state_attributes.Count) { return false; }

            // Compare each KeyValuePair within the list
            for (int i = 0; i < state_attributes.Count; i++)
            {
                if (state_attributes[i].Key != other.state_attributes[i].Key || state_attributes[i].Value != other.state_attributes[i].Value)
                {
                    return false;
                }
            }

            //If we made it here, we have a match
            return true;
        }
        //Fnnctions for getting our HashCode
        public override int GetHashCode()
        {
            // Combine hash codes of stateString and each KeyValuePair in the list
            int hash = stateString.GetHashCode();
            foreach (var pair in state_attributes)
            {
                hash ^= pair.Key.GetHashCode() ^ pair.Value.GetHashCode();
            }
            return hash;
        }

    }

    //Replay Buffer Key Class - State_Class and Action taken
    [System.Serializable]
    public class Replay_Buffer_Key : IComparable<Replay_Buffer_Key>
    {
        [SerializeField] public State_Class State { get; set; }
        [SerializeField] public string Action { get; set; }
        [SerializeField] public int ExperienceID { get; set; }
        public Replay_Buffer_Key(State_Class state, string action, int expID = -1)
        {
            this.State = state;
            this.Action = action;
            this.ExperienceID = expID;
        }

        // Implementing IComparable<T> interface
        public int CompareTo(Replay_Buffer_Key other)
        {
            if (other == null)
                return 1;

            // Compare State String
            int stateComparison = this.State.stateString.CompareTo(other.State.stateString);
            if (stateComparison != 0)
                return stateComparison;

            // Compare Attributes 
            int attributeComparison = 0;
            foreach (var att in this.State.state_attributes)
            {
                if (this.State.state_attributes.Contains(att))
                {
                    //Nothing
                }
                else
                {
                    attributeComparison = 1;
                }

            }
            if (attributeComparison != 0)
                return attributeComparison;

            // Compare Action
            int actionComparison = this.Action.CompareTo(other.Action);
            if (actionComparison != 0)
                return actionComparison;

            // Compare ExperienceID
            return this.ExperienceID.CompareTo(other.ExperienceID);
        }

    }

    //Replay Buffer Values - Reward for action, Next State (State_Class), and Done flag
    [System.Serializable]
    public class Replay_Buffer_Values
    {
        [SerializeField] public float Reward { get; set; }
        [SerializeField] public State_Class NextState { get; set; }
        [SerializeField] public bool Done { get; set; }

        public Replay_Buffer_Values(float reward, State_Class nextState, bool done)
        {
            this.Reward = reward;
            this.NextState = nextState;
            this.Done = done;
        }

    }

    //Class for serializing/deserializing our QTable saved data
    [System.Serializable]
    public class QTable_Data_Holder
    {
        [SerializeField] public string state_String;
        [SerializeField] public List<AttributesPair<string, string>> attribute_Pairs = new List<AttributesPair<string, string>>();
        [SerializeField] public List<AttributesPair<string, float>> action_values = new List<AttributesPair<string, float>>();

        public QTable_Data_Holder(string sString, List<AttributesPair<string, string>> attPairs, List<AttributesPair<string, float>> actAndVals)
        {
            this.state_String = sString;
            this.attribute_Pairs = attPairs;
            this.action_values = actAndVals;
        }
    }

    //Class for serializing and deserializing Replay buffer
    [System.Serializable]
    public class RB_Data_Holder
    {
        [SerializeField] public string stateString;
        [SerializeField] public List<AttributesPair<string, string>> stateAttributes = new List<AttributesPair<string, string>>();
        [SerializeField] public string action;
        [SerializeField] public int experiID;

        [SerializeField] public float reward;
        [SerializeField] public string newStateString;
        [SerializeField] public List<AttributesPair<string, string>> newStateAttributes = new List<AttributesPair<string, string>>();
        [SerializeField] public bool done;

        public RB_Data_Holder(string StateString, List<AttributesPair<string, string>> StateAttributes, string action, int exID, float Reward, string NewStateString, List<AttributesPair<string, string>> NewStateAttributes, bool Done)
        {
            this.stateString = StateString;
            this.stateAttributes = StateAttributes;
            this.action = action;
            this.experiID = exID;

            this.reward = Reward;
            this.newStateString = NewStateString;
            this.newStateAttributes = NewStateAttributes;
            this.done = Done;
        }
    }

}

