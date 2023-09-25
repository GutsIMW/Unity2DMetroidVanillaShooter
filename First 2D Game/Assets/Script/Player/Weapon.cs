using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent (typeof(PlayerMovement))]
public class Weapon : MonoBehaviour
{

    public LayerMask enemyLayer;
    PlayerMovement playerMovement;
    Controller2D controller;
    public Transform firePointStand;
    public Transform firePointCrouch;
    public Transform firePointWallSliding;
    public GameObject bulletPrefab;
    public float fireRate = 0.1f; // Time between two fire
    float nextFireTime = 0f; // time left before next fire
    public Animator animator;
    public Transform meleePoint;
    Vector2 meleeRange = new Vector2(1.58f, 3.16f); // la technique pour visualiser la capsule c'est de mettre le component capsule collider sur le meleePoint et de régler dans l'editeur puis copy paste les valeurs
    public int meleeDamage = 40;
    public Vector2 meleeKnockback = new Vector2(700f, 0f);
    private float meleeRate;
    float nextMeleeTime = 0f;


    void Start()
    {
        RuntimeAnimatorController animatorController = gameObject.GetComponentInChildren<Animator>().runtimeAnimatorController;
        foreach(AnimationClip clip in animatorController.animationClips)
        {
            if(clip.name == "PlayerMelee") meleeRate = clip.length; // Melee rate must be equal or superior to melee animation
        }


        playerMovement = GetComponent<PlayerMovement>();
        controller = GetComponent<Controller2D>();
    }

    void Update()
    {
        nextFireTime -= Time.deltaTime;
        nextMeleeTime -= Time.deltaTime;
    }



    public void Melee()
    {
        if(nextMeleeTime > 0f || !controller.collisions.below) return; // If we're already melee attacking or if we're in the air, then we can't melee attack
        nextMeleeTime = meleeRate;

        // Play a melee animation and a sound effect and block the player movement
        animator.SetTrigger("Melee"); // Trigger the animation
        FindObjectOfType<AudioManager>().Play("Melee"); // Play the Sound

        RuntimeAnimatorController animatorController = gameObject.GetComponentInChildren<Animator>().runtimeAnimatorController;
        float animationLength = -1f;
        foreach(AnimationClip clip in animatorController.animationClips)
        {
            if(clip.name == "PlayerMelee") animationLength = clip.length; // We get the animation length to block movement during melee attack animation
        }
        if(animationLength == -1f) Debug.Log("Error: PlayerMelee Animation not found");
        playerMovement.BlockMovement(animationLength);

        // la technique pour visualiser la capsule créé c'est de mettre le component capsule collider sur le meleePoint et de régler dans l'editeur puis copy paste les valeurs
        Collider2D[] hitEnemies = Physics2D.OverlapCapsuleAll(meleePoint.position, meleeRange, CapsuleDirection2D.Vertical, 0f, enemyLayer);
        
        // Damage the enemies
        foreach(Collider2D enemy in hitEnemies)
        {
            Debug.Log("We hit " + enemy.name);
            Enemy tmp = enemy.GetComponent<Enemy>();
            if(tmp != null) tmp.takeDamage(meleeDamage, meleeKnockback);
        }
    }

    public void Shoot()
    {
        if(nextFireTime > 0f) return;
        nextFireTime = fireRate;

        // Shooting logic
        if(playerMovement.isCrouching && controller.collisions.below) Instantiate(bulletPrefab, firePointCrouch.position, firePointCrouch.rotation); // If the player is crouching
        else if(playerMovement.wallSliding) Instantiate(bulletPrefab, firePointWallSliding.position, firePointWallSliding.rotation);
        else Instantiate(bulletPrefab, firePointStand.position, firePointStand.rotation); // If not
    }
}
