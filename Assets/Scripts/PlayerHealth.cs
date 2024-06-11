using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerHealth : MonoBehaviour
{
    public static PlayerHealth instance;
    public Animator animator;
    public int maxHealth = 100;
    public int currentHealth;

    private void Awake()
    {
        instance = this;
    }

    void Start()
    {
        currentHealth = maxHealth;
    }

    public void TakeDamage(int damage)
    {
        currentHealth -= damage;
        animator.SetTrigger("hit");
        if (currentHealth <= 0)
        {
            animator.SetTrigger("isDead");
            Destroy(gameObject);
        }
    }
}
