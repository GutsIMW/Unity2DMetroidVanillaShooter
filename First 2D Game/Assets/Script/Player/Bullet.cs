using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Bullet : MonoBehaviour
{
    public float speed = 20f;
    public int damage = 20;
    public float lifeTime = 2f;
    public Rigidbody2D rb;
    public GameObject impactEffect;
    

    // Start is called before the first frame update
    void Start()
    {
        rb.velocity = transform.right * speed; // Move the bullet
        Destroy(gameObject, lifeTime);  // Destroy the bullet after lifeTime seconds
        FindObjectOfType<AudioManager>().Play("Bullet");
    }

    void OnTriggerEnter2D(Collider2D hitInfo)
    {
        Enemy enemy = hitInfo.GetComponent<Enemy>(); // Only not null if the collider is an Enemy
        if(enemy != null) enemy.takeDamage(damage); // If we touch an enemy then we deal damage to him

        if(!hitInfo.GetComponent<Bullet>()) // If we don't touch another bullet, then we destroy the current bullet and we instantiate an impact effect
        {
            Instantiate(impactEffect, transform.position, transform.rotation);
            Destroy(gameObject);
        }
    }

    /* Destroy the bullet if it goes over the camera

    void OnBecameInvisible()
    {
        enabled = false;
        Destroy(gameObject);
    }
    */
}
