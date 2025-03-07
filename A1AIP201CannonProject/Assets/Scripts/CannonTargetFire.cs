using UnityEngine;

public class CannonTargetFire : MonoBehaviour
{
    [Header("References")]
    public GameObject projectilePrefab; // Must have CustomPhysicsBody and KinematicProjectile components
    public Transform spawnPoint;
    public CannonController cannon;     // For rotating the cannon pivot

    void Update()
    {
        if (Input.GetMouseButtonDown(0))
        {
            Vector3 targetPoint = Camera.main.ScreenToWorldPoint(Input.mousePosition);
            targetPoint.z = 0f;
            FireAtTarget(targetPoint);
        }
    }

    void FireAtTarget(Vector3 target)
    {
        // Rotate the cannon to face the target.
        Vector2 direction = target - cannon.cannonPivot.position;
        float angleDeg = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;
        cannon.cannonPivot.rotation = Quaternion.Euler(0, 0, angleDeg);

        // Instantiate the projectile.
        GameObject projectile = Instantiate(projectilePrefab, spawnPoint.position, Quaternion.identity);

        // Get the necessary components.
        CustomPhysicsBody physicsBody = projectile.GetComponent<CustomPhysicsBody>();
        KinematicProjectile kinProj = projectile.GetComponent<KinematicProjectile>();

        if (physicsBody != null && kinProj != null)
        {
            // Set the projectile to kinematic mode so that custom physics is bypassed.
            physicsBody.SetKinematic(true);
            // Set the target position for the projectile.
            kinProj.SetTargetPosition(target);
            // Optionally, adjust kinProj.kinematicSpeed if needed (or leave it to the inspector)
        }
        else
        {
            Debug.LogError("Projectile is missing required components!");
        }
    }
}
