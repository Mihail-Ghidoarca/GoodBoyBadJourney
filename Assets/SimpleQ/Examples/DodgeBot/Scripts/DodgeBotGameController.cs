using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace QLearningExample
{
    public class DodgeBotGameController : MonoBehaviour
    {
        //Our Dodge bots to run
        [SerializeField]
        public List<DodgeBotExample> dodgeBots = new List<DodgeBotExample>();

        //Amount of runs to be split between them
        public int amountOfTries = 1000;

        public void StartDodgeBotRun()
        {
            if (dodgeBots.Count <= 0)
            {
                foreach (Transform child in transform)
                {
                    if (child.GetComponent<DodgeBotExample>() != null && child.gameObject.activeInHierarchy) { dodgeBots.Add(child.GetComponent<DodgeBotExample>()); }
                }
            }

            int amountPerAgent = amountOfTries / dodgeBots.Count;

            foreach (var d in dodgeBots)
            {

                d.trainingSessionsCount = amountPerAgent;
                d.gameObject.GetComponent<OpenQLearningBrain>().epsilonDynamicDecayTotalSessionValue = amountPerAgent;
                d.StartTraining();
            }
        }

    }
}
