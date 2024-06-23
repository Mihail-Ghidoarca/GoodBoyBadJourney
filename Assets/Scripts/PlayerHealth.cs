using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class PlayerHealth : MonoBehaviour
{
    public static PlayerHealth instance;
    public Animator animator;
    public int maxHealth = 100;
    public int currentHealth;
    public Rigidbody2D rb;
    public float knockbackForce = 1f;

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
        Vector2 knockbackDirection = (Vector2)transform.position + new Vector2(10f, 10f);
        rb.AddForce(knockbackForce * knockbackDirection, ForceMode2D.Impulse);
        if (currentHealth <= 0)
        {
            animator.SetTrigger("isDead");
            Destroy(gameObject);
        }
    }

    

}
