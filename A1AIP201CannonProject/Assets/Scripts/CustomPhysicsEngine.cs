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
        // Get the current acceleration and gravity
        Vector2 currentAcceleration = body.Acceleration;
        Vector2 gravityEffect = new Vector2(0, CustomPhysicsBody.GRAVITY * body.GetGravityScale());

        // Update velocity with acceleration
        Vector2 updatedVelocity = body.Velocity + currentAcceleration * deltaTime;

        // Update velocity with gravity
        updatedVelocity += gravityEffect * deltaTime;

        // Set the new velocity back to the physics body
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

    /*
     * CheckCollisions is called to check whether the custom colliders are colliding. Using the list, it iterates and compares a point collider to
     * an axis-aligned rectangle. As it checks the bounds, it will determine whether the point has collides with the rectanlge and will
     * print a debug statement and destroy the point collider for testing.
     */
    private void CheckCollisions()
    {
        for (int i = 0; i < colliders.Count - 1; i++)
        {
            CustomCollider a = colliders[i];

            for (int j = i + 1; j < colliders.Count; j++)
            {
                CustomCollider b = colliders[j];

                bool pointToRect = (a.type == CustomCollider.Type.POINT && b.type == CustomCollider.Type.AXIS_ALIGNED_RECTANGLE ||
                                    b.type == CustomCollider.Type.POINT && a.type == CustomCollider.Type.AXIS_ALIGNED_RECTANGLE);

                if (pointToRect)
                {
                    CustomCollider point = a.type == CustomCollider.Type.POINT ? a : b;
                    CustomCollider aaRect = b.type == CustomCollider.Type.AXIS_ALIGNED_RECTANGLE ? b : a;

                    float width = aaRect.transform.localScale.x;
                    float height = aaRect.transform.localScale.y;

                    float lhs = aaRect.transform.localPosition.x - (width / 2f);
                    float rhs = aaRect.transform.localPosition.x + (width / 2f);
                    float top = aaRect.transform.localPosition.y + (height / 2f);
                    float bot = aaRect.transform.localPosition.y - (height / 2f);

                    bool onLHS = (point.transform.localPosition.x < lhs);
                    bool onRHS = (point.transform.localPosition.x > rhs);
                    bool below = (point.transform.localPosition.y < bot);
                    bool above = (point.transform.localPosition.y > top);

                    if (onLHS || onRHS || above || below) continue;

                    Debug.Log("COLLISION DETECTED");
                    Destroy(point.gameObject);
                }
            }
        }
    }

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
