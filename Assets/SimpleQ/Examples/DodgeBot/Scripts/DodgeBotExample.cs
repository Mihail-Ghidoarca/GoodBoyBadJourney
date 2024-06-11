using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using QLearning;

namespace QLearningExample
{

    //States (state/state attrubutes) and Actions

    //For the dodge bot example, we will be using the following states, attributes, and actions:
    //-State = Lane Position: The current lane the bot is occupying (left, middle, or right).
    //-Attributes =
    //---Distance to Obstacle: The distance between the bot and the nearest obstacle in its lane.
    //---Relative Speed of Obstacle: The speed of the nearest obstacle relative to the bot's speed.
    //---Presence of Obstacle: A binary value indicating whether there is an obstacle in the bot's lane or not.
    //Actions =
    //---Move to the left lane
    //---Stay in current lane
    //---Move to the right lane


    //Replay Buffer

    //Dodge bot logic simplified:

    //We will use a raycast to mimic the bots sight, and allow it to make a decision according to the information
    //then we will use that choice to move the agent to it's choosen position, before updating the reward
    //and saving after a certain amount of steps. 

    //A class for our different Dodge Attributes, so we can easily create new ones when making a choice/updating rewards
    [System.Serializable]
    public class DodgeBotAttributes
    {
        //Our attribute pairs
        public AttributesPair<string, string> ObstaclePresent;
        public AttributesPair<string, string> TargetDistance;
        public AttributesPair<string, string> ObstacleSpeed;
        public AttributesPair<string, string> LeftObstaclePresent;
        public AttributesPair<string, string> RightObstaclePresent;

        //For creating a new instance of the Attributes with the key values already set
        public DodgeBotAttributes(string obstaclePresentVal, string targetDistanceVal, string obstacleSpeedVal, string leftObstPresentVal, string rightObstPresentVal)
        {
            ObstaclePresent = new AttributesPair<string, string>("Obstacle Present", obstaclePresentVal);
            TargetDistance = new AttributesPair<string, string>("Target Distance", targetDistanceVal);
            ObstacleSpeed = new AttributesPair<string, string>("Obstacle Speed", obstacleSpeedVal);
            LeftObstaclePresent = new AttributesPair<string, string>("Left Obstacle Present", leftObstPresentVal);
            RightObstaclePresent = new AttributesPair<string, string>("Right Obstacle Present", rightObstPresentVal);
        }


    }

    //Class for string states (only have three) - makes it easier to check against than typing out every time
    public class stringStates
    {
        public string stateLeft = "Left_Lane";
        public string stateCenter = "Center_Lane";
        public string stateRight = "Right_Lane";
    }

    //Class for our dodge actions - makes it easier to check against than typing out every time
    public class dodgeStringActions
    {
        public string moveLeft = "Left";
        public string stayHere = "Stay";
        public string moveRight = "Right";

    }

    public class DodgeBotExample : MonoBehaviour
    {
        //We get our brain in the start function with this model
        private OpenQLearningBrain botBrain;

        [Header("Obstacle Variables")]
        //Obstacle Instantiation Points
        public List<GameObject> InstiationPoints = new List<GameObject>();

        //Speed for out objects movement
        public float objectSpeed;
        private float obstaclesInstantiatingSpeed;
        private float obstacleSpeedCountTiming;

        //Obstacles Prefab
        public GameObject obstaclePrefab;
        private List<GameObject> instantiatedObstacles = new List<GameObject>();
        private List<GameObject> obstToDestroy = new List<GameObject>();

        [Header("Agent (Bot) Variables")]
        //Player for controlling
        public GameObject dodgeBotPrefab;

        //String state and dodge string actions variable type holders
        private stringStates stringPositionStates = new stringStates();
        private dodgeStringActions dodgeStringActions = new dodgeStringActions();

        //Also going to set the brain path value here programatically
        private string brainSavePath = "Dodge_Bot.brain";

        [Header("Lane Variables")]
        //State for where we currently are
        private string currentLane;
        //List of Lane Positions
        public List<Transform> lanePositions = new List<Transform>();

        //This bool is important - it tells us whether we have been not been hit
        [HideInInspector] public bool botHasNotBeenHit = true;
        //This way, we can slightly alter the flag function of our replay buffer, allowing us to track if the bot was
        //hit during a choice, or not.

        //Bool for when a session is running
        private bool sessionRunning;

        //Steps to be taken 
        public int stepsToBeTaken;

        //Training Sessions To Be Run
        public int trainingSessionsCount;
        public int trainingCounter;

        [Header("Raycast Sight Settings")]
        public Transform raySightPosition;
        public float forwardRayDistance;
        public float leftAndRightRayDistance;

        //To get our original roation and position - incase it changes
        private Quaternion originalRotation;
        private Vector3 originalPosition;

        //Get the animator from our object in the start
        private Animator anim;



        //We fill in the Inspector programatically here, before starting up the brain
        void Start()
        {
            //Lets get the bot's brain
            botBrain = GetComponent<OpenQLearningBrain>();

            //Now let's get our list of State's and Actions (clear it just in case, and populate it with an example)
            botBrain.State_And_Actions.Clear();

            //An open brain class for our list
            Open_QBrain_Class oQB = new Open_QBrain_Class();


            //State string
            oQB.State.stateString = stringPositionStates.stateCenter;

            //Attributes
            DodgeBotAttributes dbA = new DodgeBotAttributes("False", "0", "0", "False", "False");
            oQB.State.state_attributes.Add(dbA.ObstaclePresent);
            oQB.State.state_attributes.Add(dbA.TargetDistance);
            oQB.State.state_attributes.Add(dbA.ObstacleSpeed);
            oQB.State.state_attributes.Add(dbA.LeftObstaclePresent);
            oQB.State.state_attributes.Add(dbA.RightObstaclePresent);

            //Actions - IMPORTANT OUR BRAIN HAS ACTION EXAMPLE SO IT CAN ADD THESE TO LEARNED STATES
            oQB.Actions.Add(dodgeStringActions.moveLeft);
            oQB.Actions.Add(dodgeStringActions.stayHere);
            oQB.Actions.Add(dodgeStringActions.moveRight);

            //Add to the actual list 
            botBrain.State_And_Actions.Add(oQB);

            //And of course, set the path
            botBrain.brainsSaveDirectory = brainSavePath;

            //Now we can check and initialize our brain
            botBrain.InitializeQBrain();

            //Add our script to our bot for later work
            dodgeBotPrefab.GetComponent<ExampleBotTrigger>().dodgeController = this;

            //Get our original rotation and position
            originalRotation = dodgeBotPrefab.transform.rotation;
            originalPosition = dodgeBotPrefab.transform.position;

            //Get the animator
            if (dodgeBotPrefab.GetComponent<Animator>() != null) { anim = dodgeBotPrefab.GetComponent<Animator>(); }


        }

        private void Update()
        {
            if (sessionRunning)
            {
                if (botHasNotBeenHit)
                {
                    MoveCurrentObstacles();

                    obstacleSpeedCountTiming += Time.deltaTime * (1.75f + objectSpeed);

                    if (obstacleSpeedCountTiming >= obstaclesInstantiatingSpeed)
                    {
                        Transform trans = InstiationPoints[Random.Range(0, InstiationPoints.Count)].transform;
                        GameObject newObs = Instantiate(obstaclePrefab);
                        Vector3 vPos = trans.position;
                        newObs.transform.position = vPos;
                        instantiatedObstacles.Add(newObs);

                        obstaclesInstantiatingSpeed = Random.Range(1.5f, 2.5f);
                        obstacleSpeedCountTiming = 0;
                    }


                }
            }
        }

        //Function to start the everything running (button click call)
        public void StartTraining()
        {
            //In case we are already running
            if (sessionRunning) { return; }

            sessionRunning = true;

            if (trainingSessionsCount <= 1)
            {
                objectSpeed = 0.25f;

                obstaclesInstantiatingSpeed = 1f;

                currentLane = stringPositionStates.stateCenter;

                //Set our decay interaction amount action counter
                botBrain.ResetDecayInteractionCounter();


                //Start Instantiating the obstacles
                //StartCoroutine(InstantiatingOurObstacles());

                //Start our Training Loop
                //StartCoroutine(ExampleTrainingLoop());

                //Start our bot choicesIE
                StartCoroutine(BotMovementIE());
            }
            else
            {
                StartCoroutine(TrainingSessions());
            }


        }

        public IEnumerator TrainingSessions()
        {
            sessionRunning = false;
            trainingCounter = 0;

            //Set our decay interaction amount action counter
            botBrain.ResetAnnealingEpisodeDecayCount();

            while (trainingCounter < trainingSessionsCount)
            {
                if (!sessionRunning)
                {
                    botHasNotBeenHit = true;

                    sessionRunning = true;

                    trainingCounter++;

                    objectSpeed = 0.15f;

                    obstaclesInstantiatingSpeed = 1f;

                    currentLane = stringPositionStates.stateCenter;

                    //Adjust our decay session amount episode counter
                    botBrain.UpdateEpisodeForSessionAnnealingDecay();

                    //Start Instantiating the obstacles
                    //StartCoroutine(InstantiatingOurObstacles());

                    //Start our Training Loop
                    //StartCoroutine(ExampleTrainingLoop());

                    //Start our bot choicesIE
                    StartCoroutine(BotMovementIE());
                    yield return new WaitForSeconds(0.001f);
                }

                yield return null;
            }

            yield break;
        }


        //-- Our Bots Controls for the AI

        //Our IEnumerator to keep calling our bots ray decision making at specified intervals
        public IEnumerator BotMovementIE()
        {
            //Make sure we set our not hit to true
            botHasNotBeenHit = true;

            //Safety Counter
            int safetyCounter = stepsToBeTaken;

            //Keep looping until we are hit or just want to stop (safety counter
            while (botHasNotBeenHit)
            {
                //Call the bots raycheck decision, and reward function
                Bot_DoYouThing();

                //Update our safety counter (like time in the game)
                safetyCounter--;

                //If the counter is done - end the session
                if (safetyCounter <= 0) { Debug.Log("Finished the entire run."); break; }
                if (sessionRunning == false) { break; }

                //Return the seconds
                yield return new WaitForSeconds(0.1f);

                //Call our animation false here
                if (anim != null) { anim.SetBool("Move", false); }

            }

            if (!botHasNotBeenHit) { Debug.Log("Bot has been hit."); }

            //Reset for another run
            ResetOurSession();

            sessionRunning = false;

            yield break;
        }

        //Our simple function for a raycast check, choice request, choice made
        public void Bot_DoYouThing()
        {
            //Set the bool before moving
            botHasNotBeenHit = true;

            //Current state lane is saved in a global variable 
            string laneBeforeMoves = currentLane;

            //Float for our distance checking
            float oldDist = 0, newDist = 0;

            //Using our Raycast to get values for our state class
            State_Class sC = StateAndAttributesRaycast(laneBeforeMoves, ref oldDist);

            //Now we can pass in our variables to the brains decision making center and return an action choice
            string actionChoosen = botBrain.MakeAChoice(sC.stateString, sC.state_attributes);

            //A float reward for updating our reward
            float reward = 0f;

            //Check our actions and move accordingly
            if (actionChoosen == dodgeStringActions.moveLeft)
            {
                //Tried to move to a non-exsistent position -
                if (laneBeforeMoves == stringPositionStates.stateLeft)
                {
                    reward -= 1.5f;
                    currentLane = laneBeforeMoves;
                }
                else if (laneBeforeMoves == stringPositionStates.stateRight)
                {
                    //In the right lane, move to the center
                    dodgeBotPrefab.transform.position = new Vector3(lanePositions[1].position.x, originalPosition.y, originalPosition.z);
                    currentLane = stringPositionStates.stateCenter;
                }
                else if (laneBeforeMoves == stringPositionStates.stateCenter)
                {
                    //In center lane move to the left lane
                    dodgeBotPrefab.transform.position = new Vector3(lanePositions[0].position.x, originalPosition.y, originalPosition.z);
                    currentLane = stringPositionStates.stateLeft;
                }

                //If there was no reason to move punish
                if (sC.state_attributes[0].value == "False")
                {
                    reward -= 0.5f;
                }
                else
                {
                    reward += 0.45f;
                }

                //If we had a value to the left and we moved to it
                if (sC.state_attributes[3].Value == "True")
                {
                    reward -= 0.45f;
                }

                //Call our animator
                if (anim != null) { anim.SetBool("Move", true); }

            }
            else if (actionChoosen == dodgeStringActions.stayHere)
            {
                //Stay in current position

                currentLane = laneBeforeMoves;

                //If there was no reason to move reward
                if (sC.state_attributes[0].value == "False")
                {
                    reward += 0.45f;
                }
                else
                {
                    reward -= 0.5f;
                }


            }
            else if (actionChoosen == dodgeStringActions.moveRight)
            {
                //Tried to move to a non-exsistent positions
                if (laneBeforeMoves == stringPositionStates.stateRight)
                {
                    reward -= 1.5f;
                    currentLane = laneBeforeMoves;
                }
                else if (laneBeforeMoves == stringPositionStates.stateLeft)
                {
                    //In the left lane, move to the center
                    dodgeBotPrefab.transform.position = new Vector3(lanePositions[1].position.x, originalPosition.y, originalPosition.z);
                    currentLane = stringPositionStates.stateCenter;
                }
                else if (laneBeforeMoves == stringPositionStates.stateCenter)
                {
                    //In center lane move to the right lane
                    dodgeBotPrefab.transform.position = new Vector3(lanePositions[2].position.x, originalPosition.y, originalPosition.z);
                    currentLane = stringPositionStates.stateRight;
                }

                //If there was no reason to move punish
                if (sC.state_attributes[0].value == "False")
                {
                    reward -= 0.5f;
                }
                else
                {
                    reward += 0.45f;
                }

                //Moved to the right right when there was something there
                if (sC.state_attributes[4].Value == "True")
                {
                    reward -= 0.45f;
                }

                //Call our animator
                if (anim != null) { anim.SetBool("Move", true); }

            }

            //Get our new state (old state is saved in the brain from when the choice is made, so doesn't need to added again
            sC = StateAndAttributesRaycast(currentLane, ref newDist);
            //Have we crossed into somethings path
            if (sC.state_attributes[0].value == "False")
            {
                reward += 0.45f;

            }
            else
            {
                //Check if our new oobject position is futher than our old objects position
                if (newDist > oldDist)
                {
                    reward += 0.45f;
                }
                else
                {
                    reward -= 0.5f;
                }
            }

            if (!botHasNotBeenHit) { reward -= 2f; }

            //And let's update our reward
            botBrain.UpdateReward(reward, sC, botHasNotBeenHit);



            //Won't save here, as we don't want to encounter a slow down from over saving
        }

        //Function for raycasting and reutrning a state class from what is seen/noted
        public State_Class StateAndAttributesRaycast(string ourState, ref float distance)
        {
            State_Class state = new State_Class();

            //First lets use a raycast to gather some attribute information
            bool obstacleInLane = false;
            float distanceFromTarget = 0f;
            float obstacleSpeed = 0f;
            bool obstacleToLeft = false;
            bool obstacleToRight = false;

            //A simple raycast check to set the variables above
            RaycastHit hit;
            if (Physics.Raycast(raySightPosition.position, dodgeBotPrefab.transform.TransformDirection(Vector3.forward), out hit, forwardRayDistance))
            {
                //To keep it simple, we are just using a tag check to see if there is an obstacle
                if (hit.transform.gameObject.tag == "Obstacle")
                {
                    //We hit, so we assign our values
                    obstacleInLane = true;
                    distanceFromTarget = hit.distance;
                    obstacleSpeed = objectSpeed;

                }
                else
                {
                    //Debug.DrawRay(transform.position, transform.TransformDirection(Vector3.forward) * 200f, Color.red);
                }

            }
            else
            {
                Debug.DrawRay(raySightPosition.position, dodgeBotPrefab.transform.TransformDirection(Vector3.forward) * forwardRayDistance, Color.white);
                //Debug.Log("Did not Hit");
            }

            //Raycast Left
            for (int i = 0; i < 2; i++)
            {
                float angle = 0;
                if (i == 0) { angle = -45f; }
                else { angle = -90f; }
                Vector3 direction = (Quaternion.Euler(0f, angle, 0f) * originalRotation) * Vector3.forward;
                if (Physics.Raycast(raySightPosition.position, direction, out hit, leftAndRightRayDistance))
                {
                    if (hit.transform.gameObject.tag == "Obstacle")
                    {
                        //We hit, so we assign our values
                        obstacleToLeft = true;
                    }
                }
                else
                {
                    Debug.DrawRay(raySightPosition.position, direction * leftAndRightRayDistance, Color.red);
                    //Debug.Log("Did not Hit");
                }


            }

            //Raycast Right
            for (int i = 0; i < 2; i++)
            {
                float angle = 0;
                if (i == 0) { angle = 45f; }
                else { angle = 90f; }
                Vector3 direction = (Quaternion.Euler(0f, angle, 0f) * originalRotation) * Vector3.forward;
                if (Physics.Raycast(raySightPosition.position, direction, out hit, leftAndRightRayDistance))
                {
                    if (hit.transform.gameObject.tag == "Obstacle")
                    {
                        //We hit, so we assign our values
                        obstacleToRight = true;
                    }
                }
                else
                {
                    Debug.DrawRay(raySightPosition.position, direction * leftAndRightRayDistance, Color.red);
                    //Debug.Log("Did not Hit");
                }
            }


            //Now lets add these to a new attributes class, and then a list
            DodgeBotAttributes dBA = new DodgeBotAttributes(obstacleInLane.ToString(), distanceFromTarget.ToString(), obstacleSpeed.ToString(), obstacleToLeft.ToString(), obstacleToRight.ToString());
            List<AttributesPair<string, string>> lAP = new List<AttributesPair<string, string>>();
            lAP.Add(dBA.ObstaclePresent);
            lAP.Add(dBA.TargetDistance);
            lAP.Add(dBA.ObstacleSpeed);
            lAP.Add(dBA.LeftObstaclePresent);
            lAP.Add(dBA.RightObstaclePresent);

            state.stateString = ourState;
            state.state_attributes = lAP;

            distance = distanceFromTarget;

            return state;
        }





        //-- Our Game's Controls for creating and moving obstacles

        //IE for calling Obstacles movements
        IEnumerator ExampleTrainingLoop()
        {
            //Keep going while our bot has not been hit
            while (botHasNotBeenHit)
            {
                MoveCurrentObstacles();

                yield return new WaitForSeconds(objectSpeed);
            }

            yield break;
        }

        //To move our obstacles down the runway
        public void MoveCurrentObstacles()
        {
            //Loop through the obstacles in our list
            foreach (GameObject obs in instantiatedObstacles)
            {
                //Move them down on the z position
                Vector3 v3 = new Vector3(obs.transform.position.x, obs.transform.position.y, obs.transform.position.z - objectSpeed);
                obs.transform.position = v3;
                //If we are passed a certain point - add to the destroy list
                if (obs.transform.position.z < InstiationPoints[0].transform.position.z - 50f) { obstToDestroy.Add(obs); }
            }

            //Destroy the obstacles in our list
            if (obstToDestroy.Count > 0)
            {
                obstaclesInstantiatingSpeed += 0.05f;
                objectSpeed += 0.001f;

                for (int i = 0; i < obstToDestroy.Count; i++)
                {
                    instantiatedObstacles.Remove(obstToDestroy[i]);
                    Destroy(obstToDestroy[i]);
                }
            }

            obstToDestroy.Clear();
        }

        //IE to Instantiate Our New Block
        IEnumerator InstantiatingOurObstacles()
        {
            float objectCounterTraining = 2f;
            obstacleSpeedCountTiming = objectCounterTraining;

            while (botHasNotBeenHit)
            {
                obstacleSpeedCountTiming += Time.deltaTime * obstaclesInstantiatingSpeed;

                if (obstacleSpeedCountTiming >= 2f)
                {
                    int r = Random.Range(1, 4);

                    for (int i = 0; i < r; i++)
                    {
                        obstacleSpeedCountTiming = 0f;
                        Transform trans = InstiationPoints[Random.Range(0, InstiationPoints.Count)].transform;
                        GameObject newObs = Instantiate(obstaclePrefab);
                        Vector3 vPos = trans.position;
                        newObs.transform.position = vPos;
                        instantiatedObstacles.Add(newObs);
                        yield return new WaitForSeconds(0.025f);
                    }



                    yield return new WaitForSeconds(0.001f);
                }

                yield return null;
            }

            yield break;
        }

        public void ResetOurSession()
        {
            //Destory our obstacles
            if (instantiatedObstacles.Count > 0)
            {
                for (int i = 0; i < instantiatedObstacles.Count; i++)
                {
                    Destroy(instantiatedObstacles[i]);
                }
            }

            instantiatedObstacles.Clear();

            //Place Player Back In Position
            dodgeBotPrefab.transform.position = lanePositions[1].position;
        }

    }
}
