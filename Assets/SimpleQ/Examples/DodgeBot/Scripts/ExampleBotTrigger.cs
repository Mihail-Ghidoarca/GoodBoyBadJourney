using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace QLearningExample
{

    public class ExampleBotTrigger : MonoBehaviour
    {
        [HideInInspector] public bool hasNotBeenHit;
        [HideInInspector] public DodgeBotExample dodgeController;

        // Start is called before the first frame update
        void Start()
        {
            hasNotBeenHit = true;
        }


        public void OnTriggerEnter(Collider col)
        {
            if (col.gameObject.tag == "Obstacle")
            {
                hasNotBeenHit = false;
                dodgeController.botHasNotBeenHit = false;
            }
        }

    }
}
