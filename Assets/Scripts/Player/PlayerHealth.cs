using System.Collections;
using System.Collections.Generic;
using System.Threading;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
public class PlayerHealth : MonoBehaviour
{
    public Image healthBar;
    public static PlayerHealth instance;
    public Animator animator;
    public int maxHealth = 100;
    public int currentHealth;
    public Rigidbody2D rb;
    public float knockbackForce = 1f;
    public float playerDamageDelay;
    public float lastTimeDamaged = 1;
    private void Awake()
    {
        instance = this;
    }

    void Start()
    {
        currentHealth = maxHealth;
    }

    private void Update()
    {
        playerDamageDelay += Time.deltaTime;
    }

    public void TakeDamage(int damage)
    {
        if (playerDamageDelay > lastTimeDamaged)
        {
            playerDamageDelay = 0;
            currentHealth -= damage;
            healthBar.fillAmount = currentHealth / 150f;
            animator.SetTrigger("hit");
            Vector2 knockbackDirection = (Vector2)transform.position + new Vector2(10f, 10f);
            rb.AddForce(knockbackForce * knockbackDirection, ForceMode2D.Impulse);
        }

        if (currentHealth <= 0)
        {
            animator.SetTrigger("isDead");
            gameObject.gameObject.SetActive(false);
            Destroy(gameObject);
            Time.timeScale = 0;
            SceneManager.LoadScene("FirstZone", LoadSceneMode.Single);
            Time.timeScale = 1;
        }

    }

}
