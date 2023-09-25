using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerMovementOldVersion : MonoBehaviour
{

    public CharacterController2D controller;
    public Animator animator;
    private bool canMove = true;
    public float runSpeed = 40f;

    float horizontalMove = 0f;
    public bool jump = false;
    public bool crouch = false; // Public because we use it on the Weapon.cs script
    public bool shoot = false; // Needed for the run animation

    // Update is called once per frame
    void Update()
    {
        if(PauseMenu.GetGameIsPaused()) return; // If game is paused, then we can't move

        horizontalMove = Input.GetAxisRaw("Horizontal") * runSpeed;

        animator.SetFloat("Speed", Mathf.Abs(horizontalMove));

        if(Input.GetButtonDown("Jump"))
        {
            jump = true;
            animator.SetBool("IsJumping", true);
        }

        if(Input.GetButtonDown("Crouch")) crouch = true;
        else if(Input.GetButtonUp("Crouch")) crouch = false;

        if(Input.GetButtonDown("Fire1")) animator.SetBool("IsShooting", true);
        else if(Input.GetButtonUp("Fire1")) animator.SetBool("IsShooting", false);
    }

    public void OnLanding()
    {
        animator.SetBool("IsJumping", false);
    }

    public void OnCrouching(bool isCrouching)
    {
        animator.SetBool("IsCrouching", isCrouching);
    }

    void FixedUpdate()
    {
        if(canMove) controller.Move(horizontalMove * Time.fixedDeltaTime, crouch, jump);
        else controller.Move(0, false, false); // If we can't move, we do this to stop applied forces
        jump = false;
    }

    // Block movement for a given amount of time
    public void BlockMovement(float time)
    {
        StartCoroutine(BlockMovementCoroutine(time));
    }

    IEnumerator BlockMovementCoroutine(float time)
    {
        canMove = false; // We block movement
        yield return new WaitForSeconds(time); // Wait time amount of second
        canMove = true; // We unblock movement
    }
}
