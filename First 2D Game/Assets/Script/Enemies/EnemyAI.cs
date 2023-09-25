using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Pathfinding;

public class EnemyAI : MonoBehaviour
{

    public Transform target;
    [SerializeField] private Transform groundCheck; // Prevent jumping forever
    [SerializeField] private LayerMask whatIsGround;  // A mask determining what is ground to the enemy
    [SerializeField] public Animator animator;

    [SerializeField] bool canFly = false; // Indicate if the enemy is a flyer
    [SerializeField] float speed = 10f;
    [SerializeField] public Vector2 jumpForce = new Vector2 (30f, 75f); // Define the jumpForce of the enemy (only efficient with non flyer enemy)
    [SerializeField] float nextWaypointDistance = 3f;
    [SerializeField] private bool grounded; // Indicate if the enemy is grounded or not
    const float groundedRadius = .2f; // Radius of the overlap circle to determine if grounded

    public Transform enemyGFX;

    Path path;
    int currentWaypoint = 0;
    bool reachedEndOfPath = false;
    [SerializeField] float seekerOffsetY = 0; // Offset on the y axis to adapt the starting point of the path (near the ground for crabs enemy for example);

    Seeker seeker;
    Rigidbody2D rb;

    // Start is called before the first frame update
    void Start()
    {
        seeker = GetComponent<Seeker>();
        rb = GetComponent<Rigidbody2D>();
        if(canFly) rb.gravityScale = 0;

        InvokeRepeating("UpdatePath", 0f, 0.5f);
    }

    void UpdatePath()
    {
        if(seeker.IsDone())
        {
            Vector2 offset = new Vector2(0f,seekerOffsetY); 
            //seeker.StartPath(rb.position + offset, target.position, OnPathComplete);
            Vector2 pos = transform.position;
            seeker.StartPath(pos + offset, target.position, OnPathComplete);
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

    // Update is called once per frame
    void FixedUpdate()
    {
        //    Determine if the enemy is grounded or no
        bool wasGrounded = grounded;
		grounded = false;

		// The enemy is grounded if a circlecast to the groundcheck position hits anything designated as ground
		// This can be done using layers instead but Sample Assets will not overwrite your project settings.
		Collider2D[] colliders = Physics2D.OverlapCircleAll(groundCheck.position, groundedRadius, whatIsGround);
		for (int i = 0; i < colliders.Length; i++)
		{
			if (colliders[i].gameObject != gameObject)
			{
				grounded = true;
            }
		}

        
        //    AI Movement depending the PathFinding
        
        if(path == null) return;
        if(currentWaypoint >= path.vectorPath.Count)
        {
            reachedEndOfPath = true;
            return;
        }
        else
        {
            reachedEndOfPath = false;
        }

        Vector2 direction = ((Vector2)path.vectorPath[currentWaypoint]) - rb.position;
        direction.Normalize();
        Vector2 force = direction * speed * Time.deltaTime;

        if(canFly) // If is a flyer...
        {
            rb.velocity = force; // ...then add force in all axis (x and y)
        }
        /*
        else if(Vector2.Angle( Mathf.Sign(direction.x) == -1 ? Vector2.left : Vector2.right, direction) >= 50f && grounded) // If need to jump...
        {
            Debug.Log("JUMP et angle = " + Vector2.Angle( Mathf.Abs(direction.x) == -1 ? Vector2.left : Vector2.right,direction));
            Debug.Log(Mathf.Sign(direction.x) + "    " + direction);
            rb.AddForce(jumpForce); // ...then Jump
        }*/
        else // Doesnt fly and no need to jump, then just move on the x axis
        {
            Vector2 newVelocity = rb.velocity;
            newVelocity.x = force.x;
            rb.velocity = newVelocity;
        }

        float distance = Vector2.Distance(rb.position, path.vectorPath[currentWaypoint]);
        if(distance < nextWaypointDistance)
        {
            currentWaypoint++;
        }

        if(force.x >= 0.01f)
        {
            enemyGFX.localScale = new Vector3(-1f,1f,1f);
        }
        else if(force.x <= -0.01f)
        {
            enemyGFX.localScale = new Vector3(1f,1f,1f);
        }
    }
}
