using UnityEngine;

/*
* This class is used for firing a projectile at the target using kinematic physics.
* - It is used only in Scene02 for pointing at a target and shooting.
*   + Left mouse click is used to shoot the projectile.
* - Inherits from monobehaviour to use Unity's Update().
*/

public class CannonTargetFire : MonoBehaviour
{
    [Header("Firing Settings")]
    [SerializeField] private GameObject projectilePrefab; // Projectile prefab with a CustomPhysicsBody component.
    [SerializeField] private Transform spawnPoint; // Where the projectile is spawned.
    [SerializeField] private CannonController cannon; // Reference to the cannon controller.

    /*
     * Update() is called once per frame.
     * - On click input, convert mouse position to target vector.
     * - Fires projectile to target vector.
     */
    private void Update()
    {
        if (Input.GetMouseButtonDown(0))
        {
            Vector3 targetPoint = Camera.main.ScreenToWorldPoint(Input.mousePosition);
            targetPoint.z = 0f;
            FireAtTarget(targetPoint);
        }
    }

    /*
     * FireAtTarget() instantiates and sets the velocity for the projectile's physics.
     * - Projectile must have a CustomPhysicsBodyComponent and KinematicProjectile to be fired.
     *  + Unlike Scene01 this has an additional kinematic component which essentially
     *    means that the projectile reaches the target at a prescribed speed and time.
     */
    private void FireAtTarget(Vector3 target)
    {
        // Rotate the cannon to face the target
        Vector2 direction = target - cannon.GetCannonPivot().position;
        float angleDeg = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;
        cannon.GetCannonPivot().rotation = Quaternion.Euler(0, 0, angleDeg);

        GameObject projectile = Instantiate(projectilePrefab, spawnPoint.position, Quaternion.identity);

        // Get the necessary components.
        CustomPhysicsBody physicsBody = projectile.GetComponent<CustomPhysicsBody>();
        KinematicProjectile kinProj = projectile.GetComponent<KinematicProjectile>();

        if (physicsBody != null && kinProj != null)
        {
            // Set the projectile to kinematic mode so that custom physics is bypassed
            physicsBody.SetKinematic(true);
            // Set the target position for the projectile
            kinProj.SetTargetPosition(target);
        }
        else
        {
            Debug.LogError("Projectile is missing required components!");
        }
    }
}
