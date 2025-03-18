using UnityEngine;

/*
* This class is used for creating object's physics body settings.
* - It allows objects to have velocity, acceleration, gravity, mass, and restitution (bounciness).
*  + GRAVITY is set as a constant to a default -9.81 for emulating earth's standard gravity. 
*  + gravityScale and mass allows custom adjustment of the rate of gravity applied.
*  + initialVelocity and initialAcceleration allows the designer to set initial values during the spawn of the object.
*  + Hybrid approach to allow velocity and acceleration by the physics engine rather than the inspector due to class encapsulation.
*  + mass and restitution also affect interaction physics through collisions.
*  + gravityDelay and isKinematic allow custom kinematics to be used as opposed to default physics here.
* - Provides manual control over physics calculations.
* - Inherits from monobehaviour to use Unity's in-built Start() and Update().
*/

public class CustomPhysicsBody: MonoBehaviour
{
    [Header("Physics Settings")]
    [SerializeField] private Vector2 initialVelocity; // Allows setting an initial velocity in Inspector.
    [SerializeField] private Vector2 initialAcceleration; // Allows setting an initial acceleration in Inspector.
    [SerializeField] private float gravityScale = 1.0f; // Gravity multiplier (Default = 1, full gravity effect).
    [SerializeField] private float mass = 1.0f;  // Mass property (Default = 1, affects gravity & momentum).
    [SerializeField] private float restitution = 1.0f; // Restitution (Default = 1, determines bounce intensity).
    [Header("Kinematic Settings")]
    [SerializeField] private float gravityDelay = 0f; // Delays gravity effect (Used as timer).
    [SerializeField] private bool isKinematic = false; // Tracks if physics updates should be applied.
    private bool isGrounded = false; // Tracks if object is on the ground.



    public Vector2 Velocity { get; private set; } // Object's current velocity (updated by physics).
    public Vector2 Acceleration { get; private set; } // Object's current acceleration.
    public const float GRAVITY = -9.81f; // Default gravity (simulating Earth's gravity).

    /*
     * Start() is called when the object is initialised.
     * - Sets initial velocity and acceleration.
     * - Registers this physics body in the CustomPhysicsEngine.
     *  + A ternary operator is used just in case of an error in referencing the scene instance is null.
     */

    private void Start()
    {
        SetVelocity(initialVelocity);
        SetAcceleration(initialAcceleration);
        CustomPhysicsEngine.Instance?.RegisterBody(this);
    }

    /*
     * OnDestroy() is called when the object is removed from the scene.
     * - Deregisters this physics body from the CustomPhysicsEngine to prevent errors.
     *  + A ternary operator is used just in case of an error in referencing the scene instance is null.
     */
    private void OnDestroy()
    {
        CustomPhysicsEngine.Instance?.DeregisterBody(this);
    }

    // ReduceGravityDelay() reduces gravity delay by a time value before allowing gravity.
    public void ReduceGravityDelay(float deltaTime) => gravityDelay -= deltaTime;

    #region **Setters & Getters**
    /*
     * SetVelocity() using new velocity which directly modifies the physics value while ensuring:
     * - No NaN (Not-a-Number) values.
     * - Objects wake up from the grounded state if velocity is applied.
     * - Small velocities are rounded to zero to prevent jittery movement.
     */

    public Vector2 SetVelocity(Vector2 newVelocity)
    {
        // Prevents NaN errors which occurred in CircleToCircle() collision
        if (float.IsNaN(newVelocity.x) || float.IsNaN(newVelocity.y)) { return Velocity; }

        // If new velocity is applied, wake up the object
        if (newVelocity.magnitude > 0.1f) { SetGrounded(false); }

        // Manually stop very small movement, prevents jitteryness
        float velocityThreshold = 0.1f;
        if (Mathf.Abs(newVelocity.x) < velocityThreshold) { newVelocity.x = 0f; }
        if (Mathf.Abs(newVelocity.y) < velocityThreshold) { newVelocity.y = 0f; }

        Velocity = newVelocity;
        return Velocity;
    }
    // SetInitialVelocity() using a new initial velocity when spawning the object.
    public Vector2 SetInitialVelocity(Vector2 newInitialVelocity) => initialVelocity = newInitialVelocity;
    // SetAcceleration() using new acceleration which directly modifies the physics value.
    public Vector2 SetAcceleration(Vector2 newAcceleration) => Acceleration = newAcceleration;
    // SetInitialAcceleration() using a new initial acceleration when spawning the object.
    public Vector2 SetInitialAcceleration(Vector2 newInitialAcceleration) => initialAcceleration = newInitialAcceleration;
    // SetGravityDelay() using a new gravity delay for preventing gravity being used.
    public float SetGravityDelay(float newGravityDelay) => gravityDelay = newGravityDelay;
    // SetRestitution() using new resititution which directly modifies the physics value.
    public float SetRestitution(float newRestitution) => restitution = newRestitution;
    /*
     * SetGrounded() uses the value to set the object being on the ground (on top of an AAB) to be false/true.
     * - Used by physics calculations to disable gravity when resting.
     */
    public void SetGrounded(bool value) => isGrounded = value;
    /*
     * SetKinematic() uses the value to set the kinematic state.
     * - Used to create alternate physics updates.
     */
    public void SetKinematic(bool value) => isKinematic = value;

    // GetGravityScale() returns the object's gravity scale which directly modifies the physics value.
    public float GetGravityScale() => gravityScale;
    // GetGravityDelay() returns the object's gravity delay which may prevent gravity being applied.
    public float GetGravityDelay() => gravityDelay;
    /*
     * GetMass() returns the object's mass which directly modifies the physics value.
     * - Must avoid negative mass values due to calculation issues.
     */
    public float GetMass() => mass;
    // GetRestitution() returns the object's resitution which directly modifies the physics value.
    public float GetRestitution() => restitution;
    /* 
     * IsGrounded() returns true if the object is on the ground (on top of an AAB). False otherwise.
     * - Grounded objects stop gravity updates.
     */
    public bool IsGrounded() => isGrounded;
    /* 
     * IsKinematic() returns true if the object is in a kinematic state. False otherwise.
     * - Bypasses physics updates when true, otherwise use the CustomPhysicsEngine updates.
     */
    public bool IsKinematic() => isKinematic;
    #endregion

}
