using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CustomPhysicsEngine : MonoBehaviour
{
    /*
     * This class is the main physics engine which will handle the collisions and physics body checks.
     * The class is made into a singleton pattern due to the fact that only one physics engine may exist
     * within the scene. The Awake() promptly handles any issues of this and the static Instance variable
     * is used to be called within other scripts for use.
     */
    public static CustomPhysicsEngine Instance { get; private set; }

    private void Awake()
    {
        // Ensure only one instance exists
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(gameObject); // Prevent duplicate instances
        }
    }

    /* 
     * Lists are used to collect the physics bodies and colliders rather than arrays to reduce the inefficiency
     * of using arrays multiple times to reference the physics bodies within the scene.
     */

    private List<CustomPhysicsBody> physicsBodies = new List<CustomPhysicsBody>();
    private List<CustomCollider> colliders = new List<CustomCollider>();


    /* 
     * Update is called at fixed framerate intervals, making it much more valiable for accuracy and ensuring that physics
     * behaves the same across devices. FixedUpdate is more viable when applying forces, velocity updates, gravity calculations,
     * and collision detection. In our case, we save deltaTime as a local variable, which is then passed as an argument along
     * a loop through the list of physics bodies to call each function individually.
     */
    private void FixedUpdate()
    {
        float deltaTime = Time.fixedDeltaTime;

        foreach (var body in physicsBodies)
        {
            UpdateVelocity(body, deltaTime);
            UpdatePosition(body, deltaTime);
        }

        CheckCollisions();
    }

    /*
     * UpdateVelocity is called to update the velocities of the custom physics body by applying acceleration, gravity, and time
     * which is used as a parameter.
     */
    private void UpdateVelocity(CustomPhysicsBody body, float deltaTime)
    {
        // If the object is grounded, skip gravity updates
        if (body.IsGrounded()) return;

        // Apply gravity with optional mass scaling
        float gravityEffect = CustomPhysicsBody.GRAVITY * body.GetGravityScale();
        gravityEffect *= body.GetMass(); // F = mg

        // Apply gravity using gravity scale
        Vector2 newAcceleration = new Vector2(body.Acceleration.x, body.Acceleration.y + gravityEffect);

        // Update velocity using acceleration
        Vector2 updatedVelocity = body.Velocity + newAcceleration * deltaTime;

        // Apply damping for smooth bounce decay
        float dampingFactor = 0.99f;
        updatedVelocity *= dampingFactor;

        body.SetVelocity(updatedVelocity);
    }


    /*
     * UpdatePosition is called to update the positions of the custom physics body by adding the position to the 
     * physics body's velocity multiplied by the deltaTime parameter. The typecast is used because transform.position is a Vector3
     * therefore since body.Velocity is Vector2 we need to convert it. This does not affect anything as the z value will be 0 by default.
     */
    private void UpdatePosition(CustomPhysicsBody body, float deltaTime)
    {
        body.transform.position += (Vector3)body.Velocity * deltaTime;
    }

    #region Collision Check Methods
    /*
     * CheckCollisions is called to check whether the custom colliders are colliding. Using the list, it iterates and compares all
     * objects which can collide. Based on the type of collision, an action is performed in response.
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

    private void CircleToCircleCollisionCheck(CustomCollider a, CustomCollider b)
    {
        if (!IsCollisionBetween(a, b, CustomCollider.Type.CIRCLE, CustomCollider.Type.CIRCLE)) return;

        // Update bounds before using them
        a.UpdateBounds();
        b.UpdateBounds();

        Bounds aBounds = a.GetBounds();
        Bounds bBounds = b.GetBounds();

        Vector2 aPos = aBounds.center;
        Vector2 bPos = bBounds.center;
        float aRad = aBounds.extents.x; // Radius = half of width
        float bRad = bBounds.extents.x; // Radius = half of width

        Vector2 aToB = bPos - aPos;
        float distance = aToB.magnitude;
        float intersectionDepth = (aRad + bRad) - distance;

        // Fix: Prevent NaN errors when circles are at the same position
        if (distance == 0) return; // If circles overlap perfectly, do nothing (temporary fix)

        if (intersectionDepth > 0)
        {
            Debug.Log("Circle-Circle Collision");

            CustomPhysicsBody bodyA = a.GetComponent<CustomPhysicsBody>();
            CustomPhysicsBody bodyB = b.GetComponent<CustomPhysicsBody>();

            float aMass = bodyA.GetMass();
            float bMass = bodyB.GetMass();
            float massCombined = aMass + bMass;

            // Prevent division by zero
            if (massCombined == 0) return;

            // Compute normalized collision vectors
            Vector2 bToAN = (aPos - bPos).normalized;
            Vector2 aToBN = aToB.normalized;

            // Fix potential NaN positions
            if (float.IsNaN(bToAN.x) || float.IsNaN(bToAN.y) || float.IsNaN(aToBN.x) || float.IsNaN(aToBN.y))
            {
                Debug.LogError("NaN detected in collision resolution.");
                return;
            }

            // Resolve penetration by pushing objects apart based on mass ratio
            Vector2 aRes = bToAN * intersectionDepth * (bMass / massCombined);
            Vector2 bRes = aToBN * intersectionDepth * (aMass / massCombined);

            a.transform.position += (Vector3)aRes;
            b.transform.position += (Vector3)bRes;

            /*
            *  Applies the elastic collision formula to swap velocities based on mass and restitution.
            */
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


    private void CircleToRectCollisionCheck(CustomCollider a, CustomCollider b)
    {
        if (!IsCollisionBetween(a, b, CustomCollider.Type.CIRCLE, CustomCollider.Type.AXIS_ALIGNED_RECTANGLE))
            return;

        CustomCollider circle = (a.type == CustomCollider.Type.CIRCLE) ? a : b;
        CustomCollider aaRect = (b.type == CustomCollider.Type.AXIS_ALIGNED_RECTANGLE) ? b : a;

        // Update bounds before using them
        circle.UpdateBounds();
        aaRect.UpdateBounds();

        Bounds circleBounds = circle.GetBounds();
        Bounds rectBounds = aaRect.GetBounds();

        Vector2 circlePos = circleBounds.center;
        Vector2 rectPos = rectBounds.center;
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

        // Collision check
        Vector2 circleToClosest = closestPoint - circlePos;
        if (circleToClosest.sqrMagnitude >= circleRadius * circleRadius)
            return; // No collision

        CustomPhysicsBody body = circle.GetComponent<CustomPhysicsBody>();
        Vector2 normal = circleToClosest.normalized;

        Debug.Log("Circle-Rect Collision");

        // Check if the circle is **only touching the top**
        bool touchingTop = (Mathf.Abs(circlePos.y - topBound) < circleRadius);
        bool fullyOnTop = (circlePos.x - circleRadius > leftBound && circlePos.x + circleRadius < rightBound);

        // Only ground if touching the top AND fully on top
        if (touchingTop && fullyOnTop && Mathf.Abs(body.Velocity.y) < 0.2f)
        {
            body.SetVelocity(Vector2.zero);
            body.SetGrounded(true);
            body.transform.position = new Vector2(circlePos.x, topBound + circleRadius + 0.01f);
            Debug.Log("Object is grounded and stopped.");
            return;
        }

        // If the circle is **not fully over the rectangle's top**, gravity resumes
        if (!fullyOnTop || !touchingTop)
        {
            body.SetGrounded(false);
            Debug.Log("Circle is NOT fully on top - resuming gravity.");
        }

        // Apply reflection only if it is a valid bounce
        float restitutionDecay = 0.98f;
        float newRestitution = body.GetRestitution() * restitutionDecay;
        body.SetRestitution(newRestitution);
        Vector2 reflectedVelocity = Vector2.Reflect(body.Velocity, normal) * newRestitution;

        // Fix tiny movement that prevent bouncing
        if (Mathf.Abs(reflectedVelocity.magnitude) < 0.1f)
            reflectedVelocity += normal * 0.2f;

        body.SetVelocity(reflectedVelocity);
    }

    private void PointToRectCollisionCheck(CustomCollider a, CustomCollider b)
    {
        if (!IsCollisionBetween(a, b, CustomCollider.Type.POINT, CustomCollider.Type.AXIS_ALIGNED_RECTANGLE))
            return;

        CustomCollider point = (a.type == CustomCollider.Type.POINT) ? a : b;
        CustomCollider aaRect = (b.type == CustomCollider.Type.AXIS_ALIGNED_RECTANGLE) ? b : a;

        // Ensure bounds are updated before checking collisions
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
            return; // No collision

        Debug.Log("Point-Rectangle Collision");
        Destroy(point.gameObject);
    }


    private bool IsCollisionBetween(CustomCollider a, CustomCollider b, CustomCollider.Type typeA, CustomCollider.Type typeB)
    {
        return (a.type == typeA && b.type == typeB) || (a.type == typeB && b.type == typeA);
    }
    #endregion

    #region Register and Deregister methods
    // This method is used to register the CustomPhysicsBody upon generation, adding it to the list.
    public void RegisterBody(CustomPhysicsBody body)
    {
        if (!physicsBodies.Contains(body))
        {
            physicsBodies.Add(body);
        }
    }

    // This method is used to deregister the CustomPhysicsBody upon destruction, removing it from the list.
    public void DeregisterBody(CustomPhysicsBody body)
    {
        if (physicsBodies.Contains(body))
        {
            physicsBodies.Remove(body);
        }
    }
    // This method is used to register the CustomCollider upon generation, adding it to the list.
    public void RegisterCollider(CustomCollider collider)
    {
        if (!colliders.Contains(collider))
        {
            colliders.Add(collider);
        }
    }
    // This method is used to deregister the CustomCollider upon destruction, removing it from the list.
    public void DeregisterCollider(CustomCollider collider)
    {
        if (colliders.Contains(collider))
        {
            colliders.Remove(collider);
        }
    }

    #endregion
}
