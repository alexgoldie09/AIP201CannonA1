using System.Collections.Generic;
using UnityEngine;

/*
* This class is used for firing a projectile using the current cannon settings.
* - It is used only in Scene01 for simple launch parameters following the engine physics.
*   + Spacebar is used to shoot the projectile.
* - Displays a live trajectory preview using a LineRenderer.
* - Inherits from monobehaviour to use Unity's Update().
*/

public class CannonFire : MonoBehaviour
{
    [Header("Firing Settings")]
    [SerializeField] private GameObject projectilePrefab; // Projectile prefab with a CustomPhysicsBody component.
    [SerializeField] private Transform spawnPoint; // Where the projectile is spawned.
    [SerializeField] private CannonController cannon; // Reference to the cannon controller.

    [Header("Trajectory Preview Settings")]
    [SerializeField] private LineRenderer lineRenderer; // LineRenderer component to generate line.
    [SerializeField] private int numPoints = 200; // Number of simulation steps for the trajectory.
    [SerializeField] private float timeStep = 0.02f; // Time step used for simulating the trajectory.
    [SerializeField] private float floorLevel = -10f; // The y-level below which the trajectory stops.
    private float simulationDampingFactor = 0.99f; // Damping factor matching the physics engine.

    /*
     * Update() is called once per frame.
     * - Calculates and sets the trajectory preview.
     * - Fires projectile based on spacebar input.
     */
    private void Update()
    {
        // Calculate the initial velocity from the cannon's settings
        Vector2 initialVelocity = GetLaunchVelocity();

        /* 
         * Retrieve projectile physics settings from the prefab.
         * If there is no Custom Physics Body, set mass and gravityScale to 1 by default
         */
        CustomPhysicsBody projPhysics = projectilePrefab.GetComponent<CustomPhysicsBody>();
        float mass = (projPhysics != null) ? projPhysics.GetMass() : 1f;
        float gravityScale = (projPhysics != null) ? projPhysics.GetGravityScale() : 1f;

        // Calculate the trajectory points using simulation vectors
        List<Vector3> trajectoryPoints = CalculateTrajectory(spawnPoint.position, initialVelocity, mass, gravityScale);
        // Set the dot position based on the returned vector, visualising the line
        lineRenderer.positionCount = trajectoryPoints.Count;
        lineRenderer.SetPositions(trajectoryPoints.ToArray());

        // Fire the projectile when the Spacebar is pressed
        if (Input.GetKeyDown(KeyCode.Space))
        {
            Fire();
        }
    }

    /*
     * Fire() instantiates and sets the velocity for the projectile's physics.
     * - Projectile must have a CustomPhysicsBodyComponent to be fired.
     */
    private void Fire()
    {
        GameObject projectile = Instantiate(projectilePrefab, spawnPoint.position, Quaternion.identity);
        CustomPhysicsBody physicsBody = projectile.GetComponent<CustomPhysicsBody>();

        if (physicsBody != null)
        {
            // Set the projectile's velocity based on the cannon's angle and power
            physicsBody.SetInitialVelocity(GetLaunchVelocity());
        }
        else
        {
            Debug.LogError("Projectile is missing CustomPhysicsBody component!");
        }
    }

    /*
     * CalculateTrajectory() uses a start position, initial velocity, mass, and gravity scale to
     * calculate the points on which the projectile will travel and then return those points as
     * a List<Vector3>.
     * - It uses CustomPhysicsEngine calculations alongside the cannon pivot location, power, 
     *   and angle to determine an accurate trajectory of the projectile's path.
     * - The higher the number of points, the more accurate the parabola is in the line.
     * - The lower the time step, the size and angle of the line is more accurate to the trajectory.
     */
    private List<Vector3> CalculateTrajectory(Vector3 startPosition, Vector2 initialVelocity, float mass, float gravityScale)
    {
        List<Vector3> points = new List<Vector3>();
        Vector3 currentPosition = startPosition;
        Vector2 currentVelocity = initialVelocity;
        float dt = timeStep; // 0.02f was used as a comfortable default

        for (int i = 0; i < numPoints; i++) // 200f was used as a comfortable default
        {
            points.Add(currentPosition);

            // Apply gravity (F = GRAVITY * mass * gravityScale) to the vertical velocity
            currentVelocity.y += CustomPhysicsBody.GRAVITY * gravityScale * mass * dt;
            // Apply damping to simulate energy loss
            currentVelocity *= simulationDampingFactor;
            // Update the trajectory's position to the next position
            currentPosition += (Vector3)(currentVelocity * dt);

            // Stop simulation if the projectile drops below the floor
            if (currentPosition.y < floorLevel)
            {
                points.Add(currentPosition);
                break;
            }

            /* 
             * Optionally, check for collision with any axis-aligned rectangle collider in the scene.
             * If the collider is not an axis-aligned rectangle, continue trajectory preview.
             */
            foreach (CustomCollider col in FindObjectsOfType<CustomCollider>())
            {
                if (col.type != CustomCollider.Type.AXIS_ALIGNED_RECTANGLE) { continue; }
                
                col.UpdateBounds();

                if (col.GetBounds().Contains(currentPosition))
                {
                    points.Add(currentPosition);
                    return points;
                }
            }
        }
        return points;
    }

    // Used for Editor debugging. Commented out.
    //private void OnDrawGizmos()
    //{
    //    if (spawnPoint == null || cannon == null || projectilePrefab == null)
    //        return;

    //    CustomPhysicsBody projPhysics = projectilePrefab.GetComponent<CustomPhysicsBody>();
    //    float mass = (projPhysics != null) ? projPhysics.GetMass() : 1f;
    //    float gravityScale = (projPhysics != null) ? projPhysics.GetGravityScale() : 1f;

    //    Vector2 initialVelocity = GetLaunchVelocity();
    //    List<Vector3> trajectoryPoints = CalculateTrajectory(spawnPoint.position, initialVelocity, mass, gravityScale);

    //    Gizmos.color = Color.cyan;
    //    for (int i = 0; i < trajectoryPoints.Count - 1; i++)
    //    {
    //        Gizmos.DrawLine(trajectoryPoints[i], trajectoryPoints[i + 1]);
    //    }
    //}

    /*
     * GetLaunchVelocity() returns the cannon's firing velocity as a new vector.
     * - Converts the cannon's launch angle to radians (since trigonometric functions use rad).
     * - Velocity vector is a product of the cannon's power with the angle using the appropriate
     *   trigonometric function.
     *   + This forms the parabolic shape for shooting at a proper angle
     */
    public Vector2 GetLaunchVelocity()
    {
        float angle = cannon.GetLaunchAngle() * Mathf.Deg2Rad;
        float power = cannon.GetPower();

        float xVelocity = power * Mathf.Cos(angle);
        float yVelocity = power * Mathf.Sin(angle);

        return new Vector2(xVelocity, yVelocity);
    }
}
