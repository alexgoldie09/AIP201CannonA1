using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CustomPhysicsEngine : MonoBehaviour
{
    /*
     * This class is the core physics manager which will handle the collisions and physics body checks, as well as
     * interactions.
     * - Uses a singleton pattern to ensure only one physics engine exists in the scene.
     * - Stores the objects in the scene as Lists<> in order to be dynamic and memory efficient in adding or removing objects.
     * - Handles velocity, position, and collision detection updates through recursive loops.
     * - Applies custom gravity, damping, and restitution for realistic interactions.
     * - Inherits from monobehaviour to use Unity's in-built Awake() and Update().
     */
    public static CustomPhysicsEngine Instance { get; private set; } // Singleton instance

    /*
     * Awake() is called before all initialisation. **NOTE: Order is important to prevent initialisation errors**
     * - If a duplicate exists, it is destroyed to maintain a single-instance system.
     */

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(gameObject);
        }
    }

    private List<CustomPhysicsBody> physicsBodies = new List<CustomPhysicsBody>(); // List of physics objects in scene.
    private List<CustomCollider> colliders = new List<CustomCollider>(); // List of colliders in scene.


    /* 
     * FixedUpdate() is called at a fixed time interval, ensuring consistent physics calculations.
     * - Iterates through all registered physics bodies to update velocity & position.
     * - Runs the collision detection system after movement updates.
     */
    private void FixedUpdate()
    {
        float deltaTime = Time.fixedDeltaTime;

        foreach (var body in physicsBodies)
        {
            UpdateVelocity(body, deltaTime); // Apply gravity & acceleration.
            UpdatePosition(body, deltaTime); // Move object based on velocity.
        }

        CheckCollisions(); // Handle collision detection & response.
    }

    /*
     * UpdateVelocity() applies acceleration, gravity, and damping to objects.
     * - Uses mass-scaling for gravity to allow different object weights.
     * - Applies damping to simulate energy loss over time.
     */
    private void UpdateVelocity(CustomPhysicsBody body, float deltaTime)
    {
        // If the object is grounded, skip gravity updates
        if (body.IsGrounded()) { return; }

        float gravityEffect = CustomPhysicsBody.GRAVITY * body.GetGravityScale();
        gravityEffect *= body.GetMass(); // F = mg

        // Calculate new acceleration using the gravity effect
        Vector2 newAcceleration = new Vector2(body.Acceleration.x, body.Acceleration.y + gravityEffect);

        // Apply acceleration to velocity
        Vector2 updatedVelocity = body.Velocity + newAcceleration * deltaTime;

        // Apply damping for smooth bouncing
        float dampingFactor = 0.99f;
        updatedVelocity *= dampingFactor;

        body.SetVelocity(updatedVelocity);
    }


    /*
     * UpdatePosition() moves objects based on velocity.
     * - Adds velocity to position while ensuring z remains at 0 (for 2D calculations).
     */
    private void UpdatePosition(CustomPhysicsBody body, float deltaTime) => body.transform.position += (Vector3)body.Velocity * deltaTime;

    #region **Collision Detection System**
    /*
     * CheckCollisions() iterates through all registered colliders to check for interactions.
     * - Uses nested loops to compare each collider against every other collider.
     * - Calls appropriate collision resolution functions based on object type.
     */
    private void CheckCollisions()
    {
        for (int i = 0; i < colliders.Count - 1; i++)
        {
            for (int j = i + 1; j < colliders.Count; j++)
            {
                CustomCollider a = colliders[i];
                CustomCollider b = colliders[j];

                PointToRectCollisionCheck(a, b);
                CircleToRectCollisionCheck(a, b);
                CircleToCircleCollisionCheck(a, b);

            }
        }
    }
    /*
     * CircleToCircleCollisionCheck() handles elastic collisions between circles.
     * - Uses bounding box calculations to check if the circles overlap.
     * - If they overlap, the objects are pushed apart to resolve penetration.
     * - The velocity is updated using the elastic collision formula, factoring in:
     *  + Mass (heavier objects influence lighter objects more)
     *  + Restitution (bounciness, to determine energy retention)
     *  + Momentum conservation (ensuring a realistic velocity swap)
     */
    private void CircleToCircleCollisionCheck(CustomCollider a, CustomCollider b)
    {
        // If the collider types match continue to calculation otherwise exit function
        if (!IsCollisionBetween(a, b, CustomCollider.Type.CIRCLE, CustomCollider.Type.CIRCLE)) { return; }

        // Update bounds should scale ever change, then store and save information.
        a.UpdateBounds();
        b.UpdateBounds();
        Bounds aBounds = a.GetBounds();
        Bounds bBounds = b.GetBounds();

        // Get collider a and b positions, using the bounds.center property
        Vector2 aPos = aBounds.center;
        Vector2 bPos = bBounds.center;

        // Get collider a and b radii, using the bounds.extents propety since radius is half of width
        float aRad = aBounds.extents.x;
        float bRad = bBounds.extents.x;

        // Calculate whether the objects overlap through distance and intersection depth
        Vector2 aToB = bPos - aPos;
        float distance = aToB.magnitude;
        float intersectionDepth = (aRad + bRad) - distance;

        if (distance == 0) { return; } // Prevents division by zero

        // If the objects overlap
        if (intersectionDepth > 0)
        {
            Debug.Log("Circle-Circle Collision"); // Print to indicate successful collision check

            // Store and save the CustomPhysicsBody component to allow easy access for reference
            CustomPhysicsBody bodyA = a.GetComponent<CustomPhysicsBody>();
            CustomPhysicsBody bodyB = b.GetComponent<CustomPhysicsBody>();

            // Store and save mass then calculate combined mass
            float aMass = bodyA.GetMass();
            float bMass = bodyB.GetMass();
            float massCombined = aMass + bMass;

            if (massCombined == 0) { return; } // Prevent division by zero

            // Compute normalized collision vectors
            Vector2 bToAN = (aPos - bPos).normalized;
            Vector2 aToBN = aToB.normalized;

            // Calculate restitution of both colliders
            Vector2 aRes = bToAN * intersectionDepth * (bMass / massCombined);
            Vector2 bRes = aToBN * intersectionDepth * (aMass / massCombined);

            // Push them apart to prevent penetration
            a.transform.position += (Vector3)aRes;
            b.transform.position += (Vector3)bRes;

            // Apply the elastic collision formula to swap velocities based on mass and restitution
            Vector2 vA = bodyA.Velocity;
            Vector2 vB = bodyB.Velocity;
            Vector2 newVA = ((aMass - bMass) / massCombined) * vA + ((2 * bMass) / massCombined) * vB;
            Vector2 newVB = ((bMass - aMass) / massCombined) * vB + ((2 * aMass) / massCombined) * vA;

            // Apply restitution for realistic bounce
            float restitutionDecay = 0.98f;
            bodyA.SetRestitution(bodyA.GetRestitution() * restitutionDecay);
            bodyB.SetRestitution(bodyB.GetRestitution() * restitutionDecay);

            bodyA.SetVelocity(newVA * bodyA.GetRestitution());
            bodyB.SetVelocity(newVB * bodyB.GetRestitution());
        }
    }
    /*
     * CircleToRectCollisionCheck() handles collisions between a circle and an axis-aligned rectangle.
     * - Uses bounding box calculations to determine the closest point on the rectangle.
     * - If the closest point is inside the circle’s radius, a collision is detected.
     * - The velocity is reflected based on the collision normal.
     * - If the circle is resting fully on top, gravity is disabled.
     */

    private void CircleToRectCollisionCheck(CustomCollider a, CustomCollider b)
    {
        // If the collider types match continue to calculation otherwise exit function
        if (!IsCollisionBetween(a, b, CustomCollider.Type.CIRCLE, CustomCollider.Type.AXIS_ALIGNED_RECTANGLE)) { return; }

        CustomCollider circle = (a.type == CustomCollider.Type.CIRCLE) ? a : b;
        CustomCollider aaRect = (b.type == CustomCollider.Type.AXIS_ALIGNED_RECTANGLE) ? b : a;

        // Update bounds should scale ever change, then store, and save information
        circle.UpdateBounds();
        aaRect.UpdateBounds();
        Bounds circleBounds = circle.GetBounds();
        Bounds rectBounds = aaRect.GetBounds();

        // Get collider circle and rectangle positions, using the bounds.center property
        Vector2 circlePos = circleBounds.center;
        Vector2 rectPos = rectBounds.center;
        // Get collider circle radius, using the bounds.extents propety since radius is half of width
        float circleRadius = circleBounds.extents.x;

        // Get rectangle bounds
        float leftBound = rectBounds.min.x;
        float rightBound = rectBounds.max.x;
        float topBound = rectBounds.max.y;
        float bottomBound = rectBounds.min.y;

        // Find the closest point on the rectangle to the circle
        float closestX = Mathf.Clamp(circlePos.x, leftBound, rightBound);
        float closestY = Mathf.Clamp(circlePos.y, bottomBound, topBound);
        Vector2 closestPoint = new Vector2(closestX, closestY);

        // Check if the closest point is within the circle’s radius
        Vector2 circleToClosest = closestPoint - circlePos;
        if (circleToClosest.sqrMagnitude >= circleRadius * circleRadius) { return; } // No collision

        CustomPhysicsBody body = circle.GetComponent<CustomPhysicsBody>();
        Vector2 normal = circleToClosest.normalized;

        Debug.Log("Circle-Rect Collision");

        // Check if the circle is only touching the top and is fully on top
        bool touchingTop = (Mathf.Abs(circlePos.y - topBound) < circleRadius);
        bool fullyOnTop = (circlePos.x - circleRadius > leftBound && circlePos.x + circleRadius < rightBound);

        // If the circle is resting fully on top and moving slowly, stop it, and set the object on top of the rectangle
        if (touchingTop && fullyOnTop && Mathf.Abs(body.Velocity.y) < 0.2f)
        {
            body.SetVelocity(Vector2.zero);
            body.SetGrounded(true);
            body.transform.position = new Vector2(circlePos.x, topBound + circleRadius + 0.01f);
            Debug.Log("Object is grounded and stopped.");
            return;
        }

        // If the circle is not fully over the rectangle's top, gravity resumes
        if (!fullyOnTop || !touchingTop)
        {
            body.SetGrounded(false);
            Debug.Log("Circle is NOT fully on top - resuming gravity.");
        }

        // Apply restitution decay (energy loss from collision)
        float restitutionDecay = 0.98f;
        float newRestitution = body.GetRestitution() * restitutionDecay;
        body.SetRestitution(newRestitution);

        // Reflect velocity using the collision normal
        Vector2 reflectedVelocity = Vector2.Reflect(body.Velocity, normal) * newRestitution;

        // Prevent tiny velocities from stopping bounces too early
        if (Mathf.Abs(reflectedVelocity.magnitude) < 0.1f) { reflectedVelocity += normal * 0.2f; }

        body.SetVelocity(reflectedVelocity);
    }
    /*
     * PointToRectCollisionCheck() handles collisions between a point and an axis-aligned rectangle.
     * - Uses bounding box checks to determine if the point is inside the rectangle.
     * - If the point is inside the rectangle, it is destroyed.
     * - This is used for things like point-based hit detection.
     */
    private void PointToRectCollisionCheck(CustomCollider a, CustomCollider b)
    {
        // If the collider types match continue to calculation otherwise exit function
        if (!IsCollisionBetween(a, b, CustomCollider.Type.POINT, CustomCollider.Type.AXIS_ALIGNED_RECTANGLE)) { return; }

        CustomCollider point = (a.type == CustomCollider.Type.POINT) ? a : b;
        CustomCollider aaRect = (b.type == CustomCollider.Type.AXIS_ALIGNED_RECTANGLE) ? b : a;

        // Update bounds should scale ever change, then store, and save information
        point.UpdateBounds();
        aaRect.UpdateBounds();
        Bounds pointBounds = point.GetBounds();
        Bounds rectBounds = aaRect.GetBounds();

        Vector2 pointPos = pointBounds.center;  // The point's position
        Vector2 rectMin = rectBounds.min;  // Bottom-left corner of the rectangle
        Vector2 rectMax = rectBounds.max;  // Top-right corner of the rectangle

        // Check if the point is inside the rectangle's bounds
        if (pointPos.x < rectMin.x || pointPos.x > rectMax.x ||
            pointPos.y < rectMin.y || pointPos.y > rectMax.y)
        { return; } // No collision

        Debug.Log("Point-Rectangle Collision");
        Destroy(point.gameObject);
    }

    /*
     * IsCollisionBetween() checks if two colliders match a specific collision type pair, and returns true/false based on that pair.
     * - Used to determine if the objects should be checked for collision.
     * - Allows flexible pairing of different collider types (e.g., Circle vs. Rectangle).
     * - Returns 'true' if the two colliders match the specified types, false otherwise.
     */
    private bool IsCollisionBetween(CustomCollider a, CustomCollider b, CustomCollider.Type typeA, CustomCollider.Type typeB) => 
        (a.type == typeA && b.type == typeB) || (a.type == typeB && b.type == typeA);
    #endregion

    #region **Register and Deregister methods**
    /* 
     * RegisterBody() adds a body object to the engine's CustomPhysicsBody list. 
     * - Used upon spawning the object to hold reference.
     * - Ensures duplicates of that object don't exist before adding.
     *  + Searches the CustomPhysicsBody list to make sure that body does not exist first.
     */
    public void RegisterBody(CustomPhysicsBody body)
    {
        if (!physicsBodies.Contains(body))
        {
            physicsBodies.Add(body);
        }
    }
    /* 
     * DeregisterBody() removes a body object from the engine's CustomPhysicsBody list. 
     * - Used upon destroying the object to no longer require reference.
     * - Ensures duplicates of that object no longer exist in the scene.
     *  + Searches the CustomPhysicsBody list to make sure it removes the correct body.
     */
    public void DeregisterBody(CustomPhysicsBody body)
    {
        if (physicsBodies.Contains(body))
        {
            physicsBodies.Remove(body);
        }
    }
    /* 
     * RegisterCollider() adds a collider object to the engine's CustomCollider list. 
     * - Used upon spawning the object to hold reference.
     * - Ensures duplicates of that object don't exist before adding.
     *  + Searches the CustomCollider list to make sure that body does not exist first.
     */
    public void RegisterCollider(CustomCollider collider)
    {
        if (!colliders.Contains(collider))
        {
            colliders.Add(collider);
        }
    }
    /* 
     * DeregisterCollider() removes a collider object from the engine's CustomCollider list. 
     * - Used upon destroying the object to no longer require reference.
     * - Ensures duplicates of that object no longer exist in the scene.
     *  + Searches the CustomCollider list to make sure it removes the correct collider.
     */
    public void DeregisterCollider(CustomCollider collider)
    {
        if (colliders.Contains(collider))
        {
            colliders.Remove(collider);
        }
    }

    #endregion
}
