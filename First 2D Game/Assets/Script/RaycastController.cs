using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent (typeof(BoxCollider2D))]
public class RaycastController : MonoBehaviour
{
    public LayerMask collisionMask;

    public const float skinWidth = .00015f;
    const float dstBetweenRays = .25f;
    [HideInInspector] public int horizontalRayCount; // Number of Ray casted in the horizontal axis
    [HideInInspector] public int verticalRayCount; // Number of Ray casted in the vertical axis
    [HideInInspector] protected float horizontalRaySpacing;
    [HideInInspector] protected float verticalRaySpacing;

    [HideInInspector] public new BoxCollider2D collider;
    protected RaycastOrigins raycastOrigins;

    // Start is called before the first frame update
    protected virtual void Awake()
    {
        collider = GetComponent<BoxCollider2D>();
    }

    protected virtual void Start()
    {
        CalculateRaySpacing();
    }
    
    protected void CalculateRaySpacing()
    {
        Bounds bounds = collider.bounds;
        bounds.Expand(skinWidth * -2);

        float boundsWidth = bounds.size.x;
        float boundsHeight = bounds.size.y;

        horizontalRayCount = Mathf.RoundToInt(boundsHeight/dstBetweenRays) + 1;
        verticalRayCount = Mathf.RoundToInt(boundsWidth/dstBetweenRays) + 1;

        horizontalRaySpacing = bounds.size.y / (horizontalRayCount - 1);
        verticalRaySpacing = bounds.size.x / (verticalRayCount - 1);
    }

    public void UpdateRaycastOrigins()
    {
        Bounds bounds = collider.bounds;
        bounds.Expand(skinWidth * -2);

        raycastOrigins.bottomLeft = new Vector2(bounds.min.x, bounds.min.y);
        raycastOrigins.bottomRight = new Vector2(bounds.max.x, bounds.min.y);
        raycastOrigins.topLeft = new Vector2(bounds.min.x, bounds.max.y);
        raycastOrigins.topRight = new Vector2(bounds.max.x, bounds.max.y);

    }
    public struct RaycastOrigins
    {
        public Vector2 topLeft, topRight;
        public Vector2 bottomLeft, bottomRight;
    }
}
