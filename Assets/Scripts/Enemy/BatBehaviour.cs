using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BatBehaviour : MonoBehaviour, IDamageable
{
    [field: SerializeField] public float MaxHealth { get; set; }
    public float speed;
    private GameObject player;
    public bool chase = false;
    public Transform startingPoint;
    public Animator animator;
    public bool HasTakenDamage { get; set; }
    public float CurrentHealth { get; set; }

    private Rigidbody2D RB;
    

    private void Start()
    {
        player = GameObject.FindGameObjectWithTag("Player");
        CurrentHealth = MaxHealth;
    }

    private void Update()
    {
        if(player == null)
        {
            return;
        }
        if (chase == true)
            Chase();
        else
        {
            animator.SetBool("isChasing", false);
            ReturnToStartPoint();
        }
        Flip();
    }

    private void Chase()
    {
        animator.SetBool("isChasing", true);
        transform.position = Vector2.MoveTowards(transform.position, player.transform.position, speed * Time.deltaTime);
    }

    private void ReturnToStartPoint()
    {
        transform.position = Vector2.MoveTowards(transform.position, startingPoint.position, speed * Time.deltaTime);
    }

    private void Flip()
    {
        if (transform.position.x < player.transform.position.x)
        {
            transform.rotation = Quaternion.Euler(0, 0, 0);
        }
        else
            transform.rotation = Quaternion.Euler(0, 180, 0);
    }

    public void TakeDamage(float damageAmount)
    {
        HasTakenDamage = true;
        animator.SetTrigger("Hurt");
        CurrentHealth -= damageAmount;
        if (CurrentHealth <= 0f)
        {
            animator.SetTrigger("Death");
            gameObject.SetActive(false);
            Die();
        }
    }

    public void Die()
    {
        Destroy(gameObject);
    }

}
