using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/*

Player movement from Sebastian Lague tutorials about "Creating a 2D Platformer" video serie
https://www.youtube.com/watch?v=MbWK8bCAU2w&list=PLFt_AvWsXl0f0hqURlhyIoAabKPgRsqjz&index=2 

*/

[RequireComponent (typeof(Controller2D))]
public class PlayerMovement : MonoBehaviour
{

    public bool canMove = true;

    public float maxJumpHeight = 4; // Height in Unity unit that the player can jump
    public float minJumpHeight = 1;
    public float timeToJumpApex = .4f; // Time to reach the apex of our jump
    float accelerationTimeAirborne = 0.1f;
    float accelerationTimeGrounded = 0.1f;
    public float moveSpeed = 6;
    public Vector2 wallJumpClimb;
    public Vector2 wallJumpOff;
    public Vector2 wallLeap;
    public float wallSlideSpeedMax = 3;
    public float wallStickTime = .3f;
    float timeToWallUnstick;

    float gravity;
    float maxJumpVelocity;
    float minJumpVelocity;
    Vector3 velocity; // A vector3 cause we work with the transform which use a Vector3 (x,y and z)
    float velocityXSmoothing;

    Controller2D controller;
    Animator animator;
    private SpriteRenderer playerSprite;
    public Transform rotationPoint; // The parent of all firePoint, MeleePoint, ... used to rotate easily when changing the facing direction
    public int oldFaceDir;
    
    Vector2 directionalInput;
    public bool wallSliding;
    bool wasWallSliding;
    int wallDirX;
    bool isJumping;

    public bool isCrouching = false;
    Vector2 colliderSizeStanding;
    Vector2 colliderOffsetStanding;
    Vector2 colliderSizeCrouching;
    Vector2 colliderOffsetCrouching;

    // Start is called before the first frame update
    void Start()
    {
        controller = GetComponent<Controller2D>();
        animator = GetComponentInChildren<Animator>();
        playerSprite = GetComponentInChildren<SpriteRenderer>();

        gravity = -(2 * maxJumpHeight)/Mathf.Pow(timeToJumpApex, 2); // This is a Kinematic equation : deltaVelocity = initialVelocity * time + (acceleration * time²)/2 ; Here deltaVelocity = maxJumpHeight, initialVelocity = 0, time = timeToJumpAPex, and acceleration = gravity
        maxJumpVelocity = Mathf.Abs(gravity) * timeToJumpApex; // This is another Kinematic equation : finalVelocity = initialVelocity + acceleration * time ; Here finalVelocity = maxJumpVelocity, initialVelocity = 0, acceleration = gravity, and time = timeToJumpApex
        minJumpVelocity = Mathf.Sqrt(2 * Mathf.Abs(gravity) * minJumpHeight);

        colliderSizeStanding = controller.collider.size; // Used to adapt collider when crouching
        colliderOffsetStanding = controller.collider.offset;

        colliderSizeCrouching = new Vector2(controller.collider.size.x, 1.5f);
        colliderOffsetCrouching = new Vector2(controller.collider.offset.x, -0.750973f);
    }

    // Update is called once per frame
    void Update()
    {
        //Physics2D.SyncTransforms();
        if(!canMove) return;
        oldFaceDir = controller.collisions.faceDir;
        wasWallSliding = wallSliding;

        HandleCrouching();
        CalculateVelocity();
        HandleWallSliding();

        animator.SetFloat("Speed", Mathf.Abs(velocity.x));
        if(controller.collisions.below || wallSliding) animator.SetBool("IsJumping", isJumping);
        if(wasWallSliding ^ wallSliding)
        {
            animator.SetBool("IsWallSliding", wallSliding);
            rotationPoint.transform.Rotate(0,180,0);
        }

        controller.Move(velocity * Time.deltaTime, directionalInput);

        if(oldFaceDir != controller.collisions.faceDir) Flip();

        if(controller.collisions.above || controller.collisions.below)
        {
            if(controller.collisions.slidingDownMaxSlope)
            {
                velocity.y += controller.collisions.slopeNormal.y * -gravity * Time.deltaTime;
            }
            else
            {
                velocity.y = 0;
            }
        }

        isJumping = !controller.collisions.below && !controller.collisions.climbingSlope && !controller.collisions.descendingSlope && !controller.collisions.slidingDownMaxSlope && !wallSliding; // used in the HandleWallSliding() method

        Physics2D.SyncTransforms(); // When a Transform component changes, any Rigidbody2D or Collider2D on that Transform or its children may need to be repositioned, rotated or scaled depending on the change to the Transform.
    }

    public void SetDirectionnalInput(Vector2 input)
    {
        directionalInput = input;
    }

    public void OnJumpInputDown()
    {
        isJumping = true;
        animator.SetBool("IsJumping", true);
        if(wallSliding)
        {
            if(wallDirX == directionalInput.x)
            {
                velocity.x = -wallDirX * wallJumpClimb.x;
                velocity.y = wallJumpClimb.y;
            }
            else if(directionalInput.x == 0)
            {
                velocity.x = -wallDirX * wallJumpOff.x;
                velocity.y = wallJumpOff.y;
            }
            else
            {
                velocity.x = -wallDirX * wallLeap.x;
                velocity.y = wallLeap.y;
            }
        }
        if(controller.collisions.below)
        {
            if(controller.collisions.slidingDownMaxSlope)
            {
                if(directionalInput.x != -Mathf.Sign(controller.collisions.slopeNormal.x)) // Not jumping against max slope
                {
                    velocity.y = maxJumpVelocity * controller.collisions.slopeNormal.y;
                    velocity.x = maxJumpVelocity * controller.collisions.slopeNormal.x;
                }
            }
            else
            {
                velocity.y = maxJumpVelocity;
            }
        }
    }

    public void OnJumpInputUp()
    {
        if(velocity.y > minJumpVelocity) velocity.y = minJumpVelocity;
    }

    void HandleWallSliding()
    {
        wallDirX = (controller.collisions.left) ? -1 : 1;

        wallSliding = false;
        if((controller.collisions.left || controller.collisions.right) && !controller.collisions.below) // && velocity.y < 0 ; c'est comme ça dans le tuto, mais dcp ça ne marche que une fois que le player redescend
        {
            wallSliding = true;

            if(velocity.y < -wallSlideSpeedMax)
            {
                velocity.y = -wallSlideSpeedMax;
            }

            if(timeToWallUnstick > 0)
            {
                // This if() statement can be source of bug (not totaly sure), if yes, just remove the isJumping variable from this object and allow to wall jump only when velocity.y < 0 (see above)
                if(!isJumping)
                {
                    velocityXSmoothing = 0;
                    velocity.x = 0;
                }
                
                if(directionalInput.x != wallDirX && directionalInput.x != 0)
                {
                    timeToWallUnstick -= Time.deltaTime;
                }
                else
                {
                    timeToWallUnstick = wallStickTime;
                }
            }
            else
            {
                timeToWallUnstick = wallStickTime;
            }
        }
    }

    void CalculateVelocity()
    {
        float effectiveMoveSpeed = isCrouching ? moveSpeed * 0.5f : moveSpeed;
        float targetVelocityX = directionalInput.x * effectiveMoveSpeed;
        velocity.x = Mathf.SmoothDamp(velocity.x, targetVelocityX, ref velocityXSmoothing, controller.collisions.below ? accelerationTimeGrounded : accelerationTimeAirborne);
        velocity.y += gravity * Time.deltaTime;
    }

    public void Flip()
    {
        playerSprite.flipX = !playerSprite.flipX;
        playerSprite.transform.localPosition *=-1; // We adjust the position of the sprite to fit the position of the BoxCollider2Ds

        rotationPoint.transform.Rotate(0,180,0);
    }

    public void HandleCrouching()
    {
        if(directionalInput.y == -1 && !isCrouching && (controller.collisions.below || controller.collisions.climbingSlope || controller.collisions.descendingSlope))
        {
            isCrouching = true;
            animator.SetBool("IsCrouching", true);
            transform.Translate(new Vector3(0,0.001f,0)); // Avoid going through obstacle
            controller.collider.size = colliderSizeCrouching;
            controller.collider.offset = colliderOffsetCrouching;
            controller.UpdateRaycastOrigins();
        }
        else if(isCrouching && directionalInput.y != -1)
        {
            isCrouching = false;
            animator.SetBool("IsCrouching", false);
            transform.Translate(new Vector3(0,0.001f,0)); // Avoid going through obstacle
            controller.collider.size = colliderSizeStanding;
            controller.collider.offset = colliderOffsetStanding;
            controller.UpdateRaycastOrigins();    
        }
    }

    public void GetKnockback(Vector2 knockback)
    {
        velocity = knockback;
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