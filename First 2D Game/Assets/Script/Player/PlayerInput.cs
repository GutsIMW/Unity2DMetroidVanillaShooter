using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent (typeof(PlayerMovement))]
public class PlayerInput : MonoBehaviour
{
    PlayerMovement playerMovement;
    Animator animator;
    Weapon weapon;

    // Start is called before the first frame update
    void Start()
    {
        playerMovement = GetComponent<PlayerMovement>();
        animator = GetComponentInChildren<Animator>();
        weapon = GetComponent<Weapon>();
    }

    // Update is called once per frame
    void Update()
    {
        if(PauseMenu.GetGameIsPaused()) return; // If game is paused, then we can't shoot or attack

        Vector2 directionalInput = new Vector2(Input.GetAxisRaw("Horizontal"), Input.GetAxisRaw("Vertical"));
        playerMovement.SetDirectionnalInput(directionalInput);

        if(Input.GetButtonDown("Jump"))
        {
            playerMovement.OnJumpInputDown();
        }
        if(Input.GetButtonUp("Jump"))
        {
            playerMovement.OnJumpInputUp();
        }

        if(Input.GetButton("Fire1"))
        {
            weapon.Shoot();
            animator.SetBool("IsShooting", true);
        }
        else if(Input.GetButtonUp("Fire1"))
        {
            animator.SetBool("IsShooting", false);
        }
        else if(Input.GetKeyDown(KeyCode.LeftAlt)) // We can melee attack only if we're not already shooting
        {
            weapon.Melee();
        }

        

        if(Input.GetKeyDown(KeyCode.R)) GetComponent<Player>().GameOver();
    }
}
