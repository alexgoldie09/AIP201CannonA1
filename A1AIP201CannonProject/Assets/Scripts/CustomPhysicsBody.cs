using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CustomPhysicsBody: MonoBehaviour
{
    /*
     * This class is used for creating physics body settings.
     * Gravity is set as a constant to a default -9.81 for emulating earth's standard gravity however a 
     * gravity scale variable has been added to the inspector to allow custom adjustment of the rate of gravity applied.
     * Initial velocity and Initial acceleration are used to so that the designer may adjust an initial
     * speed or value however a hybrid approach to allow velocity and acceleration variables to be adjustable 
     * by the physics engine rather than the inspector due to class encapsulation and control.
     * Mass and restitution is used to account for bounciness and weight of the physics body object.
     */
    [SerializeField] private Vector2 initialVelocity;
    [SerializeField] private Vector2 initialAcceleration;
    [SerializeField] float gravityScale = 1.0f; // Gravity scale (Default = 1, full gravity effect applied)
    [SerializeField] private float mass = 1.0f;  // Mass property (Default = 1)
    [SerializeField] private float restitution = 1.0f; // Restitution (Default = 1, full bounce)
    [SerializeField] private bool isGrounded = false; // Tracks if object is on ground


    public Vector2 Velocity { get; private set; }
    public Vector2 Acceleration { get; private set; }
    public const float GRAVITY = -9.81f;

    private void Start()
    {
        SetVelocity(initialVelocity); // Set velocity to initial velocity on start
        SetAcceleration(initialAcceleration); // Set acceleration to initial acceleration on start
        /* 
         * Add this object to the custom physics engine list, a ternary operator is used just in case of
         * an error in referencing scene instance is null
        */
        CustomPhysicsEngine.Instance?.RegisterBody(this);
    }

    private void OnDestroy()
    {
        /* 
         * Remove this object from the custom physics engine list, a ternary operator is used just in case of
         * an error in referencing the scene instance is null
        */
        CustomPhysicsEngine.Instance?.DeregisterBody(this);
    }

    #region Setters & Getters
    /*
     * The following methods are used to set the velocity and acceleration to the new
     * value.
     */

    public Vector2 SetVelocity(Vector2 velocity)
    {
        if (float.IsNaN(velocity.x) || float.IsNaN(velocity.y)) return Velocity; // Prevents NaN errors

        // If new velocity is applied, wake up the object
        if (velocity.magnitude > 0.1f)
        {
            SetGrounded(false);
        }

        // Manually stop very small movement
        float velocityThreshold = 0.1f;
        if (Mathf.Abs(velocity.x) < velocityThreshold) velocity.x = 0f;
        if (Mathf.Abs(velocity.y) < velocityThreshold) velocity.y = 0f;

        Velocity = velocity;
        return Velocity;
    }

    public Vector2 SetInitialVelocity(Vector2 newVelocity) => initialVelocity = newVelocity;
    public Vector2 SetAcceleration(Vector2 accel) => Acceleration = accel;
    public Vector2 SetInitialAcceleration(Vector2 newAccel) => initialAcceleration = newAccel;
    public float SetRestitution(float newRestitution) => restitution = newRestitution;
    public void SetGrounded(bool value) => isGrounded = value;

    /*
     * The following methods are used to get the gravity scale, mass, and restitution 
     * so that it can be used for calculations.
     */

    public float GetGravityScale() => gravityScale;
    public float GetMass() => mass;  // Ensure mass is never negative
    public float GetRestitution() => restitution;
    public bool IsGrounded() => isGrounded;
    #endregion

}
