using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
public class DiamondCollectible : MonoBehaviour
{
    [SerializeField] private string _colliderScript;

    [SerializeField] private UnityEvent _collisionEntered;
    [SerializeField] private UnityEvent _collisionExit;

    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (collision.gameObject.tag == "Player")
        {
            _collisionEntered?.Invoke();
            GlobalVars.playerScore += 1;
            Debug.Log(GlobalVars.playerScore);
        }
    }

    private void OnCollisionExit2D(Collision2D collision)
    {
        if (collision.gameObject.tag == "Player")
        {
            _collisionExit?.Invoke();
        }
    }

}
