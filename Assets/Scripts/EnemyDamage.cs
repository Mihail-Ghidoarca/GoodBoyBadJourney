using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemyDamage : MonoBehaviour
{
    public int damage;
    public PlayerHealth playerHealth;
    private float delay;
    private void OnCollisionEnter2D(Collision2D collision)
    {
        delay = Time.deltaTime;
        if (collision.gameObject.tag == "Player") 
        {
            playerHealth.TakeDamage(damage);
            delay = 0;
        }
    }


}
