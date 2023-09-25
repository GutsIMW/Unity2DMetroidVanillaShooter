using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Enemy : MonoBehaviour
{
    public int health;
    public GameObject deathEffect;
    public Transform player;

    public void takeDamage(int damage)
    {
        health -= damage;
        StartCoroutine(HurtEnemie());
        if(health <= 0) die();
    }

    // Deal damage and knockback
    public void takeDamage(int damage, Vector2 knockback)
    {
        takeDamage(damage);
        // Vector2 knockbackDirection = transform.position - player.position; // Use to knockback in the right direction
        // gameObject.GetComponent<Rigidbody2D>().AddForce(knockback * knockbackDirection.normalized);
    }

    // Color the enemy in red when hited
    IEnumerator HurtEnemie()
    {
        SpriteRenderer gfx = gameObject.GetComponentInChildren<SpriteRenderer>();
        if(gfx == null) yield return null; // Si pas de SpriteRender alors on arrête la coroutine

        gfx.color = new Color (0.8f, 0.2f, 0.2f, 1f); // On rougit l'ennemie
        yield return new WaitForSeconds(0.2f);
        gameObject.GetComponentInChildren<SpriteRenderer>().color = Color.white; // On le remet dans sa couleur habituelle
    }

    void die()
    {
        Instantiate(deathEffect, transform.position, Quaternion.identity);
        Destroy(gameObject);
    }
}
