using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(LineRenderer))]
public class TrajectoryVisualiser : MonoBehaviour
{
    public CannonController cannon;         // Reference to the cannon for angle and power.
    public Transform spawnPoint;             // The spawn position of the projectile.
    public GameObject projectilePrefab;      // Reference to the projectile prefab for physics properties.
    public int numPoints = 50;               // Maximum simulation steps.
    public float timeStep = 0.02f;           // Simulation time step (similar to FixedDeltaTime).
    public float floorLevel = -10f;          // Floor level at which the simulation stops.

    // Damping factor (matching what your physics engine applies).
    private float simulationDampingFactor = 0.99f;

    private LineRenderer lineRenderer;
    private CustomPhysicsBody projectilePhysics;

    private void Awake()
    {
        lineRenderer = GetComponent<LineRenderer>();

        // Automatically get the physics properties from the projectile prefab.
        if (projectilePrefab != null)
        {
            projectilePhysics = projectilePrefab.GetComponent<CustomPhysicsBody>();
            if (projectilePhysics == null)
            {
                Debug.LogWarning("The projectile prefab is missing the CustomPhysicsBody component.");
            }
        }
    }

    private void Update()
    {
        Vector2 initialVelocity = GetLaunchVelocity();
        List<Vector3> trajectoryPoints = CalculateTrajectory(spawnPoint.position, initialVelocity, 
            projectilePrefab.GetComponent<CustomPhysicsBody>().GetMass(), 
            projectilePrefab.GetComponent<CustomPhysicsBody>().GetGravityScale());
        lineRenderer.positionCount = trajectoryPoints.Count;
        lineRenderer.SetPositions(trajectoryPoints.ToArray());
    }

    // Uses the cannon's angle and power to calculate the launch velocity.
    private Vector2 GetLaunchVelocity()
    {
        float angle = cannon.GetLaunchAngle() * Mathf.Deg2Rad;
        float power = cannon.GetPower();
        float xVelocity = power * Mathf.Cos(angle);
        float yVelocity = power * Mathf.Sin(angle);
        return new Vector2(xVelocity, yVelocity);
    }

    private List<Vector3> CalculateTrajectory(Vector3 startPosition, Vector2 initialVelocity, float mass, float gravityScale)
    {
        List<Vector3> points = new List<Vector3>();
        Vector3 currentPosition = startPosition;
        Vector2 currentVelocity = initialVelocity;
        float dt = timeStep;

        for (int i = 0; i < numPoints; i++)
        {
            points.Add(currentPosition);

            // Apply gravity based on the projectile prefab's mass and gravity scale.
            currentVelocity.y += CustomPhysicsBody.GRAVITY * gravityScale * mass * dt;
            // Apply damping (using the same simulationDampingFactor as in runtime).
            currentVelocity *= simulationDampingFactor;

            // Update the position.
            currentPosition += (Vector3)(currentVelocity * dt);

            // Stop if below the floor level.
            if (currentPosition.y < floorLevel)
            {
                points.Add(currentPosition);
                break;
            }

            // Optionally, check for collision with any AXIS_ALIGNED_RECTANGLE collider.
            foreach (CustomCollider col in FindObjectsOfType<CustomCollider>())
            {
                if (col.type != CustomCollider.Type.AXIS_ALIGNED_RECTANGLE)
                    continue;

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

    private void OnDrawGizmos()
    {
        if (spawnPoint == null || cannon == null || projectilePrefab == null)
            return;

        // Retrieve the projectile's physics component from the prefab.
        CustomPhysicsBody projPhysics = projectilePrefab.GetComponent<CustomPhysicsBody>();
        float mass = (projPhysics != null) ? projPhysics.GetMass() : 1f;
        float gravityScale = (projPhysics != null) ? projPhysics.GetGravityScale() : 1f;

        Vector2 initialVelocity = GetLaunchVelocity();
        List<Vector3> trajectoryPoints = CalculateTrajectory(spawnPoint.position, initialVelocity, mass, gravityScale);

        Gizmos.color = Color.cyan;
        for (int i = 0; i < trajectoryPoints.Count - 1; i++)
        {
            Gizmos.DrawLine(trajectoryPoints[i], trajectoryPoints[i + 1]);
        }
    }
}
