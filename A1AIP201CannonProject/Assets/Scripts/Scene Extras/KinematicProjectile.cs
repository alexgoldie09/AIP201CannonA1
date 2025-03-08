using UnityEngine;

/*
* This class is used for handling the kinematic motion of a projectile.
* - It is used only in Scene02 for calculating its own kinematic physicss.
* - Inherits from monobehaviour to use Unity's Start() and Update().
*/

public class KinematicProjectile : MonoBehaviour
{
    [SerializeField] private float speed = 10f; // The speed at which the projectile moves in kinematic mode.
    [SerializeField] private float gravityDelayOnTransition = 0.2f; // Delay before gravity takes effect.
    private Vector3 targetPosition; // The target position.
    private CustomPhysicsBody physicsBody; // The projectile's custom physics body component.

    /*
     * Start() is called when the object is initialised.
     * - Sets physicsBody to this objects component.
     */
    private void Start()
    {
        physicsBody = GetComponent<CustomPhysicsBody>();
    }

    /*
    * Update() is called once per frame.
    * - While in kinematic state, the projectile moves linearly towards the target.
    *   + Calculates distance and computes projectile time as distance / speed.
    *   + Updates projectile position.
    * - Kinematic state ends when the projectile reaches the target position or near it.
    *   + Calculates velocity as current direction * speed, continues that velocity.
    *   + Gravity is delayed to create a more natural curve off.
    *   + Projectile is no longer kinematic to ensure custom physics resume.
    *   + Destroy projectile to ensure the scene doesn't get crowded.
    */
    private void Update()
    {
        if (physicsBody != null && physicsBody.IsKinematic())
        {
            Vector3 direction = (targetPosition - transform.position).normalized;
            transform.position += direction * speed * Time.deltaTime;

            if (Vector3.Distance(transform.position, targetPosition) < 0.1f)
            {
                // Snap to target.
                transform.position = targetPosition;
                Vector3 currentVelocity = direction * speed;
                physicsBody.SetVelocity(currentVelocity);

                physicsBody.SetGravityDelay(gravityDelayOnTransition);
                physicsBody.SetKinematic(false);

                // Destroy the kinematic object after 4 seconds (Safety check)
                Destroy(gameObject, 4f);
            }
        }
    }

    // SetTargetPosition() uses a new target position to set the current target position.
    public void SetTargetPosition(Vector3 newTargetPosition) => targetPosition = newTargetPosition;
}
