using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ContactDamage : MonoBehaviour
{
    public int damage;
    public PlayerHealth playerHealth;
    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.gameObject.tag == "Player")
        {
            PlayerHealth.instance.TakeDamage(damage);
            Debug.Log("You took damage");
        }
    }

}
