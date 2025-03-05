using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CustomCollider: MonoBehaviour
{
    /*
     * This class is used for creating the object's collider.
     * - It defines custom types to be used for various collision detection calculations. 
     * - It establishes bounds and bound checks for the collider based on the type. 
     * - Inherits from monobehaviour to use Unity's in-built Start() and Update().
     */
    public enum Type
    {
        POINT,
        AXIS_ALIGNED_RECTANGLE,
        CIRCLE
    }

    public Type type; // The type of collider, set in the Inspector.

    private Bounds colliderBounds; // Stores the computed bounding box for collision checks.

    [SerializeField]
    private float destroyThresholdY = -10f; // The y-position threshold below which the object is destroyed. Used for testing.

    /*
     * Start() is called when the object is initialised. 
     * - Registers this collider in the CustomPhysicsEngine.
     *  + A ternary operator is used just in case of an error in referencing the scene instance is null.
     * - Updates its bounds to prepare for physics calculations.
     */

    private void Start()
    {
        CustomPhysicsEngine.Instance?.RegisterCollider(this);
        UpdateBounds();
    }

    /*
     * OnDestroy() is called when the object is about to be removed from the scene.
     * - Deregisters this collider from the CustomPhysicsEngine to prevent errors.
     *  + A ternary operator is used just in case of an error in referencing the scene instance is null.
     */
    private void OnDestroy()
    {
        CustomPhysicsEngine.Instance?.DeregisterCollider(this);
    }

    /*
     * Update() is called once per frame.
     * - Destroys the object if it falls below the defined y-threshold.
     *  + This is useful for cleaning up off-screen objects in the game.
     */
    private void Update()
    {
        if (transform.position.y < destroyThresholdY)
        {
            Destroy(gameObject);
        }
    }

    /*
     * UpdateBounds() adjusts the collider's bounds based on its type and current position.
     * - AXIS_ALIGNED_RECTANGLE: Uses the object's position and scale for bounding.
     * - CIRCLE: Uses the x-scale to determine radius and creates a square bounding box.
     * - POINT: Assigns a very small bounding box for precision in point collisions.
     */
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
            float pointSize = 0.01f; // Tiny box around the point
            colliderBounds = new Bounds(pos, new Vector3(pointSize, pointSize, 0));
        }
    }

    /*
     * GetBounds() returns the object's computed bounds for it's collider.
     * - Used by the CustomPhysicsEngine for collision detection.
     */
    public Bounds GetBounds() => colliderBounds;

    /*
    * OnDrawGizmos() draws the collider's bounds in the Unity Editor.
    * - This helps visualise the collider in development.
    *   + Blue color represents rectangles, yellow represents circles.
    *   + Depending on the type, the approapriate Draw() function is used.
    */
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
            Gizmos.DrawWireSphere(colliderBounds.center, colliderBounds.extents.x);
        }
    }
}
