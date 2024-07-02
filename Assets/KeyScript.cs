using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
public class KeyScript : MonoBehaviour
{
    [SerializeField] private string _colliderScript;

    [SerializeField] private UnityEvent _collisionEntered;
    [SerializeField] private UnityEvent _collisionExit;

    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (collision.gameObject.tag == "Player")
            _collisionEntered?.Invoke();
    }

    private void OnCollisionExit2D(Collision2D collision)
    {
        if(collision.gameObject.tag == "Player")
        {
            _collisionExit?.Invoke();
        }
    }

}
