using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CustomCollider: MonoBehaviour
{
    /*
     * This class is used for creating the object's collider.
     * It is separated via type and depending on which type will
     * be used accordingly in the calculations for collisions.
     * An enum is used to create a custom data set for the type,
     * which is then made into a variable to be changed within the
     * inspector.
     */
    public enum Type
    {
        POINT,
        AXIS_ALIGNED_RECTANGLE,
        CIRCLE
    }

    public Type type;

    private Bounds colliderBounds; // Stores precomputed bounds

    [SerializeField]
    private float destroyThresholdY = -10f; // Destroy the object if its y position is below this value


    private void Awake()
    {
        /* 
         * Add this object to the custom physics engine list, a ternary operator is used just in case of
         * an error in referencing scene instance is null
        */
        CustomPhysicsEngine.Instance?.RegisterCollider(this);
        UpdateBounds(); // Precompute bounds
    }

    private void OnDestroy()
    {
        /* 
         * Remove this object from the custom physics engine list, a ternary operator is used just in case of
         * an error in referencing the scene instance is null
        */
        CustomPhysicsEngine.Instance?.DeregisterCollider(this);
    }

    private void Update()
    {
        // Check if the object's y position is below the threshold
        if (transform.position.y < destroyThresholdY)
        {
            Destroy(gameObject);
        }
    }

    /// <summary>
    /// Calculates and updates the collider's bounds.
    /// </summary>
    public void UpdateBounds()
    {
        Vector3 pos = transform.position;
        Vector3 size = transform.localScale;

        if (type == Type.AXIS_ALIGNED_RECTANGLE)
        {
            colliderBounds = new Bounds(pos, size);
        }
        else if (type == Type.CIRCLE)
        {
            float radius = size.x / 2f; // Assuming uniform scale for circles
            colliderBounds = new Bounds(pos, new Vector3(radius * 2, radius * 2, 0));
        }
        else if (type == Type.POINT)
        {
            // A point is very small, so we use a minimal size to avoid issues
            float pointSize = 0.01f; // Tiny box around the point
            colliderBounds = new Bounds(pos, new Vector3(pointSize, pointSize, 0));
        }
    }

    /// <summary>
    /// Returns the bounds of this collider.
    /// </summary>
    public Bounds GetBounds()
    {
        return colliderBounds;
    }

    /// <summary>
    /// Draws the collider bounds using Gizmos for debugging.
    /// </summary>
    private void OnDrawGizmos()
    {
        UpdateBounds(); // Ensure bounds are updated before drawing

        Gizmos.color = type == Type.AXIS_ALIGNED_RECTANGLE ? Color.blue : Color.yellow;

        if (type == Type.AXIS_ALIGNED_RECTANGLE)
        {
            Gizmos.DrawWireCube(colliderBounds.center, colliderBounds.size);
        }
        else if (type == Type.CIRCLE)
        {
            Gizmos.DrawWireSphere(colliderBounds.center, colliderBounds.extents.x); // Radius
        }
    }
}
