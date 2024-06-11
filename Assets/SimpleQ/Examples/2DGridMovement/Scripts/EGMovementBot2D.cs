using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Random = UnityEngine.Random;
using QLearning;

namespace QLearningExample
{

    //Class for the grid states we want to add
    public class Grid_State_Class
    {
        public int mapId;
        public int steps;
        public string status;
        public int x;
        public int y;
    }

    public class Grid_Statuses
    {
        public string open = "Open";
        public string obstacle = "Obstacle";
        public string gold = "Gold";
        public string start = "Start";
        public string end = "End";
    }

    public class Grid_Movement_Actions
    {
        public const string forward = "Forward";
        public const string backward = "Backward";
        public const string left = "Left";
        public const string Right = "Right";
    }

    //Class for map ids for InitSeed
    public class MapIds
    {
        //We are using a ten numbers to create this (i.e. ten maps)
        public int mapId;
    }

    //Class to capture position choosen
    public class Grid_Position
    {
        public int x;
        public int y;
    }

    public class EGMovementBot2D : MonoBehaviour
    {
        [Header("Our Brain")]
        //Brain for movement
        public OpenQLearningBrain movementBrainExample;

        [Header("Cell Vairabels")]
        //Cell variables
        public GameObject grid_area;
        public GameObject cell_prefab;

        //Sizes
        private int grid_width = 9;
        private int grid_height = 9;

        //States for our grid cells (giving them all this grid cells id, so it can learn from it's 
        private List<Grid_State_Class> Grid_States_For_Map = new List<Grid_State_Class>();

        //Our Map Ids
        private MapIds map_Id;

        //Statuses strings (better than writing them out every time
        private Grid_Statuses grid_statuses = new Grid_Statuses();

        //For checking we are not trying to leave the map, and punishing for trying
        public Grid_Position currentPosition = new Grid_Position();
        public Grid_Position choosenPosition = new Grid_Position();

        //For start position Grid State
        public Grid_State_Class startPosition = new Grid_State_Class();
        public Grid_Position endPosition = new Grid_Position();

        //For our Current state
        public Grid_State_Class currentState = new Grid_State_Class();

        //For our bots movements
        public Grid_Movement_Actions movementChoices = new Grid_Movement_Actions();

        //For our state string
        private State_Class stateString;

        //Bool for movement
        private bool botMove;
        private bool successfulRun;
        private bool currentlyNavigating;
        public int runTest;
        private int runsSoFar;

        //Bot speed
        public float botSpeed;

        public void Start()
        {
            //Add an example state and action for the bot to use when learning
            AddBaseStateAndActions();

            //Generate a room from a randomn seed
            GenerateARoomFromSeed();

            //Load the brain if not loaded
            if (!movementBrainExample.loaded)
            {
                movementBrainExample.InitializeQBrain();
            }

        }

        //Function for adding variables to brain states and actions
        public void AddBaseStateAndActions()
        {
            //Return if the list already has variables (shouldn't, becuase I don't want to fill them in the inspector)
            if (movementBrainExample.State_And_Actions.Count > 0) { return; }

            //Create an open brain
            Open_QBrain_Class oB = new Open_QBrain_Class();

            //Get an example state for our example state
            Grid_State_Class gS = new Grid_State_Class();

            //Add example variables
            gS.mapId = -1;
            gS.steps = 0;
            gS.status = "Status";
            gS.x = 0;
            gS.y = 0;

            //Create a state string for it
            stateString = CreateOurStateString(gS);

            //Add the state
            oB.State.stateString = "";

            //Add actions
            oB.Actions = new List<string>();
            oB.Actions.Add(Grid_Movement_Actions.left);
            oB.Actions.Add(Grid_Movement_Actions.Right);
            oB.Actions.Add(Grid_Movement_Actions.backward);
            oB.Actions.Add(Grid_Movement_Actions.forward);

            //These variables to our brain list to give it an example to build a brain from
            movementBrainExample.State_And_Actions.Add(oB);


        }

        //Generate game from seed
        public void GenerateARoomFromSeed()
        {
            //Clear everything first
            Grid_States_For_Map.Clear();
            startPosition = new Grid_State_Class();

            //Generate one of our map IDs
            map_Id = ReturnAMapId();

            //Set our random state seed
            Random.InitState(map_Id.mapId);

            //Build the grid
            BuildAGrid();


            currentlyNavigating = false;


        }

        //Function for building a grid
        public void BuildAGrid()
        {
            //Have cheated and placed a grid component onto the grid_area,
            //so we can simply do the following:

            //Lists for Used positions/gold/obstacles
            List<Grid_Position> UnusablePositions = new List<Grid_Position>();
            List<Grid_Position> obsPos = new List<Grid_Position>();
            List<Grid_Position> goldPos = new List<Grid_Position>();
            //For Start and End
            Grid_Position start_pos = new Grid_Position();
            Grid_Position end_pos = new Grid_Position();
            //For obstacle and gold positions
            Grid_Position other_pos = new Grid_Position();


            //Random end position 
            end_pos.x = Random.Range(0, grid_width);
            end_pos.y = Random.Range(grid_height - 2, grid_height);

            //Random start position 
            start_pos.x = Random.Range(0, grid_width);
            start_pos.y = Random.Range(0, 3);

            //Debug.Log(start_pos.x + " , " + start_pos.y);
            //Debug.Log(end_pos.x + " , " + end_pos.y);

            UnusablePositions.Add(start_pos);
            UnusablePositions.Add(end_pos);

            //Get our gold and obstacle positions
            int tries = 0;
            int rObstacles = Random.Range(8, 15);
            for (int i = 0; i < rObstacles; i++)
            {
                other_pos = new Grid_Position();
                other_pos.x = Random.Range(1, grid_width);
                other_pos.y = Random.Range(1, grid_height);
                foreach (Grid_Position gP in UnusablePositions)
                {
                    if (gP.x == other_pos.x && gP.y == other_pos.y)
                    {
                        if (tries < 1000)
                        {
                            i--;
                            tries++;
                            other_pos = null;
                            break;
                        }
                        else
                        {
                            other_pos = null;
                            break;
                        }
                    }
                }
                if (other_pos != null)
                {
                    UnusablePositions.Add(other_pos);
                    obsPos.Add(other_pos);
                }
            }

            tries = 0;
            int goldToPlace = Random.Range(2, 7);
            for (int i = 0; i < goldToPlace; i++)
            {
                other_pos = new Grid_Position();
                other_pos.x = Random.Range(0, grid_width);
                other_pos.y = Random.Range(0, grid_height);
                foreach (Grid_Position gP in UnusablePositions)
                {
                    if (gP.x == other_pos.x && gP.y == other_pos.y)
                    {
                        if (tries < 1000)
                        {
                            i--;
                            tries++;
                            other_pos = null;
                            break;
                        }
                        else
                        {
                            other_pos = null;
                            break;
                        }
                    }
                }
                if (other_pos != null)
                {
                    UnusablePositions.Add(other_pos);
                    goldPos.Add(other_pos);
                }
            }

            //Instantiate all the objects
            for (int i = 0; i < grid_height; i++)
            {
                for (int j = 0; j < grid_width; j++)
                {
                    GameObject c = Instantiate(cell_prefab);
                    c.transform.SetParent(grid_area.transform);

                    Grid_State_Class grid_State_Class = new Grid_State_Class();
                    grid_State_Class.x = j;
                    grid_State_Class.y = i;
                    grid_State_Class.status = grid_statuses.open;

                    //End position
                    //If this is our random end position
                    if (j == end_pos.x && i == end_pos.y)
                    {
                        //End position
                        Image img = GridCellImage(j, i);
                        img.color = Color.blue;
                        grid_State_Class.status = grid_statuses.end;
                        endPosition = new Grid_Position();
                        endPosition.x = j;
                        endPosition.y = i;
                    }
                    //Debug.Log(i + "," + j);
                    //Start position
                    //If this is our random start position
                    if (i == start_pos.y && j == start_pos.x)
                    {

                        Image img = GridCellImage(j, i);
                        img.color = Color.green;
                        grid_State_Class.status = grid_statuses.start;
                        startPosition = grid_State_Class;
                    }

                    //Check our lists to see if it is a obstacle or gold cell
                    foreach (Grid_Position ob in obsPos)
                    {
                        if (ob.x == j && ob.y == i)
                        {
                            Image img = GridCellImage(j, i);
                            img.color = Color.red;
                            grid_State_Class.status = grid_statuses.obstacle;
                            break;
                        }
                    }
                    foreach (Grid_Position g in goldPos)
                    {
                        if (g.x == j && g.y == i)
                        {
                            Image img = GridCellImage(j, i);
                            img.color = Color.yellow;
                            grid_State_Class.status = grid_statuses.obstacle;
                            break;
                        }
                    }

                    Grid_States_For_Map.Add(grid_State_Class);

                }

            }


        }



        //COroutine for bot to move through grid
        public IEnumerator MoveThroughGrid()
        {
            //Set our sorted value to false, that way we can speed up the bot training
            movementBrainExample.sortedOutput = false;
            //Get our current state, positions, and update steps
            currentState = GetCurrentGridState(startPosition.x, startPosition.y);
            currentPosition.x = startPosition.x;
            currentPosition.y = startPosition.y;
            currentState.steps = 20;


            //Set our movement step bool, and navigatind bool to true
            botMove = true;
            currentlyNavigating = true;

            //Loop though steps
            while (currentState.steps > 0)
            {
                //CHeck if bot can move
                if (botMove)
                {
                    //Move out bot
                    botMove = false;
                    StartCoroutine(BotNavigate(currentPosition.x, currentPosition.y));

                }

                //If we have succeeded, bteak
                if (successfulRun) { break; }

                yield return null;
            }

            //Update Episodes if we are annealing the decay
            if (movementBrainExample.temperatureAnnealingDecay || movementBrainExample.epsilonAnnealingDecay)
            {
                movementBrainExample.UpdateEpisodeForSessionAnnealingDecay();
            }

            //If we haven't succeeded from a run, we want to run again
            runsSoFar++;
            if (runsSoFar < runTest && !successfulRun)
            {
                StartCoroutine(MoveThroughGrid());
                yield break;
            }

            //Else lets get our info
            if (runsSoFar >= runTest || successfulRun)
            {
                if (successfulRun) { Debug.Log("Run Successful" + " || " + runsSoFar + " runs so far."); }
                else { Debug.Log("Unsuccessful run."); }

                botMove = false;
                currentlyNavigating = false;

                yield break;
            }

            botMove = false;
            currentlyNavigating = false;

            //Set our sorted back to true (just in case)
            movementBrainExample.sortedOutput = true;

            yield break;

        }


        public IEnumerator BotNavigate(int x, int y)
        {
            //Instaniate new position objeects
            choosenPosition = new Grid_Position();
            currentPosition = new Grid_Position();

            //Get our position
            currentPosition.x = x;
            currentPosition.y = y;

            //Get current grid state - Done before navigation, and when a move is actuall made
            currentState = GetCurrentGridState(x, y);
            currentState.mapId = map_Id.mapId;

            //Get image original colour before changing it
            Image img = GridCellImage(currentPosition.x, currentPosition.y);
            Color ogCol = img.color;

            //Change Color to player color to show we are here
            img.color = Color.black;

            //Make a choice
            stateString = CreateOurStateString(currentState);
            string choice = movementBrainExample.MakeAChoice(stateString.stateString, stateString.state_attributes);

            //Declare reward variable for our reward
            float reward = 0.0f;


            //Get our new position
            switch (choice)
            {
                case Grid_Movement_Actions.left:
                    choosenPosition.x = x - 1;
                    choosenPosition.y = y;
                    break;
                case Grid_Movement_Actions.Right:
                    choosenPosition.x = x + 1;
                    choosenPosition.y = y;
                    break;
                case Grid_Movement_Actions.forward:
                    choosenPosition.x = x;
                    choosenPosition.y = y + 1;
                    break;
                case Grid_Movement_Actions.backward:
                    choosenPosition.x = x;
                    choosenPosition.y = y - 1;
                    break;

            }

            //Check we have choosen a valid direction (i.e - not out of bounds - may obstacles later)
            bool outOfBounds = false;
            outOfBounds = TryingToMoveOutOfBounds(choosenPosition.x, choosenPosition.y);

            yield return new WaitForSeconds(botSpeed);

            if (outOfBounds)
            {
                choosenPosition = currentPosition;
                //Debug.Log(currentPosition.x + " , " + currentPosition.y + " 2 ");
                reward = -0.5f;
                currentState.steps -= 1;
            }
            else
            {
                //Check to see if we have moved closer
                int chosXDif = Mathf.Abs(choosenPosition.x - endPosition.x);
                int oldxDif = Mathf.Abs(currentPosition.x - endPosition.x);
                int chosYDif = Mathf.Abs(choosenPosition.y - endPosition.y);
                int oldYDif = Mathf.Abs(currentPosition.y - endPosition.y);
                if (chosXDif > oldxDif)
                {
                    //We have moved futher away
                    reward -= 0.25f;
                }
                else if (chosXDif < oldxDif)
                {
                    //We have moved closer
                    reward += 0.25f;
                }

                if (chosYDif > oldYDif)
                {
                    //We have moved futher away
                    reward -= 0.25f;
                }
                else if (chosYDif < oldYDif)
                {
                    //We have moved closer
                    reward += 0.25f;
                }

                //Update our grid state and reset our current grid color
                Grid_State_Class newState = new Grid_State_Class();
                newState = GetNextGridState(choosenPosition.x, choosenPosition.y);
                newState.steps = currentState.steps - 1;
                currentState = newState;
                currentPosition = choosenPosition;
                reward += RewardsUpdate(currentState);
            }

            //Get our current state string for updating
            currentState.mapId = map_Id.mapId;
            stateString = CreateOurStateString(currentState);

            //Been getting errornous states (Fixed but left this in) of states with no actions being saved
            //The instantces above fixed this - remember, if the brain can't access something/find something
            //It means you have put it in incorrectly - check your brain for missing actions/other errors
            if (currentState != null)
            {
                //Debug.Log(reward);
                //Update QLearning Rewards Values
                movementBrainExample.UpdateReward(reward, stateString, successfulRun);

                //Save - with the new batch learning, this may be slower. Save is now called after every batch run
                //movementBrainExample.Save_Open_QBrain();
            }

            //Change the colot back to the original
            img.color = ogCol;

            //Repeat this again
            botMove = true;

            yield break;
        }



        //Function for updating rewards amount
        public float RewardsUpdate(Grid_State_Class gridState)
        {
            float floatToReturn = 0.0f;

            if (gridState.steps <= 0)
            {
                floatToReturn -= 0.5f;
            }
            else
            {
                floatToReturn -= 0.1f;
            }

            if (gridState.status == grid_statuses.obstacle)
            {
                floatToReturn -= 0.3f;
            }
            else if (gridState.status == grid_statuses.gold)
            {
                floatToReturn += 0.15f;
            }
            else if (gridState.status == grid_statuses.end)
            {
                floatToReturn += 2f;
                successfulRun = true;
            }
            else
            {
                floatToReturn += 0.125f;
                //Nothing
            }


            return floatToReturn;
        }


        //Get our MapId
        public MapIds ReturnAMapId()
        {
            MapIds mapIds = new MapIds();

            System.Random rand = new System.Random();
            mapIds.mapId = rand.Next(0, 11);
            //Debug.Log(mapIds.mapId);

            return mapIds;
        }


        //Grid postion functions
        public Grid_State_Class GetCurrentGridState(int x, int y)
        {
            Grid_State_Class gsClass = null;
            foreach (Grid_State_Class gSC in Grid_States_For_Map)
            {
                if (gSC.x == x && gSC.y == y)
                {
                    gsClass = gSC;
                    break;
                }
            }




            return gsClass;
        }
        public Grid_State_Class GetNextGridState(int x, int y)
        {
            Grid_State_Class gsClass = null;
            foreach (Grid_State_Class gSC in Grid_States_For_Map)
            {
                if (gSC.x == x && gSC.y == y)
                {
                    gsClass = gSC;
                    break;
                }
            }



            return gsClass;
        }


        //Simple Function to return the image of a cell we are on
        public Image GridCellImage(int x, int y)
        {

            int ind = ((y * grid_height) + x);
            //Debug.Log(ind);
            Image img = grid_area.transform.GetChild(ind).gameObject.GetComponent<Image>();

            return img;

        }


        //Function for checking we are still in bounds
        public bool TryingToMoveOutOfBounds(int x, int y)
        {
            //Debug.Log(x + " , " + y);
            if (x < 0 || x >= grid_width || y < 0 || y >= grid_height)
            {
                return true;
            }
            else
            {
                return false;
            }
        }


        //Functions for states and actions 
        public State_Class CreateOurStateString(Grid_State_Class gridState)
        {
            string s = string.Empty;

            s += gridState.mapId;
            s += " , " + gridState.steps;
            s += " , " + gridState.status;
            s += " , " + gridState.x;
            s += " , " + gridState.y;

            List<AttributesPair<string, string>> n = new List<AttributesPair<string, string>>();

            State_Class sC = new State_Class();
            sC.stateString = s;
            sC.state_attributes = n;

            return sC;

        }


        //Button Click Functions
        public void CreateANewMap()
        {
            if (currentlyNavigating) { return; }

            currentlyNavigating = true;

            StartCoroutine(CreateNewMap());

        }

        public IEnumerator CreateNewMap()
        {
            List<GameObject> toDestory = new List<GameObject>();
            foreach (Transform g in grid_area.transform)
            {
                toDestory.Add(g.gameObject);
            }
            for (int i = toDestory.Count - 1; i >= 0; i--)
            {
                Destroy(toDestory[i]);
            }

            yield return new WaitForSeconds(1f);

            GenerateARoomFromSeed();
            yield break;
        }

        public void StartRunning()
        {
            if (currentlyNavigating) { return; }

            //Set the Episodes counter to 0 if we are annealing the decay
            if (movementBrainExample.temperatureAnnealingDecay || movementBrainExample.epsilonAnnealingDecay)
            {
                Debug.Log("Here.");
                movementBrainExample.ResetAnnealingEpisodeDecayCount();
            }

            currentlyNavigating = true;
            runsSoFar = 0;
            successfulRun = false;
            StartCoroutine(MoveThroughGrid());
        }
    }
}