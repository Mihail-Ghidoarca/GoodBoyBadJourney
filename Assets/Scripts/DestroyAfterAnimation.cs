using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DestroyAfterAnimation : MonoBehaviour
{
    public float delay = 0f;
    public Animator animator;
    public int damage;
    void Start()
    {
        Destroy(gameObject, GetComponent<Animator>().GetCurrentAnimatorStateInfo(0).length + delay);
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if(collision.gameObject.tag == "Ground")
        {
            Destroy(gameObject);
        }

        if (collision.gameObject.tag == "Player")
        {
            PlayerHealth.instance.TakeDamage(damage);
            Debug.Log("You took damage");
            Destroy(gameObject);
        }
    }

}
