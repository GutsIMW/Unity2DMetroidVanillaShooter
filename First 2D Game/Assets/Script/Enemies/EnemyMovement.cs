using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Pathfinding;
using GutsIMW.Utils;

[RequireComponent (typeof(Controller2D))]
public class EnemyMovement : MonoBehaviour
{
    public LayerMask collisionMask;
    public bool canMove = true;
    bool isJumping = false;

    private Vector2 startingPosition;
    private Vector3 roamingPosition;
    public Vector2 roamingRange = new Vector2(2, 6f);
    float roamingTime = 3f; // The maximum time an enemy can roam to the same point
    float roamingDuration = 0f; // The current duration since the enemy is roaming
    [Range(6, 100f)] public float aggroRange = 10f;

    Controller2D controller;
    Rigidbody2D rb;
    Animator animator;
    private SpriteRenderer enemySprite;
    [SerializeField] int oldFaceDir;

    public float maxJumpHeight = 1.5f; // Height in Unity unit that the enemy can jump
    public float timeToJumpApex = .4f; // Time to reach the apex of the jump
    float accelerationTimeAirborne = 0.5f;
    float accelerationTimeGrounded = 0.5f;
    public float moveSpeed = 6;

    float gravity;
    float maxJumpVelocity;
    Vector3 velocity; // A vector3 cause we work with the transform which use a Vector3 (x,y and z)
    float velocityXSmoothing;

    Vector2 movementDir;


    // Pathfinding properties
    public Transform target;
    public Player player;
    Path path;
    int currentWaypoint = 0;
    [SerializeField] float nextWaypointDistance = 1f; // Need a small number
    bool reachedEndOfPath = false;
    [SerializeField] float seekerOffsetY = 0; // Offset on the y axis to adapt the starting point of the path (near the ground for crabs enemy for example);
    Seeker seeker;

    // Start is called before the first frame update
    void Start()
    {
        seeker = GetComponent<Seeker>();
        controller = GetComponent<Controller2D>();
        rb = GetComponent<Rigidbody2D>();
        animator = GetComponentInChildren<Animator>();
        enemySprite = GetComponentInChildren<SpriteRenderer>();

        startingPosition = transform.position;
        roamingPosition = GetRoamingPosition();

        gravity = -(2 * maxJumpHeight)/Mathf.Pow(timeToJumpApex, 2); // This is a Kinematic equation : deltaVelocity = initialVelocity * time + (acceleration * time²)/2 ; Here deltaVelocity = maxJumpHeight, initialVelocity = 0, time = timeToJumpAPex, and acceleration = gravity
        maxJumpVelocity = Mathf.Abs(gravity) * timeToJumpApex; // This is another Kinematic equation : finalVelocity = initialVelocity + acceleration * time ; Here finalVelocity = maxJumpVelocity, initialVelocity = 0, acceleration = gravity, and time = timeToJumpApex

        InvokeRepeating("UpdatePath", 0f, 0.5f);
    }

    // Update is called once per frame
    void Update()
    {
        if(roamingDuration >= 0f) roamingDuration -= Time.deltaTime;

        if(!canMove) return;

        oldFaceDir = controller.collisions.faceDir;

        //    AI Movement depending the PathFinding
        
        if(path == null) return;
        if(currentWaypoint >= path.vectorPath.Count)
        {
            if(target == null) roamingPosition = GetRoamingPosition();
            reachedEndOfPath = true;
            return;
        }
        else
        {
            if(roamingDuration < 0f && target == null)
            {
                roamingPosition = GetRoamingPosition();
                roamingDuration = roamingTime;
            }
            reachedEndOfPath = false;
        }
        
        Vector2 targetDir = path.vectorPath[currentWaypoint] - transform.position;
        movementDir.x = Mathf.Sign(targetDir.x);
        movementDir.y = Mathf.Sign(targetDir.y);

        if(controller.collisions.below && gameObject.tag == "Jumper") animator.SetBool("IsJumping", isJumping);
        if(ShouldJump(targetDir)) Jump();

        CalculateVelocity();
        if(gameObject.tag == "Crab") animator.SetFloat("Speed", Mathf.Abs(velocity.x));
        controller.Move(velocity * Time.deltaTime, movementDir);

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

        isJumping = !controller.collisions.below && !controller.collisions.climbingSlope && !controller.collisions.descendingSlope && !controller.collisions.slidingDownMaxSlope;

        float distance = Vector2.Distance(transform.position, path.vectorPath[currentWaypoint]);
        if(distance < nextWaypointDistance)
        {
            currentWaypoint++;
        }

        Physics2D.SyncTransforms(); // When a Transform component changes, any Rigidbody2D or Collider2D on that Transform or its children may need to be repositioned, rotated or scaled depending on the change to the Transform.
    }


    void CalculateVelocity()
    {
        float targetVelocityX = movementDir.x * moveSpeed;
        velocity.x = Mathf.SmoothDamp(velocity.x, targetVelocityX, ref velocityXSmoothing, controller.collisions.below ? accelerationTimeGrounded : accelerationTimeAirborne);
        velocity.y += gravity * Time.deltaTime;
    }

    private bool ShouldJump(Vector2 targetDir)
    {
        return controller.collisions.left && movementDir.x == -1 || controller.collisions.right && movementDir.x == 1; // Jump if the enemy collide with an obstacle on his path
    }

    private void Jump()
    {
        isJumping = true;
        animator.SetBool("IsJumping", true);
        if(controller.collisions.below)
        {
            if(controller.collisions.slidingDownMaxSlope)
            {
                if(movementDir.x != -Mathf.Sign(controller.collisions.slopeNormal.x)) // Not jumping against max slope
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

    public void Flip()
    {
        enemySprite.flipX = !enemySprite.flipX;
        //enemySprite.transform.localPosition *=-1; // We adjust the position of the sprite to fit the position of the BoxCollider2Ds
    }

    void UpdatePath()
    {
        if(Vector2.Distance(transform.position, player.transform.position) > aggroRange)
        {
            target = null;
            //roamingPosition = GetRoamingPosition();
        }
        else
        {
            target = player.transform;
        }

        if(seeker.IsDone())
        {
            Vector2 offset = new Vector2(0f,seekerOffsetY); 
            //seeker.StartPath(rb.position + offset, target.position, OnPathComplete);
            Vector2 pos = transform.position;
            seeker.StartPath(pos + offset, target != null ? target.position : roamingPosition, OnPathComplete);
        }
    }

    void OnPathComplete(Path p)
    {
        if(!p.error)
        {
            path = p;
            currentWaypoint = 0;
        }
    }

    private Vector2 GetRoamingPosition()
    {
        Vector2 raycastOrigin =  startingPosition + UtilsClass.GetRandomDir() * Random.Range(roamingRange.x, roamingRange.y);
        RaycastHit2D hit = Physics2D.Raycast(raycastOrigin, Vector2.down, 10f, collisionMask);
        if(!hit) hit.point = startingPosition; // Si on touche pas alors on retourne au spawn de l'ennemie

        RaycastHit2D hitTmp = Physics2D.Raycast(startingPosition, hit.point - startingPosition, Vector2.Distance(startingPosition, hit.point), collisionMask);
        if(hitTmp) // Si il y a un obstacle entre l'ennemie et sa prochaine destination
        {
            hit.point = hitTmp.point; // On adapte la prochaine destination
        }

        return hit.point;
    }

    /*
    void OnDrawGizmos()
    {
        Gizmos.color = Color.green;
        Gizmos.DrawSphere(roamingPosition, 1f);

        Gizmos.color = Color.blue;
        Gizmos.DrawSphere(startingPosition, 1f);
    }
    */
}
