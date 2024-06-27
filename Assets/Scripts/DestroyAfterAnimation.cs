using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DestroyAfterAnimation : MonoBehaviour
{
    public float delay = 3f;
    public Animator animator;
    public int damage;
    public bool playerWasHit;
    void Start()
    {
        Destroy(gameObject, GetComponent<Animator>().GetCurrentAnimatorStateInfo(0).length + delay);
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        SetPlayerWasHitToFalse();
        Debug.Log(collision.gameObject.tag);
        if (collision.gameObject.tag == "Player")
        {
            PlayerHealth.instance.TakeDamage(damage);
            Debug.Log("You took damage");
            Destroy(gameObject);
        }
    }

    void SetPlayerWasHitToFalse()
    {
        playerWasHit = false;
    }

}
