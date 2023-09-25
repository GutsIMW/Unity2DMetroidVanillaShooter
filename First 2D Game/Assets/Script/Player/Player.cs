using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Player : MonoBehaviour
{
    public int maxHealth = 100;
    public int currentHealth;
    public HealthBar healthBar;
    public GameObject GameOverUI;

    // Start is called before the first frame update
    void Start()
    {
        currentHealth = maxHealth;
        healthBar.SetMaxHealth(maxHealth);
    }

    void OnCollisionEnter2D(Collision2D collision)
    {
        if(collision.gameObject.layer == LayerMask.NameToLayer("Enemies"))
        {
            takeDamage(20);
            Vector2 knockbackDirection = transform.position -  collision.gameObject.transform.position; // Use to knockback in the right direction
            Vector2 knockback = new Vector2(15f,5f) * knockbackDirection; // Determine the force with which the player will be knocked out
            GetComponent<PlayerMovement>().GetKnockback(knockback);
        }
    }

    void takeDamage(int damage)
    {
        currentHealth -= damage;
        healthBar.setHealth(currentHealth);

        StartCoroutine(HurtPlayer()); // C NUL, A SUPPRIMER ET A AMELIORER (PEUT ETRE PLUTOT UTILISER UNE ANIMATION)

        if(currentHealth <= 0) GameOver();
    }

    // Cette fonction (HurtPlayer()) est nul !!! à supprimer 
    IEnumerator HurtPlayer()
    {
        gameObject.GetComponentInChildren<SpriteRenderer>().color = new Color (0.8f, 0.2f, 0.2f, 1f);
        yield return new WaitForSeconds(0.3f);
        gameObject.GetComponentInChildren<SpriteRenderer>().color = Color.white;
    }

    public void GameOver()
    {
        GameOverUI.SetActive(true); // Active the GameOver Screen
    }
}
